using UnityEngine;

namespace Shears.UI
{
    public readonly ref struct StyleData
    {
        public Color Color { get; }

        public StyleData(Color color)
        {
            Color = color;
        }
    }
}
