using Microsoft.AspNetCore.Identity;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public class ApplicationRole: IdentityRole<Guid>
    {
        public ApplicationRole()
        {
            MenuOptionRoles = new List<MenuOptionRole>();
            Users = new List<ApplicationUser>();
        }

        // Object Values


        // Relationships
        public ICollection<MenuOptionRole> MenuOptionRoles { get; private set; }
        public ICollection<ApplicationUser> Users { get; private set; }
    }
}