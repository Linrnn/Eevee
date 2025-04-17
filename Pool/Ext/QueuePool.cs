﻿using System.Collections.Generic;

namespace Eevee.Pool
{
    public static class QueuePool
    {
        public static Queue<T> Alloc<T>() => ICollectionPool<Queue<T>>.InternalAlloc();
        public static Queue<T> Alloc<T>(ref Queue<T> collection) => collection = ICollectionPool<Queue<T>>.InternalAlloc();
        public static Queue<T> TryAlloc<T>(ref Queue<T> collection) => collection ??= ICollectionPool<Queue<T>>.InternalAlloc();

        public static void Release2Pool<T>(this Queue<T> collection)
        {
            collection.Clear();
            ICollectionPool<Queue<T>>.InternalRelease(collection);
        }
        public static void Release<T>(ref Queue<T> collection)
        {
            collection.Clear();
            ICollectionPool<Queue<T>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref Queue<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<Queue<T>>.InternalRelease(collection);
            collection = null;
        }
    }
}