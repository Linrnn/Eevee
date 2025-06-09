using Eevee.Collection;
using Eevee.Fixed;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 网格法 + 松散四叉树
    /// </summary>
    public sealed class LooseQuadTree : QuadTreeBasic
    {
        #region 数据
        private QuadNode[][] _nodes;
        #endregion

        #region 复写
        internal override void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            base.OnCreate(treeId, shape, depthCount, in maxBoundary);
            var nodes = new QuadNode[depthCount][];
            nodes[0] = new[] { _root };
            _nodes = nodes;

            for (int left = maxBoundary.Left(), top = maxBoundary.Top(), upperDepth = 0, depth = 1; depth < depthCount; ++upperDepth, ++depth)
            {
                int length = 1 << depth << depth; // length = 4^depth
                var upperNodes = nodes[upperDepth];
                var depthNodes = new QuadNode[length];

                _halfBoundaries[depth] = _halfBoundaries[upperDepth] / 2;
                nodes[depth] = depthNodes;

                for (int i = 0; i < length; i += ChildCount)
                {
                    var parent = upperNodes[i / ChildCount];
                    parent.Children = new QuadNode[ChildCount];

                    for (int childId = 0; childId < ChildCount; ++childId)
                    {
                        var boundary = parent.CountChildBoundary(childId);
                        int x = (boundary.X - left) / (boundary.W << 1);
                        int y = (top - boundary.Y) / (boundary.H << 1);
                        var child = CreateNode(in boundary, depth, childId, x, y, parent);

                        parent.Children[childId] = child;
                        depthNodes[GetNodeId(depth, x, y)] = child;
                    }
                }
            }
        }
        internal override void OnDestroy()
        {
            foreach (var nodes in _nodes)
            {
                foreach (var node in nodes)
                    node.Clean();
                nodes.Clean();
            }

            _nodes.Clean();
            base.OnDestroy();
        }

        internal override QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent)
        {
            var looseBoundary = new AABB2DInt(boundary.Center(), boundary.Size());
            return new QuadNode(in boundary, in looseBoundary, depth, childId, x, y, parent);
        }
        internal override QuadNode CountNode(in AABB2DInt aabb, QuadCountNodeMode mode = CountMode)
        {
            if (!CountArea(in aabb, mode, out var area))
                return null;

            for (int left = _maxBoundary.Left(), top = _maxBoundary.Top(), depth = _maxDepth; depth >= 0; --depth)
            {
                var boundary = _halfBoundaries[depth];
                if (boundary.X < area.W || boundary.Y < area.H)
                    continue;

                int x = (area.X - left) / (boundary.X << 1);
                int y = (top - area.Y) / (boundary.Y << 1);
                var node = GetNode(depth, x, y);
                return node;
            }

            return null;
        }
        internal override QuadNode GetNode(int depth, int x, int y) => _nodes[depth][GetNodeId(depth, x, y)];

        internal override TCollection GetNodes<TCollection>(TCollection collection)
        {
            collection.Clear();
            foreach (var nodes in _nodes)
            foreach (var node in nodes)
                collection.Add(node);
            return collection;
        }
        internal override TCollection GetNodes<TCollection>(int depth, TCollection collection)
        {
            collection.Clear();
            foreach (var node in _nodes[depth])
                collection.Add(node);
            return collection;
        }
        #endregion
    }
}