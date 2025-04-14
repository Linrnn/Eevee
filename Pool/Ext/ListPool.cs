using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class ListPool
    {
        public static List<T> Alloc<T>() => ICollectionPool<List<T>>.Alloc();
        public static List<T> Alloc<T>(ref List<T> collection) => collection = ICollectionPool<List<T>>.Alloc();
        public static List<T> TryAlloc<T>(ref List<T> collection) => collection ??= ICollectionPool<List<T>>.Alloc();

        public static void Release2Pool<T>(this List<T> collection)
        {
            collection.Clear();
            ICollectionPool<List<T>>.Release(collection);
        }
        public static void Release<T>(ref List<T> collection)
        {
            collection.Clear();
            ICollectionPool<List<T>>.Release(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref List<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<List<T>>.Release(collection);
            collection = null;
        }
    }
}