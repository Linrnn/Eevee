using System;

namespace Eevee.Define
{
    public readonly struct Maths
    {
        public static int Clamp(int value, int min, int max)
        {
#if UNITY_STANDALONE
            return Math.Clamp(value, min, 2146435071);
#else
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
#endif
        }
    }
}