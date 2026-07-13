using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class NegotiatedFreightRate : Entity
    {
        public NegotiatedFreightRate(
            Guid negotiatedFreightRateId,
            Guid freightRateId,
            Guid transportCompanyId,
            Guid companyTypeId,
            Guid serviceTypeId,
            Guid originZoneId,
            Guid destinationZoneId,
            Guid vehicleTypeId,
            Guid clientCompanyId,
            decimal specialPrice,
            DateTime validFrom,
            DateTime validTo,
            bool isActive,
            Tracking? tracking = null)
            : base(negotiatedFreightRateId)
        {
            FreightRateId = freightRateId;
            TransportCompanyId = transportCompanyId;
            CompanyTypeId = companyTypeId;
            ServiceTypeId = serviceTypeId;
            OriginZoneId = originZoneId;
            DestinationZoneId = destinationZoneId;
            VehicleTypeId = vehicleTypeId;
            ClientCompanyId = clientCompanyId;
            SpecialPrice = specialPrice;
            ValidFrom = validFrom;
            ValidTo = validTo;
            IsActive = isActive;
            Tracking = tracking;
        }

        public Guid FreightRateId { get; private set; }
        public Guid TransportCompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid ServiceTypeId { get; private set; }
        public Guid OriginZoneId { get; private set; }
        public Guid DestinationZoneId { get; private set; }
        public Guid VehicleTypeId { get; private set; }
        public Guid ClientCompanyId { get; private set; }
        public decimal SpecialPrice { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidTo { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public FreightRate? FreightRate { get; private set; }
        public Company? TransportCompany { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public ServiceType? ServiceType { get; private set; }
        public Zone? OriginZone { get; private set; }
        public Zone? DestinationZone { get; private set; }
        public VehicleType? VehicleType { get; private set; }
        public Company? ClientCompany { get; private set; }

        public void UpdateRate(decimal specialPrice, DateTime validFrom, DateTime validTo, Guid? updatedBy = null)
        {
            SpecialPrice = specialPrice;
            ValidFrom = validFrom;
            ValidTo = validTo;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetClient(Guid clientCompanyId, Guid? updatedBy = null)
        {
            ClientCompanyId = clientCompanyId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetFreightRate(FreightRate? freightRate, Guid? updatedBy = null)
        {
            FreightRate = freightRate;
            FreightRateId = freightRate?.Id ?? FreightRateId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(
            FreightRate? freightRate,
            Company? transportCompany,
            CompanyType? companyType,
            ServiceType? serviceType,
            Zone? originZone,
            Zone? destinationZone,
            VehicleType? vehicleType,
            Company? clientCompany,
            Guid? updatedBy = null)
        {
            FreightRate = freightRate;
            FreightRateId = freightRate?.Id ?? FreightRateId;
            TransportCompany = transportCompany;
            TransportCompanyId = transportCompany?.Id ?? TransportCompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            ServiceType = serviceType;
            ServiceTypeId = serviceType?.Id ?? ServiceTypeId;
            OriginZone = originZone;
            OriginZoneId = originZone?.Id ?? OriginZoneId;
            DestinationZone = destinationZone;
            DestinationZoneId = destinationZone?.Id ?? DestinationZoneId;
            VehicleType = vehicleType;
            VehicleTypeId = vehicleType?.Id ?? VehicleTypeId;
            ClientCompany = clientCompany;
            ClientCompanyId = clientCompany?.Id ?? ClientCompanyId;
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