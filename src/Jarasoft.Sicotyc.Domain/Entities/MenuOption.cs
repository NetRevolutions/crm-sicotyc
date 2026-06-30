using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class MenuOption
    {
        public MenuOption
        (
            Option option,
            Tracking? tracking = null
        )
        {
            Option = option;
            Tracking = tracking;
            MenuOptionRoles = new List<MenuOptionRole>();
        }

        // Object Values
        public Guid OptionId => Option.OptionId;
        public Option Option { get; private set; }

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Relationships
        public ICollection<MenuOptionRole> MenuOptionRoles { get; private set; }

        public void Update(Option option, Guid? updatedBy = null)
        {
            Option = option;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

    }
}
