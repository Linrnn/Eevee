using System;

namespace Eevee.Collection
{
    public static class EArray
    {
        public static void Clean(this Array source)
        {
            Array.Clear(source, 0, source.Length);
        }
    }
}