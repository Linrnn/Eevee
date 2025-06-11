using Eevee.Fixed;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 网格法 + 四叉树
    /// </summary>
    public sealed class MeshQuadTree : QuadTreeBasic
    {
        #region 数据
        private QuadNode[][] _nodes;
        #endregion

        #region 复写
        internal override void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary);
            _nodes = QuadTreeExt.OnCreate(this, _root, depthCount, in maxBoundary);
        }
        internal override void OnDestroy()
        {
            QuadTreeExt.OnDestroy(ref _nodes);
            base.OnDestroy();
        }

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent) => new(in boundary, in boundary, depth, childId, x, y, parent);
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][QuadExt.GetNodeId(depth, x, y)];
        internal override bool CountNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadTreeExt.CountNodeIndex(in _maxBoundary, _maxDepth, in aabb, mode, out var area, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            for (var parent = this.TryGetNode(in idx); parent is not null; parent = parent.Parent)
            {
                if (!Geometry.Contain(in parent.LooseBoundary, in area))
                    continue;

                index = parent.Index;
                return true;
            }

            index = QuadIndex.Invalid;
            return false;
        }

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        protected override void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            var node = this.TryGetNode(in index);
            if (node is null)
                return;

            IterateParent(in checker, node.Parent, elements);
            IterateChildren(in checker, node, elements);
        }
        #endregion
    }
}