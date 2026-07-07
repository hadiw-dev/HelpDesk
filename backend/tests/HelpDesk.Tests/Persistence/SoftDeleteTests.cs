using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Persistence;

public class SoftDeleteTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task RemovingEntity_SetsIsDeleted_AndExcludesItFromDefaultQueries()
    {
        await using var context = CreateContext();

        var category = new Category { Id = Guid.NewGuid(), Name = "Temporary", DisplayOrder = 99 };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        var visible = await context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
        var ignoringFilters = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == category.Id);

        Assert.Null(visible);
        Assert.NotNull(ignoringFilters);
        Assert.True(ignoringFilters!.IsDeleted);
        Assert.NotNull(ignoringFilters.DeletedAt);
    }

    [Fact]
    public async Task AddingEntity_StampsCreatedAt()
    {
        await using var context = CreateContext();

        var category = new Category { Id = Guid.NewGuid(), Name = "Audited", DisplayOrder = 1 };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, category.CreatedAt);
    }
}
