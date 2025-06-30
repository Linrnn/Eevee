using Eevee.Collection;
using Eevee.Fixed;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 静态网格 + 松散四叉树
    /// </summary>
    public sealed class LooseQuadTree : BasicQuadTree
    {
        #region Field
        private QuadNode[][,] _nodes;
        #endregion

        #region BasicQuadTree
        internal override void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary, ArrayPool<QuadElement> pool)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary, pool);
            QuadTreeExt.OnCreate(this, _root, depthCount, out _nodes);
        }
        internal override void OnDestroy()
        {
            QuadTreeExt.OnDestroy(ref _nodes);
            base.OnDestroy();
        }

        internal override QuadNode CreateRoot()
        {
            var boundary = _maxBoundary;
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());
            var rootIndex = QuadIndex.Root;
            var node = new QuadNode();
            node.OnAlloc(in boundary, in looseBoundary, rootIndex.Depth, rootIndex.X, rootIndex.Y, null, _elementPool);
            return node;
        }
        internal override QuadNode CreateNode(int depth, int x, int y, QuadNode parent)
        {
            int childId = QuadExt.GetChildId(x, y);
            var boundary = parent.GetChildBoundary(childId);
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());

            var node = new QuadNode();
            node.OnAlloc(in boundary, in looseBoundary, depth, x, y, parent, _elementPool);
            return node;
        }
        internal override QuadNode GetOrAddNode(int depth, int x, int y) => _nodes[depth][x, y];
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][x, y];

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        internal override bool TryGetNodeIndex(in AABB2DInt shape, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadTreeExt.TryGetNodeIndex(in _maxBoundary, _maxDepth, in shape, mode, out _, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }
        protected override void IterateQuery<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            int count = QuadExt.GetNodeSideCount(index.Depth) - 1;
            int si = Math.Max(index.X - 1, 0);
            int ei = Math.Min(index.X + 1, count);
            int sj = Math.Max(index.Y - 1, 0);
            int ej = Math.Min(index.Y + 1, count);

            int iteratedCount = (ei - si) * (ej - ej) + (index.Depth << QuadExt.ChildSideCount);
            StackAllocSet<QuadNode>.GetSize(ref iteratedCount, out int scale, out int capacity);
            var iterated = new StackAllocSet<QuadNode>(scale, stackalloc int[iteratedCount], stackalloc byte[capacity]);

            for (int i = si; i <= ei; ++i)
            {
                for (int j = sj; j <= ej; ++j)
                {
                    var node = GetNode(index.Depth, si, sj);
                    if (!checker.CheckNode(in node.Boundary))
                        continue;

                    IterateQueryParentCheckRepeat(in checker, node.Parent, ref iterated, elements);
                    IterateQueryChildren(in checker, node, elements);
                }
            }

            iterated.Dispose();
        }
        #endregion
    }
}