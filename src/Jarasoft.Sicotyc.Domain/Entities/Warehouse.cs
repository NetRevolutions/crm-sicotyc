using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Warehouse : Entity
    {
        public Warehouse(Guid warehouseId, Guid companyId, Guid companyTypeId, Guid districtId,
            string name, string address, bool isActive, Tracking? tracking = null)
            : base(warehouseId)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            DistrictId = districtId;
            Name = name;
            Address = address;
            IsActive = isActive;
            Tracking = tracking;
        }

        public Guid CompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid DistrictId { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public Company? Company { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public District? District { get; private set; }

        public void UpdateContact(string name, string address, Guid? updatedBy = null)
        {
            Name = name;
            Address = address;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetCompany(Guid companyId, Guid companyTypeId, Guid? updatedBy = null)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetDistrict(Guid districtId, Guid? updatedBy = null)
        {
            DistrictId = districtId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(Company? company, CompanyType? companyType, District? district, Guid? updatedBy = null)
        {
            Company = company;
            CompanyId = company?.Id ?? CompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            District = district;
            DistrictId = district?.Id ?? DistrictId;
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
