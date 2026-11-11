using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// Base class for an optional value. Should use the generic version instead.
    /// </summary>
    [System.Serializable]
    public abstract class Option
    {
        [SerializeField]
        [Tooltip("Whether or not this optional field has a value.")]
        protected bool hasValue;

        /// <summary>
        /// Whether or not this optional field has a value.
        /// </summary>
        public virtual bool HasValue
        {
            get => hasValue;
            set => hasValue = value;
        }

        /// <summary>
        /// Choose the first valid value from the passed <see cref="Option"/>s.
        /// </summary>
        /// <typeparam name="T">The type to choose from.</typeparam>
        /// <param name="option1">The first <see cref="Option"/> to evaluate.</param>
        /// <param name="option2">The second <see cref="Option"/> to evaluate.</param>
        /// <param name="defaultValue">The default value to resort to if the passed <see cref="Option"/>s have no values.</param>
        /// <returns>The first valid value from the passed <see cref="Option"/>s.</returns>
        public static T Choose<T>(Option<T> option1, Option<T> option2, T defaultValue = default)
        {
            if (option1.HasValue)
                return option1.Value;
            else if (option2.HasValue)
                return option2.Value;
            else
                return defaultValue;
        }

        /// <inheritdoc cref="Choose{T}(Option{T}, Option{T}, T)"/>
        /// <param name="option3">The third <see cref="Option"/> to evaluate.</param>
        public static T Choose<T>(
            Option<T> option1,
            Option<T> option2,
            Option<T> option3,
            T defaultValue = default
        )
        {
            if (option1.HasValue)
                return option1.Value;
            else if (option2.HasValue)
                return option2.Value;
            else if (option3.HasValue)
                return option3.Value;
            else
                return defaultValue;
        }

        /// <inheritdoc cref="Choose{T}(Option{T}, Option{T}, T)"/>
        public static Option<T> ChooseOption<T>(Option<T> option1, Option<T> option2)
        {
            if (option1.HasValue)
                return option1;
            else if (option2.HasValue)
                return option2;
            else
                return option1;
        }
    }

    /// <summary>
    /// A serializable optional value. Has safe methods for trying to read the field's value.<br/>
    /// An <see cref="Option"/> has no value if its <see cref="HasValue"/> flag is false or if its <see cref="Value"/> is null.
    /// </summary>
    /// <typeparam name="T">The type of optional value.</typeparam>
    [Serializable]
    public class Option<T> : Option, IReadOnlyOption<T>
    {
        [SerializeField, ShowIf(nameof(hasValue))]
        [Tooltip("The field value.")]
        private T value;

        /// <summary>
        /// The optional value.
        /// </summary>
        public T Value
        {
            get
            {
                if (!base.HasValue)
                    return default;
                else
                    return value;
            }
            set
            {
                bool changed = false;

                if (!EqualityComparer<T>.Default.Equals(this.value, value))
                    changed = true;

                if (value != null)
                    HasValue = true;

                if (changed)
                {
                    this.value = value;
                    Changed?.Invoke(value);
                }
            }
        }

        public override bool HasValue
        {
            get => base.HasValue && value != null;
            set => base.HasValue = value;
        }

        /// <summary>
        /// Event when the value of an option changes.
        /// </summary>
        public event Action<T> Changed;

        /// <summary>
        /// Create a new empty <see cref="Option"/>.
        /// </summary>
        public Option()
        {
            HasValue = false;
        }

        /// <summary>
        /// Create a new <see cref="Option"/> with a default value.
        /// </summary>
        /// <param name="defaultValue">The value to initialize the <see cref="Option"/> with.</param>
        public Option(T defaultValue)
        {
            value = defaultValue;
            HasValue = true;
        }

        /// <summary>
        /// Get the value from this <see cref="Option"/> or a default value if it has none.
        /// </summary>
        /// <param name="defaultValue">The fallback default value.</param>
        /// <returns>Either the <see cref="Option"/>'s value or the default value.</returns>
        public T GetValueOrDefault(T defaultValue)
        {
            if (HasValue)
                return Value;
            else
                return defaultValue;
        }
    }

    /// <summary>
    /// Read-only interface for accessing an <see cref="Option"/>.
    /// </summary>
    /// <typeparam name="T">The type of optional value.</typeparam>
    public interface IReadOnlyOption<T>
    {
        /// <inheritdoc cref="Option.HasValue"/>
        public bool HasValue { get; }

        /// <inheritdoc cref="Option{T}.Value"/>
        public T Value { get; }
    }
}
