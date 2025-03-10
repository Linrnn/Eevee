using Eevee.Fixed;

namespace Eevee.Random
{
    /// <summary>
    /// Random转发
    /// </summary>
    public readonly struct RandomRelay
    {
        public static bool Bool() => RandomProxy.Impl.GetBoolean();

        public static sbyte SByte(sbyte min, sbyte max) => RandomProxy.Impl.GetSbyte(min, max);
        public static byte Byte(byte min, byte max) => RandomProxy.Impl.GetByte(min, max);

        public static short Short(short min, short max) => RandomProxy.Impl.GetInt16(min, max);
        public static ushort UShort(ushort min, ushort max) => RandomProxy.Impl.GetUInt16(min, max);

        public static int Int(int min, int max) => RandomProxy.Impl.GetInt32(min, max);
        public static uint UInt(uint min, uint max) => RandomProxy.Impl.GetUInt32(min, max);

        public static long Long(long min, long max) => RandomProxy.Impl.GetInt64(min, max);
        public static ulong ULong(ulong min, ulong max) => RandomProxy.Impl.GetUInt64(min, max);

        public static Fixed64 Number(Fixed64 min, Fixed64 max) => RandomProxy.Impl.GetFixed64(min, max);
        public static Fixed64 Number() => RandomProxy.Impl.GetFixed64();
        public static Fixed64 Rad() => RandomProxy.Impl.GetRad();
        public static Fixed64 Deg() => RandomProxy.Impl.GetDeg();

        public static Vector2D Vector2(in Vector2D p0, in Vector2D p1) => RandomProxy.Impl.GetVector2(in p0, in p1);
        public static Vector3D Vector3(in Vector3D p0, in Vector3D p1) => RandomProxy.Impl.GetVector3(in p0, in p1);
        public static Vector2D InTriangle(in Vector2D p0, in Vector2D p1, in Vector2D p2) => RandomProxy.Impl.InTriangle(in p0, in p1, in p2);

        public static Vector2D OnUnitCircle() => RandomProxy.Impl.OnUnitCircle();
        public static Vector2D InCircle(Fixed64 radius) => RandomProxy.Impl.InCircle(radius);

        public static Vector3D OnUnitSphere() => RandomProxy.Impl.OnUnitSphere();
        public static Vector3D InSphere(Fixed64 radius) => RandomProxy.Impl.InSphere(radius);

        public static Vector3D EulerAngles() => RandomProxy.Impl.GetEulerAngles();
        public static Quaternions Quaternion() => RandomProxy.Impl.GetQuaternion();
    }
}