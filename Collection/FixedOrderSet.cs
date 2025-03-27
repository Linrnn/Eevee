using Eevee.Fixed;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eevee.Collection
{
    /// <summary>
    /// 确定性顺序的集合
    /// </summary>
    public sealed class FixedOrderSet<T> : ISet<T>, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
    {
        #region 迭代器
        internal sealed class Iterator
        {
            internal FixedOrderSet<T> Context;
            internal int Cursor;
            internal bool IsKill;
            internal bool IsBusy;
            internal T Current;

            internal List<T> Orders => Context._orders;

            internal Iterator() => Reset();
            internal Iterator(FixedOrderSet<T> context)
            {
                Context = context;
                Reset();
            }

            internal bool IsBegin() => IsKill || Cursor < 0;
            internal bool IsEnd() => IsKill || Cursor >= Orders.Count;

            internal void Reset()
            {
                Cursor = -1;
                IsKill = false;
                IsBusy = false;
                Current = default;
            }
            internal bool MoveNext()
            {
                Current = default;
                ++Cursor;

                while (!IsEnd())
                {
                    if (Cursor < Orders.Count)
                    {
                        Current = Orders[Cursor];
                        return true;
                    }

                    ++Cursor;
                }

                return false;
            }
        }

        internal List<Iterator> Iterators;

        public sealed class Enumerator : IEnumerator<T>
        {
            internal Iterator Target;
            internal bool IsBusy;
            public T Current => Target.Current;
            object IEnumerator.Current => Target.Current;

            internal Enumerator() { }
            internal Enumerator(Iterator iterator) => ResetEnumerator(iterator);

            internal void ResetEnumerator(Iterator iterator)
            {
                Target = iterator;
                Target.Context.Iterators ??= new List<Iterator>();
                Target.Context.Iterators.Add(Target);
            }
            public void Dispose()
            {
                Target.IsBusy = false;
                Target.Context.Iterators.Remove(Target);
                IsBusy = false;
            }
            public bool MoveNext() => Target.MoveNext();
            public void Reset()
            {
                Target.Reset();
                IsBusy = false;
            }
        }
        #endregion

        #region 数据类型
        internal struct Slot
        {
            /// <summary> hash值，-1表示当前位置为空 </summary>
            internal int HashCode;
            /// <summary> 下一个节点位置，-1表示为最后一个节点 </summary>
            internal int Next;
            /// <summary> 存放的数值 </summary>
            internal T Value;

            internal Slot(int hashCode)
            {
                HashCode = hashCode;
                Next = 0;
                Value = default;
            }
        }
        #endregion

        #region 私有属性/方法
        private const int IntMax = 0x7FFFFFFF;
        private const int AutoTrim = 3; // 当空间长度/实际使用长度大于此值时自动释放多余空间

        private int[] _buckets; // Hash桶列表，根据数据的Hash值对桶列表长度求余之后存放在不同的桶内(桶内数据构成单向链表)
        private Slot[] _slots; // 数据存放数组，移除数据只改变 hashCode(置为空闲状态)，不释放空间
        private int _count; // 当前使用的数组实际长度
        private int _freeIndex; // 当前空闲的位置
        private int _freeCount; // 当前空闲的位置数量
        private readonly IEqualityComparer<T> _comparer; // 等价比较器，以此数据的HashCode以及进行等价比较
        private readonly List<T> _orders; // 顺序列表，用于确定固定顺序

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize(int capacity)
        {
            int size = Prime.GetNumber(capacity);

            _buckets = new int[size];
            for (int i = 0; i < _buckets.Length; ++i)
                _buckets[i] = -1;

            _slots = new Slot[size];
            for (int i = 0; i < _slots.Length; ++i)
                _slots[i] = new Slot(-1);

            _freeIndex = -1;
        }
        /// <summary>
        /// 重新设置容量大小
        /// </summary>
        private void Resize(int newSize, bool forceNewHashCodes)
        {
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; ++i)
                newBuckets[i] = -1;

            var newSlots = new Slot[newSize];
            Array.Copy(_slots, 0, newSlots, 0, _count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < _count; i++)
                {
                    var slot = newSlots[i];
                    if (slot.HashCode != -1)
                        newSlots[i].HashCode = GetHashCode(slot.Value);
                }
            }

            for (int i = 0; i < _count; ++i)
            {
                var slot = newSlots[i];
                if (slot.HashCode < 0)
                    continue;

                int bucket = slot.HashCode % newSize;
                newSlots[i].Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            _buckets = newBuckets;
            _slots = newSlots;
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        private bool Contain(T item, int bucketIdx, int hashCode)
        {
            for (int i = _buckets[bucketIdx]; i >= 0; i = _slots[i].Next)
                if (_slots[i].HashCode == hashCode && _comparer.Equals(_slots[i].Value, item))
                    return true;
            return false;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        private bool Insert(T value)
        {
            if (_buckets == null)
                Initialize(0);

            int hashCode = GetHashCode(value);
            int bucketIdx = hashCode % _buckets.Length;

            if (Contain(value, bucketIdx, hashCode))
                return false;

            int index;
            if (_freeCount > 0)
            {
                index = _freeIndex;
                _freeIndex = _slots[index].Next;
                --_freeCount;
            }
            else
            {
                if (_count == _slots.Length)
                {
                    Resize(Prime.Expand(_count), false);
                    bucketIdx = hashCode % _buckets.Length;
                }

                index = _count;
                ++_count;
            }

            _slots[index].HashCode = hashCode;
            _slots[index].Next = _buckets[bucketIdx];
            _slots[index].Value = value;
            _buckets[bucketIdx] = index;
            _orders.Add(value);
            return true;
        }

        /// <summary>
        /// 获取目标的HashCode
        /// </summary>
        private int GetHashCode(T item) => item == null ? 0 : _comparer.GetHashCode(item) & IntMax;

        /// <summary>
        /// 获取元素位置
        /// </summary>
        private int IndexOf(T item)
        {
            if (_buckets == null)
                return -1;

            int hashCode = GetHashCode(item);
            for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _slots[i].Next)
                if (_slots[i].HashCode == hashCode && _comparer.Equals(_slots[i].Value, item))
                    return i;

            return -1;
        }

        /// <summary>
        /// 判断是否为目标的子集
        /// </summary>
        private bool IsSubsetOfTarget(FixedOrderSet<T> target)
        {
            using var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
                if (!target.Contains(enumerator.Current))
                    return false;
            return true;
        }

        /// <summary>
        /// 与目标集合进行对比
        /// </summary>
        /// <param name="other">目标集合</param>
        /// <param name="returnIfUnFound">是否一遇到本集合不存在的元素就返回</param>
        /// <param name="sameCount">两个集合相同元素的数量</param>
        /// <param name="unFoundCount">本集合中不存在的元素数量</param>
        private void CheckElements(IEnumerable<T> other, bool returnIfUnFound, out int sameCount, out int unFoundCount)
        {
            sameCount = 0;
            unFoundCount = 0;
            int count = Count;
            if (count == 0)
            {
                // break right away, all we want to know is whether other has 0 or 1 elements <==== 注释来源于.Net源码
                using var enumerator = other.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ++unFoundCount;
                    break;
                }
            }
            else
            {
                var list = new List<int>(count);
                using var enumerator = other.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    int idx = IndexOf(enumerator.Current);
                    if (idx < 0)
                    {
                        ++unFoundCount;
                        if (returnIfUnFound)
                            break;
                        continue;
                    }

                    if (list.Contains(idx))
                        continue;

                    ++sameCount;
                    list.Add(idx);
                }
            }
        }

        /// <summary>
        /// 是否包含目标集合的所有元素
        /// </summary>
        private bool ContainsAllElements(IEnumerable<T> other)
        {
            using var enumerator = other.GetEnumerator();
            while (enumerator.MoveNext())
                if (!Contains(enumerator.Current))
                    return false;
            return true;
        }

        /// <summary>
        /// 从目标集合拷贝数据到当前集合
        /// </summary>
        private void CopyFrom(FixedOrderSet<T> source)
        {
            int count = source.Count;
            if (count == 0)
                return;

            int capacity = source._buckets.Length;
            int threshold = Prime.Expand(count + 1);

            if (threshold >= capacity)
            {
                _buckets = (int[])source._buckets.Clone();
                _slots = (Slot[])source._slots.Clone();
                _freeIndex = source._freeIndex;
                _freeCount = source._freeCount;
                _count = source._count;
                return;
            }

            Initialize(count);
            int idx = 0;

            for (int i = 0; i < source._count; ++i)
            {
                var slot = source._slots[i];
                if (slot.HashCode < 0)
                    continue;

                int bucketIdx = slot.HashCode % _buckets.Length;
                _slots[idx].HashCode = slot.HashCode;
                _slots[idx].Value = slot.Value;
                _slots[idx].Next = _buckets[bucketIdx];
                _buckets[bucketIdx] = idx;
                ++idx;
            }

            _count = idx;
        }
        #endregion

        #region 显示接口实现
        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item) => Insert(item);
        bool ISet<T>.Add(T item) => Insert(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        #endregion

        #region 公有属性/方法
        public int Count => _count - _freeCount;

        public FixedOrderSet() : this(0, EqualityComparer<T>.Default) { }
        public FixedOrderSet(int capacity) : this(capacity, EqualityComparer<T>.Default) { }
        public FixedOrderSet(IEqualityComparer<T> comparer) : this(0, comparer) { }
        public FixedOrderSet(IEnumerable<T> enumerator) : this(enumerator, EqualityComparer<T>.Default) { }
        public FixedOrderSet(int capacity, IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _count = 0;
            _freeCount = 0;
            _freeIndex = -1;

            if (capacity > 0)
                Initialize(capacity);
            _orders = new List<T>(capacity);
        }
        public FixedOrderSet(IEnumerable<T> enumerator, IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            if (enumerator is FixedOrderSet<T> set && _comparer.Equals(set._comparer))
            {
                CopyFrom(set);
                return;
            }

            var collection = enumerator as ICollection<T>;
            int capacity = collection?.Count ?? 0;
            Initialize(capacity);
            _orders = new List<T>(capacity);
            if (collection != null)
                UnionWith(collection);

            int count = Count;
            if (count > 0 && _slots.Length / count > AutoTrim)
                TrimExcess();
        }

        public bool Add(T item)
        {
            return Insert(item);
        }
        public bool Remove(T item)
        {
            if (_buckets == null || _count <= 0)
                return false;

            int hashCode = GetHashCode(item);
            int bucketIdx = hashCode % _buckets.Length;

            for (int i = _buckets[bucketIdx], prevIdx = -1; i >= 0; prevIdx = i, i = _slots[i].Next)
            {
                var slot = _slots[i];
                if (!(slot.HashCode == hashCode && _comparer.Equals(slot.Value, item)))
                    continue;

                // 如果目标在单链表中间位置，则更新链表指针
                if (prevIdx < 0)
                    _buckets[bucketIdx] = slot.Next;
                else
                    _slots[prevIdx].Next = slot.Next;

                _slots[i].HashCode = -1;
                _slots[i].Value = default;
                _slots[i].Next = _freeIndex;
                _freeIndex = i;
                ++_freeCount;
                _orders.Remove(item);
                return true;
            }
            return false;
        }
        public void Clear()
        {
            if (_count <= 0)
                return;

            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = -1;

            Array.Clear(_slots, 0, _count);
            _freeIndex = -1;
            _count = 0;
            _freeCount = 0;
            _orders.Clear();
        }

        public bool Contains(T item)
        {
            if (_buckets == null)
                return false;

            int hashCode = GetHashCode(item);
            return Contain(item, hashCode % _buckets.Length, hashCode);
        }

        public void CopyTo(T[] array) => CopyTo(array, 0, Count);
        public void CopyTo(T[] array, int index) => CopyTo(array, index, Count);
        public void CopyTo(T[] array, int index, int length)
        {
            for (int i = 0, offset = 0; i < _count && offset < length; ++i)
                array[index + offset++] = _orders[i];
        }

        private readonly Iterator _iterator = new();
        private Iterator GetIterator()
        {
            if (_iterator.IsBusy)
                return new Iterator
                {
                    Context = this,
                    IsBusy = true,
                };

            _iterator.Reset();
            _iterator.Context = this;
            _iterator.IsBusy = true;
            return _iterator;
        }

        private readonly Enumerator _enumerator = new Enumerator();
        public Enumerator GetEnumerator()
        {
            if (_enumerator.IsBusy)
            {
                var enumerator = new Enumerator(GetIterator());
                enumerator.IsBusy = true;
                return enumerator;
            }

            _enumerator.ResetEnumerator(GetIterator());
            _enumerator.IsBusy = true;
            return _enumerator;
        }

        public T[] ToArray()
        {
            int count = Count;
            var array = new T[count];
            CopyTo(array, 0, count);
            return array;
        }

        /// <summary>
        /// 合并另外一张Set的内容(并集)<br/>
        /// “this”集合 与 “target”集合 的并集<br/>
        /// this U target
        /// </summary>
        public void UnionWith(IEnumerable<T> other)
        {
            using var enumerator = other.GetEnumerator();
            while (enumerator.MoveNext())
                Insert(enumerator.Current);
        }

        /// <summary>
        /// 保留与指定对象的交集<br/>
        /// “this”集合 与 “target”集合 的交集<br/>
        /// this ∩ target
        /// </summary>
        public void IntersectWith(IEnumerable<T> other)
        {
            int count = Count;
            if (count == 0)
                return;

            if (other is ICollection<T> collection)
            {
                if (collection.Count == 0)
                {
                    Clear();
                    return;
                }

                if (other is FixedOrderSet<T> set && _comparer.Equals(set._comparer))
                {
                    for (int i = 0; i < _count; ++i)
                        if (_slots[i].HashCode >= 0 && !set.Contains(_slots[i].Value))
                            Remove(_slots[i].Value);
                    return;
                }
            }

            var list = new List<T>(count);
            using (var enumerator = other.GetEnumerator())
                while (enumerator.MoveNext())
                    if (IndexOf(enumerator.Current) >= 0)
                        list.Add(enumerator.Current);

            for (int i = 0; i < _count; ++i)
                if (_slots[i].HashCode >= 0 && !list.Contains(_slots[i].Value))
                    Remove(_slots[i].Value);
        }

        /// <summary>
        /// 计算与指定对象的 差集<br/>
        /// “this”集合 与 “target”集合补集 的交集<br/>
        /// this ∩ ~target
        /// </summary>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (Count == 0)
                return;

            if (ReferenceEquals(other, this))
            {
                Clear();
                return;
            }

            using var enumerator = other.GetEnumerator();
            while (enumerator.MoveNext())
                Remove(enumerator.Current);
        }

        /// <summary>
        /// 判断是否为目标的 子集
        /// </summary>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            int count = Count;
            if (count == 0)
                return true;

            if (other is FixedOrderSet<T> set && _comparer.Equals(set._comparer))
                return count <= set.Count && IsSubsetOfTarget(set);

            CheckElements(other, false, out int sameCount, out int _);
            return sameCount == count;
        }

        /// <summary>
        /// 是否为目标的 真子集
        /// </summary>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            int count = Count;
            if (other is ICollection<T> collection)
            {
                if (count == 0)
                    return collection.Count > 0;

                if (other is FixedOrderSet<T> set && _comparer.Equals(set._comparer))
                    return count < set.Count && IsSubsetOfTarget(set);
            }

            CheckElements(other, false, out int sameCount, out int unFoundCount);
            return sameCount == count && unFoundCount > 0;
        }

        /// <summary>
        /// 是否为目标的 超集(父集)
        /// </summary>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other is not ICollection<T> collection)
                return ContainsAllElements(other);

            if (collection.Count == 0)
                return true;

            if (other is FixedOrderSet<T> set && _comparer.Equals(set._comparer) && set.Count > Count)
                return false;

            return ContainsAllElements(other);
        }

        /// <summary>
        /// 是否为目标的 真超集
        /// </summary>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int count = Count;
            if (count == 0)
                return false;

            if (other is ICollection<T> collection)
            {
                if (collection.Count == 0)
                    return true;

                var set = other as FixedOrderSet<T>;
                if (set != null && _comparer.Equals(set._comparer) && set.Count >= count)
                    return false;

                return ContainsAllElements(set);
            }

            CheckElements(other, false, out int sameCount, out int unFoundCount);
            return sameCount < count && unFoundCount == 0;
        }

        /// <summary>
        /// 判断是否有重叠元素
        /// </summary>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (Count == 0)
                return false;

            using var enumerator = other.GetEnumerator();
            while (enumerator.MoveNext())
                if (Contains(enumerator.Current))
                    return true;

            return false;
        }

        /// <summary>
        /// 判断是否与目标集合相同
        /// </summary>
        public bool SetEquals(IEnumerable<T> other)
        {
            int count = Count;
            if (other is FixedOrderSet<T> set && _comparer.Equals(set._comparer))
                return count == set.Count && ContainsAllElements(set);

            if (count == 0 && other is ICollection<T> { Count: > 0 })
                return false;

            CheckElements(other, false, out int sameCount, out int unFoundCount);
            return sameCount == count && unFoundCount == 0;
        }

        /// <summary>
        /// 计算与目标集合的异或结果<br/>
        /// (“this”集合与“target”集合)的并集 与 (“this”集合与“target”集合)的交集的补集 的交集<br/>
        /// (this U target) ∩ ~(this ∩ target)
        /// </summary>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (Count == 0)
            {
                UnionWith(other);
                return;
            }

            if (ReferenceEquals(other, this))
            {
                Clear();
                return;
            }

            if (_comparer.Equals(EqualityComparer<T>.Default))
            {
                var set = new FixedOrderSet<T>(other);
                using var enumerator = set.GetEnumerator();
                while (enumerator.MoveNext()) // 尝试移除，存在则移除成功，否则添加
                    if (!Remove(enumerator.Current))
                        Insert(enumerator.Current);
            }
            else
            {
                using var enumerator = other.GetEnumerator();
                while (enumerator.MoveNext())
                    if (!Remove(enumerator.Current))
                        Insert(enumerator.Current);
            }
        }

        /// <summary>
        /// 释放多余空间
        /// </summary>
        public void TrimExcess()
        {
            int count = Count;
            if (count == 0)
            {
                _buckets = null;
                _slots = null;
                return;
            }

            int newSize = Prime.GetNumber(count);
            if (_slots.Length <= newSize)
                return;

            var newSlots = new Slot[newSize];
            int[] newBuckets = new int[newSize];

            int newIdx = 0;
            for (int i = 0; i < _count; i++)
            {
                var slot = _slots[i];
                if (slot.HashCode < 0)
                    continue;

                int bucketId = slot.HashCode % newSize;
                slot.Next = newBuckets[bucketId] - 1;

                newSlots[newIdx] = slot;
                newBuckets[bucketId] = newIdx;
                ++newIdx;
            }

            _count = newIdx;
            _slots = newSlots;
            _buckets = newBuckets;
            _freeCount = 0;
            _freeIndex = -1;
        }
        #endregion

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