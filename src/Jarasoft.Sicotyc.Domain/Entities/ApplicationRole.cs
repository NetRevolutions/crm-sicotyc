using Microsoft.AspNetCore.Identity;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public class ApplicationRole: IdentityRole<Guid>
    {
    public static readonly Guid SuperAdministradorId = Guid.Parse("E8B4B95E-7F0B-4C73-9E76-5E16BF7B0001");
    public static readonly Guid AdministradorEmpresaId = Guid.Parse("E8B4B95E-7F0B-4C73-9E76-5E16BF7B0002");
    public static readonly Guid UsuarioId = Guid.Parse("E8B4B95E-7F0B-4C73-9E76-5E16BF7B0003");
    public static readonly Guid CoordinadorId = Guid.Parse("E8B4B95E-7F0B-4C73-9E76-5E16BF7B0004");
    public static readonly Guid ChoferId = Guid.Parse("E8B4B95E-7F0B-4C73-9E76-5E16BF7B0005");
    public static readonly Guid FacturacionId = Guid.Parse("E8B4B95E-7F0B-4C73-9E76-5E16BF7B0006");

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