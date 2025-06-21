using System.Runtime.CompilerServices;

namespace Eevee.Utils
{
    public readonly struct HashHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FastMod(uint value, uint divisor, ulong multiplier)
        {
            return (uint)(((multiplier * value >> 32) + 1) * divisor >> 32);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetFastModMultiplier(uint divisor) => ulong.MaxValue / divisor + 1;
    }
}