using System;
using UnityEngine;

namespace Shears
{
    public interface IHeapItem<T> : IComparable<T>
    {
        public int HeapIndex { get; set; }
    }
}
