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
                cos = default;
                return false;
            }

            var magnitude = (lhs.SqrMagnitude() * rhs.SqrMagnitude()).Sqrt();
            if (magnitude.RawValue == 0)
            {
                cos = default;
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
                cos = default;
                return false;
            }

            var magnitude = (lhs.SqrMagnitude() * rhs.SqrMagnitude()).Sqrt();
            if (magnitude.RawValue == 0)
            {
                cos = default;
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

        #region 向量旋转
        public static Vector2D Rotate(in Vector2D point, Fixed64 rad)
        {
            var cos = Maths.Cos(rad);
            var sin = Maths.Sin(rad);
            return new Vector2D
            {
                X = point.X * cos - point.Y * sin,
                Y = point.X * sin + point.Y * cos,
            };
        }
        public static Vector2D Rotate(Vector2DInt point, Fixed64 rad)
        {
            var cos = Maths.Cos(rad);
            var sin = Maths.Sin(rad);
            return new Vector2D
            {
                X = point.X * cos - point.Y * sin,
                Y = point.X * sin + point.Y * cos,
            };
        }
        public static Vector2D RotateDeg(in Vector2D point, Fixed64 deg)
        {
            var cos = Maths.CosDeg(deg);
            var sin = Maths.SinDeg(deg);
            return new Vector2D
            {
                X = point.X * cos - point.Y * sin,
                Y = point.X * sin + point.Y * cos,
            };
        }
        public static Vector2D RotateDeg(Vector2DInt point, Fixed64 deg)
        {
            var cos = Maths.CosDeg(deg);
            var sin = Maths.SinDeg(deg);
            return new Vector2D
            {
                X = point.X * cos - point.Y * sin,
                Y = point.X * sin + point.Y * cos,
            };
        }
        public static Vector2D Rotate(in Vector2D point, in Vector2D dir)
        {
            Check.Normal(in dir);
            return new Vector2D
            {
                X = point.X * dir.X - point.Y * dir.Y,
                Y = point.X * dir.Y + point.Y * dir.X,
            };
        }
        public static Vector2D Rotate(Vector2DInt point, in Vector2D dir)
        {
            Check.Normal(in dir);
            return new Vector2D
            {
                X = point.X * dir.X - point.Y * dir.Y,
                Y = point.X * dir.Y + point.Y * dir.X,
            };
        }

        public static Fixed64 RotateX(in Vector2D point, Fixed64 rad) => point.X * Maths.Cos(rad) - point.Y * Maths.Sin(rad);
        public static Fixed64 RotateX(Vector2DInt point, Fixed64 rad) => point.X * Maths.Cos(rad) - point.Y * Maths.Sin(rad);
        public static Fixed64 RotateXDeg(in Vector2D point, Fixed64 deg) => point.X * Maths.CosDeg(deg) - point.Y * Maths.SinDeg(deg);
        public static Fixed64 RotateXDeg(Vector2DInt point, Fixed64 deg) => point.X * Maths.CosDeg(deg) - point.Y * Maths.SinDeg(deg);
        public static Fixed64 RotateX(in Vector2D point, in Vector2D dir)
        {
            Check.Normal(in dir);
            return point.X * dir.X - point.Y * dir.Y;
        }
        public static Fixed64 RotateX(Vector2DInt point, in Vector2D dir)
        {
            Check.Normal(in dir);
            return point.X * dir.X - point.Y * dir.Y;
        }

        public static Fixed64 RotateY(in Vector2D point, Fixed64 rad) => point.X * Maths.Sin(rad) + point.Y * Maths.Cos(rad);
        public static Fixed64 RotateY(Vector2DInt point, Fixed64 rad) => point.X * Maths.Sin(rad) + point.Y * Maths.Cos(rad);
        public static Fixed64 RotateYDeg(in Vector2D point, Fixed64 deg) => point.X * Maths.SinDeg(deg) + point.Y * Maths.CosDeg(deg);
        public static Fixed64 RotateYDeg(Vector2DInt point, Fixed64 deg) => point.X * Maths.SinDeg(deg) + point.Y * Maths.CosDeg(deg);
        public static Fixed64 RotateY(in Vector2D point, in Vector2D dir)
        {
            Check.Normal(in dir);
            return point.X * dir.Y + point.Y * dir.X;
        }
        public static Fixed64 RotateY(Vector2DInt point, in Vector2D dir)
        {
            Check.Normal(in dir);
            return point.X * dir.Y + point.Y * dir.X;
        }
        #endregion

        #region 重心/质心
        public static Fixed64 Barycentric(Fixed64 v0, Fixed64 v1, Fixed64 v2, Fixed64 a0, Fixed64 a1) => v0 + (v1 - v0) * a0 + (v2 - v0) * a1;
        public static Vector2D Barycentric(in Vector2D p0, in Vector2D p1, in Vector2D p2, Fixed64 a0, Fixed64 a1) => new()
        {
            X = Barycentric(p0.X, p1.X, p2.X, a0, a1),
            Y = Barycentric(p0.Y, p1.Y, p2.Y, a0, a1),
        };
        #endregion
    }
}