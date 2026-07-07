using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Options;
using HelpDesk.Application.Features.Assignments.Interfaces;
using HelpDesk.Application.Features.Auth.Interfaces;
using HelpDesk.Application.Features.Comments.Interfaces;
using HelpDesk.Application.Features.Dashboard.Interfaces;
using HelpDesk.Application.Features.Lookups.Interfaces;
using HelpDesk.Application.Features.Notifications.Interfaces;
using HelpDesk.Application.Features.Reports.Interfaces;
using HelpDesk.Application.Features.Tickets.Interfaces;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure;

/// <summary>
/// Registers persistence and infrastructure-level services. ASP.NET Identity's <c>AddIdentity</c>
/// call lives in the Api project instead: it requires the ASP.NET Core shared framework
/// (only implicitly referenced by Microsoft.NET.Sdk.Web), which this class library does not carry.
/// <c>UserManager&lt;T&gt;</c>/<c>RoleManager&lt;T&gt;</c> themselves resolve fine here — only
/// <c>SignInManager&lt;T&gt;</c> needs the web framework, and this stateless JWT API doesn't use it.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
