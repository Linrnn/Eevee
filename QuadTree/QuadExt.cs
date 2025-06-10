using Eevee.Collection;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    internal readonly struct QuadExt
    {
        internal const int ChildCount = 4; // 每个节点的子节点数量
        internal const QuadCountNodeMode CountMode = QuadCountNodeMode.NotIntersect;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnDestroy(ref QuadNode[][] nodes)
        {
            foreach (var depthNodes in nodes)
            {
                foreach (var node in depthNodes)
                    node.Clean();
                depthNodes.Clean();
            }

            nodes.Clean();
            nodes = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CountArea(in AABB2DInt maxBoundary, in AABB2DInt aabb, QuadCountNodeMode mode, out AABB2DInt area)
        {
            if (mode == QuadCountNodeMode.NotIntersect)
            {
                area = aabb;
                return true;
            }

            if (Geometry.UnsafeIntersect(in aabb, in maxBoundary, out var intersect)) // 处理边界，减少触发“LooseBoundary.Contain()”的次数
            {
                area = intersect;
                return true;
            }

            switch (mode)
            {
                case QuadCountNodeMode.OnlyIntersect:
                    area = default;
                    return false;

                case QuadCountNodeMode.IntersectOffset:
                    area = (intersect.W >= 0, intersect.H >= 0) switch
                    {
                        (false, false) => new AABB2DInt(intersect.X - intersect.W, intersect.Y - intersect.H, 0, 0),
                        (false, true) => new AABB2DInt(intersect.X - intersect.W, intersect.Y, 0, intersect.H),
                        (true, false) => new AABB2DInt(intersect.X, intersect.Y - intersect.H, intersect.W, 0),
                        _ => intersect,
                    };
                    return true;

                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, "Error!");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CountNodeIndex(in AABB2DInt maxBoundary, int maxDepth, IReadOnlyList<Vector2DInt> halfBoundaries, in AABB2DInt area, QuadCountNodeMode mode, out QuadIndex index)
        {
            for (int depth = maxDepth; depth >= 0; --depth)
            {
                var boundary = halfBoundaries[depth];
                if (boundary.X < area.W || boundary.Y < area.H)
                    continue;

                int x = (area.X - maxBoundary.Left()) / (boundary.X << 1);
                int y = (maxBoundary.Top() - area.Y) / (boundary.Y << 1);
                index = new QuadIndex(depth, x, y);
                return true;
            }

            index = QuadIndex.Invalid;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeId(in QuadIndex index) => index.IsValid() ? GetNodeId(index.Depth, index.X, index.Y) : -1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeId(int depth, int x, int y) => x + (1 << depth) * y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodes(QuadNode[][] nodes, ICollection<QuadNode> returnNodes)
        {
            foreach (var depthNodes in nodes)
            foreach (var node in depthNodes)
                returnNodes.Add(node);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodes(QuadNode[][] nodes, int depth, ICollection<QuadNode> returnNodes)
        {
            foreach (var node in nodes[depth])
                returnNodes.Add(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception ShapeNotImplementException(int treeId, QuadShape shape) => new NotImplementedException($"TreeId:{treeId}, Shape:{shape} not implement.");
    }
}