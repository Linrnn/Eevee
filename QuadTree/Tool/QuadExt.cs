using Eevee.Fixed;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    internal static class QuadExt
    {
        internal const int ChildCount = 4; // 节点的子节点数量
        internal const QuadCountNodeMode CountMode = QuadCountNodeMode.NotIntersect;

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
        internal static Vector2DInt GetHalfBoundary(in AABB2DInt maxBoundary, int depth) => new(maxBoundary.X >> depth, maxBoundary.Y >> depth);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrtGetArea(in AABB2DInt maxBoundary, in AABB2DInt aabb, QuadCountNodeMode mode, out AABB2DInt area)
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
        internal static bool TrtGetNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt area, QuadCountNodeMode mode, out QuadIndex index)
        {
            for (int depth = maxDepth; depth >= 0; --depth)
            {
                var boundary = GetHalfBoundary(in maxBoundary, depth);
                if (boundary.X < area.W || boundary.Y < area.H)
                    continue;

                int left = maxBoundary.Left();
                int top = maxBoundary.Top();
                GetNodeIndex(area.X, area.Y, left, top, boundary.X, boundary.Y, out int ix, out int iy);
                index = new QuadIndex(depth, ix, iy);
                return true;
            }

            index = QuadIndex.Invalid;
            return false;
        }

        /// <summary>
        /// 通过节点索引获得父节点的索引
        /// </summary>
        /// <param name="cix">索引的横值</param>
        /// <param name="ciy">索引的纵值</param>
        /// <param name="pix">父节点索引的横值</param>
        /// <param name="piy">父节点索引的横值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetParentIndex(int cix, int ciy, out int pix, out int piy)
        {
            pix = cix << 1;
            piy = ciy << 1;
        }
        /// <summary>
        /// 通过坐标获得节点索引
        /// </summary>
        /// <param name="px">四叉树的横坐标</param>
        /// <param name="py">四叉树的纵坐标</param>
        /// <param name="bl">四叉树的左边界</param>
        /// <param name="bt">四叉树的上边界</param>
        /// <param name="hw">当前层级边界尺寸的半宽</param>
        /// <param name="hh">当前层级边界尺寸的半高</param>
        /// <param name="ix">索引的横值</param>
        /// <param name="iy">索引的纵值</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodeIndex(int px, int py, int bl, int bt, int hw, int hh, out int ix, out int iy)
        {
            ix = (px - bl) / (hw << 1);
            iy = (bt - py) / (hh << 1);
        }
        /// <summary>
        /// 通过索引获得节点中心点
        /// </summary>
        /// <param name="ix">索引的横值</param>
        /// <param name="iy">索引的纵值</param>
        /// <param name="bl">四叉树的左边界</param>
        /// <param name="bt">四叉树的上边界</param>
        /// <param name="hw">当前层级边界尺寸的半宽</param>
        /// <param name="hh">当前层级边界尺寸的半高</param>
        /// <param name="px">四叉树的横坐标</param>
        /// <param name="py">四叉树的纵坐标</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodeCenter(int ix, int iy, int bl, int bt, int hw, int hh, out int px, out int py)
        {
            px = bl + ix * (hw << 1);
            py = bt - iy * (hh << 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeId(this QuadIndex index) => index.IsValid() ? GetNodeId(index.Depth, index.X, index.Y) : -1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNodeId(int depth, int x, int y) => x + (1 << depth) * y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Exception BuildShapeException(int treeId, QuadShape shape) => new NotImplementedException($"TreeId:{treeId}, Shape:{shape} not implement.");
    }
}