using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class Heap<T> where T : IHeapItem<T>
    {
        private readonly List<T> items;
        private int currentItemCount;

        public int Count => items.Count;

        public Heap(int startSize = 10)
        {
            items = new(startSize);
        }

        public void Enqueue(T item)
        {
            item.HeapIndex = Count;
            items.Add(item);

            if (currentItemCount > 0)
                SortUp(item);

            currentItemCount++;
        }

        public T Dequeue()
        {
            T firstItem = items[0];
            currentItemCount--;

            if (currentItemCount > 0)
            {
                items[0] = items[currentItemCount];
                items[0].HeapIndex = 0;

                items.RemoveAt(currentItemCount);
                SortDown(items[0]);
            }
            else
                items.RemoveAt(0);

            return firstItem;
        }

        public bool Contains(T item)
        {
            if (item.HeapIndex < currentItemCount)
                return Equals(items[item.HeapIndex], item);
            else
                return false;
        }

        public void Clear()
        {
            items.Clear();
            currentItemCount = 0;
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int leftChildIndex = item.HeapIndex * 2 + 1;
                int rightChildIndex = item.HeapIndex * 2 + 2;
                int swapIndex;

                if (leftChildIndex < currentItemCount)
                {
                    swapIndex = leftChildIndex;

                    if (rightChildIndex < currentItemCount)
                    {
                        if (items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0)
                            swapIndex = rightChildIndex;
                    }

                    if (item.CompareTo(items[swapIndex]) < 0)
                        Swap(item, items[swapIndex]);
                    else
                        return;
                }
                else
                    return;
            }
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];

                if (item.CompareTo(parentItem) > 0)
                    Swap(item, parentItem);
                else
                    break;

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            (itemB.HeapIndex, itemA.HeapIndex) = (itemA.HeapIndex, itemB.HeapIndex);
        }
    }
}
