using System.Collections;
using UnityEngine;

namespace InternProject.Logging
{
    [ExecuteInEditMode]
    public class LoggerSamples : MonoBehaviour, IKBLoggable
    {
        [SerializeField]
        private bool _run;

        [SerializeField]
        private KBLog _log;

        [SerializeField]
        private KBLogLevel _logLevels;

        public KBLogLevel LogLevels { get => _logLevels; set => _logLevels = value; }

        private void Start()
        {
            if (!Application.isPlaying)
                return;
        }

        private void Update()
        {
            if (_run)
            {
                this.Log(_log);
                _run = false;
            }
        }

        private IEnumerator IELog()
        {
            while (true)
            {
                this.Log(_log);
                yield return new WaitForSeconds(0.5f);
            }
        }

    }
}
