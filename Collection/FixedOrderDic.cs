using Eevee.Define;
using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 确定性顺序的字典
    /// </summary>
    [Serializable]
    public sealed class FixedOrderDic<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyList<TKey>, IDictionary where TKey : notnull
    {
        #region Type
        internal enum ReturnType
        {
            DictionaryEntry, // DictionaryEntry
            KeyValuePair, // KeyValuePair`2
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly FixedOrderDic<TKey, TValue> _enumerator;
            private readonly ReturnType _returnType;
            private readonly int _version;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(FixedOrderDic<TKey, TValue> enumerator, ReturnType returnType)
            {
                _enumerator = enumerator;
                _returnType = returnType;
                _version = enumerator._order.GetVersion();
                _index = 0;
                _current = default;
            }

            #region IEnumerator`1
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
            #endregion

            #region IEnumerator
            readonly object IEnumerator.Current => _returnType switch
            {
                ReturnType.DictionaryEntry => new DictionaryEntry(_current.Key, _current.Value),
                ReturnType.KeyValuePair => _current,
                _ => throw new NotImplementedException($"Enumerator fail, returnType:{_returnType} is illegal"),
            };

            public bool MoveNext()
            {
                int version = _enumerator._order.GetVersion();
                int count = _enumerator.Count;
                Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_version, version, nameof(_version), "MoveNext fail, _version:{0} != _enumerator._order._version:{1}", new AssertArgs<int, int>(_version, version));

                if (_version != version || _index >= count)
                {
                    _index = count + 1;
                    _current = default;
                    return false;
                }

                var key = _enumerator._order[_index];
                _current = new KeyValuePair<TKey, TValue>(key, _enumerator._collection[key]);
                ++_index;
                return true;
            }
            public void Reset() => Dispose();
            #endregion

            #region IDisposable
            public void Dispose()
            {
                _index = 0;
                _current = default;
            }
            #endregion

            #region IDictionaryEnumerator
            public DictionaryEntry Entry => new(_current.Key, _current.Value);
            public object Key => _current.Key;
            public object Value => _current.Value;
            #endregion
        }

        private readonly struct ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
        {
            private readonly FixedOrderDic<TKey, TValue> _enumerator;

            internal ValueCollection(FixedOrderDic<TKey, TValue> enumerator) => _enumerator = enumerator;

            #region ICollection`1
            public int Count => _enumerator.Count;
            bool ICollection<TValue>.IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException("ValueCollection`1.Add");
            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException("ValueCollection`1.Remove");
            void ICollection<TValue>.Clear() => throw new NotSupportedException("ValueCollection`1.Clear");

            public bool Contains(TValue item) => _enumerator.ContainsValue(item);
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                _enumerator.CheckCount();
                for (int count = _enumerator.Count, i = 0, j = arrayIndex; i < count; ++i, ++j)
                    array[j] = _enumerator[_enumerator._order[i]];
            }
            #endregion

            #region ICollection
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => _enumerator;

            void ICollection.CopyTo(Array array, int index)
            {
                _enumerator.CheckCount();
                Assert.Equal<ArgumentException, AssertArgs<object, int>, int>(array.Rank, 1, nameof(array), "array:{0}, Rank is {1}, isn't 1.", new AssertArgs<object, int>(array.GetType(), array.Rank));

                switch (array)
                {
                    case TValue[] values: CopyTo(values, index); break;
                    case object[] objects:
                        for (int count = _enumerator.Count, i = 0, j = index; i < count; ++i, ++j)
                            objects[j] = _enumerator[_enumerator._order[i]];
                        break;

                    default: throw new NotImplementedException($"CopyTo fail, array:{array.GetType().FullName}, type is illegal");
                }
            }
            #endregion

            #region Enumerator
            internal ValueEnumerator GetEnumerator() => new(_enumerator);
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new ValueEnumerator(_enumerator);
            IEnumerator IEnumerable.GetEnumerator() => new ValueEnumerator(_enumerator);
            #endregion
        }

        private struct ValueEnumerator : IEnumerator<TValue>
        {
            private readonly FixedOrderDic<TKey, TValue> _enumerator;
            private readonly int _version;
            private int _index;
            private TValue _current;

            internal ValueEnumerator(FixedOrderDic<TKey, TValue> enumerator)
            {
                _enumerator = enumerator;
                _version = enumerator._order.GetVersion();
                _index = 0;
                _current = default;
            }

            #region IEnumerator`1
            public readonly TValue Current => _current!;
            #endregion

            #region IEnumerator
            readonly object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                int version = _enumerator._order.GetVersion();
                int count = _enumerator.Count;
                Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_version, version, nameof(_version), "MoveNext fail, _version:{0} != _enumerator._order._version:{1}", new AssertArgs<int, int>(_version, version));

                if (_version != version || _index >= count)
                {
                    _index = count + 1;
                    _current = default;
                    return false;
                }

                var key = _enumerator._order[_index];
                _current = _enumerator._collection[key];
                ++_index;
                return true;
            }
            public void Reset() => Dispose();
            #endregion

            #region IDisposable
            public void Dispose()
            {
                _index = 0;
                _current = default;
            }
            #endregion
        }
        #endregion

        #region Field/Constructor
        private readonly Dictionary<TKey, TValue> _collection;
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField] private WeakOrderList<TKey> _order;
#else
        private readonly WeakOrderList<TKey> _order;
#endif

        public FixedOrderDic()
        {
            CheckComparer();
            _collection = new Dictionary<TKey, TValue>();
            _order = new WeakOrderList<TKey>();
        }
        public FixedOrderDic(int capacity)
        {
            CheckComparer();
            _collection = new Dictionary<TKey, TValue>(capacity);
            _order = new WeakOrderList<TKey>(capacity);
        }
        public FixedOrderDic(IDictionary<TKey, TValue> other)
        {
            CheckComparer();
            int capacity = other.Count;
            _collection = new Dictionary<TKey, TValue>(capacity);
            _order = new WeakOrderList<TKey>(capacity);
            this.AddRange0GC(other);
        }
        public FixedOrderDic(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            CheckComparer();
            _collection = new Dictionary<TKey, TValue>();
            _order = new WeakOrderList<TKey>();
            this.AddRange0GC(other);
        }
        public FixedOrderDic(IEqualityComparer<TKey> comparer)
        {
            CheckComparer(comparer);
            _collection = new Dictionary<TKey, TValue>(comparer);
            _order = new WeakOrderList<TKey>();
        }
        public FixedOrderDic(int capacity, IEqualityComparer<TKey> comparer)
        {
            CheckComparer(comparer);
            _collection = new Dictionary<TKey, TValue>(capacity, comparer);
            _order = new WeakOrderList<TKey>(capacity);
        }
        public FixedOrderDic(IDictionary<TKey, TValue> other, IEqualityComparer<TKey> comparer)
        {
            CheckComparer();
            int capacity = other.Count;
            _collection = new Dictionary<TKey, TValue>(capacity, comparer);
            _order = new WeakOrderList<TKey>(capacity);
            this.AddRange0GC(other);
        }
        public FixedOrderDic(IEnumerable<KeyValuePair<TKey, TValue>> other, IEqualityComparer<TKey> comparer)
        {
            CheckComparer();
            _collection = new Dictionary<TKey, TValue>();
            _order = new WeakOrderList<TKey>();
            this.AddRange0GC(other);
        }
        #endregion

        #region IDictionary`2
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckCount();
                return _collection[key];
            }
            set
            {
                CheckCount();
                _collection[key] = value;
                if (_collection.Count == _order.Count + 1)
                    _order.Add(key);
                CheckCount();
            }
        }
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => GetKeys();
        ICollection<TValue> IDictionary<TKey, TValue>.Values => GetValues();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            CheckCount();
            return _collection.ContainsKey(key);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            CheckCount();
            return _collection.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            CheckCount();
            _collection.Add(key, value);
            _order.Add(key);
            CheckCount();
        }
        public bool Remove(TKey key)
        {
            CheckCount();
            if (!_collection.Remove(key))
                return false;

            _order.Remove(key);
            CheckCount();
            return true;
        }
        #endregion

        #region ICollection`1
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckCount();
                return _collection.Count;
            }
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair) => Add(pair.Key, pair.Value);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair) => Remove(pair.Key);
        public void Clear()
        {
            CheckCount();
            _collection.Clear();
            _order.Clear();
            CheckCount();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair) => ContainsKey(pair.Key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            CheckCount();
            for (int count = _collection.Count, i = 0, j = arrayIndex; i < count; ++i, ++j)
                array[j] = new KeyValuePair<TKey, TValue>(_order[i], _collection[_order[i]]);
        }
        #endregion

        #region IReadOnlyDictionary`2
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => GetKeys();
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => GetValues();
        #endregion

        #region IReadOnlyList`1
        TKey IReadOnlyList<TKey>.this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckCount();
                return _order[index];
            }
        }
        #endregion

        #region IDictionary
        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;
        ICollection IDictionary.Keys => GetKeys();
        ICollection IDictionary.Values => GetValues();
        object IDictionary.this[object key]
        {
            get
            {
                if (key is TKey tKey)
                    return this[tKey];

                CheckCount();
                return null;
            }
            set => this[(TKey)key] = (TValue)value;
        }

        void IDictionary.Add(object key, object value) => Add((TKey)key, (TValue)value);
        void IDictionary.Remove(object key)
        {
            if (key is TKey tKey)
                Remove(tKey);
        }
        bool IDictionary.Contains(object key) => key is TKey tKey && ContainsKey(tKey);
        #endregion

        #region ICollection
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        void ICollection.CopyTo(Array array, int index)
        {
            CheckCount();
            Assert.Equal<ArgumentException, AssertArgs<object, int>, int>(array.Rank, 1, nameof(array), "array:{0}, Rank is {1}, isn't 1.", new AssertArgs<object, int>(array.GetType(), array.Rank));

            switch (array)
            {
                case KeyValuePair<TKey, TValue>[] pairs: CopyTo(pairs, index); break;

                case DictionaryEntry[] entries:
                    for (int count = _collection.Count, i = 0, j = index; i < count; ++i, ++j)
                        entries[j] = new DictionaryEntry(_order[i], _collection[_order[i]]);
                    break;

                case object[] objects:
                    for (int count = _collection.Count, i = 0, j = index; i < count; ++i, ++j)
                        objects[j] = new KeyValuePair<TKey, TValue>(_order[i], _collection[_order[i]]);
                    break;

                default: throw new NotImplementedException($"CopyTo fail, array:{array.GetType().FullName}, type is illegal");
            }
        }
        #endregion

        #region Enumerator
        public Enumerator GetEnumerator() => new(this, ReturnType.KeyValuePair);
        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => _order.GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this, ReturnType.KeyValuePair);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, ReturnType.KeyValuePair);
        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, ReturnType.DictionaryEntry);
        #endregion

        #region Public
        public IEqualityComparer<TKey> Comparer => _collection.Comparer;

        public bool ContainsValue(TValue value)
        {
            CheckCount();
            return _collection.ContainsValue(value);
        }
        public bool TryAdd(TKey key, TValue value)
        {
            CheckCount();
            if (!_collection.TryAdd(key, value))
                return false;

            _order.Add(key);
            CheckCount();
            return true;
        }
        public bool Remove(TKey key, out TValue value)
        {
            CheckCount();
            if (!_collection.Remove(key, out value))
                return false;

            _order.Remove(key);
            CheckCount();
            return true;
        }

        public void TrimExcess()
        {
            CheckCount();
            _collection.TrimExcess();
            _order.Capacity = Count;
            CheckCount();
        }
        public void TrimExcess(int capacity)
        {
            CheckCount();
            _collection.TrimExcess(capacity);
            _order.Capacity = capacity;
            CheckCount();
        }
        public int EnsureCapacity(int capacity)
        {
            CheckCount();
            int size = _collection.EnsureCapacity(capacity);
            _order.Capacity = size;
            CheckCount();
            return size;
        }

        public bool CheckEquals() // 检测“_collection”与“_order”是否一致
        {
            CheckCount();
            if (_collection.Count != _order.Count)
                return false;

            foreach (var pair in _collection) // 因为“_order”可能有重复项，所以不遍历“_order”
                if (!_order.Contains(pair.Key))
                    return false;
            return true;
        }
        public ReadOnlySpan<TKey> AsSpan()
        {
            CheckCount();
            return _order.AsReadOnlySpan();
        }
        public IEnumerable<KeyValuePair<TKey, TValue>> AsPair()
        {
            CheckCount();
            return this;
        }
        #endregion

        #region Private
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WeakOrderList<TKey> GetKeys()
        {
            CheckCount();
            return _order;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueCollection GetValues()
        {
            CheckCount();
            return new ValueCollection(this);
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckComparer(IEqualityComparer<TKey> comparer = null)
        {
            if (comparer == null || comparer == EqualityComparer<TKey>.Default)
                Assert.Convert<ArgumentException, AssertArgs<object>, TKey, IEquatable<TKey>>(nameof(comparer), "T:{0} 未继承 IEquatable<T>", new AssertArgs<object>(typeof(TKey)));
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCount() => Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_collection.Count, _order.Count, "Count", "Count fail, _data.Count:{0} != _order.Count:{1}", new AssertArgs<int, int>(_collection.Count, _order.Count));
        #endregion
    }
}