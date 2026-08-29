using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Grids.Editor
{
    [CustomPropertyDrawer(typeof(GridNodeDataCache))]
    public class GridNodeDataCachePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new GridNodeDataCacheInspector(property);
        }
    }
}
