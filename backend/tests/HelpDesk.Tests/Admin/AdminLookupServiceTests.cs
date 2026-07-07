using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Features.Admin.Lookups.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Admin;

public class AdminLookupServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_OrdersByDisplayOrder()
    {
        await using var db = CreateDbContext();
        db.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), Name = "Software", DisplayOrder = 2 },
            new Category { Id = Guid.NewGuid(), Name = "Hardware", DisplayOrder = 1 });
        await db.SaveChangesAsync();

        var sut = new AdminLookupService<Category>(db);
        var result = await sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Hardware", result[0].Name);
        Assert.Equal("Software", result[1].Name);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesEntity()
    {
        await using var db = CreateDbContext();
        var sut = new AdminLookupService<Priority>(db);

        var result = await sut.CreateAsync(new LookupUpsertRequest { Name = "Urgent", DisplayOrder = 5, IsActive = true });

        Assert.Equal("Urgent", result.Name);
        Assert.Equal(1, await db.Priorities.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsConflictAppException()
    {
        await using var db = CreateDbContext();
        db.Statuses.Add(new Status { Id = Guid.NewGuid(), Name = "Open", DisplayOrder = 1 });
        await db.SaveChangesAsync();

        var sut = new AdminLookupService<Status>(db);

        await Assert.ThrowsAsync<ConflictAppException>(
            () => sut.CreateAsync(new LookupUpsertRequest { Name = "Open", DisplayOrder = 2 }));
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesFields()
    {
        await using var db = CreateDbContext();
        var id = Guid.NewGuid();
        db.Categories.Add(new Category { Id = id, Name = "Hardware", DisplayOrder = 1, IsActive = true });
        await db.SaveChangesAsync();

        var sut = new AdminLookupService<Category>(db);
        var result = await sut.UpdateAsync(id, new LookupUpsertRequest { Name = "Hardware & Peripherals", DisplayOrder = 3, IsActive = false });

        Assert.Equal("Hardware & Peripherals", result.Name);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsNotFoundAppException()
    {
        await using var db = CreateDbContext();
        var sut = new AdminLookupService<Category>(db);

        await Assert.ThrowsAsync<NotFoundAppException>(
            () => sut.UpdateAsync(Guid.NewGuid(), new LookupUpsertRequest { Name = "X", DisplayOrder = 1 }));
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntity()
    {
        await using var db = CreateDbContext();
        var id = Guid.NewGuid();
        db.Priorities.Add(new Priority { Id = id, Name = "Low", DisplayOrder = 1 });
        await db.SaveChangesAsync();

        var sut = new AdminLookupService<Priority>(db);
        await sut.DeleteAsync(id);

        var visible = await db.Priorities.FirstOrDefaultAsync(p => p.Id == id);
        Assert.Null(visible);

        var actual = await db.Priorities.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
        Assert.NotNull(actual);
        Assert.True(actual!.IsDeleted);
    }
}
