using System.Text;
using Asp.Versioning;
using HelpDesk.Api.Filters;
using HelpDesk.Api.Middleware;
using HelpDesk.Api.Services;
using HelpDesk.Application;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Options;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting HelpDesk.Api");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ---- Application / Infrastructure ----
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ---- ASP.NET Identity (configuration only; no auth endpoints yet) ----
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // ---- JWT ----
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing.");

    if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || Encoding.UTF8.GetByteCount(jwtOptions.SecretKey) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:SecretKey must be configured and at least 32 bytes long (set via 'dotnet user-secrets set \"Jwt:SecretKey\" \"...\"'). " +
            "A short or missing key would make JWT signatures forgeable.");
    }

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    // ---- Role-based and policy-based authorization ----
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"))
        .AddPolicy("RequireManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"))
        .AddPolicy("RequireAgentOrAbove", policy => policy.RequireRole("Admin", "Manager", "IT Support Agent"));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // ---- API versioning ----
    builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    // ---- Controllers ----
    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

    // ---- Swagger / OpenAPI ----
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "HelpDesk API",
            Version = "v1",
            Description = "IT Help Desk & Ticketing Management System API"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter a valid JWT bearer token."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ---- CORS ----
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    const string corsPolicyName = "FrontendCorsPolicy";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicyName, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ---- Health checks ----
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");

    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "ready"]);

    var app = builder.Build();

    // ---- Apply pending EF Core migrations on startup (opt-in) ----
    // Off by default so the existing local-dev workflow (manual `dotnet ef database update`) is
    // unchanged. The Docker Compose stack sets this to `true` so a fresh `docker compose up` is a
    // genuinely self-contained, working deployment without a manual migration step. Safe for a
    // single-replica deployment like this one; a multi-replica production deployment should apply
    // migrations as a separate release step instead, to avoid several instances racing to migrate.
    if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
    {
        using var migrationScope = app.Services.CreateScope();
        migrationScope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }

    // ---- Middleware pipeline ----
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseSerilogRequestLogging();

    // Swagger is available in every environment, not just Development — it documents the API
    // shape, which is already discoverable to anyone with network access to the API; it carries
    // no secrets. Keeping it on lets the Docker Compose "production-shaped" stack still serve it.
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HelpDesk API v1");
    });

    if (!app.Environment.IsDevelopment())
    {
        // HSTS relies on the browser having already seen at least one HTTPS response, so it's only
        // meaningful once the app is actually served over HTTPS in a non-dev environment.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseCors(corsPolicyName);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "HelpDesk.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Exposed for WebApplicationFactory-based integration tests.
/// </summary>
public partial class Program
{
}
