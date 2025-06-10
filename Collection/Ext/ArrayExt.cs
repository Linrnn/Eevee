using Eevee.Fixed;
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

        public static T[] SharedRent<T>(int capacity) => capacity > 0 ? ArrayPool<T>.Shared.Rent(capacity) : Array.Empty<T>();
        public static void SharedReturn<T>(this T[] source)
        {
            if (source.Length > 0)
                ArrayPool<T>.Shared.Return(source, true);
        }
        public static void SharedReturn<T>(ref T[] source)
        {
            if (source.Length > 0)
                ArrayPool<T>.Shared.Return(source, true);
            source = null;
        }

        public static void Clean<T>(this T[] source) => (source as IList).Clear();
    }
}