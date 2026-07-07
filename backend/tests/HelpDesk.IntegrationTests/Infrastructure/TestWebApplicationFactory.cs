using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HelpDesk.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real Api pipeline in the Development environment so it targets the LocalDB
/// instance that already has the InitialCreate migration applied.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
