using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class District : Entity
    {
        public District(Guid districtId, Guid cityId, string name, string ubigeo, Tracking? tracking = null)
            : base(districtId)
        {
            CityId = cityId;
            Name = name;
            Ubigeo = ubigeo;
            Tracking = tracking;
        }

        public Guid CityId { get; private set; }
        public string Name { get; private set; }
        public string Ubigeo { get; private set; }
        public Tracking? Tracking { get; private set; }

        public void Update(string name, string ubigeo, Guid? updatedBy = null)
        {
            Name = name;
            Ubigeo = ubigeo;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
