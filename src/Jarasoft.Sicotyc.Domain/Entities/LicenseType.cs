using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class LicenseType : Entity
    {
        public LicenseType(
            Guid licenseTypeId,
            string name,
            string code,
            string? description,
            Tracking? tracking = null)
            : base(licenseTypeId)
        {
            Drivers = new List<Driver>();
            Name = name;
            Code = code;
            Description = description;
            Tracking = tracking;
        }

        public string Name { get; private set; }
        public string Code { get; private set; }
        public string? Description { get; private set; }
        public Tracking? Tracking { get; private set; }

        public ICollection<Driver> Drivers { get; private set; }

        public void Update(string name, string code, string? description, Guid? updatedBy = null)
        {
            Name = name;
            Code = code;
            Description = description;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}