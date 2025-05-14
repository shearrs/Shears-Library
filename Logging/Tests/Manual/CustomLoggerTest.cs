using UnityEngine;

namespace Shears.Logging
{
    public class CustomLoggerTest : MonoBehaviour, ISHLogger
    {
        public ISHLogFormatter Formatter => new SHLogFormatter(
                                                    SHLogFormats.NoPrefix, 
                                                    SHLogFormats.DefaultMessage, 
                                                    (log, message) => $"({log.Color}) {message}",
                                                    (log, formatter) => formatter.SetColor(log, $"{formatter.FormatMessage(log)} - {formatter.FormatPrefix(log)}")
                                                );
            

        public void Log(SHLog log)
        {
            Log(log, Formatter);
        }

        public void Log(SHLog log, ISHLogFormatter formatter)
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
