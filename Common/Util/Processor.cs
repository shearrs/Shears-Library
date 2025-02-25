using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.Common
{
    public class Processor : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onBeginProcessing;
        [SerializeField] private UnityEvent<float> onUpdateProcessing;
        [SerializeField] private UnityEvent onEndProcessing;
        private Coroutine processCoroutine;

        public bool IsProcessing => processCoroutine != null;

        public event Action OnBeginProcessing;
        public event Action<float> OnUpdateProcessing;
        public event Action OnEndProcessing;

        public void BeginProcessing(Action onComplete, float processTime)
        {
            if (IsProcessing)
                return;

            processCoroutine = StartCoroutine(IEProcess(onComplete, processTime));

            onBeginProcessing.Invoke();
            OnBeginProcessing?.Invoke();
        }

        public void EndProcessing()
        {
            if (!IsProcessing)
                return;

            StopCoroutine(processCoroutine);
            processCoroutine = null;

            onEndProcessing.Invoke();
            OnEndProcessing?.Invoke();
        }

        private IEnumerator IEProcess(Action onComplete, float processTime)
        {
            float elapsedTime = 0;
            while (elapsedTime < processTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / processTime;

                onUpdateProcessing.Invoke(t);
                OnUpdateProcessing?.Invoke(t);

                yield return null;
            }

            EndProcessing();
            onComplete?.Invoke();
        }
    }
}
