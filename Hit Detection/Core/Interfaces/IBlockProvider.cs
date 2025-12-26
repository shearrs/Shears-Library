using UnityEngine;

namespace Shears.HitDetection
{
    public interface IBlockProvider
    {
        public bool CanBlock(HitData3D data);
    }
}
