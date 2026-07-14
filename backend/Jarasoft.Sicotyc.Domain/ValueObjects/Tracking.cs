namespace Jarasoft.Sicotyc.Domain.ValueObjects
{
    public record Tracking
    (
        DateTime CreatedAt,
        Guid? CreatedBy,
        DateTime? UpdatedAt,
        Guid? UpdatedBy,
        bool IsDeleted,
        DateTime? DeletedAt,
        Guid? DeletedBy
    );
}
