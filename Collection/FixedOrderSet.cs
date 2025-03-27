using Eevee.Define;
using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Eevee.Collection
{
    /// <summary>
    /// 确定性顺序的集合
    /// </summary>
    public sealed class FixedOrderSet<T> : ISet<T>, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
    {
        #region Field/Constructor
        private readonly HashSet<T> _data;
        private readonly WeakOrderList<T> _order;

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

            // ReSharper disable PossibleMultipleEnumeration
            set.UnionWith0GC(enumerator);
            list.AddRange0GC(enumerator);
            // ReSharper restore PossibleMultipleEnumeration

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
        /// 合并另外一张Set的内容(并集)<br/>
        /// “this”集合 与 “target”集合 的并集<br/>
        /// this U target
        /// </summary>
        public void UnionWith(IEnumerable<T> other) => this.UnionWith0GC(other);
        /// <summary>
        /// 保留与指定对象的交集<br/>
        /// “this”集合 与 “target”集合 的交集<br/>
        /// this ∩ target
        /// </summary>
        public void IntersectWith(IEnumerable<T> other) => this.IntersectWith0GC(other);
        /// <summary>
        /// 计算与指定对象的 差集<br/>
        /// “this”集合 与 “target”集合补集 的交集<br/>
        /// this ∩ ~target
        /// </summary>
        public void ExceptWith(IEnumerable<T> other) => this.ExceptWith0GC(other);
        /// <summary>
        /// 计算与目标集合的异或结果<br/>
        /// (“this”集合与“target”集合)的并集 与 (“this”集合与“target”集合)的交集的补集 的交集<br/>
        /// (this U target) ∩ ~(this ∩ target)
        /// </summary>
        public void SymmetricExceptWith(IEnumerable<T> other) => this.SymmetricExceptWith0GC(other);
        #endregion

        #region 显示接口实现
        bool ICollection<T>.IsReadOnly => false;

        // ReSharper disable once AssignNullToNotNullAttribute
        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        #endregion

        #region 公有属性/方法
        public int Count
        {
            get
            {
                CheckCount();
                return _data.Count;
            }
        }

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

        public void CopyTo(T[] array, int index = 0)
        {
            CheckCount();
            _order.CopyTo(array, index);
        }
        public void CopyTo(T[] array, int index, int length)
        {
            CheckCount();
            _order.CopyTo(array, index, length);
        }

        public WeakOrderList<T>.Enumerator GetEnumerator()
        {
            CheckCount();
            return new WeakOrderList<T>.Enumerator(_order);
        }

        /// <summary>
        /// 判断是否为目标的 子集
        /// </summary>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsSubsetOf(_order);
        }

        /// <summary>
        /// 是否为目标的 真子集
        /// </summary>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsProperSubsetOf(_order);
        }

        /// <summary>
        /// 是否为目标的 超集(父集)
        /// </summary>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsSupersetOf(_order);
        }

        /// <summary>
        /// 是否为目标的 真超集
        /// </summary>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            CheckCount();
            return _data.IsProperSupersetOf(_order);
        }

        /// <summary>
        /// 判断是否有重叠元素
        /// </summary>
        public bool Overlaps(IEnumerable<T> other)
        {
            CheckCount();
            return _data.Overlaps(_order);
        }

        /// <summary>
        /// 判断是否与目标集合相同
        /// </summary>
        public bool SetEquals(IEnumerable<T> other)
        {
            CheckCount();
            return _data.SetEquals(_order);
        }
        #endregion

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCount() => Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(_data.Count, _order.Count, "Count", "Count fail, _data.Count:{0} != _order.Count:{1}", new AssertArgs<int, int>(_data.Count, _order.Count));

        public void OnDeserialization(object sender)
        {
            throw new NotImplementedException();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}