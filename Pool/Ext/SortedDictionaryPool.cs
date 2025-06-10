using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class SortedDictionaryPool
    {
        public static SortedDictionary<TKey, TValue> Alloc<TKey, TValue>() => CollectionPool<SortedDictionary<TKey, TValue>>.InternalAlloc();
        public static SortedDictionary<TKey, TValue> Alloc<TKey, TValue>(ref SortedDictionary<TKey, TValue> collection) => collection = CollectionPool<SortedDictionary<TKey, TValue>>.InternalAlloc();
        public static SortedDictionary<TKey, TValue> TryAlloc<TKey, TValue>(ref SortedDictionary<TKey, TValue> collection) => collection ??= CollectionPool<SortedDictionary<TKey, TValue>>.InternalAlloc();

        public static void Release2Pool<TKey, TValue>(this SortedDictionary<TKey, TValue> collection)
        {
            collection.Clear();
            CollectionPool<SortedDictionary<TKey, TValue>>.InternalRelease(collection);
        }
        public static void Release<TKey, TValue>(ref SortedDictionary<TKey, TValue> collection)
        {
            collection.Clear();
            CollectionPool<SortedDictionary<TKey, TValue>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<TKey, TValue>(ref SortedDictionary<TKey, TValue> collection)
        {
            if (collection is null)
                return;

            collection.Clear();
            CollectionPool<SortedDictionary<TKey, TValue>>.InternalRelease(collection);
            collection = null;
        }
    }
}