using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    #region Point
    internal readonly struct PointCircleChecker : IIntersectChecker
    {
        private readonly Vector2DInt _area;

        internal PointCircleChecker(Vector2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt bounds) => bounds.Contain(_area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Contain(_area);
    }

    internal readonly struct PointAABBChecker : IIntersectChecker
    {
        private readonly Vector2DInt _area;

        internal PointAABBChecker(Vector2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt bounds) => bounds.Contain(_area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Contain(_area);
    }
    #endregion

    #region Circle
    internal readonly struct CircleChecker : IIntersectChecker
    {
        private readonly CircleInt _area;

        internal CircleChecker(in CircleInt area) => _area = area;
        public bool CheckNode(in AABB2DInt bounds) => bounds.Intersect(in _area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Intersect(in _area);
    }

    internal readonly struct CircleAABBChecker : IIntersectChecker
    {
        private readonly CircleInt _area;

        internal CircleAABBChecker(in CircleInt area) => _area = area;
        public bool CheckNode(in AABB2DInt bounds) => bounds.Intersect(in _area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Intersect(in _area);
    }
    #endregion

    #region AABB
    internal readonly struct AABBCircleChecker : IIntersectChecker
    {
        private readonly AABB2DInt _area;

        internal AABBCircleChecker(in AABB2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt bounds) => bounds.Intersect(in _area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Intersect(in _area);
    }

    internal readonly struct AABBChecker : IIntersectChecker
    {
        private readonly AABB2DInt _area;

        internal AABBChecker(in AABB2DInt area) => _area = area;
        public bool CheckNode(in AABB2DInt bounds) => bounds.Intersect(in _area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Intersect(in _area);
    }
    #endregion

    #region OBB
    internal readonly struct OBBChecker : IIntersectChecker
    {
        private readonly AABB2DInt _area;
        private readonly RectChecker _checker;

        internal OBBChecker(in AABB2DInt area, in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
        {
            _area = area;
            _checker = new RectChecker(in leftBottom, in rightBottom, in rightTop, in leftTop);
        }
        public bool CheckNode(in AABB2DInt bounds) => bounds.Intersect(in _area);
        public bool CheckElement(in AABB2DInt bounds) => bounds.Intersect(in _area) && _checker.Intersect(in bounds);
    }
    #endregion

    #region Polygon
    internal readonly struct PolygonChecker : IIntersectChecker
    {
        private readonly AABB2DInt _area;
        private readonly QuadrangleChecker _checker;

        internal PolygonChecker(in AABB2DInt area, in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
        {
            _area = area;
            _checker = new QuadrangleChecker(in leftBottom, in rightBottom, in rightTop, in leftTop);
        }
        public bool CheckNode(in AABB2DInt bounds) => bounds.Intersect(in _area);
        public bool CheckElement(in AABB2DInt bounds) => _checker.Intersect(in bounds);
    }
    #endregion

    #region Tools
    internal readonly struct RectChecker
    {
        private readonly Vector2D _lb;
        private readonly Vector2D _rb;
        private readonly Vector2D _rt;
        private readonly Vector2D _lt;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RectChecker(in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
        {
            _lb = leftBottom;
            _rb = rightBottom;
            _rt = rightTop;
            _lt = leftTop;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt aabb)
        {
            var lb = new Vector2D(aabb.Left(), aabb.Bottom());
            var rb = new Vector2D(aabb.Right(), aabb.Bottom());
            var rt = new Vector2D(aabb.Right(), aabb.Top());
            var lt = new Vector2D(aabb.Left(), aabb.Top());

            return IsIn(in lb, in _lb, in _rb, in _rt, in _lt) || IsIn(in rb, in _lb, in _rb, in _rt, in _lt) || IsIn(in rt, in _lb, in _rb, in _rt, in _lt) || IsIn(in lt, in _lb, in _rb, in _rt, in _lt) || IsIn(in _lb, in lb, in rb, in rt, in lt) || IsIn(in _rb, in lb, in rb, in rt, in lt) || IsIn(in _rt, in lb, in rb, in rt, in lt) || IsIn(in _lt, in lb, in rb, in rt, in lt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsIn(in Vector2D point, in Vector2D lb, in Vector2D rb, in Vector2D rt, in Vector2D lt)
        {
            return Vector2D.Dot(point - lb, lt - lb).RawValue >= 0 && Vector2D.Dot(point - rb, lb - rb).RawValue >= 0 && Vector2D.Dot(point - rt, rb - rt).RawValue >= 0 && Vector2D.Dot(point - lt, rt - lt).RawValue >= 0;
        }
    }

    internal readonly struct QuadrangleChecker
    {
        private readonly Vector2D _lb;
        private readonly Vector2D _rb;
        private readonly Vector2D _rt;
        private readonly Vector2D _lt;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadrangleChecker(in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
        {
            _lb = leftBottom;
            _rb = rightBottom;
            _rt = rightTop;
            _lt = leftTop;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt aabb)
        {
            return IsIn(aabb.LeftBottom()) || IsIn(aabb.RightBottom()) || IsIn(aabb.RightTop()) || IsIn(aabb.LeftTop());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsIn(in Vector2D point)
        {
            var p0 = (_lt.X - _lb.X) * (point.Y - _lb.Y) + (_lb.X - point.X) * (_lt.Y - _lb.Y);
            var p1 = (_rt.X - _lt.X) * (point.Y - _lt.Y) + (_lt.X - point.X) * (_rt.Y - _lt.Y);
            var p2 = (_rb.X - _rt.X) * (point.Y - _rt.Y) + (_rt.X - point.X) * (_rb.Y - _rt.Y);
            var p3 = (_lb.X - _rb.X) * (point.Y - _rb.Y) + (_rb.X - point.X) * (_lb.Y - _rb.Y);

            return p0.RawValue >= 0 && p1.RawValue >= 0 && p2.RawValue >= 0 && p3.RawValue >= 0 || p0.RawValue <= 0 && p1.RawValue <= 0 && p2.RawValue <= 0 && p3.RawValue <= 0;
        }
    }
    #endregion
}