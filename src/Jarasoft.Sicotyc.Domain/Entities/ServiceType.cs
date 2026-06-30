using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class ServiceType : Entity
    {
        public ServiceType(
            Guid serviceTypeId,
            string name,
            string code,
            Tracking? tracking = null)
            : base(serviceTypeId)
        {
            Name = name;
            Code = code;
            Tracking = tracking;
        }

        public string Name { get; private set; }
        public string Code { get; private set; }
        public Tracking? Tracking { get; private set; }

        public void Update(string name, string code, Guid? updatedBy = null)
        {
            Name = name;
            Code = code;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}