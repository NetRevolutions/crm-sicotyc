namespace Jarasoft.Sicotyc.Application.Abstractions.Authentication;

public static class ApplicationRoleRules
{
    public static readonly string[] AdministratorAssignableRoles =
    [
        ApplicationRoles.Administrator,
        ApplicationRoles.Coordinator,
        ApplicationRoles.Operations,
        ApplicationRoles.Billing,
        ApplicationRoles.User
    ];

    public static readonly string[] SuperAdministratorAssignableRoles =
    [
        ApplicationRoles.SuperAdministrator,
        ApplicationRoles.Administrator,
        ApplicationRoles.Coordinator,
        ApplicationRoles.Operations,
        ApplicationRoles.Billing,
        ApplicationRoles.User,
        ApplicationRoles.Customer
    ];
}