using Eevee.Fixed;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    internal static class QuadTreeNodeExt
    {
        internal const int ChildSideCount = 2; // Sqrt(ChildCount)
        internal const int ChildCount = 4; // 节点的子节点数量
        internal const QuadTreeCountNodeMode ElementCountMode = QuadTreeCountNodeMode.NotIntersect;
        internal const QuadTreeCountNodeMode QueryCountMode = QuadTreeCountNodeMode.IntersectOffset;

        /// <summary>
        /// 每一层的节点数量的根号
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeSideCount(int depth) => 1 << depth;
        /// <summary>
        /// 每一层的节点数量
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeCount(int depth) => 1 << depth << depth;
        /// <summary>
        /// 每一层的边界尺寸<br/>
        /// 四分之一的面积
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2DInt GetDepthExtents(in AABB2DInt maxBoundary, int depth) => new(maxBoundary.W >> depth, maxBoundary.H >> depth);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrtGetArea(in AABB2DInt maxBoundary, in AABB2DInt shape, QuadTreeCountNodeMode mode, out AABB2DInt area)
        {
            if (mode == QuadTreeCountNodeMode.NotIntersect)
            {
                area = shape;
                return true;
            }

            if (Geometry.UnsafeIntersect(in shape, in maxBoundary, out var intersect)) // 处理边界，减少触发“LooseBoundary.Contain()”的次数
            {
                area = intersect;
                return true;
            }

            switch (mode)
            {
                case QuadTreeCountNodeMode.OnlyIntersect:
                    area = default;
                    return false;

                case QuadTreeCountNodeMode.IntersectOffset:
                    int x = intersect.X;
                    int y = intersect.Y;
                    int w = intersect.W;
                    int h = intersect.H;
                    if (intersect.W < 0)
                    {
                        x = intersect.X < maxBoundary.X ? intersect.X - intersect.W : intersect.X + intersect.W - 1;
                        w = 0;
                    }
                    if (intersect.H < 0)
                    {
                        y = intersect.Y < maxBoundary.Y ? intersect.Y - intersect.H : intersect.Y + intersect.H - 1;
                        h = 0;
                    }
                    area = new AABB2DInt(x, y, w, h);
                    return true;

                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, "Error!");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrtGetNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt area, out QuadTreeIndex index)
        {
            for (int depth = maxDepth; depth >= 0; --depth)
            {
                var extents = GetDepthExtents(in maxBoundary, depth);
                if (extents.X < area.W || extents.Y < area.H)
                    continue;

                int left = maxBoundary.Left();
                int bottom = maxBoundary.Bottom();
                var idx = GetNodeIndex(area.X, area.Y, left, bottom, extents.X, extents.Y);
                index = new QuadTreeIndex(depth, idx.X, idx.Y);
                return true;
            }

            index = QuadTreeIndex.Invalid;
            return false;
        }

        /// <summary>
        /// 通过坐标获得节点索引
        /// </summary>
        /// <param name="bx">四叉树的横坐标</param>
        /// <param name="by">四叉树的纵坐标</param>
        /// <param name="bb">四叉树的左边界</param>
        /// <param name="bt">四叉树的下边界</param>
        /// <param name="hw">当前层级边界尺寸的半宽</param>
        /// <param name="hh">当前层级边界尺寸的半高</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2DInt GetNodeIndex(int bx, int by, int bb, int bt, int hw, int hh) => new()
        {
            X = (bx - bb) / (hw << 1),
            Y = (by - bt) / (hh << 1),
        };
        /// <summary>
        /// 通过索引获得节点中心点
        /// </summary>
        /// <param name="ix">索引的横值</param>
        /// <param name="iy">索引的纵值</param>
        /// <param name="bl">四叉树的左边界</param>
        /// <param name="bb">四叉树的下边界</param>
        /// <param name="hw">当前层级边界尺寸的半宽</param>
        /// <param name="hh">当前层级边界尺寸的半高</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2DInt GetNodeCenter(int ix, int iy, int bl, int bb, int hw, int hh) => new()
        {
            X = bl + hw + ix * (hw << 1),
            Y = bb + hh + iy * (hh << 1),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeId(this QuadTreeIndex index) => index.IsValid() ? GetNodeId(index.Depth, index.X, index.Y) : -1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeId(int depth, int x, int y) => x + (1 << depth) * y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetChildId(this QuadTreeIndex index) => GetChildId(index.X, index.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetChildId(int x, int y) => x & 1 | (y & 1) << 1; // 相对于父节点的索引

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception BuildShapeException(int treeId, QuadTreeShape shape) => new NotImplementedException($"TreeId:{treeId}, Shape:{shape} not implement.");
    }
}