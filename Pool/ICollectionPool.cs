using Eevee.Diagnosis;
using System;
using System.Collections.Generic;

namespace Eevee.Pool
{
    public sealed class ICollectionPool<TCollection> where TCollection : class, new()
    {
        private ICollectionPool _pool;
        private const bool ReleaseCheck = true;

        public TCollection Alloc() => PrivateAlloc(_pool);
        internal static TCollection InternalAlloc() => PrivateAlloc(ICollectionPool.Impl);
        private static TCollection PrivateAlloc(ICollectionPool collectionPool)
        {
            var value = collectionPool?.Value(typeof(TCollection));
            if (value?.GetPool<TCollection>() is { } pool && pool.TryPop(out var collection))
                return collection;
            return new TCollection();
        }

        public void Release(TCollection collection) => PrivateRelease(collection, ref _pool);
        internal static void InternalRelease(TCollection collection) => PrivateRelease(collection, ref ICollectionPool.Impl);
        private static void PrivateRelease(TCollection collection, ref ICollectionPool collectionPool)
        {
            collectionPool ??= new ICollectionPool();
            var value = collectionPool.Value(typeof(TCollection));
            if (ReleaseCheck)
                Assert.False<InvalidOperationException, AssertArgs<Type, int>>(value?.GetPool<TCollection>()?.Contains(collection) ?? false, nameof(collection), "Pools is Contains, Type:{0}, HashCode:{1}", new AssertArgs<Type, int>(collection.GetType(), collection.GetHashCode()));

            var newValue = value ?? collectionPool.SetMaxCount<TCollection>();
            if (!newValue.IsFull())
                newValue.GetPool<TCollection>().Push(collection);
        }
    }

    public sealed class ICollectionPool
    {
        #region 实例成员
        internal Dictionary<Type, ICollectionPoolValue> Pools;

        internal ICollectionPoolValue Value(Type key) => Pools?.GetValueOrDefault(key);
        public ICollectionPoolValue SetMaxCount<TCollection>(int maxCount = ICollectionPoolValue.ConstMaxCount)
        {
            var key = typeof(TCollection);
            Pools ??= new Dictionary<Type, ICollectionPoolValue>();
            if (!Pools.ContainsKey(key))
                Pools.Add(key, new ICollectionPoolValue(ICollectionPoolValue.NewPool<TCollection>()));

            var value = Pools[key];
            value.MaxCount = maxCount;
            return value;
        }
        public void SetAllMaxCount(int maxCount)
        {
            if (Pools != null)
                foreach (var pair in Pools)
                    pair.Value.MaxCount = maxCount;
        }

        public void Clean<TCollection>(int maxCount) => Pools?.Remove(typeof(TCollection));
        public void CleanAll() => Pools = null;
        #endregion

        #region 静态成员
        internal static ICollectionPool Impl;
        public static void CleanImpl() => Impl = null;
        #endregion
    }
}