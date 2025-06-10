using Eevee.Fixed;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    internal static class QuadTreeExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static QuadNode[][] OnCreate(QuadTreeBasic tree, QuadNode root, int depthCount, in AABB2DInt maxBoundary, IList<Vector2DInt> halfBoundaries)
        {
            var nodes = new QuadNode[depthCount][];
            nodes[0] = new[] { root };

            for (int left = maxBoundary.Left(), top = maxBoundary.Top(), upperDepth = 0, depth = 1; depth < depthCount; ++upperDepth, ++depth)
            {
                int length = 1 << depth << depth; // length = 4^depth
                var upperNodes = nodes[upperDepth];
                var depthNodes = new QuadNode[length];

                halfBoundaries[depth] = halfBoundaries[upperDepth] / 2;
                nodes[depth] = depthNodes;

                for (int i = 0; i < length; i += QuadExt.ChildCount)
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
        internal static QuadNode GetNode(this QuadTreeBasic tree, in QuadIndex index) => index.IsValid() ? tree.GetNode(index.Depth, index.X, index.Y) : null;
    }
}