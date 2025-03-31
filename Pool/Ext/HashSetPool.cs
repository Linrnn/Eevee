using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class HashSetPool
    {
        public static HashSet<T> Alloc<T>() => ICollectionPool<HashSet<T>, T>.Alloc();
        public static HashSet<T> Alloc<T>(ref HashSet<T> collection) => ICollectionPool<HashSet<T>, T>.Alloc(ref collection);

        public static void Release2Pool<T>(this HashSet<T> collection) => ICollectionPool<HashSet<T>, T>.Release(collection);
        public static void Release<T>(ref HashSet<T> collection) => ICollectionPool<HashSet<T>, T>.Release(ref collection);
    }
}