﻿using Eevee.Collection;

namespace Eevee.Pool
{
    public static class WeakOrderListPool
    {
        public static WeakOrderList<T> Alloc<T>() => ICollectionPool<WeakOrderList<T>>.InternalAlloc();
        public static WeakOrderList<T> Alloc<T>(ref WeakOrderList<T> collection) => collection = ICollectionPool<WeakOrderList<T>>.InternalAlloc();
        public static WeakOrderList<T> TryAlloc<T>(ref WeakOrderList<T> collection) => collection ??= ICollectionPool<WeakOrderList<T>>.InternalAlloc();

        public static void Release2Pool<T>(this WeakOrderList<T> collection)
        {
            collection.Clear();
            ICollectionPool<WeakOrderList<T>>.InternalRelease(collection);
        }
        public static void Release<T>(ref WeakOrderList<T> collection)
        {
            collection.Clear();
            ICollectionPool<WeakOrderList<T>>.InternalRelease(collection);
            collection = null;
        }
        public static void TryRelease<T>(ref WeakOrderList<T> collection)
        {
            if (collection == null)
                return;

            collection.Clear();
            ICollectionPool<WeakOrderList<T>>.InternalRelease(collection);
            collection = null;
        }
    }
}