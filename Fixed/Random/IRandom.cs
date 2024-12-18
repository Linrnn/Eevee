namespace Eevee.Fixed
{
    public interface IRandom
    {
        sbyte GetSbyte(sbyte min, sbyte max);
        byte GetByte(byte min, byte max);

        short GetInt16(short min, short max);
        ushort GetUInt16(ushort min, ushort max);

        int GetInt32(int min, int max);
        uint GetUInt32(uint min, uint max);

        long GetInt64(long min, long max);
        ulong GetUInt64(ulong min, ulong max);

        // todo lrn 缺少：FixNum，InCircle，OnCircle，InSphere，OnSphere
    }
}