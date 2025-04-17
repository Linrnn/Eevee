﻿using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class HashSetPool
    {
        public static HashSet<T> Alloc<T>() => ICollectionPool<HashSet<T>>.InternalAlloc();
        public static HashSet<T> Alloc<T>(ref HashSet<T> collection) => collection = ICollectionPool<HashSet<T>>.InternalAlloc();
        public static HashSet<T> TryAlloc<T>(ref HashSet<T> collection) => collection ??= ICollectionPool<HashSet<T>>.InternalAlloc();

        public static void Release2Pool<T>(this HashSet<T> collection)
        {
            collection.Clear();
            ICollectionPool<HashSet<T>>.InternalRelease(collection);
        }
        public static void Release<T>(ref HashSet<T> collection)
        {
            collection.Clear();
            ICollectionPool<HashSet<T>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref HashSet<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<HashSet<T>>.InternalRelease(collection);
            collection = null;
        }
    }
}