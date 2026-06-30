using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class VehicleType : Entity
    {
        public VehicleType(Guid vehicleTypeId, string name, Tracking? tracking = null)
            : base(vehicleTypeId)
        {
            Name = name;
            Tracking = tracking;
        }

        public string Name { get; private set; }
        public Tracking? Tracking { get; private set; }

        public void Update(string name, Guid? updatedBy = null)
        {
            Name = name;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}