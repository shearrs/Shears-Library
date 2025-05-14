using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Singleton class for logging messages to any <see cref="ISHLogger"/>.
    /// <para>Supports 3 default targets (<see cref="SHUnityConsoleLogger"/>, <see cref="SHUIConsoleLogger"/>, <see cref="SHFileLogger"/>) and a custom settable target that implements <see cref="ISHLogger"/>.</para>
    /// </summary>
    public class SHLogger : ProtectedSingleton<SHLogger>
    {
        /// <summary>
        /// Specifies the target of the <see cref="SHLogger"/>.
        /// <para>
        /// <see cref="UnityConsole"/> => outputs to <see cref="SHUnityConsoleLogger"/>,<br/>
        /// <see cref="UIConsole"/> => outputs to <see cref="SHUIConsoleLogger"/>,<br/>
        /// <see cref="File"/> => outputs to <see cref="SHFileLogger"/>,<br/>
        /// <see cref="Custom"/> => outputs to <see cref="CustomLogger"/>.
        /// </para>
        /// </summary>
        public enum LogType { UnityConsole, UIConsole, File, Custom }

        #region Logging Variables
        [Header("Logging")]
        [SerializeField, Tooltip("Whether or not the logger logs.")]
        private bool _enabled = true;

        [SerializeField, Tooltip("Specifies the target of the logger.")]
        private LogType _logType;

        [SerializeField, Tooltip("Specifies which log levels will be logged. Anything not selected will be stripped.")]
        private SHLogLevel _logLevels;

        [SerializeField, Tooltip("The custom logger to output to when LogType is set to Custom.")]
        private InterfaceReference<ISHLogger> _customLogger = new();

        [SerializeField, Tooltip("Overrides the formatter of the target logger to always use this formatter.")]
        private InterfaceReference<ISHLogFormatter> _formatterOverride = new();

        private SHUnityConsoleLogger _unityLogger;
        private SHUIConsoleLogger _uiLogger;
        private SHFileLogger _fileLogger;

        private ISHLogger CurrentLogger
        {
            get
            {
                InitializeLogger();

                return _logType switch
                {
                    LogType.UnityConsole => _unityLogger,
                    LogType.UIConsole => _uiLogger,
                    LogType.File => _fileLogger,
                    LogType.Custom => _customLogger.Value,
                    _ => null
                };
            }
        }

        public static LogType LoggingType { get => Instance._logType; set => Instance._logType = value; }

        /// <summary>
        /// The current formatter being used to format logs. If <see cref="FormatterOverride"/> is not set, this is the current logger target's formatter.
        /// </summary>
        public static ISHLogFormatter CurrentFormatter
        {
            get
            {
                if (FormatterOverride != null && FormatterOverride.IsValid())
                    return FormatterOverride;

                var currentLogger = Instance.CurrentLogger;

                if (currentLogger == null)
                    return SHLogFormatter.Empty;

                return currentLogger.Formatter;
            }
        }

        /// <summary>
        /// If this is not null, this formatter is used by default instead of the current logger target's formatter.
        /// </summary>
        public static ISHLogFormatter FormatterOverride
        {
            get => Instance._formatterOverride.Value;
            set => Instance._formatterOverride.Value = value;
        }

        public static ISHLogger CustomLogger { get => Instance._customLogger.Value; set => Instance._customLogger.Value = value; }
        public static string LogFilePath
        {
            get
            {
                if (Instance._fileLogger == null)
                    Instance._fileLogger = new();

                return Instance._fileLogger.GetFilePath();
            }
        }
        #endregion

        #region Color Variables
        [Header("Log Level Colors")]
        [SerializeField, Tooltip("The colors to display for each SHLogLevel.\n\nIf not set, automatically sets to a default.")]
        private SHLogColors _logColors;

        public static SHLogColors LogColors { get => Instance._logColors; set => Instance._logColors = value; }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            InitializeColors();
        }

        #region Log Convenience Functions
        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="context">The context associated with this log. If the <see cref="SHLogger"/>'s <see cref="LogType"/> is set to <see cref="LogType.UnityConsole"/>, the context will be highlighted upon selecting the log.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="level">The severity/level of this log.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, Object context = null, string prefix = "", SHLogLevel level = SHLogLevel.Log, Color color = default, ISHLogFormatter formatter = default,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, context, prefix, level, color), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, ISHLogFormatter formatter,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, string prefix, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, prefix: prefix), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="level">The severity/level of this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, SHLogLevel level, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, level: level), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, Color color, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, color: color), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="level">The severity/level of this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, string prefix, SHLogLevel level, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, prefix: prefix, level: level), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, string prefix, Color color, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, prefix: prefix, color: color), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="context">The context associated with this log. If the <see cref="SHLogger"/>'s <see cref="LogType"/> is set to <see cref="LogType.UnityConsole"/>, the context will be highlighted upon selecting the log.</param>
        /// <param name="level">The severity/level of this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, Object context, SHLogLevel level, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, context, level: level), formatter, callerFilePath, callerLineNumber);

        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="context">The context associated with this log. If the <see cref="SHLogger"/>'s <see cref="LogType"/> is set to <see cref="LogType.UnityConsole"/>, the context will be highlighted upon selecting the log.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, Object context, Color color, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, context, color: color), formatter, callerFilePath, callerLineNumber);
        #endregion

        #region Logging
        /// <summary>
        /// Logs a message to the current logger.
        /// </summary>
        /// <param name="log">The log to send.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(SHLog log, ISHLogFormatter formatter = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        {
            if (!Instance._enabled || (Instance._logLevels & log.Level) == 0)
                return;

            Instance.InstLog(log, formatter, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Clears all output from the current logger.
        /// </summary>
        public static void Clear()
        {
            if (!Instance._enabled)
                return;

            Instance.InstClear();
        }

        public static void SaveLogFile() => Instance.InstSaveLogFile();

        /// <summary>
        /// Instance call of <see cref="Log"/>.
        /// <para>Assigns the <see cref="SHLog"/>'s file path and line number then logs to <see cref="CurrentLogger"/>.</para>
        /// </summary>
        /// <param name="log">The log to send.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        private void InstLog(SHLog log, ISHLogFormatter formatter, string callerFilePath, long callerLineNumber)
        {
            log.CallerFilePath = callerFilePath;
            log.CallerLineNumber = callerLineNumber;

            if (CurrentLogger == null)
            {
                Debug.LogWarning("Current logger is null!");
                return;
            }
            
            formatter ??= _formatterOverride.Value;

            if (formatter == null || !formatter.IsValid())
                CurrentLogger.Log(log);
            else
                CurrentLogger.Log(log, formatter);
        }

        /// <summary>
        /// Instance call of <see cref="Clear"/>.
        /// <para>Clears all output from <see cref="CurrentLogger"/>.</para>
        /// </summary>
        private void InstClear()
        {
            if (CurrentLogger == null)
            {
                Debug.LogWarning("Current logger is null!");
                return;
            }

            CurrentLogger.Clear();
        }

        private void InstSaveLogFile()
        {
            if (_fileLogger == null)
            {
                Debug.LogWarning("Cannot save because file logger is null!");
                return;
            }

            _fileLogger.Save();
        }

        private void InitializeLogger()
        {
            if (_logType == LogType.UnityConsole && _unityLogger == null)
                _unityLogger = new();
            else if (_logType == LogType.UIConsole && _uiLogger == null)
                InitializeUIConsole();
            else if (_logType == LogType.File && _fileLogger == null)
                _fileLogger = new();
            else if (_logType == LogType.Custom && _customLogger == null)
                Debug.LogWarning("Custom logger is not set!");
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="Color"/> for the passed <see cref="SHLogLevel"/> from the logger's <see cref="SHLogColors"/>.
        /// </summary>
        /// <param name="level">The log level to get a <see cref="Color"/> for.</param>
        /// <returns>The <see cref="Color"/> of the passed <see cref="SHLogLevel"/>.</returns>
        public static Color GetColorForLogLevel(SHLogLevel level)
        {
            Instance.InitializeColors();

            return Instance._logColors.GetColorForLogLevel(level);
        }

        private void InitializeColors()
        {
            if (_logColors != null)
                return;

            var colors = Resources.Load<SHLogColors>("SHLogging/Default Log Colors");

            if (colors == null)
                colors = ScriptableObject.CreateInstance<SHLogColors>();

            _logColors = colors;
        }

        private void InitializeUIConsole()
        {
            const string uiConsoleFilePath = "SHLogging/UIConsole";

            var foundConsole = FindFirstObjectByType<SHUIConsoleLogger>();

            if (foundConsole != null)
            {
                _uiLogger = foundConsole;
                return;
            }

            GameObject consoleGO = Resources.Load<GameObject>(uiConsoleFilePath);

            if (consoleGO == null)
            {
                Debug.LogWarning($"Could not find SHUIConsole GameObject at {uiConsoleFilePath}!");
                return;
            }

            if (!consoleGO.TryGetComponent<SHUIConsoleLogger>(out var consoleComponent))
            {
                Debug.LogWarning("SHUIConsole GameObject does not have a SHUIConsole Component!");
                return;
            }

            var newUIConsole = Instantiate(consoleComponent);

            if (!newUIConsole.TryGetComponent<Canvas>(out var canvas))
            {
                Debug.LogWarning("SHUIConsole GameObject does not have a Canvas Component!");
                return;
            }

            canvas.worldCamera = Camera.main;

            _uiLogger = newUIConsole;
        }
    }
}
