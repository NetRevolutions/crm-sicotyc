using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class VehicleType : Entity
    {
        public VehicleType(Guid vehicleTypeId, string name, Tracking? tracking = null)
            : base(vehicleTypeId)
        {
            FreightRates = new List<FreightRate>();
            NegotiatedFreightRates = new List<NegotiatedFreightRate>();
            Vehicles = new List<Vehicle>();
            Name = name;
            Tracking = tracking;
        }

        public string Name { get; private set; }
        public Tracking? Tracking { get; private set; }

        public ICollection<FreightRate> FreightRates { get; private set; }
        public ICollection<NegotiatedFreightRate> NegotiatedFreightRates { get; private set; }
        public ICollection<Vehicle> Vehicles { get; private set; }

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