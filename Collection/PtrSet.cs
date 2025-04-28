#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrSet<T> : ISet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        #region Feild/Constructor
        public NativeHashSet<T> Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrSet(AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeHashSet<T>(7, allocator);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrSet(int capacity, AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeHashSet<T>(capacity, allocator);
        #endregion

        #region ISet`1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ISet<T>.Add(T item) => Ptr.Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ISet<T>.IsSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ISet<T>.IsSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ISet<T>.Overlaps(IEnumerable<T> other) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly bool ISet<T>.SetEquals(IEnumerable<T> other) => throw new NotImplementedException();
        #endregion

        #region ICollection`1
        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Ptr.Count;
        }
        readonly bool ICollection<T>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => Ptr.Add(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => Ptr.Remove(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Ptr.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => Ptr.Contains(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            var native = Ptr.ToNativeArray(Allocator.Temp);
            NativeArray<T>.Copy(native, array, arrayIndex);
            native.Dispose();
        }
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