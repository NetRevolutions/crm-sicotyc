using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Zone : Entity
    {
        public Zone(Guid zoneId, string name, string? description, bool isActive, Tracking? tracking = null)
            : base(zoneId)
        {
            CompanyZones = new List<CompanyZone>();
            OriginFreightRates = new List<FreightRate>();
            DestinationFreightRates = new List<FreightRate>();
            OriginNegotiatedFreightRates = new List<NegotiatedFreightRate>();
            DestinationNegotiatedFreightRates = new List<NegotiatedFreightRate>();
            Name = name;
            Description = description;
            IsActive = isActive;
            Tracking = tracking;
        }

        public string Name { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public ICollection<CompanyZone> CompanyZones { get; private set; }
        public ICollection<FreightRate> OriginFreightRates { get; private set; }
        public ICollection<FreightRate> DestinationFreightRates { get; private set; }
        public ICollection<NegotiatedFreightRate> OriginNegotiatedFreightRates { get; private set; }
        public ICollection<NegotiatedFreightRate> DestinationNegotiatedFreightRates { get; private set; }

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
