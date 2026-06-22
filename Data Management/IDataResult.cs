using UnityEngine;

namespace Shears.DataManagement
{
    public interface IDataResult<T>
    {
        public T Create(DataMap data);
    }
}
