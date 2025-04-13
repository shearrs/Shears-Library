using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Input
{
    public class ManagedInputStack : MonoBehaviour
    {
        [SerializeField, ReadOnly] private List<ManagedInputMap> inputMaps = new();

        public void Add(ManagedInputMap map)
        {
            if (inputMaps.Contains(map))
                return;

            inputMaps.Add(map);

            UpdateActiveMap();
        }

        public void Remove(ManagedInputMap map)
        {
            map.DisableAllInputs();

            inputMaps.Remove(map);

            UpdateActiveMap();
        }

        private void UpdateActiveMap()
        {
            if (inputMaps.Count == 0)
                return;

            foreach (var map in inputMaps)
                map.DisableAllInputs();

            inputMaps[^1].EnableAllInputs();
        }
    }
}
