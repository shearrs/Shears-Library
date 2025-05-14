using System.Runtime.CompilerServices;

namespace Shears.Logging
{
    /// <summary>
    /// Represents an object that is loggable and has a dedicated <see cref="SHLogLevel"/>.
    /// </summary>
    public interface ISHLoggable
    {
        /// <summary>
        /// The log levels to log. Anything not selected will be stripped.
        /// </summary>
        public SHLogLevel LogLevels { get; set; }
    }

    public static class ISHLoggableExtensions
    {
        public static void Log(this ISHLoggable logger, SHLog log, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        {
            if ((logger.LogLevels & log.Level) == 0)
                return;

            SHLogger.Log(log, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber);
        }
    }
}
