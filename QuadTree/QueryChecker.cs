using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    #region Api
    internal interface IQueryIntersectChecker
    {
        bool CheckNode(in AABB2DInt aabb);
        bool CheckElement(in AABB2DInt aabb);
    }
    #endregion

    #region QueryBox
    internal readonly struct QueryBoxNodeCircleChecker : IQueryIntersectChecker
    {
        private readonly AABB2DInt _box;

        internal QueryBoxNodeCircleChecker(in AABB2DInt box) => _box = box;
        public bool CheckNode(in AABB2DInt aabb) => _box.Intersect_Box_Box(in aabb);
        public bool CheckElement(in AABB2DInt aabb) => _box.Intersect_Box_Circle(in aabb);
    }

    internal readonly struct QueryBoxNodeBoxChecker : IQueryIntersectChecker
    {
        private readonly AABB2DInt _box;

        internal QueryBoxNodeBoxChecker(in AABB2DInt box) => _box = box;
        public bool CheckNode(in AABB2DInt aabb) => _box.Intersect_Box_Box(in aabb);
        public bool CheckElement(in AABB2DInt aabb) => _box.Intersect_Box_Box(in aabb);
    }
    #endregion

    #region QueryCircle
    internal readonly struct QueryCircleNodeCircleChecker : IQueryIntersectChecker
    {
        private readonly AABB2DInt _circle;

        internal QueryCircleNodeCircleChecker(in AABB2DInt circle) => _circle = circle;
        public bool CheckNode(in AABB2DInt aabb) => _circle.Intersect_Box_Box(in aabb);
        public bool CheckElement(in AABB2DInt aabb) => _circle.Intersect_Circle_Circle(in aabb);
    }

    internal readonly struct QueryCircleNodeBoxChecker : IQueryIntersectChecker
    {
        private readonly AABB2DInt _circle;

        internal QueryCircleNodeBoxChecker(in AABB2DInt circle) => _circle = circle;
        public bool CheckNode(in AABB2DInt aabb) => _circle.Intersect_Box_Box(in aabb);
        public bool CheckElement(in AABB2DInt aabb) => _circle.Intersect_Circle_Box(in aabb);
    }
    #endregion

    #region QueryRectangle
    internal readonly struct QueryRectangleChecker : IQueryIntersectChecker
    {
        private readonly AABB2DInt _aabb;
        private readonly RectangleChecker _checker;

        internal QueryRectangleChecker(in AABB2DInt aabb, in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
        {
            _aabb = aabb;
            _checker = new RectangleChecker(in leftBottom, in rightBottom, in rightTop, in leftTop);
        }
        public bool CheckNode(in AABB2DInt aabb) => _aabb.Intersect_Box_Box(in aabb);
        public bool CheckElement(in AABB2DInt aabb) => _aabb.Intersect_Box_Box(in aabb) && _checker.Intersect(in aabb);
    }
    #endregion

    #region QueryQuadrangle
    internal readonly struct QueryQuadrangleChecker : IQueryIntersectChecker
    {
        private readonly AABB2DInt _aabb;
        private readonly QuadrangleChecker _checker;

        internal QueryQuadrangleChecker(in AABB2DInt aabb, in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
        {
            _aabb = aabb;
            _checker = new QuadrangleChecker(in leftBottom, in rightBottom, in rightTop, in leftTop);
        }
        public bool CheckNode(in AABB2DInt aabb) => _aabb.Intersect_Box_Box(in aabb);
        public bool CheckElement(in AABB2DInt aabb) => _checker.Intersect(in aabb);
    }
    #endregion

    #region Tools
    internal readonly struct RectangleChecker
    {
        private readonly Vector2D _lb;
        private readonly Vector2D _rb;
        private readonly Vector2D _rt;
        private readonly Vector2D _lt;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RectangleChecker(in Vector2D leftBottom, in Vector2D rightBottom, in Vector2D rightTop, in Vector2D leftTop)
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

            return IsIn(in lb, in _lb, in _rb, in _rt, in _lt) ||
                   IsIn(in rb, in _lb, in _rb, in _rt, in _lt) ||
                   IsIn(in rt, in _lb, in _rb, in _rt, in _lt) ||
                   IsIn(in lt, in _lb, in _rb, in _rt, in _lt) ||
                   IsIn(in _lb, in lb, in rb, in rt, in lt) ||
                   IsIn(in _rb, in lb, in rb, in rt, in lt) ||
                   IsIn(in _rt, in lb, in rb, in rt, in lt) ||
                   IsIn(in _lt, in lb, in rb, in rt, in lt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsIn(in Vector2D point, in Vector2D lb, in Vector2D rb, in Vector2D rt, in Vector2D lt)
        {
            return Vector2D.Dot(point - lb, lt - lb).RawValue >= 0 &&
                   Vector2D.Dot(point - rb, lb - rb).RawValue >= 0 &&
                   Vector2D.Dot(point - rt, rb - rt).RawValue >= 0 &&
                   Vector2D.Dot(point - lt, rt - lt).RawValue >= 0;
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
            return IsIn(aabb.LeftBottom()) ||
                   IsIn(aabb.RightBottom()) ||
                   IsIn(aabb.RightTop()) ||
                   IsIn(aabb.LeftTop());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsIn(in Vector2D point)
        {
            var p0 = (_lt.X - _lb.X) * (point.Y - _lb.Y) + (_lb.X - point.X) * (_lt.Y - _lb.Y);
            var p1 = (_rt.X - _lt.X) * (point.Y - _lt.Y) + (_lt.X - point.X) * (_rt.Y - _lt.Y);
            var p2 = (_rb.X - _rt.X) * (point.Y - _rt.Y) + (_rt.X - point.X) * (_rb.Y - _rt.Y);
            var p3 = (_lb.X - _rb.X) * (point.Y - _rb.Y) + (_rb.X - point.X) * (_lb.Y - _rb.Y);

            return p0.RawValue >= 0 && p1.RawValue >= 0 && p2.RawValue >= 0 && p3.RawValue >= 0 ||
                   p0.RawValue <= 0 && p1.RawValue <= 0 && p2.RawValue <= 0 && p3.RawValue <= 0;
        }
    }
    #endregion
}