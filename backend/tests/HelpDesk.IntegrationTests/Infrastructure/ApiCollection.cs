namespace HelpDesk.IntegrationTests.Infrastructure;

/// <summary>
/// Every API test class shares this single <see cref="TestWebApplicationFactory"/> instance instead
/// of each declaring its own <c>IClassFixture</c>. Program.cs bootstraps Serilog into a process-wide
/// static (<c>Log.Logger</c>) that gets frozen the first time the host builds — a second, independent
/// WebApplicationFactory in the same test process throws "The logger is already frozen" on its own
/// host build. Sharing one factory means the host (and its Serilog bootstrap) is only built once.
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "Api collection";
}
