#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrQueue<T> : ICollection<T>, IReadOnlyCollection<T>, IDisposable where T : unmanaged, IEquatable<T>
    {
        #region Feild/Constructor
        public NativeQueue<T> Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrQueue(AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeQueue<T>(allocator);
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