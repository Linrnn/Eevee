using Eevee.Diagnosis;
using System;
using System.Collections.Generic;

namespace Eevee.Pool
{
    /// <summary>
    /// 基于接口实现的对象池
    /// </summary>
    public sealed class ObjectInterPool<T> where T : class, new()
    {
        private Stack<T> _pool;
        public bool ReleaseCheck;
        public int MaxCount;

        public ObjectInterPool(bool releaseCheck = true, int maxCount = 100)
        {
            Assert.Greater<ArgumentException, AssertArgs<int>, int>(maxCount, 0, nameof(maxCount), "Max Size must be greater than 0, value is {0}", new AssertArgs<int>(maxCount));

            ReleaseCheck = releaseCheck;
            MaxCount = maxCount;
        }
        public T Alloc()
        {
            T obj = null;
            bool success = _pool is not null && _pool.TryPop(out obj);
            T newObj;
            if (success)
            {
                newObj = obj;
            }
            else
            {
                newObj = new T();
                (newObj as IObjectCreate)?.OnCreate();
            }

            (newObj as IObjectAlloc)?.OnAlloc();
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

            if (ReleaseCheck && _pool is not null && _pool.Contains(element))
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");

            _pool ??= new Stack<T>();
            (element as IObjectRelease)?.OnRelease();

            if (_pool.Count < MaxCount)
                _pool.Push(element);
            else
                (element as IObjectDestroy)?.OnDestroy();
        }
        public void Clear()
        {
            if (_pool is not null && typeof(IObjectDestroy).IsAssignableFrom(typeof(T)))
                foreach (var obj in _pool)
                    (obj as IObjectDestroy)?.OnDestroy();

            _pool?.Clear();
        }
    }
}