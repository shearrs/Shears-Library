using UnityEngine;

namespace InternProject.Logging
{
    public class CustomLoggerTest : MonoBehaviour, IKBLogger
    {
        public IKBLogFormatter Formatter => new KBLogFormatter(
                                                    KBLogFormats.NoPrefix, 
                                                    KBLogFormats.DefaultMessage, 
                                                    (log, message) => $"({log.Color}) {message}",
                                                    (log, formatter) => formatter.SetColor(log, $"{formatter.FormatMessage(log)} - {formatter.FormatPrefix(log)}")
                                                );
            

        public void Log(KBLog log)
        {
            Log(log, Formatter);
        }

        public void Log(KBLog log, IKBLogFormatter formatter)
        {
            string message = formatter.Format(log);

            Debug.Log(message);
        }

        public void Clear()
        {
            Debug.ClearDeveloperConsole();
        }
    }
}
