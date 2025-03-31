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
            Items = ArrayExt.SharedRent<T>(capacity);
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

        internal bool IsEmpty() => Items == null && Count == 0;
        internal bool IsFull() => Items.Length == Count;
        internal bool Contains(T item) => Count > 0 && Array.IndexOf(Items, item, 0, Count) >= 0;
        internal T Last() => Items[Count - 1];

        internal ReadOnlySpan<T>.Enumerator GetEnumerator() => Items.AsReadOnlySpan(0, Count).GetEnumerator();
        internal ReadOnlySpan<T> AsReadOnlySpan() => Items.AsReadOnlySpan(0, Count);
        internal ReadOnlySpan<T> AsSpan() => Items.AsSpan(0, Count);
    }

    /// <summary>
    /// 会申请/归还内存的数组
    /// </summary>
    internal readonly struct RefArray
    {
        internal static void Add<T>(ref RefArray<T> source, T item)
        {
            var opArray = source.Items;
            if (source.IsEmpty())
            {
                opArray = ArrayExt.SharedRent<T>(RefArray<T>.DefaultCapacity);
            }
            else if (source.IsFull())
            {
                opArray = ArrayExt.SharedRent<T>(source.Count > 0 ? source.Count << 1 : RefArray<T>.DefaultCapacity);
                Array.Copy(source.Items, 0, opArray, 0, source.Count);
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

        internal static void Return<T>(RefArray<T> source) => source.Items.SharedReturn();
        internal static void Return<T>(ref RefArray<T> source)
        {
            source.Items.SharedReturn();
            source = new RefArray<T>(null);
        }

        internal static void Clean<T>(ref RefArray<T> source)
        {
            source.Items.CleanAll();
            source = new RefArray<T>(source.Items, 0);
        }
    }
}