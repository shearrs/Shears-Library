using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Static class for logging messages to any <see cref="ISHLogger"/>.
    /// <para>Supports 3 default targets (<see cref="SHUnityConsoleLogger"/>, <see cref="SHUIConsoleLogger"/>, <see cref="SHFileLogger"/>) and a custom settable target that implements <see cref="ISHLogger"/>.</para>
    /// </summary>
    public static class SHLogger
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

        private static SHLoggerSettings settings;
        private static SHUnityConsoleLogger unityLogger;
        private static SHUIConsoleLogger uiLogger;
        private static SHFileLogger fileLogger;

        private static ISHLogger CurrentLogger
        {
            get
            {
                InitializeLogger();

                return LoggingType switch
                {
                    LogType.UnityConsole => unityLogger,
                    LogType.UIConsole => uiLogger,
                    LogType.File => fileLogger,
                    LogType.Custom => CustomLogger,
                    _ => null
                };
            }
        }

        /// <summary>
        /// The current settings for use by the logger. To set a default on load, create a <see cref="SHLoggerSettings"/> asset in the Resources folder and name it "SHLogger Settings".
        /// </summary>
        public static SHLoggerSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = Resources.Load<SHLoggerSettings>("SHLogging/SHLogger Settings");

                    if (settings != null)
                        settings = Object.Instantiate(Settings);
                    else
                        settings = ScriptableObject.CreateInstance<SHLoggerSettings>();
                }

                return settings;
            }
            set => settings = value;
        }

        /// <summary>
        /// Whether or not the logger is enabled. If this is false, no logs will be logged.
        /// </summary>
        public static bool Enabled { get => Settings.Enabled; set => Settings.Enabled = value; }

        /// <summary>
        /// The log levels that will be logged. Anything not selected will be stripped.
        /// </summary>
        public static LogType LoggingType { get => Settings.LogType; set => Settings.LogType = value; }

        /// <summary>
        /// The log levels that will be logged. Anything not selected will be stripped.
        /// </summary>
        public static SHLogLevel LogLevels { get => Settings.LogLevels; set => Settings.LogLevels = value; }

        /// <summary>
        /// The current formatter being used to format logs. If <see cref="FormatterOverride"/> is not set, this is the current logger target's formatter.
        /// </summary>
        public static ISHLogFormatter CurrentFormatter
        {
            get
            {
                if (FormatterOverride != null && FormatterOverride.IsValid())
                    return FormatterOverride;

                var currentLogger = CurrentLogger;

                if (currentLogger == null)
                    return SHLogFormatter.Empty;

                return currentLogger.Formatter;
            }
        }

        /// <summary>
        /// If this is not null, this formatter is used by default instead of the current logger target's formatter.
        /// </summary>
        public static ISHLogFormatter FormatterOverride { get => Settings.FormatterOverride; set => Settings.FormatterOverride = value; }

        /// <summary>
        /// The custom logger to output to when <see cref="LogType"/> is set to <see cref="LogType.Custom"/>.
        /// </summary>
        public static ISHLogger CustomLogger { get => Settings.CustomLogger; set => Settings.CustomLogger = value; }

        /// <summary>
        /// The colors to display for each <see cref="SHLogLevel"/>. If not set, automatically sets to a default.
        /// </summary>
        public static SHLogColors LogColors { get => Settings.LogColors; set => Settings.LogColors = value; }

        /// <summary>
        /// The file path that the <see cref="SHFileLogger"/> logs to.
        /// </summary>
        public static string LogFilePath
        {
            get
            {
                fileLogger ??= new();

                return fileLogger.GetFilePath();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Awake()
        {
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            unityLogger = null;
            uiLogger = null;
            fileLogger = null;

            Application.quitting -= OnApplicationQuit;
        }

        #region Logging
        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="level">The severity/level of this log.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="context">The context associated with this log. If the <see cref="SHLogger"/>'s <see cref="LogType"/> is set to <see cref="LogType.UnityConsole"/>, the context will be highlighted upon selecting the log.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        public static void Log(string message, SHLogLevel level = SHLogLevel.Log, Color color = default, Object context = null, string prefix = "", ISHLogFormatter formatter = default,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        => Log(new SHLog(message, context, prefix, level, color), formatter, callerFilePath, callerLineNumber);

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
            if (!Enabled || (LogLevels & log.Level) == 0)
                return;

            log.CallerFilePath = callerFilePath;
            log.CallerLineNumber = callerLineNumber;

            if (CurrentLogger == null)
            {
                Debug.LogWarning("Current logger is null!");
                return;
            }

            formatter ??= FormatterOverride;

            if (formatter == null || !formatter.IsValid())
                CurrentLogger.Log(log);
            else
                CurrentLogger.Log(log, formatter);
        }

        /// <summary>
        /// Clears all output from the current logger.
        /// </summary>
        public static void Clear()
        {
            if (!Enabled)
                return;

            if (CurrentLogger == null)
            {
                Debug.LogWarning("Current logger is null!");
                return;
            }

            CurrentLogger.Clear();
        }

        /// <summary>
        /// Saves the current log file to disk. Only works if the <see cref="LoggingType"/> is set to <see cref="LogType.File"/>.
        /// </summary>
        public static void SaveLogFile()
        {
            if (fileLogger == null)
            {
                Debug.LogWarning("Cannot save because file logger is null!");
                return;
            }

            fileLogger.Save();
        }

        private static void InitializeLogger()
        {
            if (LoggingType == LogType.UnityConsole && unityLogger == null)
                unityLogger = new();
            else if (LoggingType == LogType.UIConsole && uiLogger == null)
                InitializeUIConsole();
            else if (LoggingType == LogType.File && fileLogger == null)
                fileLogger = new();
            else if (LoggingType == LogType.Custom && CustomLogger == null)
                Debug.LogWarning("Custom logger is not set!");
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="Color"/> for the passed <see cref="SHLogLevel"/> from the logger's <see cref="SHLogColors"/>.
        /// </summary>
        /// <param name="level">The log level to get a <see cref="Color"/> for.</param>
        /// <returns>The <see cref="Color"/> of the passed <see cref="SHLogLevel"/>.</returns>
        public static Color GetColorForLogLevel(SHLogLevel level) => LogColors.GetColorForLogLevel(level);

        private static void InitializeUIConsole()
        {
            const string uiConsoleFilePath = "SHLogging/UIConsole";

            var foundConsole = Object.FindFirstObjectByType<SHUIConsoleLogger>();

            if (foundConsole != null)
            {
                uiLogger = foundConsole;
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

            var newUIConsole = Object.Instantiate(consoleComponent);

            if (!newUIConsole.TryGetComponent<Canvas>(out var canvas))
            {
                Debug.LogWarning("SHUIConsole GameObject does not have a Canvas Component!");
                return;
            }

            canvas.worldCamera = Camera.main;

            uiLogger = newUIConsole;
        }
    }
}
