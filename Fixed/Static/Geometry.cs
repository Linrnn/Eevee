using System;
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
            Rotate(sx, sy, ex, ey, deg, out rx, out ry);
            na = deg;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseRotate(int sx, int sy, int ex, int ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry, out Fixed64 na)
        {
            var deg = Maths.ClampDeg(-a);
            Rotate(sx, sy, ex, ey, deg, out rx, out ry);
            na = deg;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Rotate(Fixed64 sx, Fixed64 sy, Fixed64 ex, Fixed64 ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry)
        {
            var cos = Maths.CosDeg(a);
            var sin = Maths.SinDeg(a);
            Rotate(ex - sx, ey - sy, cos, sin, out var dx, out var dy);
            rx = dx + sx;
            ry = dy + sy;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Rotate(int sx, int sy, int ex, int ey, Fixed64 a, out Fixed64 rx, out Fixed64 ry)
        {
            var cos = Maths.CosDeg(a);
            var sin = Maths.SinDeg(a);
            Rotate(ex - sx, ey - sy, cos, sin, out var dx, out var dy);
            rx = dx + sx;
            ry = dy + sy;
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

        #region 包含
        public static bool Contain(in Circle shape, in Vector2D other) => shape.SqrDistance(in other) <= shape.R.Sqr();
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
        public static bool Contain(in CircleInt shape, Vector2DInt other) => shape.SqrDistance(other) <= shape.R * shape.R;
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
            if (dx > shape.W)
                return false;

            var dy = (other.Y - shape.Y).Abs();
            if (dy > shape.H)
                return false;

            return true;
        }
        public static bool Contain(in AABB2D shape, in Circle other) => shape.Left() <= other.Left() && shape.Right() >= other.Right() && shape.Bottom() <= other.Bottom() && shape.Top() >= other.Top();
        public static bool Contain(in AABB2D shape, in AABB2D other) => shape.Left() <= other.Left() && shape.Right() >= other.Right() && shape.Bottom() <= other.Bottom() && shape.Top() >= other.Top();
        public static bool Contain(in AABB2DInt shape, Vector2DInt other)
        {
            int dx = Math.Abs(other.X - shape.X);
            if (dx > shape.W)
                return false;

            int dy = Math.Abs(other.Y - shape.Y);
            if (dy > shape.H)
                return false;

            return true;
        }
        public static bool Contain(in AABB2DInt shape, in CircleInt other) => shape.Left() <= other.Left() && shape.Right() >= other.Right() && shape.Bottom() <= other.Bottom() && shape.Top() >= other.Top();
        public static bool Contain(in AABB2DInt shape, in AABB2DInt other) => shape.Left() <= other.Left() && shape.Right() >= other.Right() && shape.Bottom() <= other.Bottom() && shape.Top() >= other.Top();

        public static bool Contain(in OBB2D shape, in Vector2D other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);

            var dx = (rx - shape.X).Abs();
            if (dx > shape.W)
                return false;

            var dy = (ry - shape.Y).Abs();
            if (dy > shape.H)
                return false;

            return true;
        }
        public static bool Contain(in OBB2D shape, in Circle other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            var reverse = new Circle(rx, ry, other.R);
            return shape.Left() <= reverse.Left() && shape.Right() >= reverse.Right() && shape.Bottom() <= reverse.Bottom() && shape.Top() >= reverse.Top();
        }
        public static bool Contain(in OBB2D shape, in AABB2D other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2D(in reverse);
            return shape.Left() <= boundary.Left() && shape.Right() >= boundary.Right() && shape.Bottom() <= boundary.Bottom() && shape.Top() >= boundary.Top();
        }
        public static bool Contain(in OBB2D shape, in OBB2D other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A - other.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A - other.A, out var rx, out var ry, out var na);
            var reverse = new OBB2D(rx, ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2D(in reverse);
            return shape.Left() <= boundary.Left() && shape.Right() >= boundary.Right() && shape.Bottom() <= boundary.Bottom() && shape.Top() >= boundary.Top();
        }
        public static bool Contain(in OBB2DInt shape, Vector2DInt other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);

            var dx = (rx - shape.X).Abs();
            if (dx > shape.W)
                return false;

            var dy = (ry - shape.Y).Abs();
            if (dy > shape.H)
                return false;

            return true;
        }
        public static bool Contain(in OBB2DInt shape, in CircleInt other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out _);
            var reverse = new CircleInt((int)rx, (int)ry, other.R);
            return shape.Left() <= reverse.Left() && shape.Right() >= reverse.Right() && shape.Bottom() <= reverse.Bottom() && shape.Top() >= reverse.Top();
        }
        public static bool Contain(in OBB2DInt shape, in AABB2DInt other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A, out var rx, out var ry, out var na);
            var reverse = new OBB2DInt((int)rx, (int)ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2DInt(in reverse);
            return shape.Left() <= boundary.Left() && shape.Right() >= boundary.Right() && shape.Bottom() <= boundary.Bottom() && shape.Top() >= boundary.Top();
        }
        public static bool Contain(in OBB2DInt shape, in OBB2DInt other)
        {
            // 计“shape”未旋转，即“other”绕“shape”的中心点，反向旋转“shape.A - other.A”度
            ReverseRotate(other.X, other.Y, shape.X, shape.Y, shape.A - other.A, out var rx, out var ry, out var na);
            var reverse = new OBB2DInt((int)rx, (int)ry, other.W, other.H, na);
            var boundary = Converts.AsAABB2DInt(in reverse);
            return shape.Left() <= boundary.Left() && shape.Right() >= boundary.Right() && shape.Bottom() <= boundary.Bottom() && shape.Top() >= boundary.Top();
        }

        public static bool Contain(in Polygon shape, in Vector2D other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in Polygon shape, in Circle other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in Polygon shape, in AABB2D other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in Polygon shape, in OBB2D other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in Polygon shape, in Polygon other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in PolygonInt shape, Vector2DInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in PolygonInt shape, in CircleInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in PolygonInt shape, in AABB2DInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in PolygonInt shape, in OBB2DInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Contain(in PolygonInt shape, in PolygonInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        #endregion

        #region 相交
        public static bool Intersect(in Circle shape, in Circle other) // 外切/相交/内切
        {
            var d = shape.SqrDistance(in other);
            if (d > (shape.R + other.R).Sqr())
                return false;
            if (d < (shape.R - other.R).Sqr())
                return false;
            return true;
        }
        public static bool Intersect(in CircleInt shape, in CircleInt other) // 外切/相交/内切
        {
            int d = shape.SqrDistance(in other);

            int ra = shape.R + other.R;
            if (d > ra * ra)
                return false;

            int rs = shape.R - other.R;
            if (d < rs * rs)
                return false;

            return true;
        }

        public static bool Intersect(in AABB2D shape, in Circle other)
        {
            var x = Fixed64.Max((shape.X - other.X).Abs() - shape.W, Fixed64.Zero);
            var y = Fixed64.Max((shape.Y - other.Y).Abs() - shape.H, Fixed64.Zero);
            return x.Sqr() + y.Sqr() <= other.R.Sqr();
        }
        public static bool Intersect(in AABB2D shape, in AABB2D other) => (shape.X - other.X).Abs() < shape.W + other.W && (shape.Y - other.Y).Abs() < shape.H + other.H;
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
        public static bool Intersect(in AABB2DInt shape, in AABB2DInt other) => Math.Abs(shape.X - other.X) < shape.W + other.W && Math.Abs(shape.Y - other.Y) < shape.H + other.H;
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

        public static bool Intersect(in OBB2D shape, in Circle other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in OBB2D shape, in AABB2D other) => new OBBIntersectChecker(in shape).Intersect(in other);
        public static bool Intersect(in OBB2D shape, in OBB2D other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in OBB2DInt shape, in CircleInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in OBB2DInt shape, in AABB2DInt other) => new OBBIntIntersectChecker(in shape).Intersect(in other);
        public static bool Intersect(in OBB2DInt shape, in OBB2DInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }

        public static bool Intersect(in Polygon shape, in Circle other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in Polygon shape, in AABB2D other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in Polygon shape, in OBB2D other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in Polygon shape, in Polygon other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in PolygonInt shape, in CircleInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in PolygonInt shape, in AABB2DInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in PolygonInt shape, in OBB2DInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        public static bool Intersect(in PolygonInt shape, in PolygonInt other)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        #endregion
    }
}