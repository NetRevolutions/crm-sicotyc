using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Company : Entity
    {
        public Company(
            Guid companyId,
            Guid companyTypeId,
            BusinessName businessName,
            TradeName tradeName,
            Ruc ruc,
            Address address,
            Email email,
            Phone phone,
            bool isTransportCompany,
            bool isActive,
            Tracking? tracking = null
            )
            : base(companyId)
        {
            Districts = new List<District>();
            Drivers = new List<Driver>();
            FreightRates = new List<FreightRate>();
            ClientNegotiatedFreightRates = new List<NegotiatedFreightRate>();
            TransportNegotiatedFreightRates = new List<NegotiatedFreightRate>();
            Quotes = new List<Quote>();
            ClientQuoteTransportOffers = new List<QuoteTransportOffer>();
            TransportQuoteTransportOffers = new List<QuoteTransportOffer>();
            UserCompanies = new List<UserCompany>();
            Vehicles = new List<Vehicle>();
            Warehouses = new List<Warehouse>();
            CompanyZones = new List<CompanyZone>();
            CompanyTypeId = companyTypeId;
            BusinessName = businessName;
            TradeName = tradeName;
            RUC = ruc;
            Address = address;
            Email = email;
            Phone = phone;
            IsTransportCompany = isTransportCompany;
            IsActive = isActive;
            Tracking = tracking;
        }
        // Object Values
        public BusinessName BusinessName { get; private set; }
        public TradeName TradeName { get; private set; }
        public Ruc RUC { get; private set; }
        public Address Address { get; private set; }
        public Email Email { get; private set; }
        public Phone Phone { get; private set; }
        public bool IsTransportCompany { get; private set; }
        public bool IsActive { get; private set; } = false;

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Relationships
        public Guid CompanyTypeId { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public ICollection<CompanyZone> CompanyZones { get; private set; } = new List<CompanyZone>();
        public ICollection<District> Districts { get; private set; } = new List<District>();
        public ICollection<Driver> Drivers { get; private set; } = new List<Driver>();
        public ICollection<FreightRate> FreightRates { get; private set; } = new List<FreightRate>();
        public ICollection<NegotiatedFreightRate> ClientNegotiatedFreightRates { get; private set; } = new List<NegotiatedFreightRate>();
        public ICollection<NegotiatedFreightRate> TransportNegotiatedFreightRates { get; private set; } = new List<NegotiatedFreightRate>();
        public ICollection<Quote> Quotes { get; private set; } = new List<Quote>();
        public ICollection<QuoteTransportOffer> ClientQuoteTransportOffers { get; private set; } = new List<QuoteTransportOffer>();
        public ICollection<QuoteTransportOffer> TransportQuoteTransportOffers { get; private set; } = new List<QuoteTransportOffer>();
        public ICollection<UserCompany> UserCompanies { get; private set; } = new List<UserCompany>();
        public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
        public ICollection<Warehouse> Warehouses { get; private set; } = new List<Warehouse>();

        // Update basic information
        public void UpdateBasicInfo(BusinessName businessName, TradeName tradeName, Ruc ruc, Guid? updatedBy = null)
        {
            BusinessName = businessName;
            TradeName = tradeName;
            RUC = ruc;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        // Update contact / optional information
        public void UpdateContact(Address? address, Email? email, Phone? phone, bool? isTransportCompany, Guid? updatedBy = null)
        {
            Address = address ?? Address;
            Email = email ?? Email;
            Phone = phone ?? Phone;

            if (isTransportCompany.HasValue)
            {
                IsTransportCompany = isTransportCompany.Value;
            }

            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        // Set or change company type relationship
        public void SetCompanyType(CompanyType? companyType, Guid? updatedBy = null)
        {
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        // Activate / Deactivate
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

        // Soft delete
        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
