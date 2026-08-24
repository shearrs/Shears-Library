using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Represents an object that is loggable and has a dedicated <see cref="SHLogLevels"/>.
    /// </summary>
    public interface ISHLoggable
    {
        /// <summary>
        /// The log levels to log. Anything not selected will be stripped.
        /// </summary>
        public SHLogLevels LogLevels { get; set; }
    }

    public static class ISHLoggableLogger
    {
        /// <summary>
        /// Logs a message to the current <see cref="ISHLogger"/>.
        /// </summary>
        /// <param name="message">The log to send.</param>
        /// <param name="context">The context associated with this log. If the <see cref="SHLogger"/>'s <see cref="LogType"/> is set to <see cref="LogType.UnityConsole"/>, the context will be highlighted upon selecting the log.</param>
        /// <param name="prefix">A custom prefix for this log.</param>
        /// <param name="color">A custom <see cref="Color"/> for this log.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        [HideInCallstack]
        public static void Log(
            this ISHLoggable logger,
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        )
        {
            if ((logger.LogLevels & SHLogLevels.Log) == 0)
                return;

            if (context == null && logger is Component component)
                context = component.gameObject;

            if (prefix == string.Empty && logger is Object loggerObject)
                prefix =
                    $"{SHLog.GetCallerClassName(SHLog.GetCallerFileName(callerFilePath))}({loggerObject.name})";

            SHLogger.Log(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>Logs a verbose message to the current <see cref="ISHLogger"/>.</summary>
        /// <inheritdoc cref="Log"/>
        [HideInCallstack]
        public static void LogVerbose(
            this ISHLoggable logger,
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        )
        {
            if ((logger.LogLevels & SHLogLevels.Verbose) == 0)
                return;

            if (context == null && logger is Component component)
                context = component.gameObject;

            if (prefix == string.Empty && logger is Object loggerObject)
                prefix =
                    $"{SHLog.GetCallerClassName(SHLog.GetCallerFileName(callerFilePath))}({loggerObject.name})";

            SHLogger.LogVerbose(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>Logs a warning to the current <see cref="ISHLogger"/>.</summary>
        /// <inheritdoc cref="Log"/>
        [HideInCallstack]
        public static void LogWarning(
            this ISHLoggable logger,
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        )
        {
            if ((logger.LogLevels & SHLogLevels.Warning) == 0)
                return;

            if (context == null && logger is Component component)
                context = component.gameObject;

            if (prefix == string.Empty && logger is Object loggerObject)
                prefix =
                    $"{SHLog.GetCallerClassName(SHLog.GetCallerFileName(callerFilePath))}({loggerObject.name})";

            SHLogger.LogWarning(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>Logs an error to the current <see cref="ISHLogger"/>.</summary>
        /// <inheritdoc cref="Log"/>
        [HideInCallstack]
        public static void LogError(
            this ISHLoggable logger,
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        )
        {
            if ((logger.LogLevels & SHLogLevels.Error) == 0)
                return;

            if (context == null && logger is Component component)
                context = component.gameObject;

            if (prefix == string.Empty && logger is Object loggerObject)
                prefix =
                    $"{SHLog.GetCallerClassName(SHLog.GetCallerFileName(callerFilePath))}({loggerObject.name})";

            SHLogger.LogError(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>Logs a fatal error to the current <see cref="ISHLogger"/>.</summary>
        /// <inheritdoc cref="Log"/>
        [HideInCallstack]
        public static void LogFatal(
            this ISHLoggable logger,
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        )
        {
            if ((logger.LogLevels & SHLogLevels.Fatal) == 0)
                return;

            if (context == null && logger is Component component)
                context = component.gameObject;

            if (prefix == string.Empty && logger is Object loggerObject)
                prefix =
                    $"{SHLog.GetCallerClassName(SHLog.GetCallerFileName(callerFilePath))}({loggerObject.name})";

            SHLogger.LogFatal(
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );
        }
    }
}
