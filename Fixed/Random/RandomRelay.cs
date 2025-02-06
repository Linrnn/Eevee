namespace Eevee.Fixed
{
    /// <summary>
    /// Random转发
    /// </summary>
    public readonly struct RandomRelay
    {
        public static sbyte Get(sbyte min, sbyte max) => RandomProxy.Impl.GetSbyte(min, max);
        public static byte Get(byte min, byte max) => RandomProxy.Impl.GetByte(min, max);

        public static short Get(short min, short max) => RandomProxy.Impl.GetInt16(min, max);
        public static ushort Get(ushort min, ushort max) => RandomProxy.Impl.GetUInt16(min, max);

        public static int Get(int min, int max) => RandomProxy.Impl.GetInt32(min, max);
        public static uint Get(uint min, uint max) => RandomProxy.Impl.GetUInt32(min, max);

        public static long Get(long min, long max) => RandomProxy.Impl.GetInt64(min, max);
        public static ulong Get(ulong min, ulong max) => RandomProxy.Impl.GetUInt64(min, max);
    }
}