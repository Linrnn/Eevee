using System;
using System.Collections;
using System.Collections.Generic;

namespace Eevee.Pool
{
    public sealed class CollectionPool<TCollection> where TCollection : class, IEnumerable, new()
    {
        private CollectionPool _pool;

        #region Alloc
        public TCollection Alloc() => PrivateAlloc(_pool);
        public static TCollection Alloc(CollectionPool collectionPool) => PrivateAlloc(collectionPool);
        internal static TCollection InternalAlloc() => PrivateAlloc(CollectionPool.Impl);
        private static TCollection PrivateAlloc(CollectionPool collectionPool)
        {
            var value = collectionPool?.Value(typeof(TCollection));
            if (value?.GetPool<TCollection>() is { } pool && pool.TryPop(out var collection))
                return collection;
            return new TCollection();
        }
        #endregion

        #region Release
        public void Release(TCollection collection) => PrivateRelease(collection, ref _pool);
        public static void Release(TCollection collection, ref CollectionPool collectionPool) => PrivateRelease(collection, ref collectionPool);
        internal static void InternalRelease(TCollection collection) => PrivateRelease(collection, ref CollectionPool.Impl);
        private static void PrivateRelease(TCollection collection, ref CollectionPool collectionPool)
        {
            collectionPool ??= new CollectionPool();
            var value = collectionPool.Value(typeof(TCollection));
            if (CollectionPool.ReleaseCheck && value?.GetPool<TCollection>() is { } pool && pool.Contains(collection))
                throw new InvalidOperationException($"Pools is Contains, FullName:{collection.GetType().FullName}, HashCode:{collection.GetHashCode()}");

            var newValue = value ?? collectionPool.SetMaxCount<TCollection>();
            if (!newValue.IsFull())
                newValue.GetPool<TCollection>().Push(collection);
        }
        #endregion
    }

    public sealed class CollectionPool
    {
        #region 实例成员
        internal Dictionary<Type, CollectionPoolValue> Pools;

        internal CollectionPoolValue Value(Type key) => Pools?.GetValueOrDefault(key);
        public CollectionPoolValue SetMaxCount<TCollection>(int maxCount = CollectionPoolValue.ConstMaxCount)
        {
            var key = typeof(TCollection);
            Pools ??= new Dictionary<Type, CollectionPoolValue>();
            if (!Pools.ContainsKey(key))
                Pools.Add(key, new CollectionPoolValue(CollectionPoolValue.NewPool<TCollection>()));

            var value = Pools[key];
            value.MaxCount = maxCount;
            return value;
        }
        public void SetAllMaxCount(int maxCount)
        {
            if (Pools is not null)
                foreach (var pair in Pools)
                    pair.Value.MaxCount = maxCount;
        }

        public void Clean<TCollection>() => Pools?.Remove(typeof(TCollection));
        public void CleanAll() => Pools = null;
        #endregion

        #region 静态成员
        public static bool ReleaseCheck = true;

        internal static CollectionPool Impl;
        public static void CleanImpl() => Impl = null;
        #endregion
    }
}