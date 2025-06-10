using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class StackPool
    {
        public static Stack<T> Alloc<T>() => CollectionPool<Stack<T>>.InternalAlloc();
        public static Stack<T> Alloc<T>(ref Stack<T> collection) => collection = CollectionPool<Stack<T>>.InternalAlloc();
        public static Stack<T> TryAlloc<T>(ref Stack<T> collection) => collection ??= CollectionPool<Stack<T>>.InternalAlloc();

        public static void Release2Pool<T>(this Stack<T> collection)
        {
            collection.Clear();
            CollectionPool<Stack<T>>.InternalRelease(collection);
        }
        public static void Release<T>(ref Stack<T> collection)
        {
            collection.Clear();
            CollectionPool<Stack<T>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref Stack<T> collection)
        {
            if (collection is null)
                return;

            collection.Clear();
            CollectionPool<Stack<T>>.InternalRelease(collection);
            collection = null;
        }
    }
}