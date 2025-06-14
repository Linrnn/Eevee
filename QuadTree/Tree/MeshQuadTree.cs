using Eevee.Fixed;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 静态网格 + 四叉树
    /// </summary>
    public sealed class MeshQuadTree : BasicQuadTree
    {
        #region 数据
        private QuadNode[][,] _nodes;
        #endregion

        #region 复写
        internal override void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary);
            QuadTreeExt.OnCreate(this, _root, depthCount, in maxBoundary, out _nodes);
        }
        internal override void OnDestroy()
        {
            QuadTreeExt.OnDestroy(ref _nodes);
            base.OnDestroy();
        }

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int x, int y, int childId, QuadNode parent)
        {
            var node = new QuadNode();
            node.OnAlloc(in boundary, in boundary, depth, x, y, childId, parent);
            return node;
        }
        internal override QuadNode GetOrAddNode(int depth, int x, int y, bool allowAdd) => _nodes[depth][x, y];
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

            for (var node = GetOrAddNode(idx.Depth, idx.X, idx.Y, false); node is not null; node = node.Parent)
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
            var node = GetOrAddNode(index.Depth, index.X, index.Y, false);
            if (node is null)
                return;

            IterateQueryParent(in checker, node.Parent, elements);
            IterateQueryChildren(in checker, node, elements);
        }
        #endregion
    }
}