using Eevee.Fixed;

namespace Eevee.QuadTree
{
    #region Point
    internal readonly struct PointCircleNodeChecker : INodeChecker
    {
        internal readonly Vector2DInt Shape;

        internal PointCircleNodeChecker(Vector2DInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
    }

    internal readonly struct PointAABBNodeChecker : INodeChecker
    {
        private readonly Vector2DInt _shape;

        internal PointAABBNodeChecker(Vector2DInt shape) => _shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, _shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(in boundary, _shape);
    }
    #endregion

    #region Circle
    internal readonly struct CircleNodeChecker : INodeChecker
    {
        internal readonly CircleInt Shape;

        internal CircleNodeChecker(in CircleInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
    }

    internal readonly struct CircleAABBNodeChecker : INodeChecker
    {
        private readonly CircleInt _shape;

        internal CircleAABBNodeChecker(in CircleInt shape) => _shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in _shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in _shape);
    }
    #endregion

    #region AABB
    internal readonly struct AABBCircleNodeChecker : INodeChecker
    {
        internal readonly AABB2DInt Shape;

        internal AABBCircleNodeChecker(in AABB2DInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
    }

    internal readonly struct AABBNodeChecker : INodeChecker
    {
        private readonly AABB2DInt _shape;

        internal AABBNodeChecker(in AABB2DInt shape) => _shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in _shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in _shape);
    }
    #endregion

    #region OBB
    internal readonly struct OBBNodeChecker : INodeChecker
    {
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;

        internal OBBNodeChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;

            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape) && Checker.Intersect(in boundary);
    }
    #endregion

    #region Polygon
    internal readonly struct PolygonNodeChecker : INodeChecker
    {
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;

        internal PolygonNodeChecker(in AABB2DInt shape, in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3)
        {
            Shape = shape;
            Checker = new PolygonIntIntersectChecker(in p0, in p1, in p2, in p3);
        }

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Checker.Intersect(in boundary);
    }
    #endregion
}