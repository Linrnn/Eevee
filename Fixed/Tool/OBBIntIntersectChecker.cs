using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct OBBIntIntersectChecker
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
        internal OBBIntIntersectChecker(in OBB2DInt shape)
        {
            shape.RotatedCorner(out var p0, out var p1, out var p2, out var p3);
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
            if (AcuteOrRightAngle(shape.LeftBottom()))
                return true;
            if (AcuteOrRightAngle(shape.RightBottom()))
                return true;
            if (AcuteOrRightAngle(shape.RightTop()))
                return true;
            if (AcuteOrRightAngle(shape.LeftTop()))
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
        private bool AcuteOrRightAngle(Vector2DInt point) // 向量夹角是锐角或者直角
        {
            if (Vector2DInt.Dot(point - _p0, _v30) < 0)
                return false;
            if (Vector2DInt.Dot(point - _p1, _v01) < 0)
                return false;
            if (Vector2DInt.Dot(point - _p2, _v12) < 0)
                return false;
            if (Vector2DInt.Dot(point - _p3, _v23) < 0)
                return false;
            return true;
        }
    }
}