using Eevee.Collection;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 几何相关<br/>
    /// 参考链接：https://zhuanlan.zhihu.com/p/511164248
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
        public static Vector2D Rotate(Fixed64 x, Fixed64 y, Fixed64 rad)
        {
            Rotate(x, y, Maths.Cos(rad), Maths.Sin(rad), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D Rotate(in Vector2D vector, Fixed64 rad)
        {
            Rotate(vector.X, vector.Y, Maths.Cos(rad), Maths.Sin(rad), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D RotateDeg(Fixed64 x, Fixed64 y, Fixed64 deg)
        {
            Rotate(x, y, Maths.CosDeg(deg), Maths.SinDeg(deg), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D RotateDeg(in Vector2D vector, Fixed64 deg)
        {
            Rotate(vector.X, vector.Y, Maths.CosDeg(deg), Maths.SinDeg(deg), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D Rotate(in Vector2D vector, in Vector2D dir)
        {
            Check.Normal(in dir);
            Rotate(vector.X, vector.Y, dir.X, dir.Y, out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Rotate(Fixed64 x, Fixed64 y, Fixed64 cos, Fixed64 sin, out Fixed64 dx, out Fixed64 dy)
        {
            dx = RotateX(x, y, cos, sin);
            dy = RotateY(x, y, cos, sin);
        }

        public static Vector2D Rotate(int x, int y, Fixed64 rad)
        {
            Rotate(x, y, Maths.Cos(rad), Maths.Sin(rad), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D Rotate(Vector2DInt vector, Fixed64 rad)
        {
            Rotate(vector.X, vector.Y, Maths.Cos(rad), Maths.Sin(rad), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D RotateDeg(int x, int y, Fixed64 deg)
        {
            Rotate(x, y, Maths.CosDeg(deg), Maths.SinDeg(deg), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D RotateDeg(Vector2DInt vector, Fixed64 deg)
        {
            Rotate(vector.X, vector.Y, Maths.CosDeg(deg), Maths.SinDeg(deg), out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        public static Vector2D Rotate(Vector2DInt vector, in Vector2D dir)
        {
            Check.Normal(in dir);
            Rotate(vector.X, vector.Y, dir.X, dir.Y, out var dx, out var dy);
            return new Vector2D(dx, dy);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Rotate(int x, int y, Fixed64 cos, Fixed64 sin, out Fixed64 dx, out Fixed64 dy)
        {
            dx = RotateX(x, y, cos, sin);
            dy = RotateY(x, y, cos, sin);
        }

        public static Fixed64 RotateX(Fixed64 x, Fixed64 y, Fixed64 rad) => RotateX(x, y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateX(in Vector2D vector, Fixed64 rad) => RotateX(vector.X, vector.Y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateXDeg(Fixed64 x, Fixed64 y, Fixed64 deg) => RotateX(x, y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateXDeg(in Vector2D vector, Fixed64 deg) => RotateX(vector.X, vector.Y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateX(in Vector2D vector, in Vector2D dir)
        {
            Check.Normal(in dir);
            return RotateX(vector.X, vector.Y, dir.X, dir.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 RotateX(Fixed64 x, Fixed64 y, Fixed64 cos, Fixed64 sin) => x * cos - y * sin;

        public static Fixed64 RotateX(int x, int y, Fixed64 rad) => RotateX(x, y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateX(Vector2DInt vector, Fixed64 rad) => RotateX(vector.X, vector.Y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateXDeg(int x, int y, Fixed64 deg) => RotateX(x, y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateXDeg(Vector2DInt vector, Fixed64 deg) => RotateX(vector.X, vector.Y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateX(Vector2DInt vector, in Vector2D dir)
        {
            Check.Normal(in dir);
            return RotateX(vector.X, vector.Y, dir.X, dir.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 RotateX(int x, int y, Fixed64 cos, Fixed64 sin) => x * cos - y * sin;

        public static Fixed64 RotateY(Fixed64 x, Fixed64 y, Fixed64 rad) => RotateY(x, y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateY(in Vector2D vector, Fixed64 rad) => RotateY(vector.X, vector.Y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateYDeg(Fixed64 x, Fixed64 y, Fixed64 deg) => RotateY(x, y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateYDeg(in Vector2D vector, Fixed64 deg) => RotateY(vector.X, vector.Y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateY(in Vector2D vector, in Vector2D dir)
        {
            Check.Normal(in dir);
            return RotateY(vector.X, vector.Y, dir.X, dir.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 RotateY(Fixed64 x, Fixed64 y, Fixed64 cos, Fixed64 sin) => x * sin + y * cos;

        public static Fixed64 RotateY(int x, int y, Fixed64 rad) => RotateY(x, y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateY(Vector2DInt vector, Fixed64 rad) => RotateY(vector.X, vector.Y, Maths.Cos(rad), Maths.Sin(rad));
        public static Fixed64 RotateYDeg(int x, int y, Fixed64 deg) => RotateY(x, y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateYDeg(Vector2DInt vector, Fixed64 deg) => RotateY(vector.X, vector.Y, Maths.CosDeg(deg), Maths.SinDeg(deg));
        public static Fixed64 RotateY(Vector2DInt vector, in Vector2D dir)
        {
            Check.Normal(in dir);
            return RotateY(vector.X, vector.Y, dir.X, dir.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 RotateY(int x, int y, Fixed64 cos, Fixed64 sin) => x * sin + y * cos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseRotate(Fixed64 sx, Fixed64 sy, Fixed64 ex, Fixed64 ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry, out Fixed64 na)
        {
            var deg = Maths.ClampDeg(-a);
            Rotate(ex - sx, ey - sy, Maths.CosDeg(deg), Maths.SinDeg(deg), out var dx, out var dy);
            rx = dx + sx;
            ry = dy + sy;
            na = deg;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseRotate(int sx, int sy, int ex, int ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry, out Fixed64 na)
        {
            var deg = Maths.ClampDeg(-a);
            Rotate(ex - sx, ey - sy, Maths.CosDeg(deg), Maths.SinDeg(deg), out var dx, out var dy);
            rx = dx + sx;
            ry = dy + sy;
            na = deg;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseLocalRotate(Fixed64 sx, Fixed64 sy, Fixed64 ex, Fixed64 ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry, out Fixed64 na)
        {
            var deg = Maths.ClampDeg(-a);
            Rotate(ex - sx, ey - sy, Maths.CosDeg(deg), Maths.SinDeg(deg), out rx, out ry);
            na = deg;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseLocalRotate(int sx, int sy, int ex, int ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry, out Fixed64 na)
        {
            var deg = Maths.ClampDeg(-a);
            Rotate(ex - sx, ey - sy, Maths.CosDeg(deg), Maths.SinDeg(deg), out rx, out ry);
            na = deg;
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

        #region 圆
        public static bool IsOutside(in Circle shape, in Vector2D point) => shape.SqrDistance(point) > shape.R.Sqr(); // 外离
        public static bool IsIntersect(in Circle shape, in Vector2D point) => shape.SqrDistance(point) == shape.R.Sqr(); // 相切
        public static bool IsContain(in Circle shape, in Vector2D point) => shape.SqrDistance(point) < shape.R.Sqr(); // 内含
        public static bool IsOutside(in Circle shape, in Circle other) => shape.SqrDistance(in other) > (shape.R + other.R).Sqr(); // 外离
        public static bool IsCircumscribed(in Circle shape, in Circle other) => shape.SqrDistance(in other) == (shape.R + other.R).Sqr(); // 外切
        public static bool IsIntersect(in Circle shape, in Circle other) // 相交
        {
            var d = shape.SqrDistance(in other);

            var ra = shape.R + other.R;
            if (d >= ra.Sqr())
                return false;

            var rs = shape.R.Sqr();
            if (d <= rs * rs)
                return false;

            return true;
        }
        public static bool IsInscribed(in Circle shape, in Circle other) => shape.SqrDistance(in other) == (shape.R - other.R).Sqr(); // 内切
        public static bool IsContain(in Circle shape, in Circle other) => shape.SqrDistance(in other) < (shape.R - other.R).Sqr(); // 内含/内离
        public static bool IsConcentric(in Circle shape, in Circle other) => shape.X == other.X && shape.Y == other.Y; // 同心

        public static bool IsOutside(in CircleInt shape, Vector2DInt point) => shape.SqrDistance(point) > shape.R * shape.R; // 外离
        public static bool IsIntersect(in CircleInt shape, Vector2DInt point) => shape.SqrDistance(point) == shape.R * shape.R; // 相切
        public static bool IsContain(in CircleInt shape, Vector2DInt point) => shape.SqrDistance(point) < shape.R * shape.R; // 内含
        public static bool IsOutside(in CircleInt shape, in CircleInt other) // 外离
        {
            int d = shape.SqrDistance(in other);
            int r = shape.R + other.R;
            return d > r * r;
        }
        public static bool IsCircumscribed(in CircleInt shape, in CircleInt other) // 外切
        {
            int d = shape.SqrDistance(in other);
            int r = shape.R + other.R;
            return d == r * r;
        }
        public static bool IsIntersect(in CircleInt shape, in CircleInt other) // 相交
        {
            int d = shape.SqrDistance(in other);

            int ra = shape.R + other.R;
            if (d >= ra * ra)
                return false;

            int rs = shape.R - other.R;
            if (d <= rs * rs)
                return false;

            return true;
        }
        public static bool IsInscribed(in CircleInt shape, in CircleInt other) // 内切
        {
            int d = shape.SqrDistance(in other);
            int r = shape.R - other.R;
            return d == r * r;
        }
        public static bool IsContain(in CircleInt shape, in CircleInt other) // 内含/内离
        {
            int d = shape.SqrDistance(in other);
            int r = shape.R - other.R;
            return d < r * r;
        }
        public static bool IsConcentric(in CircleInt shape, in CircleInt other) => shape.X == other.X && shape.Y == other.Y; // 同心
        #endregion

        #region 距离
        public static Fixed64 SqrDistance(in Segment2D shape, in Vector2D other)
        {
            var delta = shape.Delta();
            var dot = Vector2D.Dot(other - shape.Start, in delta);
            Vector2D project;
            if (dot.RawValue <= 0) // 投影点在“Start”的外侧，最近点是“Start”
                project = shape.Start;
            else if (shape.SqrLength() is { } sqrLength && dot >= sqrLength) // 投影点在“End”的外侧，最近点是“End”
                project = shape.End;
            else // 投影点在线段上
                project = shape.Start + delta * (dot / sqrLength);
            return Vector2D.SqrDistance(in other, in project);
        }
        public static Fixed64 SqrDistance(in Segment2DInt shape, Vector2DInt other)
        {
            var delta = shape.Delta();
            int dot = Vector2DInt.Dot(other - shape.Start, delta);
            Vector2D project;
            if (dot <= 0) // 投影点在“Start”的外侧，最近点是“Start”
                project = shape.Start;
            else if (shape.SqrLength() is { } sqrLength && dot >= sqrLength) // 投影点在“End”的外侧，最近点是“End”
                project = shape.End;
            else // 投影点在线段上
                project = shape.Start + (Vector2D)delta * ((Fixed64)dot / sqrLength);
            return Vector2D.SqrDistance(other, in project);
        }
        public static Fixed64 SqrDistance(in Ray2D shape, in Vector2D other)
        {
            var dot = Vector2D.Dot(other - shape.Origin, in shape.Direction);
            var project = dot.RawValue <= 0 ? shape.Origin : shape.Origin + shape.Direction * dot;
            return Vector2D.SqrDistance(in other, in project);
        }
        public static Fixed64 SqrDistance(in Line2D shape, in Vector2D other)
        {
            var dot = Vector2D.Dot(other - shape.Origin, in shape.Direction);
            var project = shape.Origin + shape.Direction * dot;
            return Vector2D.SqrDistance(in other, in project);
        }

        public static Fixed64 Distance(in Segment2D shape, in Vector2D other) => SqrDistance(in shape, in other).Sqrt();
        public static Fixed64 Distance(in Segment2DInt shape, Vector2DInt other) => SqrDistance(in shape, other).Sqrt();
        public static Fixed64 Distance(in Ray2D shape, in Vector2D other) => SqrDistance(in shape, in other).Sqrt();
        public static Fixed64 Distance(in Line2D shape, in Vector2D other) => SqrDistance(in shape, in other).Sqrt();
        #endregion

        #region 包含
        public static bool Contain(in Circle shape, in Vector2D other) => shape.R.Sqr() >= shape.SqrDistance(in other);
        public static bool Contain(in Circle shape, in Circle other) // 内含/内离/同心
        {
            if (shape.X == other.X && shape.Y == other.Y && shape.R >= other.R)
                return true;
            var d = shape.SqrDistance(in other);
            var r = shape.R - other.R;
            return d < r.Sqr();
        }
        public static bool Contain(in Circle shape, in AABB2D other)
        {
            var rSqr = shape.R.Sqr();

            var left = other.Left();
            var bottom = other.Bottom();
            if (shape.SqrDistance(left, bottom) > rSqr)
                return false;

            var right = other.Right();
            if (shape.SqrDistance(right, bottom) > rSqr)
                return false;

            var top = other.Top();
            if (shape.SqrDistance(right, top) > rSqr)
                return false;

            if (shape.SqrDistance(left, top) > rSqr)
                return false;

            return true;
        }
        public static bool Contain(in CircleInt shape, Vector2DInt other) => shape.R * shape.R >= shape.SqrDistance(other);
        public static bool Contain(in CircleInt shape, in CircleInt other) // 内含/内离/同心
        {
            if (shape.X == other.X && shape.Y == other.Y && shape.R >= other.R)
                return true;
            int d = shape.SqrDistance(in other);
            int r = shape.R - other.R;
            return d < r * r;
        }
        public static bool Contain(in CircleInt shape, in AABB2DInt other)
        {
            int rSqr = shape.R * shape.R;

            int left = other.Left();
            int bottom = other.Bottom();
            if (shape.SqrDistance(left, bottom) > rSqr)
                return false;

            int right = other.Right();
            if (shape.SqrDistance(right, bottom) > rSqr)
                return false;

            int top = other.Top();
            if (shape.SqrDistance(right, top) > rSqr)
                return false;

            if (shape.SqrDistance(left, top) > rSqr)
                return false;

            return true;
        }

        public static bool Contain(in AABB2D shape, in Vector2D other)
        {
            var dx = (other.X - shape.X).Abs();
            if (shape.W < dx)
                return false;

            var dy = (other.Y - shape.Y).Abs();
            if (shape.H < dy)
                return false;

            return true;
        }
        public static bool Contain(in AABB2D shape, in Circle other)
        {
            if (shape.Left() > other.Left())
                return false;
            if (shape.Right() < other.Right())
                return false;
            if (shape.Bottom() > other.Bottom())
                return false;
            if (shape.Top() < other.Top())
                return false;
            return true;
        }
        public static bool Contain(in AABB2D shape, in AABB2D other)
        {
            if (shape.Left() > other.Left())
                return false;
            if (shape.Right() < other.Right())
                return false;
            if (shape.Bottom() > other.Bottom())
                return false;
            if (shape.Top() < other.Top())
                return false;
            return true;
        }
        public static bool Contain(in AABB2DInt shape, Vector2DInt other)
        {
            int dx = Math.Abs(other.X - shape.X);
            if (shape.W < dx)
                return false;

            int dy = Math.Abs(other.Y - shape.Y);
            if (shape.H < dy)
                return false;

            return true;
        }
        public static bool Contain(in AABB2DInt shape, in CircleInt other)
        {
            if (shape.Left() > other.Left())
                return false;
            if (shape.Right() < other.Right())
                return false;
            if (shape.Bottom() > other.Bottom())
                return false;
            if (shape.Top() < other.Top())
                return false;
            return true;
        }
        public static bool Contain(in AABB2DInt shape, in AABB2DInt other)
        {
            if (shape.Left() > other.Left())
                return false;
            if (shape.Right() < other.Right())
                return false;
            if (shape.Bottom() > other.Bottom())
                return false;
            if (shape.Top() < other.Top())
                return false;
            return true;
        }

        public static bool Contain(in OBB2D shape, in Vector2D other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            if (shape.W < rx.Abs())
                return false;
            if (shape.H < ry.Abs())
                return false;
            return true;
        }
        public static bool Contain(in OBB2D shape, in Circle other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            var reverse = new Circle(rx, ry, other.R);
            if (-shape.W > reverse.Left())
                return false;
            if (shape.W < reverse.Right())
                return false;
            if (-shape.H > reverse.Bottom())
                return false;
            if (shape.H >= reverse.Top())
                return true;
            return false;
        }
        public static bool Contain(in OBB2D shape, in AABB2D other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2D(in reverse);
            if (-shape.W > boundary.Left())
                return false;
            if (shape.W < boundary.Right())
                return false;
            if (-shape.H > boundary.Bottom())
                return false;
            if (shape.H < boundary.Top())
                return false;
            return true;
        }
        public static bool Contain(in OBB2D shape, in OBB2D other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A - other.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A - other.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2D(in reverse);
            if (-shape.W > boundary.Left())
                return false;
            if (shape.W < boundary.Right())
                return false;
            if (-shape.H > boundary.Bottom())
                return false;
            if (shape.H < boundary.Top())
                return false;
            return true;
        }
        public static bool Contain(in OBB2DInt shape, Vector2DInt other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            if (shape.W < rx.Abs())
                return false;
            if (shape.H < ry.Abs())
                return false;
            return true;
        }
        public static bool Contain(in OBB2DInt shape, in CircleInt other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            var reverse = new Circle(rx, ry, other.R);
            if (-shape.W > reverse.Left())
                return false;
            if (shape.W < reverse.Right())
                return false;
            if (-shape.H > reverse.Bottom())
                return false;
            if (shape.H >= reverse.Top())
                return true;
            return false;
        }
        public static bool Contain(in OBB2DInt shape, in AABB2DInt other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2D(in reverse);
            if (-shape.W > boundary.Left())
                return false;
            if (shape.W < boundary.Right())
                return false;
            if (-shape.H > boundary.Bottom())
                return false;
            if (shape.H < boundary.Top())
                return false;
            return true;
        }
        public static bool Contain(in OBB2DInt shape, in OBB2DInt other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A - other.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A - other.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2D(in reverse);
            if (-shape.W > boundary.Left())
                return false;
            if (shape.W < boundary.Right())
                return false;
            if (-shape.H > boundary.Bottom())
                return false;
            if (shape.H < boundary.Top())
                return false;
            return true;
        }

        public static bool Contain(in Polygon shape, in Vector2D other) // 参考“PolygonIntersectChecker.Contain()”
        {
            for (int flag = 0, count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                ref var pi = ref shape[i];
                ref var pj = ref shape[j];

                var cross = Vector2D.Cross(other - pi, pi - pj);
                if (cross.RawValue == 0)
                    continue;
                if (flag == 0)
                    flag = cross.Sign();
                else if (flag != cross.Sign())
                    return false;
            }

            return true;
        }
        public static bool Contain(in Polygon shape, in Circle other)
        {
            var center = other.Center();
            var rSqr = other.R.Sqr();
            for (int flag = 0, count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                ref var pi = ref shape[i];
                ref var pj = ref shape[j];

                var cross = Vector2D.Cross(center - pi, pi - pj);
                if (cross.RawValue == 0)
                    return false;
                if (flag == 0)
                    flag = cross.Sign();
                else if (flag != cross.Sign())
                    return false;

                var side = new Segment2D(in pi, in pj);
                var sqrDistance = SqrDistance(in side, in center);
                if (sqrDistance < rSqr)
                    return false;
            }

            return true;
        }
        public static bool Contain(in Polygon shape, in AABB2D other)
        {
            using var checker = new PolygonIntersectChecker(in shape, true, false);
            if (!checker.Contain(other.Center()))
                return false;
            if (!checker.Contain(other.LeftBottom()))
                return false;
            if (!checker.Contain(other.RightBottom()))
                return false;
            if (!checker.Contain(other.RightTop()))
                return false;
            if (!checker.Contain(other.LeftTop()))
                return false;
            return true;
        }
        public static bool Contain(in Polygon shape, in OBB2D other)
        {
            other.RotatedCorner(out var p0, out var p1, out var p2, out var p3);
            using var checker = new PolygonIntersectChecker(in shape, true, false);
            if (!checker.Contain(other.Center()))
                return false;
            if (!checker.Contain(in p0))
                return false;
            if (!checker.Contain(in p1))
                return false;
            if (!checker.Contain(in p2))
                return false;
            if (!checker.Contain(in p3))
                return false;
            return true;
        }
        public static bool Contain(in Polygon shape, in Polygon other)
        {
            using var checker = new PolygonIntersectChecker(in shape, true, false);
            foreach (var point in other.GetPoints())
                if (!checker.Contain(in point))
                    return false;
            return true;
        }
        public static bool Contain(in PolygonInt shape, Vector2DInt other)
        {
            for (int flag = 0, count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];

                int cross = Vector2DInt.Cross(other - pi, pi - pj);
                if (cross == 0)
                    continue;
                if (flag == 0)
                    flag = cross;
                else if (flag != cross)
                    return false;
            }

            return true;
        }
        public static bool Contain(in PolygonInt shape, in CircleInt other)
        {
            var center = other.Center();
            var rSqr = (Fixed64)(other.R * other.R);
            for (int flag = 0, count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];

                int cross = Vector2DInt.Cross(center - pi, pi - pj);
                if (cross == 0)
                    return false;
                if (flag == 0)
                    flag = cross;
                else if (flag != cross)
                    return false;

                var side = new Segment2DInt(pi, pj);
                var sqrDistance = SqrDistance(in side, center);
                if (sqrDistance < rSqr)
                    return false;
            }

            return true;
        }
        public static bool Contain(in PolygonInt shape, in AABB2DInt other)
        {
            using var checker = new PolygonIntIntersectChecker(in shape, true, false);
            if (!checker.Contain(other.Center()))
                return false;
            if (!checker.Contain(other.LeftBottom()))
                return false;
            if (!checker.Contain(other.RightBottom()))
                return false;
            if (!checker.Contain(other.RightTop()))
                return false;
            if (!checker.Contain(other.LeftTop()))
                return false;
            return true;
        }
        public static bool Contain(in PolygonInt shape, in OBB2DInt other)
        {
            other.RotatedCorner(out var p0, out var p1, out var p2, out var p3);
            using var checker = new PolygonIntIntersectChecker(in shape, true, false);
            if (!checker.Contain(other.Center()))
                return false;
            if (!checker.Contain((Vector2DInt)p0))
                return false;
            if (!checker.Contain((Vector2DInt)p1))
                return false;
            if (!checker.Contain((Vector2DInt)p2))
                return false;
            if (!checker.Contain((Vector2DInt)p3))
                return false;
            return true;
        }
        public static bool Contain(in PolygonInt shape, in PolygonInt other)
        {
            using var checker = new PolygonIntIntersectChecker(in shape, true, false);
            foreach (var point in other.GetPoints())
                if (!checker.Contain(point))
                    return false;
            return true;
        }
        #endregion

        #region 相交
        public static bool Intersect(in Circle shape, in Circle other) // 外切/相交/内切
        {
            var distance = shape.SqrDistance(in other);
            var sumRadius = shape.R + other.R;
            return distance <= sumRadius.Sqr();
        }
        public static bool Intersect(in CircleInt shape, in CircleInt other) // 外切/相交/内切
        {
            int distance = shape.SqrDistance(in other);
            int sumRadius = shape.R + other.R;
            return distance <= sumRadius * sumRadius;
        }

        public static bool Intersect(in AABB2D shape, in Circle other)
        {
            var x = Fixed64.Max((shape.X - other.X).Abs() - shape.W, Fixed64.Zero);
            var y = Fixed64.Max((shape.Y - other.Y).Abs() - shape.H, Fixed64.Zero);
            return x.Sqr() + y.Sqr() <= other.R.Sqr();
        }
        public static bool Intersect(in AABB2D shape, in AABB2D other)
        {
            if ((shape.X - other.X).Abs() >= shape.W + other.W)
                return false;
            if ((shape.Y - other.Y).Abs() >= shape.H + other.H)
                return false;
            return true;
        }
        public static bool Intersect(in AABB2D shape, in AABB2D other, out AABB2D intersect)
        {
            var xMin = Fixed64.Max(shape.Left(), other.Left());
            var xMax = Fixed64.Min(shape.Right(), other.Right());
            if (xMin > xMax)
            {
                intersect = default;
                return false;
            }

            var yMin = Fixed64.Max(shape.Bottom(), other.Bottom());
            var yMax = Fixed64.Min(shape.Top(), other.Top());
            if (yMin > yMax)
            {
                intersect = default;
                return false;
            }

            intersect = AABB2D.Create(xMin, xMax, yMin, yMax);
            return true;
        }
        public static bool Intersect(in AABB2DInt shape, in CircleInt other)
        {
            var x = Fixed64.Max(Math.Abs(shape.X - other.X) - shape.W, Fixed64.Zero);
            var y = Fixed64.Max(Math.Abs(shape.Y - other.Y) - shape.H, Fixed64.Zero);
            return x.Sqr() + y.Sqr() <= other.R * other.R;
        }
        public static bool Intersect(in AABB2DInt shape, in AABB2DInt other)
        {
            if (Math.Abs(shape.X - other.X) >= shape.W + other.W)
                return false;
            if (Math.Abs(shape.Y - other.Y) >= shape.H + other.H)
                return false;
            return true;
        }
        public static bool Intersect(in AABB2DInt shape, in AABB2DInt other, out AABB2DInt intersect)
        {
            int xMin = Math.Max(shape.Left(), other.Left());
            int xMax = Math.Min(shape.Right(), other.Right());
            if (xMin > xMax)
            {
                intersect = default;
                return false;
            }

            int yMin = Math.Max(shape.Bottom(), other.Bottom());
            int yMax = Math.Min(shape.Top(), other.Top());
            if (yMin > yMax)
            {
                intersect = default;
                return false;
            }

            intersect = AABB2DInt.Create(xMin, xMax, yMin, yMax);
            return true;
        }
        internal static bool UnsafeIntersect(in AABB2DInt shape, in AABB2DInt other, out AABB2DInt intersect)
        {
            int xMin = Math.Max(shape.Left(), other.Left());
            int xMax = Math.Min(shape.Right(), other.Right());
            int yMin = Math.Max(shape.Bottom(), other.Bottom());
            int yMax = Math.Min(shape.Top(), other.Top());
            intersect = AABB2DInt.UnsafeCreate(xMin, xMax, yMin, yMax);
            return xMin <= xMax && yMin <= yMax;
        }

        public static bool Intersect(in OBB2D shape, in Circle other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            var x = Fixed64.Max(rx.Abs() - shape.W, Fixed64.Zero);
            var y = Fixed64.Max(ry.Abs() - shape.H, Fixed64.Zero);
            return x.Sqr() + y.Sqr() <= other.R.Sqr();
        }
        public static bool Intersect(in OBB2D shape, in AABB2D other) => new OBBIntersectChecker(in shape).Intersect(in other);
        public static bool Intersect(in OBB2D shape, in OBB2D other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A - other.A”度
        {
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A - other.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            return new OBBIntersectChecker(in reverse).Intersect(Converts.AsAABB2DWithoutAngle(in shape));
        }
        public static bool Intersect(in OBB2DInt shape, in CircleInt other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
        {
            ReverseLocalRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            var x = Fixed64.Max(rx.Abs() - shape.W, Fixed64.Zero);
            var y = Fixed64.Max(ry.Abs() - shape.H, Fixed64.Zero);
            return x.Sqr() + y.Sqr() <= other.R * other.R;
        }
        public static bool Intersect(in OBB2DInt shape, in AABB2DInt other) => new OBBIntIntersectChecker(in shape).Intersect(in other);
        public static bool Intersect(in OBB2DInt shape, in OBB2DInt other) // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A - other.A”度
        {
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A - other.A, out var rx, out var ry, out var na);
            var reverse = new OBB2DInt((int)rx, (int)ry, other.W, other.H, na);
            return new OBBIntIntersectChecker(in reverse).Intersect(Converts.AsAABB2DIntWithoutAngle(in shape));
        }

        public static bool Intersect(in Polygon shape, in Circle other)
        {
            using var checker = new PolygonIntersectChecker(in shape, true, false);
            return checker.Intersect(in other);
        }
        public static bool Intersect(in Polygon shape, in AABB2D other)
        {
            using var checker = new PolygonIntersectChecker(in shape, true, false);
            return checker.Intersect(in other);
        }
        public static bool Intersect(in Polygon shape, in OBB2D other)
        {
            using var checker = new PolygonIntersectChecker(in shape, true, false);
            return checker.Intersect(in other);
        }
        public static bool SATIntersect(in Polygon shape, in Polygon other) // SAT，分离轴定理
        {
            var shapeSpan = shape.GetPoints();
            var otherSpan = other.GetPoints();
            for (int count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                ref var pi = ref shape[i];
                ref var pj = ref shape[j];
                var axis = new Vector2D(pj.Y - pi.Y, pi.X - pj.X); // 等价“(pi - pj).Perpendicular();”
                if (HasSeparatingAxis(in axis, in shapeSpan, in otherSpan))
                    return false; // 存在分离轴 → 不相交
            }

            return true;
        }
        public static bool GJKIntersect(in Polygon shape, in Polygon other) // GJK，闵可夫斯基差集
        {
            var shapeSpan = shape.GetPoints();
            var otherSpan = other.GetPoints();
            bool intersect = false;
            var simplex = new StackAllocUnmanagedArray<Vector2D>(stackalloc Vector2D[3]); // 单纯形最多3个元素

            var direction = Vector2D.Right; // 初始方向，可以任意指定
            var support = Support(in direction, in shapeSpan, in otherSpan); // 初始支持点
            direction = -support; // 指向原点
            simplex.Add(in support);

            // for (int count = shape.PointCount() * other.PointCount(), i = 0; i < count; ++i)
            while (true)
            {
                support = Support(in direction, in shapeSpan, in otherSpan);
                var dot = Vector2D.Dot(in support, in direction);
                if (dot.RawValue <= 0) // 原点不在包围体内
                    break;

                simplex.Add(in support);
                if (!NearestSimplex(ref simplex, ref direction))
                    continue;

                intersect = true; // 原点在包围体内
                break;
            }

            return intersect;
        }
        public static bool Intersect(in PolygonInt shape, in CircleInt other)
        {
            using var checker = new PolygonIntIntersectChecker(in shape, true, false);
            return checker.Intersect(in other);
        }
        public static bool Intersect(in PolygonInt shape, in AABB2DInt other)
        {
            using var checker = new PolygonIntIntersectChecker(in shape, true, false);
            return checker.Intersect(in other);
        }
        public static bool Intersect(in PolygonInt shape, in OBB2DInt other)
        {
            using var checker = new PolygonIntIntersectChecker(in shape, true, false);
            return checker.Intersect(in other);
        }
        public static bool Intersect(in PolygonInt shape, in PolygonInt other) // GJK，闵可夫斯基差集
        {
            var shapeSpan = shape.GetPoints();
            var otherSpan = other.GetPoints();
            bool intersect = false;
            var simplex = new StackAllocUnmanagedArray<Vector2DInt>(stackalloc Vector2DInt[3]); // 单纯形最多3个元素

            var direction = Vector2DInt.Right; // 初始方向，可以任意指定
            var support = Support(direction, in shapeSpan, in otherSpan); // 初始支持点
            direction = -support; // 指向原点
            simplex.Add(support);

            // for (int count = shape.PointCount() * other.PointCount(), i = 0; i < count; ++i)
            while (true)
            {
                support = Support(direction, in shapeSpan, in otherSpan);
                int dot = Vector2DInt.Dot(support, direction);
                if (dot <= 0) // 原点不在包围体内
                    break;

                simplex.Add(support);
                if (!NearestSimplex(ref simplex, ref direction))
                    continue;

                intersect = true; // 原点在包围体内
                break;
            }

            return intersect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasSeparatingAxis(in Vector2D axis, in ReadOnlySpan<Vector2D> shape, in ReadOnlySpan<Vector2D> other) // 存在分离轴
        {
            ProjectSides(in axis, in shape, out var shapeMin, out var shapeMax);
            ProjectSides(in axis, in other, out var otherMin, out var otherMax);
            return shapeMin > otherMax || shapeMax < otherMin; // 两个区间不重叠
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProjectSides(in Vector2D axis, in ReadOnlySpan<Vector2D> shape, out Fixed64 min, out Fixed64 max)
        {
            min = Fixed64.MaxValue;
            max = Fixed64.MinValue;

            foreach (var point in shape)
            {
                var dot = Vector2D.Dot(in axis, in point);
                if (dot < min)
                    min = dot;
                else if (dot > max)
                    max = dot;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2D Support(in Vector2D direction, in ReadOnlySpan<Vector2D> shape, in ReadOnlySpan<Vector2D> other)
        {
            var shapeFurthest = FindFurthestPoint(in direction, in shape);
            var otherFurthest = FindFurthestPoint(-direction, in other);
            return shapeFurthest - otherFurthest;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool NearestSimplex(ref StackAllocUnmanagedArray<Vector2D> simplex, ref Vector2D direction)
        {
            switch (simplex.Count)
            {
                case 2:
                {
                    ref var p0 = ref simplex[0];
                    ref var p1 = ref simplex[1];
                    var p01 = p0 - p1;
                    var np1 = -p1;
                    direction = TripleProduct(in p01, in np1, in p01); // 垂直于AB，指向原点
                    return false;
                }
                case 3:
                {
                    ref var p0 = ref simplex[0];
                    ref var p1 = ref simplex[1];
                    ref var p2 = ref simplex[2];
                    var p12 = p1 - p2;
                    var p02 = p0 - p2;
                    var np2 = -p2;

                    var product0 = TripleProduct(in p02, in p12, in p12);
                    var dot0 = Vector2D.Dot(in product0, in np2);
                    if (dot0.RawValue > 0)
                    {
                        simplex.RemoveAt(0); // 去掉 C
                        direction = product0;
                        return false;
                    }

                    var product1 = TripleProduct(in p12, in p02, in p02);
                    var dot1 = Vector2D.Dot(in product1, in np2);
                    if (dot1.RawValue > 0)
                    {
                        simplex.RemoveAt(1); // 去掉 B
                        direction = product1;
                        return false;
                    }

                    return true; // 原点在三角形中
                }
                default: return false;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2D FindFurthestPoint(in Vector2D direction, in ReadOnlySpan<Vector2D> shape) // 寻找与“direction”最远的点
        {
            var max = Fixed64.MinValue;
            var furthest = default(Vector2D);

            foreach (var point in shape)
            {
                var dot = Vector2D.Dot(in point, in direction);
                if (dot <= max)
                    continue;

                max = dot;
                furthest = point;
            }

            return furthest;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2D TripleProduct(in Vector2D p0, in Vector2D p1, in Vector2D p2) // 向量三重积：u × (v × w) ≈ 用于获得垂直方向
        {
            var dot02 = Vector2D.Dot(in p0, in p2);
            var dot12 = Vector2D.Dot(in p1, in p2);
            return new Vector2D(p1.X * dot02 - p0.X * dot12, p1.Y * dot02 - p0.Y * dot12);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2DInt Support(Vector2DInt direction, in ReadOnlySpan<Vector2DInt> shape, in ReadOnlySpan<Vector2DInt> other)
        {
            var shapeFurthest = FindFurthestPoint(direction, in shape);
            var otherFurthest = FindFurthestPoint(-direction, in other);
            return shapeFurthest - otherFurthest;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool NearestSimplex(ref StackAllocUnmanagedArray<Vector2DInt> simplex, ref Vector2DInt direction)
        {
            switch (simplex.Count)
            {
                case 2:
                {
                    var p0 = simplex[0];
                    var p1 = simplex[1];
                    var p01 = p0 - p1;
                    var np1 = -p1;
                    direction = TripleProduct(p01, np1, p01); // 垂直于AB，指向原点
                    return false;
                }
                case 3:
                {
                    var p0 = simplex[0];
                    var p1 = simplex[1];
                    var p2 = simplex[2];
                    var p12 = p1 - p2;
                    var p02 = p0 - p2;
                    var np2 = -p2;

                    var product0 = TripleProduct(p02, p12, p12);
                    int dot0 = Vector2DInt.Dot(product0, np2);
                    if (dot0 > 0)
                    {
                        simplex.RemoveAt(0); // 去掉 C
                        direction = product0;
                        return false;
                    }

                    var product1 = TripleProduct(p12, p02, p02);
                    int dot1 = Vector2DInt.Dot(product1, np2);
                    if (dot1 > 0)
                    {
                        simplex.RemoveAt(1); // 去掉 B
                        direction = product1;
                        return false;
                    }

                    return true; // 原点在三角形中
                }
                default: return false;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2DInt FindFurthestPoint(Vector2DInt direction, in ReadOnlySpan<Vector2DInt> shape) // 寻找与“direction”最远的点
        {
            int max = int.MinValue;
            var furthest = default(Vector2DInt);

            foreach (var point in shape)
            {
                int dot = Vector2DInt.Dot(point, direction);
                if (dot <= max)
                    continue;

                max = dot;
                furthest = point;
            }

            return furthest;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2DInt TripleProduct(Vector2DInt p0, Vector2DInt p1, Vector2DInt p2) // 向量三重积：u × (v × w) ≈ 用于获得垂直方向
        {
            int dot02 = Vector2DInt.Dot(p0, p2);
            int dot12 = Vector2DInt.Dot(p1, p2);
            return new Vector2DInt(p1.X * dot02 - p0.X * dot12, p1.Y * dot02 - p0.Y * dot12);
        }
        #endregion
    }
}