using Eevee.Fixed;
using System.Collections.Generic;

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
            _nodes = QuadTreeExt.OnCreate(this, _root, depthCount, in maxBoundary, _halfBoundaries);
        }
        internal override void OnDestroy()
        {
            QuadExt.OnDestroy(ref _nodes);
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
            if (QuadExt.CountArea(in _maxBoundary, in aabb, mode, out var area))
            {
                bool exist = QuadExt.CountNodeIndex(in _maxBoundary, _maxDepth, _halfBoundaries, in area, mode, out index);
                return exist;
            }

            index = QuadIndex.Invalid;
            return false;
        }

        protected override void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements)
        {
            // todo eevee 未处理重复搜索
            for (int sx = index.X - 1, ex = index.X + 2; sx < ex; ++sx)
            {
                for (int sy = index.Y - 1, ey = index.Y + 2; sy < ey; ++sy)
                {
                    var node = GetNode(index.Depth, sx, sy);
                    if (node is null)
                        continue;

                    IterateParent(in checker, node.Parent, elements);
                    IterateChildren(in checker, node, elements);
                }
            }
        }

        internal override void GetNodes(ICollection<QuadNode> nodes) => QuadExt.GetNodes(_nodes, nodes);
        internal override void GetNodes(int depth, ICollection<QuadNode> nodes) => QuadExt.GetNodes(_nodes, depth, nodes);
        #endregion
    }
}