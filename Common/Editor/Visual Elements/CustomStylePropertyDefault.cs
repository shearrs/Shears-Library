using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    /// <summary>
    /// Wrapping struct to hold a CustomStyleProperty and a default value for it.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    public readonly struct CustomStylePropertyDefault<T>
    {
        private readonly CustomStyleProperty<T> customStyleProperty;
        private readonly T defaultValue;

        public CustomStylePropertyDefault(string propertyName, T defaultValue)
        {
            customStyleProperty = new(propertyName);
            this.defaultValue = defaultValue;
        }

        public T GetValueOrDefault(ICustomStyle customStyle)
        {
            if (customStyleProperty is CustomStyleProperty<int> intProp)
            {
                if (customStyle.TryGetValue(intProp, out int value))
                    return (T)(object)value;
                else
                    return defaultValue;
            }
            else if (customStyleProperty is CustomStyleProperty<float> floatProp)
            {
                if (customStyle.TryGetValue(floatProp, out float value))
                    return (T)(object)value;
                else
                    return defaultValue;
            }
            else if (customStyleProperty is CustomStyleProperty<Color> colorProp)
            {
                if (customStyle.TryGetValue(colorProp, out Color value))
                    return (T)(object)value;
                else
                    return defaultValue;
            }

            throw new System.ArgumentException($"{typeof(T).Name} is not a supported type!");
        }
    }
}
