using Eevee.Fixed;
using System.Buffers;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 静态网格 + 四叉树
    /// </summary>
    public sealed class MeshQuadTree : BasicQuadTree
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
            var rootIndex = QuadTreeIndex.Root;
            var node = new QuadTreeNode();
            node.OnAlloc(in boundary, in boundary, rootIndex.Depth, rootIndex.X, rootIndex.Y, null, _elementPool);
            return node;
        }
        internal override QuadTreeNode CreateNode(int depth, int x, int y, QuadTreeNode parent)
        {
            int childId = QuadTreeNodeExt.GetChildId(x, y);
            var boundary = parent.GetChildBoundary(childId);
            var node = new QuadTreeNode();
            node.OnAlloc(in boundary, in boundary, depth, x, y, parent, _elementPool);
            return node;
        }
        internal override QuadTreeNode GetOrAddNode(int depth, int x, int y) => _nodes[depth][x, y];
        internal override QuadTreeNode GetNode(int depth, int x, int y) => _nodes[depth][x, y];

        internal override void GetNodes(ICollection<QuadTreeNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadTreeNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        internal override bool TryGetNodeIndex(in AABB2DInt shape, QuadTreeCountNodeMode mode, out QuadTreeIndex index)
        {
            if (!QuadTreeExt.TryGetNodeIndex(in _maxBoundary, _maxDepth, in shape, mode, out var area, out var idx))
            {
                index = QuadTreeIndex.Invalid;
                return false;
            }

            for (var node = GetNode(idx.Depth, idx.X, idx.Y); node is not null; node = node.Parent)
            {
                if (!Geometry.Contain(in node.Boundary, in area))
                    continue;

                index = node.Index;
                return true;
            }

            index = QuadTreeIndex.Invalid;
            return false;
        }
        protected override void IterateQuery<TChecker>(in TChecker checker, in QuadTreeIndex index, ICollection<QuadTreeElement> elements)
        {
            var node = GetNode(index.Depth, index.X, index.Y);
            if (node is null)
                return;

            IterateQueryParent(in checker, node.Parent, elements);
            IterateQueryChildren(in checker, node, elements);
        }
        #endregion
    }
}