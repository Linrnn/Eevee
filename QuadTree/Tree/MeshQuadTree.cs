using Eevee.Fixed;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 静态网格 + 四叉树
    /// </summary>
    public sealed class MeshQuadTree : BasicQuadTree
    {
        #region Field
        private QuadNode[][,] _nodes;
        #endregion

        #region BasicQuadTree
        internal override void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary);
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
            var rootIndex = QuadIndex.Root;
            var node = new QuadNode();
            node.OnAlloc(in boundary, in boundary, rootIndex.Depth, rootIndex.X, rootIndex.Y, null);
            return node;
        }
        internal override QuadNode CreateNode(int depth, int x, int y, QuadNode parent)
        {
            int childId = QuadExt.GetChildId(x, y);
            var boundary = parent.GetChildBoundary(childId);
            var node = new QuadNode();
            node.OnAlloc(in boundary, in boundary, depth, x, y, parent);
            return node;
        }
        internal override QuadNode GetOrAddNode(int depth, int x, int y) => _nodes[depth][x, y];
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][x, y];
        internal override void RemoveNode(QuadNode node) { }

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        internal override bool TryGetNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadTreeExt.TryGetNodeIndex(in _maxBoundary, _maxDepth, in aabb, mode, out var area, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            for (var node = GetNode(idx.Depth, idx.X, idx.Y); node is not null; node = node.Parent)
            {
                if (!Geometry.Contain(in node.Boundary, in area))
                    continue;

                index = node.Index;
                return true;
            }

            index = QuadIndex.Invalid;
            return false;
        }
        protected override void IterateQuery<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
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