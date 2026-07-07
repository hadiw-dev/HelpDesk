namespace HelpDesk.Infrastructure.Persistence.Seed;

/// <summary>
/// Fixed identifiers for seeded lookup data and Identity roles so migrations are deterministic and idempotent.
/// </summary>
public static class SeedIds
{
    public static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static class Roles
    {
        public static readonly Guid Admin = Guid.Parse("a0000000-0000-0000-0000-000000000001");
        public static readonly Guid ItSupportAgent = Guid.Parse("a0000000-0000-0000-0000-000000000002");
        public static readonly Guid Employee = Guid.Parse("a0000000-0000-0000-0000-000000000003");
        public static readonly Guid Manager = Guid.Parse("a0000000-0000-0000-0000-000000000004");
    }

    public static class Categories
    {
        public static readonly Guid Hardware = Guid.Parse("c0000000-0000-0000-0000-000000000001");
        public static readonly Guid Software = Guid.Parse("c0000000-0000-0000-0000-000000000002");
        public static readonly Guid Network = Guid.Parse("c0000000-0000-0000-0000-000000000003");
        public static readonly Guid Email = Guid.Parse("c0000000-0000-0000-0000-000000000004");
        public static readonly Guid AccessRequest = Guid.Parse("c0000000-0000-0000-0000-000000000005");
        public static readonly Guid Other = Guid.Parse("c0000000-0000-0000-0000-000000000006");
    }

    public static class Priorities
    {
        public static readonly Guid Low = Guid.Parse("d0000000-0000-0000-0000-000000000001");
        public static readonly Guid Medium = Guid.Parse("d0000000-0000-0000-0000-000000000002");
        public static readonly Guid High = Guid.Parse("d0000000-0000-0000-0000-000000000003");
        public static readonly Guid Critical = Guid.Parse("d0000000-0000-0000-0000-000000000004");
    }

    public static class Statuses
    {
        public static readonly Guid Open = Guid.Parse("e0000000-0000-0000-0000-000000000001");
        public static readonly Guid InProgress = Guid.Parse("e0000000-0000-0000-0000-000000000002");
        public static readonly Guid Pending = Guid.Parse("e0000000-0000-0000-0000-000000000003");
        public static readonly Guid Resolved = Guid.Parse("e0000000-0000-0000-0000-000000000004");
        public static readonly Guid Closed = Guid.Parse("e0000000-0000-0000-0000-000000000005");
    }
}
