using Eevee.Fixed;
using Eevee.Pool;
using System;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 网格法 + 松散四叉树
    /// </summary>
    public sealed class LooseQuadTree : BasicQuadTree
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

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent)
        {
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());
            return new QuadNode(in boundary, in looseBoundary, depth, childId, x, y, parent);
        }
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][QuadExt.GetNodeId(depth, x, y)];
        internal override bool CountNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadTreeExt.CountNodeIndex(in _maxBoundary, _maxDepth, in aabb, mode, out _, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }

        protected override void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            var iterated = HashSetPool.Alloc<QuadNode>();
            int max = QuadExt.CountNodeSideCount(index.Depth) - 1;
            int si = Math.Max(index.X - 1, 0);
            int ei = Math.Min(index.X + 2, max);
            int sj = Math.Max(index.Y - 1, 0);
            int ej = Math.Min(index.Y + 2, max);

            for (int i = si; i < ei; ++i)
            {
                for (int j = sj; j < ej; ++j)
                {
                    var node = GetNode(index.Depth, si, sj);
                    IterateParent(in checker, node.Parent, iterated, elements);
                    IterateChildren(in checker, node, elements);
                }
            }

            iterated.Release2Pool();
        }

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);
        #endregion
    }
}