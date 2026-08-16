using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Represents a formattable log for use by the <see cref="SHLogger"/>.
    /// </summary>
    public readonly ref struct SHLog
    {
        private readonly string message;
        private readonly Object context;
        private readonly SHLogLevels level;
        private readonly bool usesCustomPrefix;
        private readonly string prefix;
        private readonly bool usesCustomColor;
        private readonly Color color;

        private readonly string callerFilePath;
        private readonly long callerLineNumber;

        #region Public Properties
        /// <summary>
        /// The main body message of the log.
        /// </summary>
        public string Message => message;

        /// <summary>
        /// The context object of the log. When logged to the Unity console, this log will highlight its context upon being clicked.
        /// </summary>
        public Object Context => context;

        /// <summary>
        /// The prefix to the log's message.
        /// </summary>
        public string Prefix => prefix;

        /// <summary>
        /// The level/severity of the log. By default, influences the prefix and the color of the log.
        /// </summary>
        public SHLogLevels Level => level;

        /// <summary>
        /// The color of the log when displayed in a logger that supports color.
        /// </summary>
        public Color Color => color;

        /// <summary>
        /// Whether or not this log uses a custom prefix.
        /// </summary>
        public bool UsesCustomPrefix => usesCustomPrefix;

        /// <summary>
        /// Whether or not this log uses a custom color.
        /// </summary>
        public bool UsesCustomColor => usesCustomColor;
        #endregion

        #region Caller Info
        /// <summary>
        /// The file path of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal string CallerFilePath => callerFilePath;

        /// <summary>
        /// The line number of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal long CallerLineNumber => callerLineNumber;

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
            Color? color = null,
            string callerFilePath = null,
            long callerLineNumber = -1
        )
        {
            this.message = message;
            this.context = context;
            this.prefix = prefix;
            this.level = level;
            this.color = color ?? Color.white;
            this.callerFilePath = callerFilePath;
            this.callerLineNumber = callerLineNumber;

            usesCustomColor = color.HasValue;
            usesCustomPrefix = prefix != string.Empty;
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
