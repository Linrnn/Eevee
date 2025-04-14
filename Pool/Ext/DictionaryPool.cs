using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class DictionaryPool
    {
        public static Dictionary<TKey, TValue> Alloc<TKey, TValue>() => ICollectionPool<Dictionary<TKey, TValue>>.Alloc();
        public static Dictionary<TKey, TValue> Alloc<TKey, TValue>(ref Dictionary<TKey, TValue> collection) => collection = ICollectionPool<Dictionary<TKey, TValue>>.Alloc();
        public static Dictionary<TKey, TValue> TryAlloc<TKey, TValue>(ref Dictionary<TKey, TValue> collection) => collection ??= ICollectionPool<Dictionary<TKey, TValue>>.Alloc();

        public static void Release2Pool<TKey, TValue>(this Dictionary<TKey, TValue> collection)
        {
            collection.Clear();
            ICollectionPool<Dictionary<TKey, TValue>>.Release(collection);
        }
        public static void Release<TKey, TValue>(ref Dictionary<TKey, TValue> collection)
        {
            collection.Clear();
            ICollectionPool<Dictionary<TKey, TValue>>.Release(collection);
            collection = null;
        }
        public static void TryRelease<TKey, TValue>(ref Dictionary<TKey, TValue> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<Dictionary<TKey, TValue>>.Release(collection);
            collection = null;
        }
    }
}