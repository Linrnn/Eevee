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

        public static T[] SharedRent<T>(int capacity) => ArrayPool<T>.Shared.Rent(capacity);
        public static void SharedReturn<T>(this T[] source) => ArrayPool<T>.Shared.Return(source, true);
    }
}