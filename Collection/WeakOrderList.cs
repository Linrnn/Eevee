﻿using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 弱顺序列表<br/>
    /// 特点：插入被挤占的元素会挪至最后；最后的元素会往“Remove”的位置填充<br/>
    /// 优点：规避“Insert”，“Remove”引发的数组偏移，从而减少耗时<br/>
    /// 缺点：“Insert”，“Remove”会打乱元素的相对位置
    /// </summary>
    [Serializable]
    public sealed class WeakOrderList<T> : IList<T>, IReadOnlyList<T>, IList
    {
        #region Type
        public struct Enumerator : IEnumerator<T>
        {
            private readonly WeakOrderList<T> _list;
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(WeakOrderList<T> list)
            {
                _list = list;
                _version = list._version;
                _index = 0;
                _current = default;
            }

            #region IEnumerator<T>
            public readonly T Current => _current;
            #endregion

            #region IEnumerator
            readonly object IEnumerator.Current => _current;
            public bool MoveNext()
            {
                Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_version, _list._version, nameof(_version), "MoveNext fail, _version:{0} != _list._version:{1}", new AssertArgs<int, int>(_version, _list._version));
                if (_version != _list._version || _index >= _list._size)
                {
                    _index = _list._size + 1;
                    _current = default;
                    return false;
                }

                _current = _list._items[_index];
                ++_index;
                return true;
            }
            public void Reset()
            {
                _index = 0;
                _current = default;
            }
            #endregion

            #region IDisposable
            public readonly void Dispose() { }
            #endregion
        }
        #endregion

        #region Field/Constructor
        private const int DefaultCapacity = 4;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField] private T[] _items; // 无法接入“ArrayPool`1”，“_items”会被“Mono”序列化，无法归还“ArrayPool`1”
        [UnityEngine.SerializeField] private int _size;
#else
        private T[] _items;
        private int _size;
#endif
        private int _version;

        public WeakOrderList() => _items = Array.Empty<T>();
        public WeakOrderList(int capacity) => _items = ArrayExt.Create<T>(capacity);
        public WeakOrderList(IEnumerable<T> other)
        {
            bool hasCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(other, out int count);
            int capacity = hasCount ? count : DefaultCapacity;
            _items = ArrayExt.Create<T>(capacity);
            _size = Math.Max(count, 0);

            if (other is not ICollection<T> collection)
                this.AddRange0GC(other);
            else if (count > 0)
                collection.CopyTo(_items, 0);
        }
        #endregion

        #region Property
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _items[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _items[index] = value;
                ++_version;
            }
        }
        public int Capacity
        {
            get => _items.Length;
            set => SetCapacity(value);
        }
        #endregion

        #region IList`1
        public int IndexOf(T item) => Array.IndexOf(_items, item, 0, _size);

        public void Insert(int index, T item)
        {
            if (_size == _items.Length)
                EnsureCapacity(_size + 1);

            if (index < _size)
                _items[_size] = _items[index];
            _items[index] = item;

            ++_size;
            ++_version;
        }
        public void RemoveAt(int index)
        {
            --_size;

            if (index < _size)
                _items[index] = _items[_size];
            _items[_size] = default;

            ++_version;
        }
        #endregion

        #region ICollection`1
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size;
        }
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (_size == _items.Length)
                EnsureCapacity(_size + 1);

            _items[_size++] = item;
            ++_version;
        }
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }
        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }

            ++_version;
        }

        public bool Contains(T item) => _size > 0 && IndexOf(item) >= 0;
        public void CopyTo(T[] array, int arrayIndex = 0) => Array.Copy(_items, 0, array, arrayIndex, _size);
        #endregion

        #region IList
        bool IList.IsFixedSize => false;
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        bool IList.Contains(object value) => Contains((T)value);
        int IList.IndexOf(object value) => IndexOf((T)value);

        int IList.Add(object value)
        {
            Add((T)value);
            return _size - 1;
        }
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value) => Remove((T)value);
        #endregion

        #region ICollection
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        void ICollection.CopyTo(Array array, int index) => Array.Copy(_items, 0, array, index, _size);
        #endregion

        #region Enumerator
        public ReadOnlySpan<T>.Enumerator GetEnumerator() => AsReadOnlySpan().GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        #endregion

        #region Extractable
        public int IndexOf(T item, int index, int count) => Array.IndexOf(_items, item, index, count);
        public int LastIndexOf(T item, int index, int count) => _size == 0 ? -1 : Array.LastIndexOf(_items, item, index, count);
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer = null) => Array.BinarySearch(_items, index, count, item, comparer);
        public void CopyTo(T[] array, int index, int length) => Array.Copy(_items, 0, array, index, length);
        public ReadOnlySpan<T> AsReadOnlySpan() => _items.AsReadOnlySpan(0, _size);
        public Span<T> AsSpan() => _items.AsSpan(0, _size);

        internal void InsertRange(int index, IEnumerable<T> other) // 调用“ICollectionExt.AddRange0GC”和“IListExt.InsertRange0GC”替代此接口
        {
            if (other is ICollection<T> collection)
            {
                int count = collection.Count;
                if (count > 0)
                {
                    int end = Math.Max(_size, index + count);
                    EnsureCapacity(_size + count);

                    Array.Copy(_items, index, _items, end, count);
                    collection.CopyTo(_items, index);
                    _size += count;
                }
            }
            else
            {
                int idx = index;
                foreach (var item in other)
                    Insert(idx++, item);
            }

            ++_version;
        }
        public void RemoveRange(int index, int count)
        {
            if (count <= 0)
                return;

            int end = _size - count;
            int length = Math.Min(count, end - index);

            Array.Copy(_items, _size - length, _items, index, length);
            Array.Clear(_items, end, count);

            _size = end;
            ++_version;
        }

        public void Reverse(int index, int count)
        {
            Array.Reverse(_items, index, count);
            ++_version;
        }
        public void Sort(IComparer<T> comparer = null) => Sort(0, _size, comparer);
        public void Sort(int index, int count, IComparer<T> comparer = null)
        {
            Array.Sort(_items, index, count, comparer);
            ++_version;
        }
        #endregion

        #region private
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int capacity)
        {
            if (capacity <= _items.Length)
                return;
            int size = _items.Length == 0 ? DefaultCapacity : _items.Length << 1;
            int clamp = Math.Clamp(size, capacity, 2146435071);
            SetCapacity(clamp);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCapacity(int capacity)
        {
            if (capacity > _items.Length)
            {
                var items = ArrayExt.Create<T>(capacity);
                if (_size > 0)
                    Array.Copy(_items, 0, items, 0, _size);
                _items = items;
            }

            ++_version;
        }
        #endregion
    }
}