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
        #region Field
        private QuadNode[][,] _nodes;
        #endregion

        #region BasicQuadTree
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

        internal override QuadNode CreateRoot()
        {
            var boundary = _maxBoundary;
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());
            var rootIndex = QuadIndex.Root;
            var node = new QuadNode();
            node.OnAlloc(in boundary, in looseBoundary, rootIndex.Depth, rootIndex.X, rootIndex.Y, null);
            return node;
        }
        internal override QuadNode CreateNode(int depth, int x, int y, QuadNode parent)
        {
            int childId = QuadExt.GetChildId(x, y);
            var boundary = parent.GetChildBoundary(childId);
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());

            var node = new QuadNode();
            node.OnAlloc(in boundary, in looseBoundary, depth, x, y, parent);
            return node;
        }
        internal override QuadNode GetOrAddNode(int depth, int x, int y) => _nodes[depth][x, y];
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][x, y];
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
            // todo eevee 需要是实现纯结构体的HashSet，内存由栈分配
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
                    var node = GetNode(index.Depth, si, sj);
                    if (node.SumCount == 0)
                        continue;
                    if (!checker.CheckNode(in node.Boundary))
                        continue;

                    IterateQueryParentCheckRepeat(in checker, node.Parent, iterated, elements);
                    IterateQueryChildren(in checker, node, elements);
                }
            }

            iterated.Release2Pool();
        }
        #endregion
    }
}