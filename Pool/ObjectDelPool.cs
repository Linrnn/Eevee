using Eevee.Diagnosis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Pool
{
    /// <summary>
    /// 基于委托实现的对象池
    /// </summary>
    public sealed class ObjectDelPool<T> : IObjectPool<T> where T : class
    {
        private Stack<T> _pool;
        private readonly Func<T> _onCreate;
        private readonly Action<T> _onAlloc;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
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

        public ObjectDelPool(Func<T> onCreate, Action<T> onAlloc, Action<T> onRelease, Action<T> onDestroy, bool releaseCheck = true, int capacity = 100)
        {
            Assert.NotNull<ArgumentNullException, AssertArgs>(onCreate, nameof(onCreate), "onCreate is null");
            Assert.Greater<ArgumentException, AssertArgs<int>, int>(capacity, 0, nameof(capacity), "Max Size must be greater than 0, value is {0}", new AssertArgs<int>(capacity));

            _onCreate = onCreate;
            _onAlloc = onAlloc;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            ReleaseCheck = releaseCheck;
            Capacity = capacity;
            _historyCapacity = capacity;
        }
        public void TryAlloc(ref T element) => element ??= Alloc();
        public T Alloc()
        {
            T obj = null;
            bool success = _pool is not null && _pool.TryPop(out obj);

            var newObj = success ? obj : _onCreate();
            _onAlloc?.Invoke(newObj);

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
            _onRelease?.Invoke(element);
            if (_pool.Count < Capacity)
                _pool.Push(element);
            else
                _onDestroy?.Invoke(element);

            ++_countRef;
            _historyCapacity = Math.Max(_countRef, _historyCapacity);
        }
        public void Clean()
        {
            if (_pool is not null && _onDestroy is not null)
                foreach (var obj in _pool)
                    _onDestroy(obj);
            _pool?.Clear();
        }
    }
}