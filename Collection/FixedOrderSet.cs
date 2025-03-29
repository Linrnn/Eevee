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
        private readonly HashSet<T> _data;
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField] private WeakOrderList<T> _order;
#else
        private readonly WeakOrderList<T> _order;
#endif

        static FixedOrderSet() => Assert.Convert<ArgumentException, AssertArgs<object>, T, IEquatable<T>>(nameof(T), "T:{0} 未继承 IEquatable<T>", new AssertArgs<object>(typeof(T)));
        public FixedOrderSet() : this(0, EqualityComparer<T>.Default) { }
        public FixedOrderSet(int capacity) : this(capacity, EqualityComparer<T>.Default) { }
        public FixedOrderSet(IEqualityComparer<T> comparer) : this(0, comparer) { }
        public FixedOrderSet(IEnumerable<T> enumerator) : this(enumerator, EqualityComparer<T>.Default) { }
        public FixedOrderSet(int capacity, IEqualityComparer<T> comparer)
        {
            _data = new HashSet<T>(capacity, comparer);
            _order = new WeakOrderList<T>(capacity);
        }
        public FixedOrderSet(IEnumerable<T> enumerator, IEqualityComparer<T> comparer)
        {
            var set = new HashSet<T>(comparer);
            var list = new WeakOrderList<T>();

            set.UnionWith0GC(enumerator);
            list.AddRange0GC(enumerator);

            _data = set;
            _order = list;
        }
        #endregion

        #region ISet`1
        public bool Add(T item)
        {
            CheckCount();
            if (!_data.Add(item))
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
        public void UnionWith(IEnumerable<T> other) => this.UnionWith0GC(other);
        /// <summary>
        /// 与目标的交集<br/>
        /// “this”与“other”的交集<br/>
        /// this ∩ other
        /// </summary>
        public void IntersectWith(IEnumerable<T> other) => this.IntersectWith0GC(other);
        /// <summary>
        /// 与目标的差集<br/>
        /// “this”与“other”的补集 的交集<br/>
        /// this ∩ ~other
        /// </summary>
        public void ExceptWith(IEnumerable<T> other) => this.ExceptWith0GC(other);
        /// <summary>
        /// 与目标的异或<br/>
        /// (“this”与“other”)的并集 与 (“this”与“other”)的交集的补集 的交集<br/>
        /// (this U other) ∩ ~(this ∩ other)
        /// </summary>
        public void SymmetricExceptWith(IEnumerable<T> other) => this.SymmetricExceptWith0GC(other);

        /// <summary>
        /// 是目标的子集<br/>
        /// “this”是“other”的子集<br/>
        /// this ⊆ other
        /// </summary>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            CheckCount();
            return this.IsSubsetOf0GC(other);
        }
        /// <summary>
        /// 是目标的超集（父集）
        /// “this”是“other”的超集（父集）<br/>
        /// this ⊇ other
        /// </summary>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsSupersetOf0GC(_order);
        }
        /// <summary>
        /// 是目标的真子集<br/>
        /// “this”是“other”的真子集<br/>
        /// this ⊊ other
        /// </summary>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsProperSubsetOf0GC(_order);
        }
        /// <summary>
        /// 是目标的真超集（父集）
        /// “this”是“other”的真超集（真父集）<br/>
        /// this ⊋ other
        /// </summary>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsProperSupersetOf0GC(_order);
        }

        /// <summary>
        /// 与目标重叠
        /// </summary>
        public bool Overlaps(IEnumerable<T> other)
        {
            CheckCount();
            return _data.Overlaps0GC(_order);
        }
        /// <summary>
        /// 与目标相同
        /// </summary>
        public bool SetEquals(IEnumerable<T> other)
        {
            CheckCount();
            return _data.SetEquals0GC(_order);
        }
        #endregion

        #region ICollection`1
        public int Count
        {
            get
            {
                CheckCount();
                return _data.Count;
            }
        }
        public bool IsReadOnly => false;

        void ICollection<T>.Add(T item) => Add(item);
        public bool Remove(T item)
        {
            CheckCount();
            if (!_data.Remove(item))
                return false;

            _order.Remove(item);
            CheckCount();
            return true;
        }
        public void Clear()
        {
            CheckCount();
            _data.Clear();
            _order.Clear();
        }

        public bool Contains(T item)
        {
            CheckCount();
            return _data.Contains(item);
        }
        public void CopyTo(T[] array, int index, int arrayIndex)
        {
            CheckCount();
            _order.CopyTo(array, index, arrayIndex);
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

        void ICollection.CopyTo(Array array, int index) => ((ICollection)_order).CopyTo(array, index);
        #endregion

        #region Enumerator
        public WeakOrderList<T>.Enumerator GetEnumerator()
        {
            CheckCount();
            return new WeakOrderList<T>.Enumerator(_order);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region Extractable
        public void CopyTo(T[] array, int index = 0)
        {
            CheckCount();
            _order.CopyTo(array, index);
        }
        public ReadOnlySpan<T> AsSpan()
        {
            CheckCount();
            return _order.AsSpan();
        }
        #endregion

        #region private
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCount() => Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_data.Count, _order.Count, "Count", "Count fail, _data.Count:{0} != _order.Count:{1}", new AssertArgs<int, int>(_data.Count, _order.Count));
        #endregion
    }
}