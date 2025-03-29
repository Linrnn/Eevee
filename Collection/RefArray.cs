using System;

namespace Eevee.Collection
{
    /// <summary>
    /// 会申请/归还内存的数组
    /// </summary>
    internal readonly struct RefArray<T>
    {
        internal const int DefaultCapacity = 4;
        internal readonly T[] Items;
        internal readonly int Count;

        internal RefArray(int capacity = DefaultCapacity)
        {
            Items = capacity > 0 ? ArrayExt.SharedRent<T>(capacity) : Array.Empty<T>();
            Count = capacity;
        }
        internal RefArray(T[] items)
        {
            var array = items ?? Array.Empty<T>();
            Items = array;
            Count = array.Length;
        }
        internal RefArray(T[] items, int count)
        {
            var array = items ?? Array.Empty<T>();
            Items = array;
            Count = Math.Min(count, array.Length);
        }

        internal bool IsNullOrEmpty() => Items == null || Count == 0;
        internal bool IsFull() => Items.Length == Count;
        internal bool Contains(T item) => Array.IndexOf(Items, item, 0, Count) >= 0;

        internal ReadOnlySpan<T> ASpan() => Items.AsReadOnlySpan(0, Count);
        internal ReadOnlySpan<T>.Enumerator GetEnumerator() => Items.AsReadOnlySpan(0, Count).GetEnumerator();
    }

    /// <summary>
    /// 会申请/归还内存的数组
    /// </summary>
    internal readonly struct RefArray
    {
        internal static void Add<T>(ref RefArray<T> source, T item)
        {
            var opArray = source.Items;
            if (source.IsFull())
            {
                opArray = ArrayExt.SharedRent<T>(source.Count > 0 ? source.Count << 1 : RefArray<T>.DefaultCapacity);
                Array.Copy(source.Items, opArray, source.Count);
                Return(ref source);
            }

            opArray[source.Count] = item;
            source = new RefArray<T>(opArray, source.Count + 1);
        }
        internal static bool Remove<T>(ref RefArray<T> source, T item)
        {
            int index = source.Items.IndexOf(item, 0, source.Count);
            return RemoveAt(ref source, index);
        }
        internal static bool RemoveAt<T>(ref RefArray<T> source, int index)
        {
            int count = source.Count;
            if (index < 0 || index >= count)
                return false;

            --count;
            Array.Copy(source.Items, index + 1, source.Items, index, count - index);
            source.Items[count] = default;
            source = new RefArray<T>(source.Items, count);
            return true;
        }

        internal static void Return<T>(RefArray<T> source)
        {
            if (!ReferenceEquals(source.Items, Array.Empty<T>()))
                source.Items.SharedReturn();
        }
        internal static void Return<T>(ref RefArray<T> source)
        {
            if (ReferenceEquals(source.Items, Array.Empty<T>()))
                return;

            source.Items.SharedReturn();
            source = new RefArray<T>(null);
        }
    }
}