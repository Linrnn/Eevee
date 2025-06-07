using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct OBBIntersectChecker
    {
        private readonly Vector2D _p0;
        private readonly Vector2D _p1;
        private readonly Vector2D _p2;
        private readonly Vector2D _p3;
        private readonly Vector2D _v01;
        private readonly Vector2D _v12;
        private readonly Vector2D _v23;
        private readonly Vector2D _v30;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OBBIntersectChecker(in OBB2D shape)
        {
            shape.RotatedCorner(out var p0, out var p1, out var p2, out var p3);
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _v01 = p0 - p1;
            _v12 = p1 - p2;
            _v23 = p2 - p3;
            _v30 = p3 - p0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Fixed64 XMin() => Fixed64.Min(_p0.X, _p1.X, _p2.X, _p3.X);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Fixed64 XMax() => Fixed64.Max(_p0.X, _p1.X, _p2.X, _p3.X);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Fixed64 YMin() => Fixed64.Min(_p0.Y, _p1.Y, _p2.Y, _p3.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Fixed64 YMax() => Fixed64.Max(_p0.Y, _p1.Y, _p2.Y, _p3.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2D shape)
        {
            if (AcuteOrRightAngle(shape.LeftBottom()))
                return true;
            if (AcuteOrRightAngle(shape.RightBottom()))
                return true;
            if (AcuteOrRightAngle(shape.RightTop()))
                return true;
            if (AcuteOrRightAngle(shape.LeftTop()))
                return true;

            if (Geometry.Contain(in shape, in _p0))
                return true;
            if (Geometry.Contain(in shape, in _p1))
                return true;
            if (Geometry.Contain(in shape, in _p2))
                return true;
            if (Geometry.Contain(in shape, in _p3))
                return true;

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AcuteOrRightAngle(in Vector2D point) // 向量夹角是锐角或者直角
        {
            if (Vector2D.Dot(point - _p0, in _v30) < 0)
                return false;
            if (Vector2D.Dot(point - _p1, in _v01) < 0)
                return false;
            if (Vector2D.Dot(point - _p2, in _v12) < 0)
                return false;
            if (Vector2D.Dot(point - _p3, in _v23) < 0)
                return false;
            return true;
        }
    }
}