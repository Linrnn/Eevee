using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct PolygonIntIntersectChecker
    {
        private readonly Vector2DInt _p0;
        private readonly Vector2DInt _p1;
        private readonly Vector2DInt _p2;
        private readonly Vector2DInt _p3;
        private readonly Vector2DInt _v01;
        private readonly Vector2DInt _v12;
        private readonly Vector2DInt _v23;
        private readonly Vector2DInt _v30;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonIntIntersectChecker(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3)
        {
            _p0 = (Vector2DInt)p0;
            _p1 = (Vector2DInt)p1;
            _p2 = (Vector2DInt)p2;
            _p3 = (Vector2DInt)p3;
            _v01 = (Vector2DInt)(p0 - p1);
            _v12 = (Vector2DInt)(p1 - p2);
            _v23 = (Vector2DInt)(p2 - p3);
            _v30 = (Vector2DInt)(p3 - p0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int XMin() => Maths.Min(_p0.X, _p1.X, _p2.X, _p3.X);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int XMax() => Maths.Max(_p0.X, _p1.X, _p2.X, _p3.X);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int YMin() => Maths.Min(_p0.Y, _p1.Y, _p2.Y, _p3.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int YMax() => Maths.Max(_p0.Y, _p1.Y, _p2.Y, _p3.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt shape)
        {
            if (SameDirection(shape.LeftBottom()))
                return true;
            if (SameDirection(shape.RightBottom()))
                return true;
            if (SameDirection(shape.RightTop()))
                return true;
            if (SameDirection(shape.LeftTop()))
                return true;

            if (Geometry.Contain(in shape, _p0))
                return true;
            if (Geometry.Contain(in shape, _p1))
                return true;
            if (Geometry.Contain(in shape, _p2))
                return true;
            if (Geometry.Contain(in shape, _p3))
                return true;

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SameDirection(Vector2DInt point) // 向量方向相同
        {
            int cross0 = Vector2DInt.Cross(point - _p0, _v30);
            int cross1 = Vector2DInt.Cross(point - _p1, _v01);
            int cross2 = Vector2DInt.Cross(point - _p2, _v12);
            int cross3 = Vector2DInt.Cross(point - _p3, _v23);
            if (cross0 >= 0 && cross1 >= 0 && cross2 >= 0 && cross3 >= 0)
                return true;
            if (cross0 <= 0 && cross1 <= 0 && cross2 <= 0 && cross3 <= 0)
                return true;
            return false;
        }
    }
}