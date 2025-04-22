using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shears
{
    [System.Serializable]
    public struct LoadAction
    {
        public enum LoadType { Load, Unload }

        [field: SerializeField] public string SceneAddress { get; set; }
        [field: SerializeField] public LoadType Type { get; set; }
        [field: SerializeField] public LoadSceneMode Mode { get; set; }

        public LoadAction(string sceneAddress, LoadType loadType, LoadSceneMode loadMode)
        {
            SceneAddress = sceneAddress;
            Type = loadType;
            Mode = loadMode;
        }
    }
}
