namespace HelpDesk.Domain.Identity;

/// <summary>The fixed set of roles seeded in Phase 1. New roles aren't creatable through the Admin UI —
/// every authorization policy in the system is hardcoded to these four names.</summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string ItSupportAgent = "IT Support Agent";
    public const string Employee = "Employee";

    public static readonly string[] All = [Admin, Manager, ItSupportAgent, Employee];
}
