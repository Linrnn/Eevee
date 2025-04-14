using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Eevee.Pool
{
    public readonly struct ICollectionPool<TCollection> where TCollection : class, new()
    {
        public static TCollection Alloc()
        {
            if (ICollectionPool.Value(typeof(TCollection))?.GetPool<TCollection>() is { } pool && pool.TryPop(out var collection))
                return collection;
            return new TCollection();
        }
        public static void Release(TCollection collection)
        {
            var value = ICollectionPool.Value(typeof(TCollection));
            Assert.False<InvalidOperationException, AssertArgs<Type, int>>(value?.GetPool<TCollection>()?.Contains(collection) ?? false, nameof(collection), "Pools is Contains, Type:{0}, HashCode:{1}", new AssertArgs<Type, int>(collection.GetType(), collection.GetHashCode()));

            var newValue = value ?? ICollectionPool.SetMaxCount<TCollection>(ICollectionPoolValue.ConstMaxCount);
            if (!newValue.IsFull())
                newValue.GetPool<TCollection>().Push(collection);
        }
    }

    public readonly struct ICollectionPool
    {
        private static Dictionary<Type, ICollectionPoolValue> _pools;
        internal static ICollectionPoolValue Value(Type key) => _pools?.GetValueOrDefault(key);

        public static ICollectionPoolValue SetMaxCount<TCollection>(int maxCount)
        {
            var key = typeof(TCollection);
            _pools ??= new Dictionary<Type, ICollectionPoolValue>();
            if (!_pools.ContainsKey(key))
                _pools.Add(key, new ICollectionPoolValue(ICollectionPoolValue.NewPool<TCollection>()));

            var value = _pools[key];
            value.MaxCount = maxCount;
            return value;
        }
        public static void SetAllMaxCount(int maxCount)
        {
            if (_pools != null)
                foreach (var pair in _pools)
                    pair.Value.MaxCount = maxCount;
        }

        public static void Clean<TCollection>(int maxCount) => _pools?.Remove(typeof(TCollection));
        public static void CleanAll() => _pools = null;
    }
}