using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// An <see cref="ISHLogger"/> for logging to a <see cref="TextMeshProUGUI"/>.
    /// </summary>
    public class SHUIConsoleLogger : MonoBehaviour, ISHLogger
    {
        [SerializeField, Tooltip("The text mesh this logger logs to.")] 
        private TextMeshProUGUI textMesh;

        private readonly List<string> messages = new();

        public string Text => textMesh.text;
        public ISHLogFormatter Formatter => SHLogFormats.DefaultWithTimestamp;

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
                Debug.LogError("Formatter is not set!");
                return;
            }

            string message = formatter.Format(log);

            messages.Add(message);
            textMesh.text += message;
        }

        public void Clear()
        {
            textMesh.text = string.Empty;
        }
    }
}
