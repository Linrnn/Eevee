using Eevee.Diagnosis;
using Eevee.Fixed;
using Eevee.Pool;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 动态网格 + 四叉树
    /// </summary>
    public sealed class DynamicQuadTree : BasicQuadTree, IDynamicQuadTree
    {
        #region Field
        private Dictionary<int, QuadTreeNode>[] _nodes; // “Key”根据“QuadExt.GetNodeId”计算得出
        private IObjectPool<QuadTreeNode> _nodePool;
        #endregion

        #region BasicQuadTree
        internal override void OnCreate(int treeId, QuadTreeShape shape, int depthCount, in AABB2DInt maxBoundary, ArrayPool<QuadTreeElement> pool)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary, pool);
            var nodes = new Dictionary<int, QuadTreeNode>[depthCount];
            for (int i = 0; i < nodes.Length; ++i)
                nodes[i] = new Dictionary<int, QuadTreeNode>(1);
            nodes[0].Add(QuadTreeIndex.Root.GetNodeId(), _root);
            _nodes = nodes;
        }
        internal override void OnDestroy()
        {
            foreach (var depthNodes in _nodes)
            foreach (var pair in depthNodes)
                _nodePool.Release(pair.Value);
            _nodes = null;
            _nodePool = null;
            base.OnDestroy();
        }

        internal override QuadTreeNode CreateRoot()
        {
            var boundary = _maxBoundary;
            var rootIndex = QuadTreeIndex.Root;
            var node = _nodePool.Alloc();
            node.OnAlloc(in boundary, in boundary, rootIndex.Depth, rootIndex.X, rootIndex.Y, null, _elementPool);
            return node;
        }
        internal override QuadTreeNode CreateNode(int depth, int x, int y, QuadTreeNode parent)
        {
            var maxBoundary = _maxBoundary;
            var extents = QuadTreeNodeExt.GetDepthExtents(in maxBoundary, depth);
            var center = QuadTreeNodeExt.GetNodeCenter(x, y, maxBoundary.Left(), maxBoundary.Bottom(), extents.X, extents.Y);
            var boundary = new AABB2DInt(center, extents);
            var node = _nodePool.Alloc();
            node.OnAlloc(in boundary, in boundary, depth, x, y, parent, _elementPool);
            return node;
        }
        internal override QuadTreeNode GetOrAddNode(int depth, int x, int y)
        {
            int nodeId = QuadTreeNodeExt.GetNodeId(depth, x, y);
            var depthNodes = _nodes[depth];
            if (depthNodes.TryGetValue(nodeId, out var node))
                return node;

            unsafe
            {
                var indexes = (Span<QuadTreeIndex>)stackalloc QuadTreeIndex[depth]; // 需要创建节点的索引
                int count = 0;

                for (var index = new QuadTreeIndex(depth, x, y); index.IsValid(); index = index.Parent())
                {
                    var depNodes = _nodes[index.Depth];
                    if (depNodes.ContainsKey(index.GetNodeId()))
                        break;

                    Assert.Less<IndexOutOfRangeException, AssertArgs<int, int>, int>(count, indexes.Length, nameof(count), "Span<QuadIndex>, count:{0} >= indexes.Length:{1}!", new AssertArgs<int, int>(count, indexes.Length));
                    indexes[count] = index;
                    ++count;
                }

                for (int i = count - 1; i >= 0; --i) // 保证父节点先创建
                {
                    ref var index = ref indexes[i];
                    var parentIndex = index.Parent();
                    var parent = GetNode(parentIndex.Depth, parentIndex.X, parentIndex.Y);
                    var child = CreateNode(index.Depth, index.X, index.Y, parent);
                    var depNodes = _nodes[index.Depth];

                    parent.AddChild(child);
                    depNodes.Add(index.GetNodeId(), child);
                }
            }

            return depthNodes[nodeId];
        }
        internal override QuadTreeNode GetNode(int depth, int x, int y) => _nodes[depth][QuadTreeNodeExt.GetNodeId(depth, x, y)];

        internal override void GetNodes(ICollection<QuadTreeNode> nodes)
        {
            foreach (var depthNodes in _nodes)
            foreach (var pair in depthNodes)
                nodes.Add(pair.Value);
        }
        internal override void GetNodes(int depth, ICollection<QuadTreeNode> nodes)
        {
            foreach (var pair in _nodes[depth])
                nodes.Add(pair.Value);
        }

        internal override bool TryGetNodeIndex(in AABB2DInt shape, QuadTreeCountNodeMode mode, out QuadTreeIndex index)
        {
            if (!QuadTreeExt.TryGetNodeIndex(in _maxBoundary, _maxDepth, in shape, mode, out var area, out var idx))
            {
                index = QuadTreeIndex.Invalid;
                return false;
            }

            int left = _maxBoundary.Left();
            int bottom = _maxBoundary.Bottom();
            for (var nIdx = idx; nIdx.IsValid(); nIdx = nIdx.Parent())
            {
                var extents = QuadTreeNodeExt.GetDepthExtents(in _maxBoundary, nIdx.Depth);
                if (extents.X < area.W || extents.Y < area.H)
                    continue;

                var center = QuadTreeNodeExt.GetNodeCenter(nIdx.X, nIdx.Y, left, bottom, extents.X, extents.Y);
                var boundary = new AABB2DInt(center, extents);
                if (!Geometry.Contain(in boundary, in area))
                    continue;

                index = nIdx;
                return true;
            }

            index = QuadTreeIndex.Invalid;
            return false;
        }
        protected override void IterateQuery<TChecker>(in TChecker checker, in QuadTreeIndex index, ICollection<QuadTreeElement> elements)
        {
            for (var idx = index; idx.IsValid(); idx = idx.Parent())
            {
                var nodes = _nodes[idx.Depth];
                if (!nodes.TryGetValue(idx.GetNodeId(), out var node))
                    continue;

                IterateQueryParent(in checker, node.Parent, elements);
                IterateQueryChildrenCheckNull(in checker, node, elements);
                break;
            }
        }
        #endregion

        #region IQuadDynamicNode
        public void Inject(IObjectPool<QuadTreeNode> pool) => _nodePool = pool;
        public void RemoveNode(QuadTreeNode node)
        {
            node.Parent.RemoveChild(node);
            if (node.HasChild())
                foreach (var child in node.ChildAsIterator())
                    RemoveNode(child);
            var depthNodes = _nodes[node.Index.Depth];
            depthNodes.Remove(node.Index.GetNodeId());
            _nodePool.Release(node);
        }
        public void RemoveEmptyNode() => TryRemoveEmptyNode(_root);
        #endregion

        #region Helper
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryRemoveEmptyNode(QuadTreeNode node)
        {
            if (node.HasChild())
                foreach (var child in node.ChildAsIterator())
                    if (child.SumIsEmpty())
                        RemoveNode(child);
                    else
                        TryRemoveEmptyNode(child);
        }
        #endregion
    }
}