using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class CompanyZone : Entity
    {
        public CompanyZone(Guid companyZoneId, Guid companyId, Guid companyTypeId, Guid zoneId, Tracking? tracking = null)
            : base(companyZoneId)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            ZoneId = zoneId;
            Tracking = tracking;
        }

        public Guid CompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid ZoneId { get; private set; }
        public Company? Company { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public Zone? Zone { get; private set; }
        public Tracking? Tracking { get; private set; }

        public void SetRelations(Company? company, CompanyType? companyType, Zone? zone, Guid? updatedBy = null)
        {
            Company = company;
            CompanyId = company?.Id ?? CompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            Zone = zone;
            ZoneId = zone?.Id ?? ZoneId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
