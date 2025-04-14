using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class StackPool
    {
        public static Stack<T> Alloc<T>() => ICollectionPool<Stack<T>>.Alloc();
        public static Stack<T> Alloc<T>(ref Stack<T> collection) => collection = ICollectionPool<Stack<T>>.Alloc();
        public static Stack<T> TryAlloc<T>(ref Stack<T> collection) => collection ??= ICollectionPool<Stack<T>>.Alloc();

        public static void Release2Pool<T>(this Stack<T> collection)
        {
            collection.Clear();
            ICollectionPool<Stack<T>>.Release(collection);
        }
        public static void Release<T>(ref Stack<T> collection)
        {
            collection.Clear();
            ICollectionPool<Stack<T>>.Release(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref Stack<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<Stack<T>>.Release(collection);
            collection = null;
        }
    }
}