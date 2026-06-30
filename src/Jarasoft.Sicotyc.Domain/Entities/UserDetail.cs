using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class UserDetail : Entity
    {
        public UserDetail(
            Guid userId,
            DateOfBirth dateOfBirth,
            Address address,
            Tracking? tracking = null
            ) 
            : base(userId)
        {
            UserId = userId;
            DateOfBirth = dateOfBirth;
            Address = address;
            Tracking = tracking;
        }

        // Object Values
        public DateOfBirth? DateOfBirth { get; private set; }
        public Address? Address { get; private set; }

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Relationships
        public Guid UserId { get; private set; }
        public ApplicationUser? User { get; private set; }

        public void Update(DateOfBirth dateOfBirth, Address address, Guid? updatedBy = null)
        {
            DateOfBirth = dateOfBirth;
            Address = address;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetUser(ApplicationUser? user, Guid? updatedBy = null)
        {
            User = user;
            UserId = user?.Id ?? UserId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }
    }
}
