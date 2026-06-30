using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class CompanyType : Entity
    {
        public CompanyType(Guid id) : base(id) { }

        // Object Values
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; } = null;

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Factory
        public static CompanyType Create(string name, string? description = null, Guid? createdBy = null)
        {
            var ct = new CompanyType(Guid.NewGuid())
            {
                Name = name,
                Description = description,
                Tracking = new Tracking(DateTime.UtcNow, createdBy, null, null, false, null, null)
            };

            return ct;
        }

        // Update
        public void Update(string name, string? description, Guid? updatedBy = null)
        {
            Name = name;
            Description = description;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        // Soft delete
        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}
