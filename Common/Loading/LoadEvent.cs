using UnityEngine;

namespace Shears.Loading
{
    public class LoadEvent : MonoBehaviour
    {
        [SerializeField] private LoadRequest request;

        public void Invoke()
        {
            Loader.EnqueueRequest(request);
        }
    }
}
