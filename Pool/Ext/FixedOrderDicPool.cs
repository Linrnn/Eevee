using Eevee.Collection;

namespace Eevee.Pool
{
    public static class FixedOrderDicPool
    {
        public static FixedOrderDic<TKey, TValue> Alloc<TKey, TValue>() => CollectionPool<FixedOrderDic<TKey, TValue>>.InternalAlloc();
        public static FixedOrderDic<TKey, TValue> Alloc<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection) => collection = CollectionPool<FixedOrderDic<TKey, TValue>>.InternalAlloc();
        public static FixedOrderDic<TKey, TValue> TryAlloc<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection) => collection ??= CollectionPool<FixedOrderDic<TKey, TValue>>.InternalAlloc();

        public static void Release2Pool<TKey, TValue>(this FixedOrderDic<TKey, TValue> collection)
        {
            collection.Clear();
            CollectionPool<FixedOrderDic<TKey, TValue>>.InternalRelease(collection);
        }
        public static void Release<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection)
        {
            collection.Clear();
            CollectionPool<FixedOrderDic<TKey, TValue>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection)
        {
            if (collection is null)
                return;

            collection.Clear();
            CollectionPool<FixedOrderDic<TKey, TValue>>.InternalRelease(collection);
            collection = null;
        }
    }
}