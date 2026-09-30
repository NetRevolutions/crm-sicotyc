using System;
using System.Collections.Generic;
using System.Text;

namespace Jarasoft.Sicotyc.Infrastructure.Identity
{
    public static class ApplicationRoles
    {
        public const string Administrator = "Administrator";
        public const string User = "User";
        public const string Coordinator = "Coordinator";
        public const string Billing = "Billing";
        public const string Operations = "Operations";

        public static readonly string[] All = [
            Administrator,
            User,
            Coordinator,
            Billing,
            Operations
        ];
    }
}
