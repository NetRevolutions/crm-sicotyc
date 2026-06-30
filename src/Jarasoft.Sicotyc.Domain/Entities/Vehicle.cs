using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Vehicle : Entity
    {
        public Vehicle(
            Guid vehicleId,
            Guid companyId,
            Guid companyTypeId,
            Guid vehicleTypeId,
            string plateNumber,
            string brand,
            string model,
            int year,
            decimal maxWeight,
            decimal maxVolumeM3,
            bool isAvailable,
            bool isActive,
            Tracking? tracking = null)
            : base(vehicleId)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            VehicleTypeId = vehicleTypeId;
            PlateNumber = plateNumber;
            Brand = brand;
            Model = model;
            Year = year;
            MaxWeight = maxWeight;
            MaxVolumeM3 = maxVolumeM3;
            IsAvailable = isAvailable;
            IsActive = isActive;
            Tracking = tracking;
        }

        public Guid CompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid VehicleTypeId { get; private set; }
        public string PlateNumber { get; private set; }
        public string Brand { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public decimal MaxWeight { get; private set; }
        public decimal MaxVolumeM3 { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public Company? Company { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public VehicleType? VehicleType { get; private set; }

        public void UpdateDetails(
            string plateNumber,
            string brand,
            string model,
            int year,
            decimal maxWeight,
            decimal maxVolumeM3,
            Guid? updatedBy = null)
        {
            PlateNumber = plateNumber;
            Brand = brand;
            Model = model;
            Year = year;
            MaxWeight = maxWeight;
            MaxVolumeM3 = maxVolumeM3;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetCompany(Guid companyId, Guid companyTypeId, Guid? updatedBy = null)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetVehicleType(Guid vehicleTypeId, Guid? updatedBy = null)
        {
            VehicleTypeId = vehicleTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(Company? company, CompanyType? companyType, VehicleType? vehicleType, Guid? updatedBy = null)
        {
            Company = company;
            CompanyId = company?.Id ?? CompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            VehicleType = vehicleType;
            VehicleTypeId = vehicleType?.Id ?? VehicleTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetAvailability(bool isAvailable, Guid? updatedBy = null)
        {
            IsAvailable = isAvailable;
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