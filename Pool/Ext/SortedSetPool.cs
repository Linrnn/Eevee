using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class SortedSetPool
    {
        public static SortedSet<T> Alloc<T>() => ICollectionPool<SortedSet<T>>.Alloc();
        public static SortedSet<T> Alloc<T>(ref SortedSet<T> collection) => collection = ICollectionPool<SortedSet<T>>.Alloc();
        public static SortedSet<T> TryAlloc<T>(ref SortedSet<T> collection) => collection ??= ICollectionPool<SortedSet<T>>.Alloc();

        public static void Release2Pool<T>(this SortedSet<T> collection)
        {
            collection.Clear();
            ICollectionPool<SortedSet<T>>.Release(collection);
        }
        public static void Release<T>(ref SortedSet<T> collection)
        {
            collection.Clear();
            ICollectionPool<SortedSet<T>>.Release(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref SortedSet<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<SortedSet<T>>.Release(collection);
            collection = null;
        }
    }
}