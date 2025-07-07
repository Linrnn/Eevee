using System.Runtime.CompilerServices;

namespace Eevee.Utils
{
    internal readonly struct HashUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint FastMod(uint value, uint divisor, ulong multiplier) => (uint)(((multiplier * value >> 32) + 1) * divisor >> 32);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong GetFastModMultiplier(uint divisor) => ulong.MaxValue / divisor + 1;
    }
}