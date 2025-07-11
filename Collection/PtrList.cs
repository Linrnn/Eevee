﻿#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrList<T> : IList<T>, IReadOnlyList<T>, IDisposable where T : unmanaged, IEquatable<T>
    {
        #region Feild/Constructor
        public NativeList<T> Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrList(AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeList<T>(allocator);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrList(int capacity, AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeList<T>(capacity, allocator);
        #endregion

        #region IList`1
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Ptr[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Ptr[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item) => Ptr.IndexOf(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            Ptr.InsertRange(index, 1);
            Ptr[index] = item;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => Ptr.RemoveAt(index);
        #endregion

        #region ICollection`1
        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Ptr.Length;
        }
        readonly bool ICollection<T>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => Ptr.Add(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            int index = Ptr.IndexOf(item);
            if (index < 0)
                return false;

            Ptr.RemoveAt(index);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Ptr.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item) => Ptr.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => NativeArray<T>.Copy(Ptr.AsReadOnly(), array, arrayIndex);
        #endregion

        #region IDisposable
        public void Dispose() => Ptr.Dispose();
        #endregion

        #region GetEnumerator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        #endregion
    }
}
#endif