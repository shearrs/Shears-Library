using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InternProject.Logging
{
    /// <summary>
    /// An <see cref="IKBLogger"/> for logging to a <see cref="TextMeshProUGUI"/>.
    /// </summary>
    public class KBUIConsoleLogger : MonoBehaviour, IKBLogger
    {
        [SerializeField] 
        private TextMeshProUGUI textMesh;

        private readonly List<string> messages = new();

        public string Text => textMesh.text;
        public IKBLogFormatter Formatter => KBLogFormats.DefaultWithTimestamp;

        public void Log(KBLog log)
        {
            Log(log, Formatter);
        }

        public void Log(KBLog log, IKBLogFormatter formatter)
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
