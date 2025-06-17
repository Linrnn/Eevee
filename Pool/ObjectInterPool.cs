using Eevee.Diagnosis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Pool
{
    /// <summary>
    /// 基于接口实现的对象池
    /// </summary>
    public sealed class ObjectInterPool<T> : IObjectPool<T> where T : class, new()
    {
        private Stack<T> _pool;
        public bool ReleaseCheck;
        public int Capacity;
        private int _countRef;
        private int _historyCapacity; // 历史容量

        public int CountRef
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _countRef;
        }
        public int HistoryCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _historyCapacity;
        }

        public ObjectInterPool(bool releaseCheck = true, int capacity = 100)
        {
            Assert.Greater<ArgumentException, AssertArgs<int>, int>(capacity, 0, nameof(capacity), "Max Size must be greater than 0, value is {0}", new AssertArgs<int>(capacity));

            ReleaseCheck = releaseCheck;
            Capacity = capacity;
        }
        public void TryAlloc(ref T element) => element ??= Alloc();
        public T Alloc()
        {
            T obj = null;
            bool success = _pool is not null && _pool.TryPop(out obj);

            T newObj;
            if (success)
                newObj = obj;
            else
                ((newObj = new T()) as IObjectCreate)?.OnCreate();
            (newObj as IObjectAlloc)?.OnAlloc();

            --_countRef;
            return newObj;
        }
        public void TryRelease(ref T element)
        {
            if (element is null)
                return;
            Release(element);
            element = null;
        }
        public void Release(T element)
        {
            Assert.NotNull<ArgumentNullException, AssertArgs>(element, nameof(element), "element is null");
            if (ReleaseCheck && _pool is not null && _pool.Contains(element))
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");

            _pool ??= new Stack<T>();
            (element as IObjectRelease)?.OnRelease();
            if (_pool.Count < Capacity)
                _pool.Push(element);
            else
                (element as IObjectDestroy)?.OnDestroy();

            ++_countRef;
            _historyCapacity = Math.Max(_countRef, _historyCapacity);
        }
        public void Clean()
        {
            if (_pool is not null && typeof(IObjectDestroy).IsAssignableFrom(typeof(T)))
                foreach (var obj in _pool)
                    (obj as IObjectDestroy)?.OnDestroy();
            _pool?.Clear();
        }
    }
}