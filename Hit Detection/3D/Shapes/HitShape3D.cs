using System.Runtime.CompilerServices;
using Shears.Logging;
using UnityEngine;

namespace Shears.HitDetection
{
    public abstract class HitShape3D : MonoBehaviour, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogUtil.Issues;

        public HitBody3D Body { get; internal set; }

        internal abstract void Sweep(DetectionHandle handle);

        /// <inheritdoc cref="ISHLoggableLogger.Log"/>
        [HideInCallstack]
        protected void Log(
            string message,
            Object context = null,
            string prefix = "",
            Color color = default,
            ISHLogFormatter formatter = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] long callerLineNumber = 0
        ) =>
            ((ISHLoggable)this).Log(
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
