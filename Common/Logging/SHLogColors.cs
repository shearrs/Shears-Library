using System.Collections.Generic;
using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> that defines which colors are used by each <see cref="SHLogLevels"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Default Log Colors", menuName = "Shears Library/Logging/Colors")]
    public class SHLogColors : ScriptableObject
    {
        [field: SerializeField, Tooltip("The color displayed for SHLogLevel.Log.")]
        public Color Log { get; private set; } = Color.white;

        [field: SerializeField, Tooltip("The color displayed for SHLogLevel.Verbose.")]
        public Color Verbose { get; private set; } = new(.78f, .78f, .78f, 1);

        [field: SerializeField, Tooltip("The color displayed for SHLogLevel.Warning.")]
        public Color Warning { get; private set; } = Color.yellow;

        [field: SerializeField, Tooltip("The color displayed for SHLogLevel.Error.")]
        public Color Error { get; private set; } = Color.red;

        [field: SerializeField, Tooltip("The color displayed for SHLogLevel.Fatal")]
        public Color Fatal { get; private set; } = new(1f, 0, .5f);

        private readonly Color nullColor = Color.magenta;

        private Dictionary<SHLogLevels, Color> logColors;

        private void DefineColorDictionary()
        {
            logColors = new()
            {
                { SHLogLevels.Log,       Log         },
                { SHLogLevels.Verbose,   Verbose     },
                { SHLogLevels.Warning,   Warning     },
                { SHLogLevels.Error,     Error       },
                { SHLogLevels.Fatal,     Fatal       }
            };
        }

        /// <summary>
        /// Gets a <see cref="Color"/> for a given <see cref="SHLogLevels"/>.
        /// </summary>
        /// <param name="level">The log level to get a <see cref="Color"/> for.</param>
        /// <returns>The <see cref="Color"/> of the passed <see cref="SHLogLevels"/>.</returns>
        public Color GetColorForLogLevel(SHLogLevels level)
        {
            if (logColors == null)
                DefineColorDictionary();

            if (logColors.TryGetValue(level, out Color color))
                return color;

            Debug.LogWarning($"Could not find color for log level: {level}! You should probably define one in SHLogColors' dictionary.");
            return nullColor;
        }
    }
}
