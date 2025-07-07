using Eevee.Define;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 内存在栈分配的集合
    /// </summary>
    internal ref struct StackAllocSet<T>
    {
        #region 类型
        private struct Entry
        {
            internal int HashCode;
            internal int Next;
            internal T Value;
            internal GCHandle Handle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void SetValue(bool referenceType, T value)
            {
                Value = value;
                if (referenceType)
                    Handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void TryFree()
            {
                if (!Handle.IsAllocated)
                    return;
                Handle.Free();
                Value = default;
            }
        }
        #endregion

        #region 字段
        private const int StartOfFreeList = -3;
        private readonly bool _referenceType;
        private readonly int _scale;
        private readonly IEqualityComparer<T> _comparer;
        private readonly Span<int> _buckets;
        private readonly Span<byte> _entries;
        private readonly int _capacity;
        private int _count;
        private int _freeCount;
        private int _freeList;
#if TARGET_64BIT
        private readonly ulong _fastModMultiplier;
#endif
        #endregion

        #region 初始化
        internal StackAllocSet(in Span<int> buckets, in Span<byte> entries)
        {
            _referenceType = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            _scale = Unsafe.SizeOf<Entry>();
            _comparer = EqualityComparer<T>.Default;
            _buckets = buckets;
            _entries = entries;
            _capacity = buckets.Length;
            _count = 0;
            _freeCount = 0;
            _freeList = -1;
#if TARGET_64BIT
            _fastModMultiplier = Utils.HashUtils.GetFastModMultiplier((uint)newSize);
#endif
        }
        internal StackAllocSet(int scale, in Span<int> buckets, in Span<byte> entries)
        {
            _referenceType = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            _scale = scale;
            _comparer = EqualityComparer<T>.Default;
            _buckets = buckets;
            _entries = entries;
            _capacity = buckets.Length;
            _count = 0;
            _freeCount = 0;
            _freeList = -1;
#if TARGET_64BIT
            _fastModMultiplier = Utils.HashUtils.GetFastModMultiplier((uint)newSize);
#endif
        }
        internal static void GetSize(ref int length, out int scale, out int capacity)
        {
            int size = Unsafe.SizeOf<Entry>();
            length = Prime.GetNumber(length);
            scale = size;
            capacity = length * size;
        }
        #endregion

        #region Internal 方法
        internal bool Contains(T item)
        {
            CheckItem(item);
            return FindItemIndex(item) >= 0;
        }
        internal bool Add(T item)
        {
            CheckItem(item);
            return AddIfNotPresent(item, out int _);
        }
        internal bool Remove(T item)
        {
            CheckItem(item);
            if (_count == 0)
                return false;

            int hashCode = _comparer.GetHashCode(item);
            ref int bucket = ref GetBucketRef(hashCode);
            int index1 = -1;
            int index2 = bucket - 1;

            for (int num = 0; index2 >= 0;)
            {
                int offset2 = index2 * _scale;
                var entry2 = GetEntry(offset2);

                if (entry2.HashCode == hashCode && _comparer.Equals(entry2.Value, item))
                {
                    if (index1 < 0)
                    {
                        bucket = entry2.Next + 1;
                    }
                    else
                    {
                        int offset1 = index1 * _scale;
                        var entry1 = GetEntry(offset1);
                        entry1.Next = entry2.Next;
                        SetEntry(offset1, in entry1);
                    }

                    entry2.Next = StartOfFreeList - _freeList;
                    entry2.TryFree();
                    SetEntry(offset2, in entry2);

                    --_count;
                    ++_freeCount;
                    _freeList = index2;
                    return true;
                }

                (index1, index2) = (index2, entry2.Next);
                ++num;
                CheckNum(num);
            }

            return false;
        }
        internal unsafe void Dispose()
        {
            if (_count <= 0)
                return;

            if (_referenceType)
                for (int i = 0, offset = 0; i < _count; ++i, offset += _scale)
                    fixed (void* ptr = &_entries[offset])
                        if (Unsafe.Read<Entry>(ptr) is { Handle: { IsAllocated: true } handle })
                            handle.Free();

            _buckets.Clear();
            _entries.Clear();
            _count = 0;
            _freeCount = 0;
            _freeList = -1;
        }
        #endregion

        #region Private 方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly unsafe Entry GetEntry(int offset)
        {
            fixed (void* ptr = &_entries[offset])
                return Unsafe.Read<Entry>(ptr);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly unsafe void SetEntry(int offset, in Entry entry)
        {
            fixed (void* ptr = &_entries[offset])
                Unsafe.Write(ptr, entry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindItemIndex(T item)
        {
            if (_count == 0)
                return -1;

            for (int num = 0, hashCode = _comparer.GetHashCode(item), bucket = GetBucketRef(hashCode) - 1; bucket >= 0; ++num)
            {
                var entry = GetEntry(bucket * _scale);
                if (entry.HashCode == hashCode && _comparer.Equals(entry.Value, item))
                    return bucket;

                bucket = entry.Next;
                CheckNum(num);
            }

            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddIfNotPresent(T value, out int location)
        {
            Assert.Less<InvalidOperationException, AssertArgs<int, int>, int>(_count, _capacity, nameof(_count), "_count:{0} >= _capacity:{1}", new AssertArgs<int, int>(_count, _capacity));
            int hashCode = _comparer.GetHashCode(value);
            ref int bucket = ref GetBucketRef(hashCode);
            int next0 = bucket - 1;

            for (int index0 = next0, num = 0; index0 >= 0; ++num)
            {
                var entry0 = GetEntry(index0 * _scale);
                if (entry0.HashCode == hashCode && _comparer.Equals(entry0.Value, value))
                {
                    location = index0;
                    return false;
                }

                index0 = entry0.Next;
                CheckNum(num);
            }

            bool hasFree = _freeCount > 0;
            int index1 = hasFree ? _freeList : _count;
            int offset1 = index1 * _scale;
            var entry1 = GetEntry(offset1);
            int next1 = entry1.Next;

            entry1.HashCode = hashCode;
            entry1.Next = next0;
            entry1.SetValue(_referenceType, value);
            SetEntry(offset1, in entry1);

            bucket = index1 + 1;
            ++_count;
            if (hasFree)
                --_freeCount;
            _freeList = StartOfFreeList - next1;
            location = index1;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ref int GetBucketRef(int hashCode)
        {
#if TARGET_64BIT
            int index = (int)Utils.HashUtils.FastMod((uint)hashCode, (uint)_capacity, _fastModMultiplier);
#else
            int index = (int)((uint)hashCode % (uint)_capacity);
#endif
            return ref _buckets[index];
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        private readonly void CheckNum(int num) => Assert.LessEqual<IndexOutOfRangeException, AssertArgs<int, int>, int>(num, _capacity, nameof(num), "num:{0} > _capacity:{1}", new AssertArgs<int, int>(num, _scale));
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        private readonly void CheckItem(T item)
        {
            if (_referenceType)
                Assert.NotNull<ArgumentNullException, AssertArgs>(item, nameof(item), "is null!");
        }
        #endregion
    }
}