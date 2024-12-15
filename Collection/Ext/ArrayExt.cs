using System;

namespace Eevee.Collection
{
    public static class ArrayExt
    {
        public static T[] Create<T>(int count) => count > 0 ? new T[count] : Array.Empty<T>();

        public static void Clean(this Array source) => Array.Clear(source, 0, source.Length);
    }
}