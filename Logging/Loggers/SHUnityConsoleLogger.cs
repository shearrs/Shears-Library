using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// An <see cref="ISHLogger"/> for logging to the Unity console.
    /// </summary>
    public class SHUnityConsoleLogger : ISHLogger
    {
        public ISHLogFormatter Formatter => SHLogFormats.Default;

        public void Log(SHLog log)
        {
            Log(log, Formatter);
        }

        public void Log(SHLog log, ISHLogFormatter formatter)
        {
            if (!formatter.IsValid())
            {
                Debug.LogError("Formatter not set!");
                return;
            }

            string message = formatter.Format(log);

            Debug.Log(message, log.Context);
        }

        public void Clear()
        {
            Debug.ClearDeveloperConsole();
        }
    }
}
