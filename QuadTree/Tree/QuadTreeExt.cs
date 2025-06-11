using Eevee.Collection;
using Eevee.Diagnosis;
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
        internal static QuadNode[][] OnCreate(QuadTreeBasic tree, QuadNode root, int depthCount, in AABB2DInt maxBoundary)
        {
            var nodes = new QuadNode[depthCount][];
            nodes[0] = new[] { root };

            for (int left = maxBoundary.Left(), top = maxBoundary.Top(), upperDepth = 0, depth = 1; depth < depthCount; ++upperDepth, ++depth)
            {
                int nodeCount = QuadExt.CountNodeCount(depth);
                var upperNodes = nodes[upperDepth];
                var depthNodes = new QuadNode[nodeCount];

                nodes[depth] = depthNodes;

                for (int i = 0; i < nodeCount; i += QuadExt.ChildCount)
                {
                    var parent = upperNodes[i / QuadExt.ChildCount];
                    parent.Children = new QuadNode[QuadExt.ChildCount];

                    for (int childId = 0; childId < QuadExt.ChildCount; ++childId)
                    {
                        var boundary = parent.CountChildBoundary(childId);
                        int x = (boundary.X - left) / (boundary.W << 1);
                        int y = (top - boundary.Y) / (boundary.H << 1);
                        var child = tree.CreateNode(in boundary, depth, childId, x, y, parent);

                        parent.Children[childId] = child;
                        depthNodes[QuadExt.GetNodeId(depth, x, y)] = child;
                    }
                }
            }

            return nodes;
        }
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
        internal static bool CountNodeIndex(in AABB2DInt maxBoundary, int maxDepth, in AABB2DInt aabb, QuadCountNodeMode mode, out AABB2DInt area, out QuadIndex index)
        {
            if (!QuadExt.CountArea(in maxBoundary, in aabb, mode, out area))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            if (!QuadExt.CountNodeIndex(in maxBoundary, maxDepth, in area, mode, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static QuadNode TryGetNode(this QuadTreeBasic tree, in QuadIndex index)
        {
            if (index.IsValid())
                return tree.GetNode(index.Depth, index.X, index.Y);

            LogRelay.Error($"[Quad] Index:{index}, is Invalid!");
            return null;
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