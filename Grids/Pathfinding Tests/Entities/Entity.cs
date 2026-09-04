using UnityEngine;

namespace Shears.Grids
{
    public class Entity : MonoBehaviour, IPathEntity
    {
        public Vector3 Position => transform.position;
        public EntityPosition EntityPosition { get; private set; }
    }
}
