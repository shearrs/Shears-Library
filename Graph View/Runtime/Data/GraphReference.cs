using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public class GraphReference<T> where T : GraphElementData
    {
        [SerializeField] private string id;
        [SerializeField] private T data;

        public string ID { get => id; set => id = value; }
        public T Data { get => data; set => data = value; }
    }
}
