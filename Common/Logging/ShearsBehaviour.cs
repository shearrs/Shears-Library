using System.Runtime.CompilerServices;
using Shears.Logging;
using UnityEngine;

namespace Shears
{
    public abstract class ShearsBehaviour : MonoBehaviour, ISHLoggable, IInterfaceSerializer
    {
        [Header("Logging")]
        [SerializeField, Tooltip("The log levels to log. Anything not selected will be stripped.")]
        private SHLogLevels logLevels = SHLogLevels.Log | SHLogUtil.Issues;

        [SerializeField, HideInInspector]
        private InterfaceDictionary __interfaceEntries = new();

        /// <inheritdoc cref="ISHLoggable.LogLevels"/>
        public SHLogLevels LogLevels
        {
            get => logLevels;
            set => logLevels = value;
        }
        InterfaceDictionary IInterfaceSerializer.InterfaceEntries => __interfaceEntries;

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
            ISHLoggableLogger.LogVerbose(
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
            ISHLoggableLogger.LogWarning(
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
            ISHLoggableLogger.LogError(
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
            ISHLoggableLogger.LogFatal(
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
