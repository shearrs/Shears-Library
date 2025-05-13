using UnityEngine;

namespace InternProject.Logging
{
    /// <summary>
    /// An <see cref="IKBLogger"/> for logging to the Unity console.
    /// </summary>
    public class KBUnityConsoleLogger : IKBLogger
    {
        public IKBLogFormatter Formatter => KBLogFormats.Default;

        public void Log(KBLog log)
        {
            Log(log, Formatter);
        }

        public void Log(KBLog log, IKBLogFormatter formatter)
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
