namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class MenuOptionRole
    {
        public MenuOptionRole(Guid optionId, Guid roleId)
        {
            OptionId = optionId;
            RoleId = roleId;
        }

        public Guid OptionId { get; private set; }
        public MenuOption? Option { get; private set; }

        public Guid RoleId { get; private set; }
        public ApplicationRole? Role { get; private set; }

        public void SetRelations(MenuOption? option, ApplicationRole? role)
        {
            Option = option;
            OptionId = option?.OptionId ?? OptionId;
            Role = role;
            RoleId = role?.Id ?? RoleId;
        }
    }
}