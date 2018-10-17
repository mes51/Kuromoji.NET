using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Util
{
    public class PriorityQueue<T> : ICollection<T> where T : IComparable<T>
    {
        List<T> Items { get; }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        public PriorityQueue() : this(10) { }

        public PriorityQueue(int capacity)
        {
            Items = new List<T>(capacity);
        }

        public void Enqueue(T item)
        {
            if (Items.Count < 1 || Items[Items.Count - 1].CompareTo(item) <= 0)
            {
                Items.Add(item);
                return;
            }

            var i = Items.BinarySearch(item);
            if (i < 0)
            {
                Items.Add(item);
            }
            else
            {
                Items.Insert(i, item);
            }
        }

        public T Dequeue()
        {
            var result = Items[0];
            Items.RemoveAt(0);
            return result;
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Remove(T item)
        {
            return Items.Remove(item);
        }

        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
