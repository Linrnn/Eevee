using Eevee.Fixed;
using System.Collections.Generic;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 动态网格 + 四叉树
    /// </summary>
    public sealed class DynamicQuadTree : BasicQuadTree
    {
        #region 数据
        private Dictionary<int, QuadNode>[] _nodes; // “Key”根据“QuadExt.GetNodeId”得出
        #endregion

        #region 复写
        internal override void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary);
            var nodes = new Dictionary<int, QuadNode>[depthCount];
            for (int i = 0; i < nodes.Length; ++i)
                nodes[i] = new Dictionary<int, QuadNode>(1);
            nodes[0].Add(QuadExt.GetRootId(), _root);
            _nodes = nodes;
        }
        internal override void OnDestroy()
        {
            foreach (var depthNodes in _nodes)
            {
                foreach (var pair in depthNodes)
                    pair.Value.OnRelease();
                depthNodes.Clear();
            }
            _nodes = null;
            base.OnDestroy();
        }

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent)
        {
            // todo eevee 未实现
            throw new System.NotImplementedException();
        }
        internal override QuadNode GetNode(int depth, int x, int y)
        {
            // todo eevee 未实现
            throw new System.NotImplementedException();
        }
        internal override QuadNode GetOrCreateNode(int depth, int x, int y)
        {
            // todo eevee 未实现
            throw new System.NotImplementedException();
        }

        internal override void GetNodes(ICollection<QuadNode> nodes)
        {
            foreach (var depthNodes in _nodes)
            foreach (var pair in depthNodes)
                nodes.Add(pair.Value);
        }
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes)
        {
            foreach (var pair in _nodes[depth])
                nodes.Add(pair.Value);
        }

        internal override bool CountNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index)
        {
            // todo eevee 未实现
            throw new System.NotImplementedException();
        }
        protected override void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            // todo eevee 未实现
            throw new System.NotImplementedException();
        }
        #endregion
    }
}