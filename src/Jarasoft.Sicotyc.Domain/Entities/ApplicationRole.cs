using Microsoft.AspNetCore.Identity;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public class ApplicationRole: IdentityRole<Guid>
    {
        public ApplicationRole()
        {
            MenuOptionRoles = new List<MenuOptionRole>();
        }

        // Object Values


        // Relationships
        public ICollection<MenuOptionRole> MenuOptionRoles { get; private set; }
    }
}