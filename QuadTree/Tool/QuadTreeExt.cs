using Eevee.Fixed;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// QuadTreeBasic子类的拓展
    /// </summary>
    internal static class QuadTreeExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnCreate(BasicQuadTree tree, QuadTreeNode root, int depthCount, out QuadTreeNode[][,] nodes)
        {
            int rootSideCount = QuadTreeNodeExt.GetNodeSideCount(0);
            var rootIndex = QuadTreeIndex.Root;
            var rootNodes = new QuadTreeNode[rootSideCount, rootSideCount];
            rootNodes[rootIndex.X, rootIndex.Y] = root;
            nodes = new QuadTreeNode[depthCount][,];
            nodes[rootIndex.Depth] = rootNodes;

            for (int depth = rootIndex.Depth + 1; depth < depthCount; ++depth)
            {
                int sideCount = QuadTreeNodeExt.GetNodeSideCount(depth);
                var upperNodes = nodes[depth - 1];
                var depthNodes = new QuadTreeNode[sideCount, sideCount];
                nodes[depth] = depthNodes;

                for (int px = 0; px < sideCount; px += QuadTreeNodeExt.ChildSideCount)
                {
                    for (int py = 0; py < sideCount; py += QuadTreeNodeExt.ChildSideCount)
                    {
                        var parent = upperNodes[px >> 1, py >> 1];
                        for (int cx = 0; cx < QuadTreeNodeExt.ChildSideCount; ++cx)
                        {
                            for (int cy = 0; cy < QuadTreeNodeExt.ChildSideCount; ++cy)
                            {
                                int x = px | cx;
                                int y = py | cy;
                                var child = tree.CreateNode(depth, x, y, parent);
                                parent.AddChild(child);
                                depthNodes[x, y] = child;
                            }
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnDestroy(ref QuadTreeNode[][,] nodes)
        {
            foreach (var depthNodes in nodes)
            foreach (var node in depthNodes)
                node.OnRelease();
            nodes = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt shape, QuadTreeCountNodeMode mode, out AABB2DInt area, out QuadTreeIndex index)
        {
            if (!QuadTreeNodeExt.TrtGetArea(in maxBoundary, in shape, mode, out area))
            {
                index = QuadTreeIndex.Invalid;
                return false;
            }

            if (!QuadTreeNodeExt.TrtGetNodeIndex(in maxBoundary, maxDepth, in area, out var idx))
            {
                index = QuadTreeIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodes(QuadTreeNode[][,] nodes, ICollection<QuadTreeNode> returnNodes)
        {
            foreach (var depthNodes in nodes)
            foreach (var node in depthNodes)
                returnNodes.Add(node);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodes(QuadTreeNode[][,] nodes, int depth, ICollection<QuadTreeNode> returnNodes)
        {
            foreach (var node in nodes[depth])
                returnNodes.Add(node);
        }
    }
}