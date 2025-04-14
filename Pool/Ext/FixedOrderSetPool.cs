using Eevee.Collection;

namespace Eevee.Pool
{
    public static class FixedOrderSetPool
    {
        public static FixedOrderSet<T> Alloc<T>() => ICollectionPool<FixedOrderSet<T>>.Alloc();
        public static FixedOrderSet<T> Alloc<T>(ref FixedOrderSet<T> collection) => collection = ICollectionPool<FixedOrderSet<T>>.Alloc();
        public static FixedOrderSet<T> TryAlloc<T>(ref FixedOrderSet<T> collection) => collection ??= ICollectionPool<FixedOrderSet<T>>.Alloc();

        public static void Release2Pool<T>(this FixedOrderSet<T> collection)
        {
            collection.Clear();
            ICollectionPool<FixedOrderSet<T>>.Release(collection);
        }
        public static void Release<T>(ref FixedOrderSet<T> collection)
        {
            collection.Clear();
            ICollectionPool<FixedOrderSet<T>>.Release(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref FixedOrderSet<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<FixedOrderSet<T>>.Release(collection);
            collection = null;
        }
    }
}