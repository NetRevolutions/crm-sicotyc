using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Common
{
    public static class Helper
    {        
        public static Tracking TouchUpdated(Tracking? tracking, Guid? updatedBy = null)
        {
            if (tracking is null)
            {
                tracking = new Tracking(DateTime.UtcNow, null, DateTime.UtcNow, updatedBy, false, null, null);
            }
            else
            {
                tracking = tracking with { UpdatedAt = DateTime.UtcNow, UpdatedBy = updatedBy };
            }

            return tracking;
        }

        public static Tracking SoftDeleted(Tracking? tracking, Guid? deletedBy = null)
        {
            if (tracking is null)
            {
                tracking = new Tracking(DateTime.UtcNow, null, null, null, true, DateTime.UtcNow, deletedBy);
            }
            else
            {
                tracking = tracking with { IsDeleted = true, DeletedAt = DateTime.UtcNow, DeletedBy = deletedBy };
            }           

            return tracking;
        }
    }
}
