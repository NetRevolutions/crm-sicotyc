namespace Jarasoft.Sicotyc.Domain.ValueObjects
{
    public record Option
    (
        Guid OptionId,
        string? Title,
        string? Icon,
        string? Url,
        int OptionOrder,
        int OptionLevel,
        Guid OptionParentId
    );    
}
