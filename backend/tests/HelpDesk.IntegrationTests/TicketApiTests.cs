using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Lookups.Dtos;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Domain.Identity;
using HelpDesk.IntegrationTests.Infrastructure;

namespace HelpDesk.IntegrationTests;

/// <summary>
/// End-to-end ticket CRUD through the real HTTP pipeline (auth -> lookups -> create -> read ->
/// update -> delete), as opposed to the unit-level TicketServiceTests which exercise TicketService
/// directly against an in-memory DbContext.
/// </summary>
[Collection(ApiCollection.Name)]
public class TicketApiTests
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public TicketApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CategoryId, Guid PriorityId)> GetLookupIdsAsync(string accessToken)
    {
        using var categoriesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/lookups/categories");
        categoriesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var categoriesResponse = await _client.SendAsync(categoriesRequest);
        categoriesResponse.EnsureSuccessStatusCode();
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<LookupItemDto>>(TestJson.Options);

        using var prioritiesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/lookups/priorities");
        prioritiesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var prioritiesResponse = await _client.SendAsync(prioritiesRequest);
        prioritiesResponse.EnsureSuccessStatusCode();
        var priorities = await prioritiesResponse.Content.ReadFromJsonAsync<List<LookupItemDto>>(TestJson.Options);

        return (categories![0].Id, priorities![0].Id);
    }

    private async Task<TicketDto> CreateTicketAsync(string accessToken, string title)
    {
        var (categoryId, priorityId) = await GetLookupIdsAsync(accessToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
        {
            Content = JsonContent.Create(new CreateTicketRequest
            {
                Title = title,
                Description = "Created by TicketApiTests.",
                CategoryId = categoryId,
                PriorityId = priorityId,
            }, options: TestJson.Options),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TicketDto>(TestJson.Options))!;
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheSameTicket()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-create");
        var title = $"Printer jam {Guid.NewGuid():N}";

        var created = await CreateTicketAsync(auth.AccessToken, title);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var getResponse = await _client.SendAsync(getRequest);

        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<TicketDto>(TestJson.Options);
        Assert.Equal(title, fetched!.Title);
        Assert.StartsWith("HD-", fetched.TicketNumber);
    }

    [Fact]
    public async Task GetById_ForNonExistentTicket_ReturnsNotFound()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-404");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutRequiredFields_ReturnsBadRequest()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-invalid");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
        {
            Content = JsonContent.Create(new CreateTicketRequest(), options: TestJson.Options),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ByOwner_PersistsChanges()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-update");
        var created = await CreateTicketAsync(auth.AccessToken, $"Original title {Guid.NewGuid():N}");

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/tickets/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateTicketRequest
            {
                Title = "Updated title",
                Description = created.Description,
                CategoryId = created.CategoryId,
                PriorityId = created.PriorityId,
                StatusId = created.StatusId,
            }, options: TestJson.Options),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var updateResponse = await _client.SendAsync(updateRequest);

        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<TicketDto>(TestJson.Options);
        Assert.Equal("Updated title", updated!.Title);
    }

    [Fact]
    public async Task Delete_ByOwnerEmployee_ReturnsForbidden()
    {
        // Deleting is RequireAgentOrAbove; the ticket's own creator (a plain Employee) still can't.
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-delete-employee");
        var created = await CreateTicketAsync(auth.AccessToken, $"To delete {Guid.NewGuid():N}");

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ByAgent_SoftDeletesTicket()
    {
        var (_, _, ownerAuth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-delete-owner");
        var created = await CreateTicketAsync(ownerAuth.AccessToken, $"Agent will delete {Guid.NewGuid():N}");

        var (_, _, agentAuth) = await AuthTestHelper.RegisterAndPromoteAsync(
            _client, _factory, "ticket-delete-agent", RoleNames.ItSupportAgent);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agentAuth.AccessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerAuth.AccessToken);
        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsPagedResultContainingCreatedTicket()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-search");
        var uniqueTitle = $"Searchable ticket {Guid.NewGuid():N}";
        await CreateTicketAsync(auth.AccessToken, uniqueTitle);

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/tickets?searchTerm={Uri.EscapeDataString(uniqueTitle)}&page=1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TicketListItemDto>>(TestJson.Options);
        Assert.Contains(result!.Items, t => t.Title == uniqueTitle);
    }

    [Fact]
    public async Task GetById_ByDifferentEmployee_ReturnsForbidden()
    {
        var (_, _, ownerAuth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-owner");
        var created = await CreateTicketAsync(ownerAuth.AccessToken, $"Private ticket {Guid.NewGuid():N}");

        var (_, _, otherAuth) = await AuthTestHelper.RegisterNewUserAsync(_client, "ticket-other");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{created.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherAuth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
