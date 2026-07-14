using Jarasoft.Sicotyc.Domain.Enums;

namespace Jarasoft.Sicotyc.Domain.ValueObjects
{
    public record DocumentIdentity
    (
        DocumentTypeEnum DocumentType,
        string DocumentNumber
    );
}
