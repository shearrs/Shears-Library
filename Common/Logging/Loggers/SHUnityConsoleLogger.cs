using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// An <see cref="ISHLogger"/> for logging to the Unity console.
    /// </summary>
    public class SHUnityConsoleLogger : ISHLogger
    {
        public ISHLogFormatter Formatter => SHLogFormats.Default;

        [HideInCallstack]
        public void Log(SHLog log)
        {
            Log(log, Formatter);
        }

        [HideInCallstack]
        public void Log(SHLog log, ISHLogFormatter formatter)
        {
            if (!formatter.IsValid())
            {
                Debug.LogError("Formatter not set!");
                return;
            }

            string message = formatter.Format(log);

            if ((log.Level & SHLogLevels.Warning) != 0)
                Debug.LogWarning(message);
            else if ((log.Level & SHLogLevels.Error) != 0)
                Debug.LogError(message);
            else
                Debug.Log(message, log.Context);
        }

        public void Clear()
        {
            Debug.ClearDeveloperConsole();
        }
    }
}
