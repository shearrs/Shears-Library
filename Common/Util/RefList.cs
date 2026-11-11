using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    [Serializable]
    public class RefList<T> : IReadOnlyRefList<T>, IList<T>, ICollection<T>
    {
        private static readonly bool SERIALIZE_REFERENCE =
            typeof(T).IsInterface || typeof(T).IsAbstract;

        [SerializeField]
        private List<T> value = new();

        [SerializeReference]
        private List<T> refValue = new();

        private List<T> SerializedValue => SERIALIZE_REFERENCE ? refValue : value;
        public IReadOnlyList<T> Value => SerializedValue;
        public int Count => ((ICollection<T>)SerializedValue).Count;
        public bool IsReadOnly => ((ICollection<T>)SerializedValue).IsReadOnly;
        public T this[int index]
        {
            get => ((IList<T>)SerializedValue)[index];
            set => ((IList<T>)SerializedValue)[index] = value;
        }

        public event Action<T> ElementAdded;
        public event Action<T> ElementRemoved;
        public event Action<IReadOnlyList<T>> Changed;

        public void Add(T element)
        {
            SerializedValue.Add(element);
            ElementAdded?.Invoke(element);
            Changed?.Invoke(SerializedValue);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            SerializedValue.AddRange(collection);
        }

        public bool Remove(T element)
        {
            if (SerializedValue.Remove(element))
            {
                ElementRemoved?.Invoke(element);
                Changed?.Invoke(SerializedValue);
                return true;
            }
            else
                return false;
        }

        public bool Contains(T element) => SerializedValue.Contains(element);

        public void Bind(Action<IReadOnlyList<T>> action)
        {
            Changed += action;
            action(Value);
        }

        public void Unbind(Action<IReadOnlyList<T>> action)
        {
            Changed -= action;
        }

        public void Unbind(object changeEvent)
        {
            if (changeEvent is not Action<IReadOnlyList<T>> action)
                return;

            Changed -= action;
        }

        public void BindElements(Action<T> onElementAdded, Action<T> onElementRemoved)
        {
            ElementAdded += onElementAdded;
            ElementRemoved += onElementRemoved;

            foreach (var element in SerializedValue)
                onElementAdded(element);
        }

        public void UnbindElements(Action<T> onElementAdded, Action<T> onElementRemoved)
        {
            ElementAdded -= onElementAdded;
            ElementRemoved -= onElementRemoved;
        }

        public void Clear()
        {
            ((ICollection<T>)SerializedValue).Clear();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)SerializedValue).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)SerializedValue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)SerializedValue).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)SerializedValue).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)SerializedValue).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)SerializedValue).RemoveAt(index);
        }
    }

    public interface IReadOnlyRefList<T>
        : IReadOnlyRef<IReadOnlyList<T>>,
            IEnumerable<T>,
            IReadOnlyList<T>
    {
        public event Action<T> ElementAdded;
        public event Action<T> ElementRemoved;

        public void BindElements(Action<T> onElementAdded, Action<T> onElementRemoved);

        public void UnbindElements(Action<T> onElementAdded, Action<T> onElementRemoved);
    }
}
