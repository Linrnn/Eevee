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
    /// 确定性顺序的集合
    /// </summary>
    [Serializable]
    public sealed class FixedOrderSet<T> : ISet<T>, IReadOnlyList<T>, ICollection
    {
        #region Field/Constructor
        private readonly HashSet<T> _collection;
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField] private WeakOrderList<T> _order;
#else
        private readonly WeakOrderList<T> _order;
#endif

        public FixedOrderSet()
        {
            CheckComparer();
            _collection = new HashSet<T>();
            _order = new WeakOrderList<T>();
        }
        public FixedOrderSet(int capacity)
        {
            CheckComparer();
            _collection = new HashSet<T>(capacity);
            _order = new WeakOrderList<T>(capacity);
        }
        public FixedOrderSet(IEnumerable<T> other)
        {
            CheckComparer();
            _collection = new HashSet<T>();
            _order = new WeakOrderList<T>();
            this.UnionWithLowGC(other);
        }
        public FixedOrderSet(IEqualityComparer<T> comparer)
        {
            CheckComparer(comparer);
            _collection = new HashSet<T>(comparer);
            _order = new WeakOrderList<T>();
        }
        public FixedOrderSet(int capacity, IEqualityComparer<T> comparer)
        {
            CheckComparer(comparer);
            _collection = new HashSet<T>(capacity, comparer);
            _order = new WeakOrderList<T>(capacity);
        }
        public FixedOrderSet(IEnumerable<T> other, IEqualityComparer<T> comparer)
        {
            CheckComparer(comparer);
            _collection = new HashSet<T>(comparer);
            _order = new WeakOrderList<T>();
            this.UnionWithLowGC(other);
        }
        #endregion

        #region ISet`1
        public bool Add(T item)
        {
            CheckCount();
            if (!_collection.Add(item))
                return false;

            _order.Add(item);
            CheckCount();
            return true;
        }

        /// <summary>
        /// 与目标的并集<br/>
        /// “this”与“other”的并集<br/>
        /// this U other
        /// </summary>
        public void UnionWith(IEnumerable<T> other) => this.UnionWithLowGC(other);
        /// <summary>
        /// 与目标的交集<br/>
        /// “this”与“other”的交集<br/>
        /// this ∩ other
        /// </summary>
        public void IntersectWith(IEnumerable<T> other) => this.IntersectWithLowGC(other);
        /// <summary>
        /// 与目标的差集<br/>
        /// “this”与“other”的补集 的交集<br/>
        /// this ∩ ~other
        /// </summary>
        public void ExceptWith(IEnumerable<T> other) => this.ExceptWithLowGC(other);
        /// <summary>
        /// 与目标的异或<br/>
        /// (“this”与“other”)的并集 与 (“this”与“other”)的交集的补集 的交集<br/>
        /// (this U other) ∩ ~(this ∩ other)
        /// </summary>
        public void SymmetricExceptWith(IEnumerable<T> other) => this.SymmetricExceptWithLowGC(other);

        /// <summary>
        /// 是目标的子集<br/>
        /// “this”是“other”的子集<br/>
        /// this ⊆ other
        /// </summary>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _collection.IsSubsetOfLowGC(other);
        }
        /// <summary>
        /// 是目标的超集（父集）
        /// “this”是“other”的超集（父集）<br/>
        /// this ⊇ other
        /// </summary>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _collection.IsSupersetOfLowGC(other);
        }
        /// <summary>
        /// 是目标的真子集<br/>
        /// “this”是“other”的真子集<br/>
        /// this ⊊ other
        /// </summary>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _collection.IsProperSubsetOfLowGC(other);
        }
        /// <summary>
        /// 是目标的真超集（父集）
        /// “this”是“other”的真超集（真父集）<br/>
        /// this ⊋ other
        /// </summary>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _collection.IsProperSupersetOfLowGC(other);
        }

        /// <summary>
        /// 与目标重叠
        /// </summary>
        public bool Overlaps(IEnumerable<T> other)
        {
            CheckCount();
            return _collection.OverlapsLowGC(other);
        }
        /// <summary>
        /// 与目标相同
        /// </summary>
        public bool SetEquals(IEnumerable<T> other)
        {
            CheckCount();
            return _collection.SetEqualsLowGC(other);
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
        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item) => Add(item);
        public bool Remove(T item)
        {
            CheckCount();
            if (!_collection.Remove(item))
                return false;

            _order.Remove(item);
            CheckCount();
            return true;
        }
        public void Clear()
        {
            CheckCount();
            _collection.Clear();
            _order.Clear();
            CheckCount();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            CheckCount();
            return _collection.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            CheckCount();
            _order.CopyTo(array, arrayIndex);
        }
        #endregion

        #region IReadOnlyList`1
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckCount();
                return _order[index];
            }
        }
        #endregion

        #region ICollection
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        void ICollection.CopyTo(Array array, int index)
        {
            CheckCount();
            ((ICollection)_order).CopyTo(array, index);
        }
        #endregion

        #region Enumerator
        public WeakOrderList<T>.Enumerator GetEnumerator()
        {
            CheckCount();
            return _order.GetEnumerator();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            CheckCount();
            return ((IEnumerable<T>)_order).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            CheckCount();
            return ((IEnumerable)_order).GetEnumerator();
        }
        #endregion

        #region Public
        public IEqualityComparer<T> Comparer => _collection.Comparer;

        public void CopyTo(T[] array)
        {
            CheckCount();
            _order.CopyTo(array);
        }
        public void CopyTo(T[] array, int index, int count)
        {
            CheckCount();
            _order.CopyTo(0, array, index, count);
        }
        public void TrimExcess()
        {
            CheckCount();
            _collection.TrimExcess();
            _order.Capacity = Count;
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

            return _collection.SetEqualsLowGC(_order);
        }
        public ReadOnlySpan<T> AsSpan()
        {
            CheckCount();
            return _order.AsReadOnlySpan();
        }
        #endregion

        #region Private
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckComparer(IEqualityComparer<T> comparer = null)
        {
            if (comparer == null || comparer == EqualityComparer<T>.Default)
                Assert.Convert<ArgumentException, AssertArgs<object>, T, IEquatable<T>>(nameof(comparer), "T:{0} 未继承 IEquatable<T>", new AssertArgs<object>(typeof(T)));
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCount() => Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_collection.Count, _order.Count, nameof(_collection.Count), "Count fail, _data.Count:{0} != _order.Count:{1}", new AssertArgs<int, int>(_collection.Count, _order.Count));
        #endregion
    }
}