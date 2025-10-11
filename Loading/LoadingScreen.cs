using System.Collections;
using UnityEngine;

namespace Shears.Loading
{
    public abstract class LoadingScreen : MonoBehaviour
    {
        public bool IsDelaying { get; protected set; }

        public abstract Coroutine Enable();
        public abstract Coroutine Disable();
    }
}
