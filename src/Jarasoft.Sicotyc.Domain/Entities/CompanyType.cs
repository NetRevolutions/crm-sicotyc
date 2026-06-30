using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class CompanyType : Entity
    {
        public CompanyType(Guid id) : base(id)
        {
            Companies = new List<Company>();
            CompanyZones = new List<CompanyZone>();
            Drivers = new List<Driver>();
            FreightRates = new List<FreightRate>();
            NegotiatedFreightRates = new List<NegotiatedFreightRate>();
            Quotes = new List<Quote>();
            QuoteTransportOffers = new List<QuoteTransportOffer>();
            Vehicles = new List<Vehicle>();
            Warehouses = new List<Warehouse>();
        }

        // Object Values
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; } = null;

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Relationships
        public ICollection<Company> Companies { get; private set; }
        public ICollection<CompanyZone> CompanyZones { get; private set; }
        public ICollection<Driver> Drivers { get; private set; }
        public ICollection<FreightRate> FreightRates { get; private set; }
        public ICollection<NegotiatedFreightRate> NegotiatedFreightRates { get; private set; }
        public ICollection<Quote> Quotes { get; private set; }
        public ICollection<QuoteTransportOffer> QuoteTransportOffers { get; private set; }
        public ICollection<Vehicle> Vehicles { get; private set; }
        public ICollection<Warehouse> Warehouses { get; private set; }

        // Factory
        public static CompanyType Create(string name, string? description = null, Guid? createdBy = null)
        {
            var ct = new CompanyType(Guid.NewGuid())
            {
                Name = name,
                Description = description,
                Tracking = new Tracking(DateTime.UtcNow, createdBy, null, null, false, null, null)
            };

            return ct;
        }

        // Update
        public void Update(string name, string? description, Guid? updatedBy = null)
        {
            Name = name;
            Description = description;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        // Soft delete
        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
