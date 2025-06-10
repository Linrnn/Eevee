using Eevee.Collection;

namespace Eevee.Pool
{
    public static class FixedOrderSetPool
    {
        public static FixedOrderSet<T> Alloc<T>() => CollectionPool<FixedOrderSet<T>>.InternalAlloc();
        public static FixedOrderSet<T> Alloc<T>(ref FixedOrderSet<T> collection) => collection = CollectionPool<FixedOrderSet<T>>.InternalAlloc();
        public static FixedOrderSet<T> TryAlloc<T>(ref FixedOrderSet<T> collection) => collection ??= CollectionPool<FixedOrderSet<T>>.InternalAlloc();

        public static void Release2Pool<T>(this FixedOrderSet<T> collection)
        {
            collection.Clear();
            CollectionPool<FixedOrderSet<T>>.InternalRelease(collection);
        }
        public static void Release<T>(ref FixedOrderSet<T> collection)
        {
            collection.Clear();
            CollectionPool<FixedOrderSet<T>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref FixedOrderSet<T> collection)
        {
            if (collection is null)
                return;

            collection.Clear();
            CollectionPool<FixedOrderSet<T>>.InternalRelease(collection);
            collection = null;
        }
    }
}