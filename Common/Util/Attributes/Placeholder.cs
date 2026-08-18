using UnityEngine;

namespace Shears
{
    public class Placeholder : PropertyAttribute
    {
        public enum Mode
        {
            Literal,
            Property,
        }

        public string Value { get; }
        public Mode PlaceholderMode { get; }

        public Placeholder(string value, Mode mode = Mode.Literal)
        {
            Value = value;
            PlaceholderMode = mode;
        }
    }
}
