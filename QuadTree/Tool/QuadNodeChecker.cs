﻿using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    #region Point
    internal readonly struct IQuadPoint2CircleChecker : IQuadChecker
    {
        internal readonly Vector2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadPoint2CircleChecker(Vector2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(Converts.AsCircleInt(in boundary), Shape);
    }

    internal readonly struct IQuadPoint2AABBChecker : IQuadChecker
    {
        internal readonly Vector2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadPoint2AABBChecker(Vector2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
    }
    #endregion

    #region Circle
    internal readonly struct IQuadCircle2CircleChecker : IQuadChecker
    {
        internal readonly CircleInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadCircle2CircleChecker(in CircleInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(Converts.AsCircleInt(in boundary), in Shape);
    }

    internal readonly struct IQuadCircle2AABBChecker : IQuadChecker
    {
        internal readonly CircleInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadCircle2AABBChecker(in CircleInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
    }
    #endregion

    #region AABB
    internal readonly struct IQuadAABB2CircleChecker : IQuadChecker
    {
        internal readonly AABB2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadAABB2CircleChecker(in AABB2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, Converts.AsCircleInt(in boundary));
    }

    internal readonly struct IQuadAABB2AABBChecker : IQuadChecker
    {
        internal readonly AABB2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadAABB2AABBChecker(in AABB2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
    }
    #endregion

    #region OBB
    internal readonly struct IQuadOBB2CircleChecker : IQuadChecker
    {
        internal readonly OBB2DInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadOBB2CircleChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;

            Origin = shape;
            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary)
        {
            var shape = Converts.AsCircleInt(in boundary);
            return Geometry.Intersect(in Shape, in shape) && Geometry.Intersect(in Origin, in shape);
        }
    }

    internal readonly struct IQuadOBB2AABBChecker : IQuadChecker
    {
        internal readonly OBB2DInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadOBB2AABBChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;

            Origin = shape;
            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary) && Checker.Intersect(in boundary);
    }
    #endregion

    #region Polygon
    internal readonly struct IQuadPolygon2CircleChecker : IQuadChecker
    {
        internal readonly PolygonInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadPolygon2CircleChecker(in PolygonInt shape)
        {
            var checker = new PolygonIntIntersectChecker(in shape, false, true);

            Origin = shape;
            Shape = AABB2DInt.Create(checker.Shape.Left(), checker.Shape.Right(), checker.Shape.Bottom(), checker.Shape.Top());
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary)
        {
            var shape = Converts.AsCircleInt(in boundary);
            return Geometry.Intersect(in Shape, in shape) && Checker.Intersect(in shape);
        }
    }

    internal readonly struct IQuadPolygon2AABBChecker : IQuadChecker
    {
        internal readonly PolygonInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IQuadPolygon2AABBChecker(in PolygonInt shape)
        {
            var checker = new PolygonIntIntersectChecker(in shape, true, false);

            Origin = shape;
            Shape = AABB2DInt.Create(checker.Shape.Left(), checker.Shape.Right(), checker.Shape.Bottom(), checker.Shape.Top());
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary) && Checker.Intersect(in boundary);
    }
    #endregion
}