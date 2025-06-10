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
            _nodes = QuadTreeExt.OnCreate(this, _root, depthCount, in maxBoundary, _halfBoundaries);
        }
        internal override void OnDestroy()
        {
            QuadExt.OnDestroy(ref _nodes);
            base.OnDestroy();
        }

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent) => new(in boundary, in boundary, depth, childId, x, y, parent);
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][QuadExt.GetNodeId(depth, x, y)];
        internal override bool CountNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadExt.CountArea(in _maxBoundary, in aabb, mode, out var area))
            {
                index = default;
                return false;
            }

            bool exist = QuadExt.CountNodeIndex(in _maxBoundary, _maxDepth, _halfBoundaries, in area, mode, out var idx);
            if (!exist)
            {
                index = default;
                return false;
            }

            for (var parent = this.GetNode(in idx); parent != null; parent = parent.Parent)
            {
                if (!Geometry.Contain(in parent.LooseBoundary, in area))
                    continue;

                index = parent.Index;
                return true;
            }

            index = default;
            return false;
        }

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadExt.GetNodes(_nodes, depth, nodes);

        protected override void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            var node = this.GetNode(in index);
            QuadTreeExt.IterateNode(this, in checker, node, elements);
        }
        #endregion
    }
}