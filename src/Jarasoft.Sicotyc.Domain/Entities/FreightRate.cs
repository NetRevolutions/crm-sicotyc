using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class FreightRate : Entity
    {
        public FreightRate(
            Guid freightRateId,
            Guid transportCompanyId,
            Guid companyTypeId,
            Guid serviceTypeId,
            Guid originZoneId,
            Guid destinationZoneId,
            Guid vehicleTypeId,
            Guid bodyworkTypeId,
            Guid platformTypeId,
            int containerSize,
            decimal price,
            DateTime validFrom,
            DateTime validTo,
            bool isActive,
            Tracking? tracking = null)
            : base(freightRateId)
        {
            TransportCompanyId = transportCompanyId;
            CompanyTypeId = companyTypeId;
            ServiceTypeId = serviceTypeId;
            OriginZoneId = originZoneId;
            DestinationZoneId = destinationZoneId;
            VehicleTypeId = vehicleTypeId;
            BodyworkTypeId = bodyworkTypeId;
            PlatformTypeId = platformTypeId;
            ContainerSize = containerSize;
            Price = price;
            ValidFrom = validFrom;
            ValidTo = validTo;
            IsActive = isActive;
            Tracking = tracking;
        }

        public Guid TransportCompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid ServiceTypeId { get; private set; }
        public Guid OriginZoneId { get; private set; }
        public Guid DestinationZoneId { get; private set; }
        public Guid VehicleTypeId { get; private set; }
        public Guid BodyworkTypeId { get; private set; }
        public Guid PlatformTypeId { get; private set; }
        public int ContainerSize { get; private set; }
        public decimal Price { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidTo { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public Company? TransportCompany { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public ServiceType? ServiceType { get; private set; }
        public Zone? OriginZone { get; private set; }
        public Zone? DestinationZone { get; private set; }
        public VehicleType? VehicleType { get; private set; }

        public void UpdateRate(
            decimal price,
            DateTime validFrom,
            DateTime validTo,
            int containerSize,
            Guid? updatedBy = null)
        {
            Price = price;
            ValidFrom = validFrom;
            ValidTo = validTo;
            ContainerSize = containerSize;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRoute(Guid originZoneId, Guid destinationZoneId, Guid? updatedBy = null)
        {
            OriginZoneId = originZoneId;
            DestinationZoneId = destinationZoneId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetEquipment(Guid vehicleTypeId, Guid bodyworkTypeId, Guid platformTypeId, Guid? updatedBy = null)
        {
            VehicleTypeId = vehicleTypeId;
            BodyworkTypeId = bodyworkTypeId;
            PlatformTypeId = platformTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(
            Company? transportCompany,
            CompanyType? companyType,
            ServiceType? serviceType,
            Zone? originZone,
            Zone? destinationZone,
            VehicleType? vehicleType,
            Guid? updatedBy = null)
        {
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