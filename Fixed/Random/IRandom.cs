namespace Eevee.Fixed
{
    public interface IRandom
    {
        sbyte GetSbyte(sbyte minInclusive, sbyte maxExclusive);
        byte GetByte(byte minInclusive, byte maxExclusive);

        short GetInt16(short minInclusive, short maxExclusive);
        ushort GetUInt16(ushort minInclusive, ushort maxExclusive);

        int GetInt32(int minInclusive, int maxExclusive);
        uint GetUInt32(uint minInclusive, uint maxExclusive);

        long GetInt64(long minInclusive, long maxExclusive);
        ulong GetUInt64(ulong minInclusive, ulong maxExclusive);

        // todo Eevee 缺少：FixNumber，InCircle，OnCircle，InSphere，OnSphere
    }
}