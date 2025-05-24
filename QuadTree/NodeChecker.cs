using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    #region Point
    internal readonly struct PointCircleNodeChecker : INodeChecker
    {
        private readonly Vector2DInt _area;

        internal PointCircleNodeChecker(Vector2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt boundary) => boundary.Contain(_area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Contain(_area);
    }

    internal readonly struct PointAABBNodeChecker : INodeChecker
    {
        private readonly Vector2DInt _area;

        internal PointAABBNodeChecker(Vector2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt boundary) => boundary.Contain(_area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Contain(_area);
    }
    #endregion

    #region Circle
    internal readonly struct CircleNodeChecker : INodeChecker
    {
        private readonly CircleInt _area;

        internal CircleNodeChecker(in CircleInt area) => _area = area;
        public bool CheckNode(in AABB2DInt boundary) => boundary.Intersect(in _area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Intersect(in _area);
    }

    internal readonly struct CircleAABBNodeChecker : INodeChecker
    {
        private readonly CircleInt _area;

        internal CircleAABBNodeChecker(in CircleInt area) => _area = area;
        public bool CheckNode(in AABB2DInt boundary) => boundary.Intersect(in _area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Intersect(in _area);
    }
    #endregion

    #region AABB
    internal readonly struct AABBCircleNodeChecker : INodeChecker
    {
        private readonly AABB2DInt _area;

        internal AABBCircleNodeChecker(in AABB2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt boundary) => boundary.Intersect(in _area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Intersect(in _area);
    }

    internal readonly struct AABBNodeChecker : INodeChecker
    {
        private readonly AABB2DInt _area;

        internal AABBNodeChecker(in AABB2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt boundary) => boundary.Intersect(in _area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Intersect(in _area);
    }
    #endregion

    #region OBB
    internal readonly struct OBBNodeChecker : INodeChecker
    {
        private readonly AABB2DInt _area;
        private readonly OBBChecker _checker;

        internal OBBNodeChecker(in AABB2DInt area, in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3)
        {
            _area = area;
            _checker = new OBBChecker(in p0, in p1, in p2, in p3);
        }
        public bool CheckNode(in AABB2DInt boundary) => boundary.Intersect(in _area);
        public bool CheckElement(in AABB2DInt boundary) => boundary.Intersect(in _area) && _checker.Intersect(in boundary);
    }
    #endregion

    #region Polygon
    internal readonly struct PolygonNodeChecker : INodeChecker
    {
        private readonly AABB2DInt _area;
        private readonly PolygonChecker _checker;

        internal PolygonNodeChecker(in AABB2DInt area, in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3)
        {
            _area = area;
            _checker = new PolygonChecker(in p0, in p1, in p2, in p3);
        }
        public bool CheckNode(in AABB2DInt boundary) => boundary.Intersect(in _area);
        public bool CheckElement(in AABB2DInt boundary) => _checker.Intersect(in boundary);
    }
    #endregion

    #region Tools
    internal readonly struct OBBChecker
    {
        private readonly Vector2DInt _p0;
        private readonly Vector2DInt _p1;
        private readonly Vector2DInt _p2;
        private readonly Vector2DInt _p3;
        private readonly Vector2DInt _p30;
        private readonly Vector2DInt _p01;
        private readonly Vector2DInt _p12;
        private readonly Vector2DInt _p23;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OBBChecker(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3)
        {
            var pi0 = (Vector2DInt)p0;
            var pi1 = (Vector2DInt)p1;
            var pi2 = (Vector2DInt)p2;
            var pi3 = (Vector2DInt)p3;
            _p0 = pi0;
            _p1 = pi1;
            _p2 = pi2;
            _p3 = pi3;
            _p30 = pi3 - pi0;
            _p01 = pi0 - pi1;
            _p12 = pi1 - pi2;
            _p23 = pi2 - pi3;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt boundary)
        {
            if (AcuteOrRightAngle(boundary.LeftBottom()))
                return true;
            if (AcuteOrRightAngle(boundary.RightBottom()))
                return true;
            if (AcuteOrRightAngle(boundary.RightTop()))
                return true;
            if (AcuteOrRightAngle(boundary.LeftTop()))
                return true;
            if (boundary.Contain(_p0))
                return true;
            if (boundary.Contain(_p1))
                return true;
            if (boundary.Contain(_p2))
                return true;
            if (boundary.Contain(_p3))
                return true;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AcuteOrRightAngle(Vector2DInt point) // 向量夹角是锐角或者直角
        {
            if (Vector2DInt.Dot(point - _p0, _p30) < 0)
                return false;
            if (Vector2DInt.Dot(point - _p1, _p01) < 0)
                return false;
            if (Vector2DInt.Dot(point - _p2, _p12) < 0)
                return false;
            if (Vector2DInt.Dot(point - _p3, _p23) < 0)
                return false;
            return true;
        }
    }

    internal readonly struct PolygonChecker
    {
        private readonly Vector2DInt _p0;
        private readonly Vector2DInt _p1;
        private readonly Vector2DInt _p2;
        private readonly Vector2DInt _p3;
        private readonly Vector2DInt _p30;
        private readonly Vector2DInt _p01;
        private readonly Vector2DInt _p12;
        private readonly Vector2DInt _p23;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonChecker(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3)
        {
            var pi0 = (Vector2DInt)p0;
            var pi1 = (Vector2DInt)p1;
            var pi2 = (Vector2DInt)p2;
            var pi3 = (Vector2DInt)p3;
            _p0 = pi0;
            _p1 = pi1;
            _p2 = pi2;
            _p3 = pi3;
            _p30 = pi3 - pi0;
            _p01 = pi0 - pi1;
            _p12 = pi1 - pi2;
            _p23 = pi2 - pi3;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt boundary)
        {
            if (SameDirection(boundary.LeftBottom()))
                return true;
            if (SameDirection(boundary.RightBottom()))
                return true;
            if (SameDirection(boundary.RightTop()))
                return true;
            if (SameDirection(boundary.LeftTop()))
                return true;
            if (boundary.Contain(_p0))
                return true;
            if (boundary.Contain(_p1))
                return true;
            if (boundary.Contain(_p2))
                return true;
            if (boundary.Contain(_p3))
                return true;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SameDirection(Vector2DInt point) // 向量方向相同
        {
            int cross0 = Vector2DInt.Cross(point - _p0, _p30);
            int cross1 = Vector2DInt.Cross(point - _p1, _p01);
            int cross2 = Vector2DInt.Cross(point - _p2, _p12);
            int cross3 = Vector2DInt.Cross(point - _p3, _p23);
            if (cross0 >= 0 && cross1 >= 0 && cross2 >= 0 && cross3 >= 0)
                return true;
            if (cross0 <= 0 && cross1 <= 0 && cross2 <= 0 && cross3 <= 0)
                return true;
            return false;
        }
    }
    #endregion
}