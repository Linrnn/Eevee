#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrQueue<T> : ICollection<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        public NativeQueue<T> Ptr;

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
        public void Add(T item) => Ptr.Enqueue(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Remove(T item) => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Ptr.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            foreach (var element in Ptr.AsReadOnly())
                if (item.Equals(element))
                    return true;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            var native = Ptr.ToArray(Allocator.Temp);
            NativeArray<T>.Copy(native, array, arrayIndex);
            native.Dispose();
        }
        #endregion

        #region GetEnumerator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Ptr.AsReadOnly().GetEnumerator();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => Ptr.AsReadOnly().GetEnumerator();
        #endregion
    }
}
#endif