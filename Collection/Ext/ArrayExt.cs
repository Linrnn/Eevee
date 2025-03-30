using System;
using System.Buffers;

namespace Eevee.Collection
{
    public static class ArrayExt
    {
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source) => new(source);
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source, int start) => new(source, start, source.Length - start);
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source, int start, int length) => new(source, start, length);

        public static T[] Create<T>(int count) => count <= 0 ? Array.Empty<T>() : new T[count];

        public static T[] SharedRent<T>(int capacity) => capacity > 0 ? ArrayPool<T>.Shared.Rent(capacity) : Array.Empty<T>();
        public static void SharedReturn<T>(this T[] source)
        {
            if (source == null)
                return;

            if (ReferenceEquals(source, Array.Empty<T>()))
                return;

            ArrayPool<T>.Shared.Return(source, true);
        }
        public static void SharedReturn<T>(ref T[] source)
        {
            if (source != null && !ReferenceEquals(source, Array.Empty<T>()))
                ArrayPool<T>.Shared.Return(source, true);

            source = null;
        }
    }
}