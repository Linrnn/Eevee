namespace Eevee.Fixed
{
    public interface IRandom
    {
        bool GetBoolean();

        sbyte GetSbyte(sbyte minInclusive, sbyte maxExclusive);
        byte GetByte(byte minInclusive, byte maxExclusive);

        short GetInt16(short minInclusive, short maxExclusive);
        ushort GetUInt16(ushort minInclusive, ushort maxExclusive);

        int GetInt32(int minInclusive, int maxExclusive);
        uint GetUInt32(uint minInclusive, uint maxExclusive);

        long GetInt64(long minInclusive, long maxExclusive);
        ulong GetUInt64(ulong minInclusive, ulong maxExclusive);

        Fixed64 GetFixed64(Fixed64 minInclusive, Fixed64 maxExclusive);
        Fixed64 GetFixed64();
        Fixed64 GetRad();
        Fixed64 GetDeg();

        Vector2D GetVector2D(in Vector2D p0, in Vector2D p1);
        Vector3D GetVector3D(in Vector3D p0, in Vector3D p1);
        Vector2D InTriangle(in Vector2D p0, in Vector2D p1, in Vector2D p2);

        Vector2D OnUnitCircle();
        Vector2D InCircle(Fixed64 radius);

        Vector3D OnUnitSphere();
        Vector3D InSphere(Fixed64 radius);

        Vector3D GetEulerAngles();
        Quaternions GetQuaternions();
    }
}