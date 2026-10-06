namespace Jarasoft.Sicotyc.Application.Abstractions.Authentication;

public static class ApplicationRoles
{
    public const string SuperAdministrator = "SuperAdministrator";
    public const string Administrator = "Administrator";
    public const string Coordinator = "Coordinator";
    public const string Operations = "Operations";
    public const string Billing = "Billing";
    public const string User = "User";
    public const string Customer = "Customer";

    public static readonly string[] All =
    [
        SuperAdministrator,
        Administrator,
        Coordinator,
        Operations,
        Billing,
        User,
        Customer
    ];
}

