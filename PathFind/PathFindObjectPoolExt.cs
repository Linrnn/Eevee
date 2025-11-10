using System.Collections.Generic;

namespace Eevee.PathFind
{
    internal static class PathFindObjectPoolExt
    {
        internal static List<T> ListAlloc<T>(this IPathFindObjectPoolGetter getter) => getter.AllocList<T>(false);
        internal static List<T> ListAlloc<T>(this IPathFindObjectPoolGetter getter, bool needLock) => getter.AllocList<T>(needLock);
        internal static List<T> ListAlloc<T>(this IPathFindObjectPoolGetter getter, int capacity)
        {
            var collection = getter.AllocList<T>(false);
            if (collection.Capacity < capacity)
                collection.Capacity = capacity;
            return collection;
        }
        internal static void Alloc<T>(this IPathFindObjectPoolGetter getter, ref List<T> collection) => collection = getter.AllocList<T>(false);
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, List<T> collection)
        {
            collection.Clear();
            getter.ReleaseList(collection);
        }
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, ref List<T> collection)
        {
            collection.Clear();
            getter.ReleaseList(collection);
            collection = null;
        }

        internal static Stack<T> StackAlloc<T>(this IPathFindObjectPoolGetter getter) => getter.AllocStack<T>();
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, Stack<T> collection)
        {
            collection.Clear();
            getter.ReleaseStack(collection);
        }

        internal static void Alloc<T>(this IPathFindObjectPoolGetter getter, ref HashSet<T> collection) => collection = getter.AllocSet<T>();
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, ref HashSet<T> collection)
        {
            collection.Clear();
            getter.ReleaseSet(collection);
            collection = null;
        }

        internal static Dictionary<TKey, TValue> MapAlloc<TKey, TValue>(this IPathFindObjectPoolGetter getter, bool needLock) => getter.AllocMap<TKey, TValue>(needLock);
        internal static void Alloc<TKey, TValue>(this IPathFindObjectPoolGetter getter, ref Dictionary<TKey, TValue> collection) => collection = getter.AllocMap<TKey, TValue>(false);
        internal static void Release<TKey, TValue>(this IPathFindObjectPoolGetter getter, Dictionary<TKey, TValue> collection, bool needLock)
        {
            collection.Clear();
            getter.ReleaseMap(collection, needLock);
        }
        internal static void Release<TKey, TValue>(this IPathFindObjectPoolGetter getter, ref Dictionary<TKey, TValue> collection)
        {
            collection.Clear();
            getter.ReleaseMap(collection, false);
            collection = null;
        }
    }
}