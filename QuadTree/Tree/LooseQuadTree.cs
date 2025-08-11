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
        private QuadTreeNode[][,] _nodes;
        #endregion

        #region BasicQuadTree
        internal override void OnCreate(int treeId, QuadTreeShape shape, int depthCount, in AABB2DInt maxBoundary, ArrayPool<QuadTreeElement> pool)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary, pool);
            QuadTreeExt.OnCreate(this, _root, depthCount, out _nodes);
        }
        internal override void OnDestroy()
        {
            QuadTreeExt.OnDestroy(ref _nodes);
            base.OnDestroy();
        }

        internal override QuadTreeNode CreateRoot()
        {
            var boundary = _maxBoundary;
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());
            var rootIndex = QuadTreeIndex.Root;
            var node = new QuadTreeNode();
            node.OnAlloc(in boundary, in looseBoundary, rootIndex.Depth, rootIndex.X, rootIndex.Y, null, _elementPool);
            return node;
        }
        internal override QuadTreeNode CreateNode(int depth, int x, int y, QuadTreeNode parent)
        {
            int childId = QuadTreeNodeExt.GetChildId(x, y);
            var boundary = parent.GetChildBoundary(childId);
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());

            var node = new QuadTreeNode();
            node.OnAlloc(in boundary, in looseBoundary, depth, x, y, parent, _elementPool);
            return node;
        }
        internal override QuadTreeNode GetOrAddNode(int depth, int x, int y) => _nodes[depth][x, y];
        internal override QuadTreeNode GetNode(int depth, int x, int y) => _nodes[depth][x, y];

        internal override void GetNodes(ICollection<QuadTreeNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadTreeNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        internal override bool TryGetNodeIndex(in AABB2DInt shape, QuadTreeCountNodeMode mode, out QuadTreeIndex index)
        {
            if (!QuadTreeExt.TryGetNodeIndex(in _maxBoundary, _maxDepth, in shape, mode, out _, out var idx))
            {
                index = QuadTreeIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }
        protected override void IterateQuery<TChecker>(in TChecker checker, in QuadTreeIndex index, ICollection<QuadTreeElement> elements)
        {
            int count = QuadTreeNodeExt.GetNodeSideCount(index.Depth) - 1;
            int si = Math.Max(index.X - 1, 0);
            int ei = Math.Min(index.X + 1, count);
            int sj = Math.Max(index.Y - 1, 0);
            int ej = Math.Min(index.Y + 1, count);

            int iteratedCount = (ei - si) * (ej - ej) + (index.Depth << QuadTreeNodeExt.ChildSideCount);
            StackAllocSet<QuadTreeNode>.GetSize(ref iteratedCount, out int scale, out int capacity);
            var iterated = new StackAllocSet<QuadTreeNode>(scale, stackalloc int[iteratedCount], stackalloc byte[capacity]);

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