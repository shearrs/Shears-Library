using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Represents a formattable log for use by the <see cref="SHLogger"/>.
    /// </summary>
    [System.Serializable]
    public struct SHLog
    {
        #region Serialized Variables
        [Header("Default Settings")]
        [SerializeField, Tooltip("The main message of the log.")]
        private string message;

        [
            SerializeField,
            Tooltip(
                "The context of the log. When logged to the Unity console, this log will highlight its context upon being clicked."
            )
        ]
        private Object context;

        [
            SerializeField,
            Tooltip(
                "The level/severity of the log. By default, influences the prefix and the color of the log."
            )
        ]
        private SHLogLevels level;

        [Header("Prefix")]
        [SerializeField, Tooltip("Whether or not to show the custom prefix textbox.")]
        private bool usesCustomPrefix;

        [
            SerializeField,
            ShowIf(nameof(usesCustomPrefix)),
            Tooltip(
                "The prefix of the log's message. If left blank, it defaults to the prefix for the current log level."
            )
        ]
        private string prefix;

        [Header("Color")]
        [SerializeField, Tooltip("Whether or not to show the custom color selector.")]
        private bool usesCustomColor;

        [
            SerializeField,
            ShowIf(nameof(usesCustomColor)),
            Tooltip("If a logger supports color, this determines the output color of this log.")
        ]
        private Color color;
        #endregion

        private string callerFilePath;
        private long callerLineNumber;

        #region Public Properties
        /// <summary>
        /// The main body message of the log.
        /// </summary>
        public string Message
        {
            readonly get => message;
            set => message = value;
        }

        /// <summary>
        /// The context object of the log. When logged to the Unity console, this log will highlight its context upon being clicked.
        /// </summary>
        public Object Context
        {
            readonly get => context;
            set => context = value;
        }

        /// <summary>
        /// The prefix to the log's message.
        /// </summary>
        public string Prefix
        {
            readonly get => prefix;
            set => prefix = value;
        }

        /// <summary>
        /// The level/severity of the log. By default, influences the prefix and the color of the log.
        /// </summary>
        public SHLogLevels Level
        {
            readonly get => level;
            set => level = value;
        }

        /// <summary>
        /// The color of the log when displayed in a logger that supports color.
        /// </summary>
        public Color Color
        {
            readonly get => color;
            set => color = value;
        }

        /// <summary>
        /// Whether or not this log uses a custom prefix.
        /// </summary>
        public bool UsesCustomPrefix
        {
            readonly get => usesCustomPrefix;
            set => usesCustomPrefix = value;
        }

        /// <summary>
        /// Whether or not this log uses a custom color.
        /// </summary>
        public bool UsesCustomColor
        {
            readonly get => usesCustomColor;
            set => usesCustomColor = value;
        }
        #endregion

        #region Caller Info
        /// <summary>
        /// The file path of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal string CallerFilePath
        {
            readonly get => callerFilePath;
            set => callerFilePath = value;
        }

        /// <summary>
        /// The line number of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal long CallerLineNumber
        {
            readonly get => callerLineNumber;
            set => callerLineNumber = value;
        }

        /// <summary>
        /// The file name of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal readonly string CallerFileName => GetCallerFileName(callerFilePath);

        /// <summary>
        /// The class name of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal readonly string CallerClassName => GetCallerClassName(CallerFileName);
        #endregion

        /// <summary>
        /// Constructs a <see cref="SHLog"/>.
        /// </summary>
        /// <param name="message">The main body message of the log.</param>
        /// <param name="context">The context object of the log. When logged to the Unity console, this log will highlight its context upon being clicked.</param>
        /// <param name="prefix">The prefix to the log's message.</param>
        /// <param name="level">The level/severity of the log. By default, influences the prefix and the color of the log.</param>
        /// <param name="color">The color of the log when displayed in a logger that supports color.</param>
        public SHLog(
            string message,
            Object context = null,
            string prefix = "",
            SHLogLevels level = SHLogLevels.Log,
            Color color = default
        )
        {
            this.message = message;
            this.context = context;
            this.prefix = prefix;
            this.level = level;
            this.color = color;

            callerFilePath = string.Empty;
            callerLineNumber = -1;

            usesCustomColor = (color != default);
            usesCustomPrefix = (prefix != string.Empty);
        }

        public static string GetCallerFileName(string callerFilePath)
        {
            int lastSlashIndex = 0;
            for (int i = 0; i < callerFilePath.Length; i++)
            {
                if (callerFilePath[i] == '\\')
                    lastSlashIndex = i;
            }

            if (lastSlashIndex != 0 && callerFilePath[lastSlashIndex] == '\\')
                lastSlashIndex++;

            string callerFileName = callerFilePath[lastSlashIndex..];

            return callerFileName;
        }

        public static string GetCallerClassName(string callerFileName)
        {
            string fileName = callerFileName;

            if (fileName == string.Empty)
                return string.Empty;

            int fileExtensionIndex = fileName.IndexOf('.');
            string className = fileName[..fileExtensionIndex];

            return className;
        }
    }
}
