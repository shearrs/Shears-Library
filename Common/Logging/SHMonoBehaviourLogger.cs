using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.Logging
{
    public abstract class SHMonoBehaviourLogger : MonoBehaviour, ISHLoggable
    {
        [Header("Logging")]
        [SerializeField, Tooltip("The log levels to log. Anything not selected will be stripped.")]
        private SHLogLevels logLevels = SHLogLevels.Log | SHLogUtil.Issues;

        /// <inheritdoc cref="ISHLoggable.LogLevels"/>
        public SHLogLevels LogLevels
        {
            get => logLevels;
            set => logLevels = value;
        }

        /// <inheritdoc cref="ISHLoggableLogger.Log"/>
        [HideInCallstack]
        public void Log(
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ISHLoggableLogger.Log(
                this,
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogVerbose"/>
        [HideInCallstack]
        public void LogVerbose(
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ISHLoggableLogger.Log(
                this,
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogWarning"/>
        [HideInCallstack]
        public void LogWarning(
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ISHLoggableLogger.Log(
                this,
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogError"/>
        [HideInCallstack]
        public void LogError(
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ISHLoggableLogger.Log(
                this,
                message,
                context,
                prefix,
                color,
                formatter,
                callerFilePath,
                callerLineNumber
            );

        /// <inheritdoc cref="ISHLoggableLogger.LogFatal"/>
        [HideInCallstack]
        public void LogFatal(
            object message,
            Object context = null,
            string prefix = "",
            Color? color = null,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ISHLoggableLogger.Log(
                this,
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
