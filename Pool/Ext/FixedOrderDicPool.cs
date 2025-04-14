using Eevee.Collection;

namespace Eevee.Pool
{
    public static class FixedOrderDicPool
    {
        public static FixedOrderDic<TKey, TValue> Alloc<TKey, TValue>() => ICollectionPool<FixedOrderDic<TKey, TValue>>.Alloc();
        public static FixedOrderDic<TKey, TValue> Alloc<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection) => collection = ICollectionPool<FixedOrderDic<TKey, TValue>>.Alloc();
        public static FixedOrderDic<TKey, TValue> TryAlloc<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection) => collection ??= ICollectionPool<FixedOrderDic<TKey, TValue>>.Alloc();

        public static void Release2Pool<TKey, TValue>(this FixedOrderDic<TKey, TValue> collection)
        {
            collection.Clear();
            ICollectionPool<FixedOrderDic<TKey, TValue>>.Release(collection);
        }
        public static void Release<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection)
        {
            collection.Clear();
            ICollectionPool<FixedOrderDic<TKey, TValue>>.Release(collection);
            collection = null;
        }
        public static void TryRelease<TKey, TValue>(ref FixedOrderDic<TKey, TValue> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<FixedOrderDic<TKey, TValue>>.Release(collection);
            collection = null;
        }
    }
}