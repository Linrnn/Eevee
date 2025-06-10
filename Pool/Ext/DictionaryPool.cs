using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class DictionaryPool
    {
        public static Dictionary<TKey, TValue> Alloc<TKey, TValue>() => CollectionPool<Dictionary<TKey, TValue>>.InternalAlloc();
        public static Dictionary<TKey, TValue> Alloc<TKey, TValue>(ref Dictionary<TKey, TValue> collection) => collection = CollectionPool<Dictionary<TKey, TValue>>.InternalAlloc();
        public static Dictionary<TKey, TValue> TryAlloc<TKey, TValue>(ref Dictionary<TKey, TValue> collection) => collection ??= CollectionPool<Dictionary<TKey, TValue>>.InternalAlloc();

        public static void Release2Pool<TKey, TValue>(this Dictionary<TKey, TValue> collection)
        {
            collection.Clear();
            CollectionPool<Dictionary<TKey, TValue>>.InternalRelease(collection);
        }
        public static void Release<TKey, TValue>(ref Dictionary<TKey, TValue> collection)
        {
            collection.Clear();
            CollectionPool<Dictionary<TKey, TValue>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<TKey, TValue>(ref Dictionary<TKey, TValue> collection)
        {
            if (collection is null)
                return;

            collection.Clear();
            CollectionPool<Dictionary<TKey, TValue>>.InternalRelease(collection);
            collection = null;
        }
    }
}