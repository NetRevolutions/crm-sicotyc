using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class MenuOption
    {
        private MenuOption()
        {
            MenuOptionRoles = new List<MenuOptionRole>();
        }

        public MenuOption
        (
            Option option,
            Tracking? tracking = null
        )
        {
            MenuOptionRoles = new List<MenuOptionRole>();
            SetOption(option);
            Tracking = tracking;
        }

        // Object Values
        public Guid OptionId { get; private set; }
        public string? Title { get; private set; }
        public string? Icon { get; private set; }
        public string? Url { get; private set; }
        public int OptionOrder { get; private set; }
        public int OptionLevel { get; private set; }
        public Guid OptionParentId { get; private set; }

        [NotMapped]
        public Option Option => new(OptionId, Title, Icon, Url, OptionOrder, OptionLevel, OptionParentId);

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Relationships
        public ICollection<MenuOptionRole> MenuOptionRoles { get; private set; }

        public void Update(Option option, Guid? updatedBy = null)
        {
            SetOption(option);
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        private void SetOption(Option option)
        {
            OptionId = option.OptionId;
            Title = option.Title;
            Icon = option.Icon;
            Url = option.Url;
            OptionOrder = option.OptionOrder;
            OptionLevel = option.OptionLevel;
            OptionParentId = option.OptionParentId;
        }
    }
}
