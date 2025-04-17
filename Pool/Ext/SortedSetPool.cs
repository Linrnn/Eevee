using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class SortedSetPool
    {
        public static SortedSet<T> Alloc<T>() => CollectionPool<SortedSet<T>>.InternalAlloc();
        public static SortedSet<T> Alloc<T>(ref SortedSet<T> collection) => collection = CollectionPool<SortedSet<T>>.InternalAlloc();
        public static SortedSet<T> TryAlloc<T>(ref SortedSet<T> collection) => collection ??= CollectionPool<SortedSet<T>>.InternalAlloc();

        public static void Release2Pool<T>(this SortedSet<T> collection)
        {
            collection.Clear();
            CollectionPool<SortedSet<T>>.InternalRelease(collection);
        }
        public static void Release<T>(ref SortedSet<T> collection)
        {
            collection.Clear();
            CollectionPool<SortedSet<T>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref SortedSet<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            CollectionPool<SortedSet<T>>.InternalRelease(collection);
            collection = null;
        }
    }
}