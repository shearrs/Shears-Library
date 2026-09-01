using Shears.Logging;
using UnityEngine;

namespace Shears.Events
{
    /// <summary>
    /// <see cref="SerializableListener"/> that sets an <see cref="Animator"/> parameter value.
    /// </summary>
    public class AnimatorListener : SerializableListener, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogUtil.Default;

        [SerializeField]
        [Tooltip("The animator to set a parameter value for.")]
        private Animator animator;

        [SerializeField]
        [Tooltip("The type of the parameter to set.")]
        private AnimatorControllerParameterType parameterType =
            AnimatorControllerParameterType.Bool;

        [SerializeField]
        [Tooltip("The name of the parameter to set.")]
        private string parameterName = "None";

        [
            SerializeField,
            ShowIf(
                nameof(parameterType),
                AnimatorControllerParameterType.Bool,
                "!_parameterName",
                "None"
            )
        ]
        [Tooltip("The boolean value to set.")]
        private bool boolValue = true;

        [
            SerializeField,
            ShowIf(
                nameof(parameterType),
                AnimatorControllerParameterType.Float,
                "!_parameterName",
                "None"
            )
        ]
        [Tooltip("The float value to set.")]
        private float floatValue = 0.0f;

        [
            SerializeField,
            ShowIf(
                nameof(parameterType),
                AnimatorControllerParameterType.Int,
                "!_parameterName",
                "None"
            )
        ]
        [Tooltip("The integer value to set.")]
        private int integerValue = 0;

        /// <summary>
        /// Flag for if the name has been hashed yet.
        /// </summary>
        private bool hasNameHash = false;

        /// <summary>
        /// The <see cref="Animator"/> hash of the parameter name.
        /// </summary>
        private int nameHash;

        /// <summary>
        /// The animator to set a parameter value for.
        /// </summary>
        public Animator Animator
        {
            get => animator;
            set => animator = value;
        }

        /// <summary>
        /// The type of the parameter to set.
        /// </summary>
        public AnimatorControllerParameterType ParameterType
        {
            get => parameterType;
            set => parameterType = value;
        }

        /// <summary>
        /// The name of the parameter to set.
        /// </summary>
        public string ParameterName
        {
            get => parameterName;
            set
            {
                if (parameterName == value)
                    return;

                parameterName = value;
                hasNameHash = false;
            }
        }

        /// <summary>
        /// The boolean value to set.
        /// </summary>
        public bool BoolValue
        {
            get => boolValue;
            set => boolValue = value;
        }

        /// <summary>
        /// The float value to set.
        /// </summary>
        public float FloatValue
        {
            get => floatValue;
            set => floatValue = value;
        }

        /// <summary>
        /// The integer value to set.
        /// </summary>
        public int IntegerValue
        {
            get => integerValue;
            set => integerValue = value;
        }

        /// <summary>
        /// Set an <see cref="UnityEngine.Animator"/> parameter value to this listener's serialized value.
        /// </summary>
        protected override void OnInvoke()
        {
            if (animator == null)
            {
                this.LogError(
                    $"{nameof(AnimatorListener)} has no assigned {nameof(UnityEngine.Animator)}!"
                );
                return;
            }

            if (string.IsNullOrEmpty(parameterName))
            {
                this.LogWarning($"{nameof(AnimatorListener)} has no assigned parameter name!");
                return;
            }

            if (!hasNameHash)
                nameHash = Animator.StringToHash(parameterName);

            switch (parameterType)
            {
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(nameHash, boolValue);
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(nameHash, floatValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(nameHash, integerValue);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(nameHash);
                    break;
            }
        }
    }
}
