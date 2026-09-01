using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class InterfaceEntry
    {
        [SerializeField]
        private Object unityObject;

        [SerializeReference]
        private object rawObject;

#if UNITY_EDITOR
        [SerializeField]
        private string fieldName;

        [SerializeField]
        private SerializableType fieldType;

        public string FieldName
        {
            get => fieldName;
            set => fieldName = value;
        }
        public SerializableType FieldType
        {
            get => fieldType;
            set => fieldType = value;
        }
#endif

        public Object UnityObject
        {
            get => unityObject;
            set => unityObject = value;
        }
        public object RawObject
        {
            get => rawObject;
            set => rawObject = value;
        }

        public InterfaceEntry(Object unityObject, object rawObject)
        {
            this.unityObject = unityObject;
            this.rawObject = rawObject;
        }
    }
}
