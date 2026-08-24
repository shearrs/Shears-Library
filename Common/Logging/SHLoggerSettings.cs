using UnityEngine;
using LogType = Shears.Logging.SHLogger.LogType;

namespace Shears.Logging
{
    [CreateAssetMenu(fileName = "SHLogger Settings", menuName = "Shears Library/Logging/Logger Settings")]
    public class SHLoggerSettings : ScriptableObject
    {
        [Header("Logging")]
        [SerializeField, Tooltip("Whether or not the logger logs.")]
        private bool enabled = true;

        [SerializeField, Tooltip("Specifies the target of the logger.")]
        private LogType logType = LogType.UnityConsole;

        [SerializeField, Tooltip("Specifies which log levels will be logged. Anything not selected will be stripped.")]
        private SHLogLevels logLevels = 0;

        [SerializeField, Tooltip("The custom logger to output to when LogType is set to Custom.")]
        private InterfaceReference<ISHLogger> customLogger = new();

        [SerializeField, Tooltip("Overrides the formatter of the target logger to always use this formatter.")]
        private InterfaceReference<ISHLogFormatter> formatterOverride = new();

        [Header("Log Level Colors")]
        [SerializeField, Tooltip("The colors to display for each SHLogLevel.\n\nIf not set, automatically sets to a default.")]
        private SHLogColors logColors;

        /// <summary>
        /// Whether or not the logger is enabled. If this is false, no logs will be logged.
        /// </summary>
        public bool Enabled { get => enabled; set => enabled = value; }

        /// <summary>
        /// The log levels that will be logged. Anything not selected will be stripped.
        /// </summary>
        public LogType LogType { get => logType; set => logType = value; }

        /// <summary>
        /// The log levels that will be logged. Anything not selected will be stripped.
        /// </summary>
        public SHLogLevels LogLevels { get => logLevels; set => logLevels = value; }

        /// <summary>
        /// If this is not null, this formatter is used by default instead of the current logger target's formatter.
        /// </summary>
        public ISHLogFormatter FormatterOverride { get => formatterOverride.Value; set => formatterOverride.Value = value; }

        /// <summary>
        /// The custom logger to output to when <see cref="LogType"/> is set to <see cref="LogType.Custom"/>.
        /// </summary>
        public ISHLogger CustomLogger { get => customLogger.Value; set => customLogger.Value = value; }

        /// <summary>
        /// The colors to display for each <see cref="SHLogLevels"/>. If not set, automatically sets to a default.
        /// </summary>
        public SHLogColors LogColors
        {
            get
            {
                if (logColors == null)
                {
                    logColors = Resources.Load<SHLogColors>("SHLogging/Default Log Colors");

                    if (logColors == null)
                        logColors = CreateInstance<SHLogColors>();
                }

                return logColors;
            }
            set => logColors = value;
        }
    }
}
