using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Zone : Entity
    {
        public Zone(Guid zoneId, string name, string? description, bool isActive, Tracking? tracking = null)
            : base(zoneId)
        {
            Name = name;
            Description = description;
            IsActive = isActive;
            Tracking = tracking;
        }

        public string Name { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public void Update(string name, string? description, Guid? updatedBy = null)
        {
            Name = name;
            Description = description;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Activate(Guid? updatedBy = null)
        {
            IsActive = true;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Deactivate(Guid? updatedBy = null)
        {
            IsActive = false;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
