using Eevee.Diagnosis;
using System;
using System.Collections.Generic;

namespace Eevee.Pool
{
    /// <summary>
    /// 基于委托实现的对象池
    /// </summary>
    public sealed class ObjectDelPool<T> where T : class
    {
        private Stack<T> _pool;
        private readonly Func<T> _onCreate;
        private readonly Action<T> _onAlloc;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        public bool ReleaseCheck;
        public int MaxCount;

        public ObjectDelPool(Func<T> onCreate, Action<T> onAlloc, Action<T> onRelease, Action<T> onDestroy, bool releaseCheck = true, int maxCount = 100)
        {
            Assert.NotNull<ArgumentNullException, AssertArgs>(onCreate, nameof(onCreate), "onCreate is null");
            Assert.Greater<ArgumentException, AssertArgs<int>, int>(maxCount, 0, nameof(maxCount), "Max Size must be greater than 0, value is {0}", new AssertArgs<int>(maxCount));

            _onCreate = onCreate;
            _onAlloc = onAlloc;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            ReleaseCheck = releaseCheck;
            MaxCount = maxCount;
        }
        public T Alloc()
        {
            T obj = null;
            bool success = _pool != null && _pool.TryPop(out obj);
            var newObj = success ? obj : _onCreate();
            _onAlloc?.Invoke(newObj);
            return newObj;
        }
        public void Release(ref T element)
        {
            Release(element);
            element = null;
        }
        public void Release(T element)
        {
            Assert.NotNull<ArgumentNullException, AssertArgs>(element, nameof(element), "element is null");

            if (ReleaseCheck && _pool != null && _pool.Contains(element))
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");

            _pool ??= new Stack<T>();
            _onRelease?.Invoke(element);

            if (_pool.Count < MaxCount)
                _pool.Push(element);
            else
                _onDestroy?.Invoke(element);
        }
        public void Clear()
        {
            if (_pool != null && _onDestroy != null)
                foreach (var obj in _pool)
                    _onDestroy(obj);

            _pool?.Clear();
        }
    }
}