using Eevee.Fixed;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    internal readonly struct QuadExt
    {
        internal const int ChildCount = 4; // 节点的子节点数量
        internal const QuadCountNodeMode CountMode = QuadCountNodeMode.NotIntersect;

        /// <summary>
        /// 每一层的节点数量的根号
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CountNodeSideCount(int depth) => 1 << depth;
        /// <summary>
        /// 每一层的节点数量
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CountNodeCount(int depth) => 1 << depth << depth;

        /// <summary>
        /// 每一层的边界尺寸<br/>
        /// 四分之一的面积
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2DInt CountHalfBoundary(in AABB2DInt maxBoundary, int depth) => new(maxBoundary.X >> depth, maxBoundary.Y >> depth);
        /// <summary>
        /// 每一层的边界尺寸
        /// 完整的面积<br/>
        /// depth可以是0，所以先左移1
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2DInt CountBoundary(in AABB2DInt maxBoundary, int depth) => new(maxBoundary.X << 1 >> depth, maxBoundary.Y << 1 >> depth);

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
        internal static bool CountNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt area, QuadCountNodeMode mode, out QuadIndex index)
        {
            for (int depth = maxDepth; depth >= 0; --depth)
            {
                var boundary = QuadExt.CountHalfBoundary(in maxBoundary, depth);
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
        internal static Exception ShapeNotImplementException(int treeId, QuadShape shape) => new NotImplementedException($"TreeId:{treeId}, Shape:{shape} not implement.");
    }
}