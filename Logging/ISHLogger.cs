using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Represents a logger that is usable by the <see cref="SHLogger"/>.
    /// </summary>
    public interface ISHLogger
    {
        /// <summary>
        /// The default formatter for this logger.
        /// </summary>
        public ISHLogFormatter Formatter { get; }

        /// <summary>
        /// Log a message with default formatting.
        /// </summary>
        /// <param name="log">The log to log.</param>
        [HideInCallstack]
        public void Log(SHLog log);

        /// <summary>
        /// Log a message with explicit formatting.
        /// </summary>
        /// <param name="log">The log to log.</param>
        /// <param name="formatter">The formatter to format the log with.</param>
        [HideInCallstack]
        public void Log(SHLog log, ISHLogFormatter formatter);

        /// <summary>
        /// Clear the contents of this log.
        /// </summary>
        public void Clear();
    }
}
