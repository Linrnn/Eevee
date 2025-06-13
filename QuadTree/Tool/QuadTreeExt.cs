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
        internal static void OnCreate(BasicQuadTree tree, QuadNode root, int depthCount, in AABB2DInt maxBoundary, out QuadNode[][] nodes)
        {
            nodes = new QuadNode[depthCount][];
            nodes[0] = new[] { root };

            for (int left = maxBoundary.Left(), top = maxBoundary.Top(), upperDepth = 0, depth = 1; depth < depthCount; ++upperDepth, ++depth)
            {
                int nodeCount = QuadExt.GetNodeCount(depth);
                var upperNodes = nodes[upperDepth];
                var depthNodes = new QuadNode[nodeCount];

                nodes[depth] = depthNodes;

                for (int i = 0; i < nodeCount; i += QuadExt.ChildCount)
                {
                    var parent = upperNodes[i / QuadExt.ChildCount];
                    for (int childId = 0; childId < QuadExt.ChildCount; ++childId)
                    {
                        var boundary = parent.CountChildBoundary(childId);
                        QuadExt.GetNodeIndex(boundary.X, boundary.Y, left, top, boundary.W, boundary.H, out int ix, out int iy);
                        var child = tree.CreateNode(in boundary, depth, ix, iy, childId, parent);

                        parent.Children[childId] = child;
                        depthNodes[QuadExt.GetNodeId(depth, ix, iy)] = child;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnDestroy(ref QuadNode[][] nodes)
        {
            foreach (var depthNodes in nodes)
            foreach (var node in depthNodes)
                node.OnRelease();
            nodes = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt aabb, QuadCountNodeMode mode, out AABB2DInt area, out QuadIndex index)
        {
            if (!QuadExt.TrtGetArea(in maxBoundary, in aabb, mode, out area))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            if (!QuadExt.TrtGetNodeIndex(in maxBoundary, maxDepth, in area, mode, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }

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
    }
}