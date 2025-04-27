#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrArray<T> : IList<T>, IReadOnlyList<T> where T : struct, IEquatable<T>
    {
        public NativeArray<T> Ptr;

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
        public void Clear() => Ptr.AsSpan().Fill(default);

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

        #region GetEnumerator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Ptr.GetEnumerator();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => Ptr.GetEnumerator();
        #endregion
    }
}
#endif