#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrArray<T> : IList<T>, IReadOnlyList<T>, IDisposable where T : struct, IEquatable<T>
    {
        #region Feild/Constructor
        public NativeArray<T> Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrArray(int capacity, Allocator allocator) => Ptr = new NativeArray<T>(capacity, allocator);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrArray(int capacity, Allocator allocator, NativeArrayOptions options) => Ptr = new NativeArray<T>(capacity, allocator, options);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrArray(T[] array, Allocator allocator) => Ptr = new NativeArray<T>(array, allocator);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrArray(in NativeArray<T> array, Allocator allocator) => Ptr = new NativeArray<T>(array, allocator);
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
        public readonly int IndexOf(T item)
        {
            for (int i = 0; i < Ptr.Length; ++i)
                if (item.Equals(Ptr[i]))
                    return i;
            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void IList<T>.Insert(int index, T item) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void IList<T>.RemoveAt(int index) => throw new NotImplementedException();
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
        readonly void ICollection<T>.Add(T item) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear() => Ptr.AsSpan().Fill(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item)
        {
            for (int i = 0; i < Ptr.Length; ++i)
                if (item.Equals(Ptr[i]))
                    return true;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(T[] array, int arrayIndex) => NativeArray<T>.Copy(Ptr, array, arrayIndex);
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