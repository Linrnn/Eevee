using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 几何相关
    /// </summary>
    public readonly struct Geometry
    {
        #region 夹角计算
        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Vector2D lhs, in Vector2D rhs) => Dot(in lhs, in rhs, out var cos) ? Maths.Acos(cos) : Fixed64.Zero;
        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Vector3D lhs, in Vector3D rhs) => Dot(in lhs, in rhs, out var cos) ? Maths.Acos(cos) : Fixed64.Zero;
        /// <summary>
        /// 返回两个旋转之间的夹角，返回无符号弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Quaternion lhs, in Quaternion rhs)
        {
            var rad = Rad(in lhs, in rhs);
            return rad.RawValue > Const.Rad180 ? Maths.Rad360 - rad : rad;
        }

        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 Angle(in Vector2D lhs, in Vector2D rhs) => Dot(in lhs, in rhs, out var cos) ? Maths.AcosDeg(cos) : Fixed64.Zero;
        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 Angle(in Vector3D lhs, in Vector3D rhs) => Dot(in lhs, in rhs, out var cos) ? Maths.AcosDeg(cos) : Fixed64.Zero;
        /// <summary>
        /// 返回两个旋转之间的夹角，返回无符号角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 Angle(in Quaternion lhs, in Quaternion rhs)
        {
            var deg = Deg(in lhs, in rhs);
            return deg.RawValue > Const.Deg180 ? Maths.Deg360 - deg : deg;
        }

        /// <summary>
        /// 返回两个向量之间的夹角，返回弧度<br/>
        /// 值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Vector2D lhs, in Vector2D rhs) => AngleRad(lhs, rhs) * Vector2D.Cross(in lhs, in rhs).Sign();
        /// <summary>
        /// 返回两个向量之间的夹角，返回弧度<br/>
        /// 值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Vector3D lhs, in Vector3D rhs, in Vector3D axis)
        {
            var rad = AngleRad(in lhs, in rhs);
            var cross = Vector3D.Cross(in lhs, in rhs);
            var dot = Vector3D.Dot(in axis, in cross);
            return rad * dot.No0Sign();
        }
        /// <summary>
        /// 返回两个旋转之间的角度，返回弧度<br/>
        /// 值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Quaternion lhs, in Quaternion rhs)
        {
            var rad = Rad(in lhs, in rhs);
            return rad.RawValue > Const.Rad180 ? rad - Maths.Rad360 : rad;
        }

        /// <summary>
        /// 返回两个向量之间的夹角，返回角度<br/>
        /// 值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngle(in Vector2D lhs, in Vector2D rhs) => Angle(in lhs, in rhs) * Vector2D.Cross(in lhs, in rhs).Sign();
        /// <summary>
        /// 返回两个向量之间的夹角，返回角度<br/>
        /// 值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngle(in Vector3D lhs, in Vector3D rhs, in Vector3D axis)
        {
            var deg = Angle(in lhs, in rhs);
            var cross = Vector3D.Cross(in lhs, in rhs);
            var dot = Vector3D.Dot(in axis, in cross);
            return deg * dot.No0Sign();
        }
        /// <summary>
        /// 返回两个旋转之间的夹角，返回角度<br/>
        /// 值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngle(in Quaternion lhs, in Quaternion rhs)
        {
            var deg = Deg(in lhs, in rhs);
            return deg.RawValue > Const.Deg180 ? deg - Maths.Deg360 : deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Dot(in Vector2D lhs, in Vector2D rhs, out Fixed64 cos)
        {
            if (lhs == Vector2D.Zero || rhs == Vector2D.Zero)
            {
                cos = Fixed64.Zero;
                return false;
            }

            var magnitude = (lhs.SqrMagnitude() * rhs.SqrMagnitude()).Sqrt();
            if (magnitude.RawValue == 0)
            {
                cos = Fixed64.Zero;
                return false;
            }

            var dot = Vector2D.Dot(in lhs, in rhs);
            cos = (dot / magnitude).ClampNg1();
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Dot(in Vector3D lhs, in Vector3D rhs, out Fixed64 cos)
        {
            if (lhs == Vector3D.Zero || rhs == Vector3D.Zero)
            {
                cos = Fixed64.Zero;
                return false;
            }

            var magnitude = (lhs.SqrMagnitude() * rhs.SqrMagnitude()).Sqrt();
            if (magnitude.RawValue == 0)
            {
                cos = Fixed64.Zero;
                return false;
            }

            var dot = Vector3D.Dot(in lhs, in rhs);
            cos = (dot / magnitude).ClampNg1();
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 Rad(in Quaternion lhs, in Quaternion rhs) => Maths.Acos(Cos(in lhs, in rhs)) << 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 Deg(in Quaternion lhs, in Quaternion rhs) => Maths.AcosDeg(Cos(in lhs, in rhs)) << 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 Cos(in Quaternion lhs, in Quaternion rhs)
        {
            var inverse = lhs.Inverse();
            return inverse.W * rhs.W - inverse.X * rhs.X - inverse.Y * rhs.Y - inverse.Z * rhs.Z;
        }
        #endregion

        /// <summary>
        /// 重心/质心
        /// </summary>
        public static Fixed64 Barycentric(Fixed64 v1, Fixed64 v2, Fixed64 v3, Fixed64 a1, Fixed64 a2) => v1 + (v2 - v1) * a1 + (v3 - v1) * a2;
        /// <summary>
        /// 重心/质心
        /// </summary>
        public static Vector2D Barycentric(in Vector2D p1, in Vector2D p2, in Vector2D p3, Fixed64 a1, Fixed64 a2) => new()
        {
            X = Barycentric(p1.X, p2.X, p3.X, a1, a2),
            Y = Barycentric(p1.Y, p2.Y, p3.Y, a1, a2),
        };
    }
}