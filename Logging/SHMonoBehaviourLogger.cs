using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.Logging
{
    public abstract class SHMonoBehaviourLogger : MonoBehaviour, ISHLoggable
    {
        [Header("Logging")]
        [SerializeField] private SHLogLevels logLevels = SHLogLevels.Log | SHLogLevels.Issues;

        public SHLogLevels LogLevels { get => logLevels; set => logLevels = value; }

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
        [HideInCallstack]
        public void Log(string message, SHLogLevels level = SHLogLevels.Log, Color color = default, Object context = null, string prefix = "", ISHLogFormatter formatter = default,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
        {
            if (context == null)
            {
                if (gameObject == null)
                    return;

                context = gameObject;
            }

            Log(new SHLog(message, context, prefix, level, color), formatter, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a message to the current logger.
        /// </summary>
        /// <param name="log">The log to send.</param>
        /// <param name="formatter">The formatter for this log. Defaults to the current <see cref="ISHLogger.Formatter"/>.</param>
        /// <param name="callerFilePath">The file path of the class who called this. Should not be set manually.</param>
        /// <param name="callerLineNumber">The line number of the class who called this. Should not be set manually.</param>
        [HideInCallstack]
        public void Log(SHLog log, ISHLogFormatter formatter = null, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = 0)
            => ISHLoggableLogger.Log(this, log, formatter, callerFilePath, callerLineNumber);
    }
}
