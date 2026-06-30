namespace Jarasoft.Sicotyc.Domain.Enums
{
    public sealed class StringValueAttribute: Attribute
    {
        public string Value { get; }

        public StringValueAttribute(string value)
        {
            Value = value;
        }
    }
}
