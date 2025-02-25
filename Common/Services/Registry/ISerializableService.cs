using Shears.Services;
using UnityEngine;

namespace Shears
{
    public interface ISerializableService
    {
        public void Register(ServiceLocator locator);
    }
}
