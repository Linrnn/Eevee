using Eevee.Diagnosis;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

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

        internal RefArray(ArrayPool<T> arrayPool, int capacity = DefaultCapacity)
        {
            Items = ArrayExt.Rent(capacity, arrayPool);
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

        internal ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Items[index];
        }

        internal bool IsEmpty() => Count == 0;
        internal bool IsFull() => Items.Length == Count;
        internal bool Contains(in T item) => Count > 0 && Array.IndexOf(Items, item, 0, Count) >= 0;
        internal int IndexOf(in T item) => Count > 0 ? Array.IndexOf(Items, item, 0, Count) : -1;
        internal ref T Last() => ref Items[Count - 1];

        public ReadOnlySpan<T>.Enumerator GetEnumerator() => Items.AsReadOnlySpan(0, Count).GetEnumerator();
        internal ReadOnlySpan<T> AsReadOnlySpan() => Items.AsReadOnlySpan(0, Count);
        internal ReadOnlySpan<T> AsSpan() => Items.AsSpan(0, Count);
    }

    /// <summary>
    /// 会申请/归还内存的数组
    /// </summary>
    internal readonly struct RefArray
    {
        internal static void Add<T>(ref RefArray<T> source, in T item, ArrayPool<T> pool)
        {
            var opArray = source.Items;
            int count = source.Count;
            if (source.IsEmpty())
            {
                opArray = ArrayExt.Rent(RefArray<T>.DefaultCapacity, pool);
            }
            else if (source.IsFull())
            {
                opArray = ArrayExt.Rent(count << 1, pool);
                Array.Copy(source.Items, 0, opArray, 0, count);
                Return(ref source, pool);
            }

            opArray[count] = item;
            source = new RefArray<T>(opArray, count + 1);
        }
        internal static void Remove<T>(ref RefArray<T> source, in T item)
        {
            int index = source.IndexOf(in item);
            RemoveAt(ref source, index);
        }
        internal static void RemoveAt<T>(ref RefArray<T> source, int index)
        {
            Assert.Range<ArgumentOutOfRangeException, AssertArgs<int, int>, int>(index, 0, source.Count - 1, nameof(index), "set fail, index:{0} out of range [0, {1})", new AssertArgs<int, int>(index, source.Count));
            int count = source.Count - 1;
            var items = source.Items;
            if (index < count)
                Array.Copy(items, index + 1, items, index, count - index);
            source[count] = default;
            source = new RefArray<T>(items, count);
        }
        internal static void WeakOrderRemoveAt<T>(ref RefArray<T> source, int index)
        {
            Assert.Range<ArgumentOutOfRangeException, AssertArgs<int, int>, int>(index, 0, source.Count - 1, nameof(index), "set fail, index:{0} out of range [0, {1})", new AssertArgs<int, int>(index, source.Count));
            int count = source.Count - 1;
            var items = source.Items;
            if (index < count)
                items[index] = items[count];
            items[count] = default;
            source = new RefArray<T>(items, count);
        }

        internal static void Return<T>(RefArray<T> source, ArrayPool<T> pool) => source.Items.Return(pool);
        internal static void Return<T>(ref RefArray<T> source, ArrayPool<T> pool)
        {
            source.Items.Return(pool);
            source = new RefArray<T>(null);
        }

        internal static void Clean<T>(ref RefArray<T> source)
        {
            source.Items.Clean();
            source = new RefArray<T>(source.Items, 0);
        }
    }
}