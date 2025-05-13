using System.Collections.Generic;
using UnityEngine;

namespace InternProject.Logging
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> that defines which colors are used by each <see cref="KBLogLevel"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Default Log Colors", menuName = "Logging/Colors")]
    public class KBLogColors : ScriptableObject
    {
        [field: SerializeField, Tooltip("The color displayed for KBLogLevel.Log.")]
        public Color Log { get; private set; } = Color.white;

        [field: SerializeField, Tooltip("The color displayed for KBLogLevel.Verbose.")]
        public Color Verbose { get; private set; } = new(.78f, .78f, .78f, 1);

        [field: SerializeField, Tooltip("The color displayed for KBLogLevel.Warning.")]
        public Color Warning { get; private set; } = Color.yellow;

        [field: SerializeField, Tooltip("The color displayed for KBLogLevel.Error.")]
        public Color Error { get; private set; } = Color.red;

        [field: SerializeField, Tooltip("The color displayed for KBLogLevel.Fatal")]
        public Color Fatal { get; private set; } = new(1f, 0, .5f);

        private readonly Color nullColor = Color.magenta;

        private Dictionary<KBLogLevel, Color> logColors;

        private void DefineColorDictionary()
        {
            logColors = new()
            {
                { KBLogLevel.Log,       Log         },
                { KBLogLevel.Verbose,   Verbose     },
                { KBLogLevel.Warning,   Warning     },
                { KBLogLevel.Error,     Error       },
                { KBLogLevel.Fatal,     Fatal       }
            };
        }

        /// <summary>
        /// Gets a <see cref="Color"/> for a given <see cref="KBLogLevel"/>.
        /// </summary>
        /// <param name="level">The log level to get a <see cref="Color"/> for.</param>
        /// <returns>The <see cref="Color"/> of the passed <see cref="KBLogLevel"/>.</returns>
        public Color GetColorForLogLevel(KBLogLevel level)
        {
            if (logColors == null)
                DefineColorDictionary();

            if (logColors.TryGetValue(level, out Color color))
                return color;

            Debug.LogWarning($"Could not find color for log level: {level}! You should probably define one in KBLogColors' dictionary.");
            return nullColor;
        }
    }
}
