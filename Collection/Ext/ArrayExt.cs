﻿using Eevee.Fixed;
using System;
using System.Buffers;
using System.Collections;

namespace Eevee.Collection
{
    public static class ArrayExt
    {
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source) => new(source);
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source, int start) => new(source, start, source.Length - start);
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source, int start, int length) => new(source, start, length);

        public static T[] Create<T>(int capacity) => capacity > 0 ? new T[capacity] : Array.Empty<T>();
        public static void AllocSize<T>(ref T[] source, int capacity)
        {
            if (source is null || source.Length < capacity)
            {
                if (capacity > 0)
                {
                    int bits = Maths.Log2(capacity);
                    if (!Maths.IsPowerOf2(capacity))
                        ++bits;
                    source = new T[1 << bits];
                }
                else
                {
                    source = Array.Empty<T>();
                }
            }
        }

        public static T[] SharedRent<T>(int capacity) => Rent(capacity, ArrayPool<T>.Shared);
        public static void SharedReturn<T>(this T[] source) => Return(source, ArrayPool<T>.Shared);
        public static void SharedReturn<T>(ref T[] source) => Return(ref source, ArrayPool<T>.Shared);

        public static T[] Rent<T>(int capacity, ArrayPool<T> pool) => capacity > 0 ? pool.Rent(capacity) : Array.Empty<T>();
        public static void Return<T>(this T[] source, ArrayPool<T> pool)
        {
            if (source.Length > 0)
                pool.Return(source, true);
        }
        public static void Return<T>(ref T[] source, ArrayPool<T> pool)
        {
            if (source.Length > 0)
                pool.Return(source, true);
            source = null;
        }

        public static void Clean<T>(this T[] source) => (source as IList).Clear();
    }
}