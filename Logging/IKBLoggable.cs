using System.Runtime.CompilerServices;
using UnityEngine;

namespace InternProject.Logging
{
    /// <summary>
    /// Represents an object that is loggable and has a dedicated <see cref="KBLogLevel"/>.
    /// </summary>
    public interface IKBLoggable
    {
        /// <summary>
        /// The log levels to log. Anything not selected will be stripped.
        /// </summary>
        public KBLogLevel LogLevels { get; set; }
    }

    public static class IKBLoggableExtensions
    {
        public static void Log(this IKBLoggable logger, KBLog log, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        {
            if ((logger.LogLevels & log.Level) == 0)
                return;

            KBLogger.Log(log, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber);
        }
    }
}
