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
        private QuadNode[][] _nodes;
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

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent)
        {
            var node = new QuadNode();
            node.OnAlloc(in boundary, in boundary, depth, childId, x, y, parent);
            return node;
        }
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][QuadExt.GetNodeId(depth, x, y)];
        internal override QuadNode GetOrCreateNode(int depth, int x, int y) => _nodes[depth][QuadExt.GetNodeId(depth, x, y)];

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        internal override bool CountNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadTreeExt.CountNodeIndex(in _maxBoundary, _maxDepth, in aabb, mode, out var area, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            // todo eevee Geometry.Contain(in parent.Boundary, in area) 不应该获取Node
            for (var parent = GetNode(idx.Depth, idx.X, idx.Y); parent is not null; parent = parent.Parent)
            {
                if (!Geometry.Contain(in parent.Boundary, in area))
                    continue;

                index = parent.Index;
                return true;
            }

            index = QuadIndex.Invalid;
            return false;
        }
        protected override void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            var node = GetNode(index.Depth, index.X, index.Y);
            if (node is null)
                return;

            IterateParent(in checker, node.Parent, elements);
            IterateChildren(in checker, node, elements);
        }
        #endregion
    }
}