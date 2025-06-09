using System.Collections;
using UnityEngine;

namespace Shears.Logging
{
    [ExecuteInEditMode]
    public class LoggerSamples : MonoBehaviour, ISHLoggable
    {
        [SerializeField]
        private bool _run;

        [SerializeField]
        private SHLog _log;

        [SerializeField]
        private SHLogLevels _logLevels;

        public SHLogLevels LogLevels { get => _logLevels; set => _logLevels = value; }

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
