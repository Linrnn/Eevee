using Eevee.Fixed;
using Eevee.Pool;
using System;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 静态网格 + 松散四叉树
    /// </summary>
    public sealed class LooseQuadTree : BasicQuadTree
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
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());
            node.OnAlloc(in boundary, in looseBoundary, depth, x, y, childId, parent);
            return node;
        }
        internal override QuadNode GetOrAddNode(int depth, int x, int y, bool allowAdd) => _nodes[depth][x, y];
        internal override void RemoveNode(QuadNode node) { }

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadTreeExt.GetNodes(_nodes, depth, nodes);

        internal override bool TryGetNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            if (!QuadTreeExt.TryGetNodeIndex(in _maxBoundary, _maxDepth, in aabb, mode, out _, out var idx))
            {
                index = QuadIndex.Invalid;
                return false;
            }

            index = idx;
            return true;
        }
        protected override void IterateQuery<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            var iterated = HashSetPool.Alloc<QuadNode>();
            int max = QuadExt.GetNodeSideCount(index.Depth) - 1;
            int si = Math.Max(index.X - 1, 0);
            int ei = Math.Min(index.X + 2, max);
            int sj = Math.Max(index.Y - 1, 0);
            int ej = Math.Min(index.Y + 2, max);

            for (int i = si; i < ei; ++i)
            {
                for (int j = sj; j < ej; ++j)
                {
                    var node = GetOrAddNode(index.Depth, si, sj, false);
                    if (node.SumCount == 0)
                        continue;
                    if (!checker.CheckNode(in node.Boundary))
                        continue;

                    IterateQueryParent(in checker, node.Parent, iterated, elements);
                    IterateQueryChildren(in checker, node, elements);
                }
            }

            iterated.Release2Pool();
        }
        #endregion
    }
}