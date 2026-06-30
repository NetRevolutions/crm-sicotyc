using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class District : Entity
    {
        public District(Guid districtId, Guid cityId, string name, string ubigeo, Guid? companyId = null, Tracking? tracking = null)
            : base(districtId)
        {
            Warehouses = new List<Warehouse>();
            CityId = cityId;
            CompanyId = companyId;
            Name = name;
            Ubigeo = ubigeo;
            Tracking = tracking;
        }

        public Guid CityId { get; private set; }
        public Guid? CompanyId { get; private set; }
        public string Name { get; private set; }
        public string Ubigeo { get; private set; }
        public Tracking? Tracking { get; private set; }

        public Company? Company { get; private set; }
        public ICollection<Warehouse> Warehouses { get; private set; }

        public void Update(string name, string ubigeo, Guid? updatedBy = null)
        {
            Name = name;
            Ubigeo = ubigeo;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetCompany(Company? company, Guid? updatedBy = null)
        {
            Company = company;
            CompanyId = company?.Id;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
