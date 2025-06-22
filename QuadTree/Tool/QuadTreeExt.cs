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
        internal static void OnCreate(BasicQuadTree tree, QuadNode root, int depthCount, out QuadNode[][,] nodes)
        {
            int rootSideCount = QuadExt.GetNodeSideCount(0);
            var rootIndex = QuadIndex.Root;
            var rootNodes = new QuadNode[rootSideCount, rootSideCount];
            rootNodes[rootIndex.X, rootIndex.Y] = root;
            nodes = new QuadNode[depthCount][,];
            nodes[rootIndex.Depth] = rootNodes;

            for (int depth = rootIndex.Depth + 1; depth < depthCount; ++depth)
            {
                int sideCount = QuadExt.GetNodeSideCount(depth);
                var upperNodes = nodes[depth - 1];
                var depthNodes = new QuadNode[sideCount, sideCount];
                nodes[depth] = depthNodes;

                for (int px = 0; px < sideCount; px += QuadExt.ChildSideCount)
                {
                    for (int py = 0; py < sideCount; py += QuadExt.ChildSideCount)
                    {
                        var parent = upperNodes[px >> 1, py >> 1];
                        for (int cx = 0; cx < QuadExt.ChildSideCount; ++cx)
                        {
                            for (int cy = 0; cy < QuadExt.ChildSideCount; ++cy)
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
        internal static void OnDestroy(ref QuadNode[][,] nodes)
        {
            foreach (var depthNodes in nodes)
            foreach (var node in depthNodes)
                node.OnRelease();
            nodes = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt shape, QuadCountNodeMode mode, out AABB2DInt area, out QuadIndex index)
        {
            if (!QuadExt.TrtGetArea(in maxBoundary, in shape, mode, out area))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            if (!QuadExt.TrtGetNodeIndex(in maxBoundary, maxDepth, in area, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodes(QuadNode[][,] nodes, ICollection<QuadNode> returnNodes)
        {
            foreach (var depthNodes in nodes)
            foreach (var node in depthNodes)
                returnNodes.Add(node);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetNodes(QuadNode[][,] nodes, int depth, ICollection<QuadNode> returnNodes)
        {
            foreach (var node in nodes[depth])
                returnNodes.Add(node);
        }
    }
}