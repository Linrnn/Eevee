using Eevee.Diagnosis;
using System;
using System.Collections.Generic;

namespace Eevee.Pool
{
    internal readonly struct ICollectionPool<TCollection, TElement> where TCollection : class, ICollection<TElement>, new()
    {
        internal const int PoolMaxCount = 128;
        private static Stack<TCollection> _pools;

        internal static TCollection Alloc() => PrivateAlloc();
        internal static TCollection Alloc(ref TCollection collection)
        {
            Assert.Null<InvalidOperationException, AssertArgs<Type, int>>(collection, nameof(collection), "collection isn't null, Type:{0}, HashCode:{1}", new AssertArgs<Type, int>(collection.GetType(), collection.GetHashCode()));
            return collection = PrivateAlloc();
        }

        internal static void Release(TCollection collection) => PrivateRelease(collection);
        internal static void Release(ref TCollection collection)
        {
            PrivateRelease(collection);
            collection = null;
        }

        internal static void Clean() => _pools?.Clear();

        private static TCollection PrivateAlloc()
        {
            if (_pools != null && _pools.TryPop(out var collection))
                return collection;
            return new TCollection();
        }
        private static void PrivateRelease(TCollection collection)
        {
            Assert.False<InvalidOperationException, AssertArgs<Type, int>>(_pools?.Contains(collection) ?? false, nameof(collection), "Pools is Contains, Type:{0}, HashCode:{1}", new AssertArgs<Type, int>(collection.GetType(), collection.GetHashCode()));
            collection.Clear();

            _pools ??= new Stack<TCollection>();
            if (_pools.Count < PoolMaxCount)
                _pools.Push(collection);
        }
    }
}