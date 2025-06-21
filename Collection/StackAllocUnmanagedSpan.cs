using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 内存在栈分配的元素集合
    /// </summary>
    internal ref struct StackAllocUnmanagedSpan<T> where T : unmanaged
    {
        private readonly Span<T> _span;
        internal int Count;

        internal StackAllocUnmanagedSpan(in Span<T> span)
        {
            _span = span;
            Count = 0;
        }

        internal ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[index];
        }
        internal ref T RefGet(int index) => ref _span[index];
        internal T Get(int index) => _span[index];

        internal void Set(int index, in T element)
        {
            Assert.Range<ArgumentOutOfRangeException, AssertArgs<int, int>, int>(index, 0, Count - 1, nameof(index), "set fail, index:{0} out of range [0, {1})", new AssertArgs<int, int>(index, Count));
            _span[index] = element;
        }
        internal void Add(in T element) => _span[Count++] = element;
        internal void RemoveAt(int index)
        {
            Assert.Range<ArgumentOutOfRangeException, AssertArgs<int, int>, int>(index, 0, Count - 1, nameof(index), "set fail, index:{0} out of range [0, {1})", new AssertArgs<int, int>(index, Count));
            int count = --Count;
            if (index < count)
            {
                int length = count - index - 1;
                _span.Slice(index + 1, length).CopyTo(_span.Slice(index, length));
            }
            _span[count] = default;
        }
    }
}