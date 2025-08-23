#if UNITY_5_3_OR_NEWER
using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Eevee.Collection
{
    public struct PtrMap<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged, IEquatable<TValue>
    {
        #region Feild/Constructor
        public NativeHashMap<TKey, TValue> Ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrMap(AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeHashMap<TKey, TValue>(7, allocator);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PtrMap(int capacity, AllocatorManager.AllocatorHandle allocator) => Ptr = new NativeHashMap<TKey, TValue>(capacity, allocator);
        #endregion

        #region IDictionary`2
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly get => Ptr[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => Ptr[key] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => Ptr.ContainsKey(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) => Ptr.TryGetValue(key, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value) => Ptr.Add(key, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key) => Ptr.Remove(key);
        #endregion

        #region ICollection`1
        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Ptr.Count;
        }
        readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair) => Ptr.Add(pair.Key, pair.Value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair) => Ptr.TryGetValue(pair.Key, out var value) && Ptr.Remove(pair.Key) && value.Equals(pair.Value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Ptr.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair) => Ptr.TryGetValue(pair.Key, out var value) && value.Equals(pair.Value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Assert.NotNull<ArgumentNullException, DiagnosisArgs>(array, nameof(array), "Target array cannot be null.");
            Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<int>, int>(arrayIndex, 0, nameof(arrayIndex), "Starting index cannot be negative. arrayIndex is {0}.", new DiagnosisArgs<int>(arrayIndex));
            Assert.GreaterEqual<ArgumentException, DiagnosisArgs<int, int, int>, int>(array.Length - arrayIndex, Ptr.Count, nameof(arrayIndex), "The destination array has insufficient space to copy all the items starting from the specified index. {0} - {1} < {2}.", new DiagnosisArgs<int, int, int>(array.Length, arrayIndex, Ptr.Count));
            int index = 0;
            foreach (var pair in Ptr)
                array[arrayIndex + index++] = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
        }
        #endregion

        #region IDisposable
        public void Dispose() => Ptr.Dispose();
        #endregion

        #region Keys/Values
        readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => throw new NotImplementedException();
        }
        readonly ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => throw new NotImplementedException();
        }

        readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => throw new NotImplementedException();
        }
        readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => throw new NotImplementedException();
        }
        #endregion

        #region GetEnumerator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => throw new NotImplementedException();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        #endregion
    }
}
#endif