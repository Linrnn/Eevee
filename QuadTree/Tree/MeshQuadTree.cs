using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 网格法 + 四叉树
    /// </summary>
    public sealed class MeshQuadTree
    {
        #region 数据/构造方法
        private const int ChildCount = 4;
        public static bool ShowLog = false;
        private static readonly int[] _logCheckIds = Array.Empty<int>(); // 需要检测的Index，null/空数组：不限制检测

        public readonly int TreeId;
        public readonly int MaxDepth; // 最大深度
        public readonly AABB2DInt MaxBoundary; // 最大包围盒
        public readonly QuadShape Shape;

        internal readonly QuadNode Root; // 根节点
        internal readonly QuadNode[][] Nodes; // 所有节点
        internal readonly Vector2DInt[] HalfBoundaries; // 每一层的边界尺寸（四分之一的面积，方便后续计算）

        public MeshQuadTree(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            var root = new QuadNode(in maxBoundary, in maxBoundary, 0, 0, 0, 0, null); // 先屏蔽松散四叉树，搜索有问题

            TreeId = treeId;
            MaxDepth = depthCount - 1;
            MaxBoundary = maxBoundary;
            Shape = shape;

            Root = root;
            Nodes = new QuadNode[depthCount][];
            Nodes[0] = new[] { root };
            HalfBoundaries = new Vector2DInt[depthCount];
            HalfBoundaries[0] = root.Boundary.HalfSize();

            int left = maxBoundary.Left();
            int top = maxBoundary.Top();
            for (int depth = 1; depth < depthCount; ++depth)
            {
                int length = 1 << depth << depth; // length = 4^depth
                var nodes = new QuadNode[length];

                HalfBoundaries[depth] = HalfBoundaries[depth - 1] / 2;
                Nodes[depth] = nodes;

                for (int i = 0; i < length; i += ChildCount)
                {
                    var parent = Nodes[depth - 1][i / ChildCount];
                    parent.Children = new QuadNode[ChildCount];

                    for (int childId = 0; childId < ChildCount; ++childId)
                    {
                        var boundary = parent.CountChildBoundary(childId);
                        int x = (boundary.X - left) / (boundary.W << 1);
                        int y = (top - boundary.Y) / (boundary.H << 1);
                        var child = new QuadNode(in boundary, in boundary, depth, childId, x, y, parent); // 先屏蔽松散四叉树，搜索有问题

                        parent.Children[childId] = child;
                        nodes[GetNodeId(depth, x, y)] = child;
                    }
                }
            }
        }
        #endregion

        #region 操作
        public void Insert(in QuadElement element)
        {
            var node = GetNode(in element.AABB);
            node.Add(in element);

            if (ShowLog && (_logCheckIds.IsNullOrEmpty() || _logCheckIds.Contains(element.Index)))
                LogRelay.Log($"[Quad] InsertElement Success, TreeId:{TreeId}, Ele:{element}");
        }
        public bool Remove(in QuadElement element)
        {
            var node = GetNode(in element.AABB);
            bool remove = node.Remove(in element);

            if (ShowLog && remove && (_logCheckIds.IsNullOrEmpty() || _logCheckIds.Contains(element.Index)))
                LogRelay.Log($"[Quad] RemoveElement Success, TreeId:{TreeId}, Ele:{element}");
            if (ShowLog && !remove && (_logCheckIds == null || _logCheckIds.Contains(element.Index)))
                LogRelay.Warn($"[Quad] RemoveElement Fail, TreeId:{TreeId}, Ele:{element}");
            return remove;
        }
        public void Update(in QuadElement preElement, in QuadElement tarElement)
        {
            var preNode = GetNode(in preElement.AABB);
            var tarNode = GetNode(in tarElement.AABB);

            if (preNode == tarNode)
            {
                if (!preNode.Update(in preElement, in tarElement))
                    LogRelay.Error($"[Quad] UpdateElement Fail, TreeId:{TreeId}, PreEle:{preElement}, TarEle:{tarElement}");
                else if (ShowLog && _logCheckIds == null || _logCheckIds.Contains(preElement.Index))
                    LogRelay.Log($"[Quad] UpdateElement Success, TreeId:{TreeId}, PreEle:{preElement}, TarEle:{tarElement}");
            }
            else
            {
                if (!preNode.Remove(in preElement))
                    LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:{TreeId}, PreEle:{preElement}, TarEle:{tarElement}");
                tarNode.Add(in tarElement);
            }
        }
        #endregion

        #region 查询
        public void QueryPoint(Vector2DInt area, ICollection<QuadElement> elements)
        {
            if (Root.IsEmpty())
                return;

            switch (Shape)
            {
                case QuadShape.Circle: RecursiveQuery(new PointCircleNodeChecker(area), new AABB2DInt(area, 0), elements); break;
                case QuadShape.AABB: RecursiveQuery(new PointAABBNodeChecker(area), new AABB2DInt(area, 0), elements); break;
            }
        }
        public void QueryCircle(in CircleInt area, ICollection<QuadElement> elements)
        {
            if (Root.IsEmpty())
                return;

            var aabb = Converts.AsAABB2DInt(in area);
            if (aabb.Contain(in MaxBoundary))
                RecursiveAdd(Root, elements);

            switch (Shape)
            {
                case QuadShape.Circle: RecursiveQuery(new CircleNodeChecker(in area), in aabb, elements); break;
                case QuadShape.AABB: RecursiveQuery(new CircleAABBNodeChecker(in area), in aabb, elements); break;
            }
        }
        public void QueryAABB(in AABB2DInt area, ICollection<QuadElement> elements)
        {
            if (Root.IsEmpty())
                return;

            if (area.Contain(in MaxBoundary))
                RecursiveAdd(Root, elements);

            switch (Shape)
            {
                case QuadShape.Circle: RecursiveQuery(new AABBCircleNodeChecker(in area), in area, elements); break;
                case QuadShape.AABB: RecursiveQuery(new AABBNodeChecker(in area), in area, elements); break;
            }
        }
        public void QueryOOB(in OBB2DInt area, ICollection<QuadElement> elements)
        {
            if (Root.IsEmpty())
                return;

            area.RotatedCorner(out var p0, out var p1, out var p2, out var p3);
            var aabb = (AABB2DInt)Converts.AsAABB2D(in area);
            var checker = new OBBNodeChecker(in aabb, in p0, in p1, in p3, in p2);
            RecursiveQuery(in checker, in aabb, elements);
        }
        public void QueryPolygon(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3, ICollection<QuadElement> elements)
        {
            if (Root.IsEmpty())
                return;

            var minX = Fixed64.Min(p0.X, p1.X, p3.X, p2.X);
            var maxX = Fixed64.Max(p0.X, p1.X, p3.X, p2.X);
            var minY = Fixed64.Min(p0.Y, p1.Y, p3.Y, p2.Y);
            var maxY = Fixed64.Max(p0.Y, p1.Y, p3.Y, p2.Y);

            var area = AABB2DInt.Create((int)minX, (int)maxY, (int)maxX, (int)minY);
            var checker = new PolygonNodeChecker(in area, in p0, in p3, in p2, in p1);
            RecursiveQuery(in checker, in area, elements);
        }
        #endregion

        #region 工具
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecursiveAdd(QuadNode node, ICollection<QuadElement> elements)
        {
            if (node.IsEmpty())
                return;

            foreach (var element in node.Elements.AsReadOnlySpan())
                elements.Add(element);

            if (!node.Children.IsNullOrEmpty())
                foreach (var child in node.Children)
                    RecursiveAdd(child, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecursiveQuery<TChecker>(in TChecker checker, in AABB2DInt area, ICollection<QuadElement> elements) where TChecker : struct, INodeChecker
        {
            var node = GetNode(in area);
            if (node == null)
                return;

            RecursiveQueryParent(in checker, node.Parent, elements);
            RecursiveQueryChildren(in checker, node, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecursiveQueryParent<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, INodeChecker
        {
            for (var parent = node; parent != null; parent = parent.Parent)
                foreach (var element in parent.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.AABB))
                        elements.Add(element);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecursiveQueryChildren<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, INodeChecker
        {
            if (node.IsEmpty())
                return;

            if (node.Elements.Count > 0)
            {
                if (!checker.CheckNode(in node.LooseBoundary))
                    return;

                foreach (var element in node.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.AABB))
                        elements.Add(element);
            }

            if (!node.Children.IsNullOrEmpty())
                foreach (var child in node.Children)
                    RecursiveQueryChildren(in checker, child, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadNode GetNode(in AABB2DInt area, bool clamp = false)
        {
            if (!area.Intersect(in MaxBoundary, out var intersect)) // 处理边界，减少触发“LooseBoundary.Contain()”的次数
                return null;
            bool iw = intersect.W < 0;
            bool ih = intersect.H < 0;
            if (iw || ih)
            {
                if (!clamp)
                    return null;

                intersect = (iw, ih) switch
                {
                    (true, true) => new AABB2DInt(intersect.X - intersect.W, intersect.Y - intersect.H, 0, 0),
                    (true, false) => new AABB2DInt(intersect.X - intersect.W, intersect.Y, 0, intersect.H),
                    (false, true) => new AABB2DInt(intersect.X, intersect.Y - intersect.H, intersect.W, 0),
                    _ => intersect,
                };
            }

            for (int depth = MaxDepth; depth >= 0; --depth)
            {
                var size = HalfBoundaries[depth];
                if (size.X < intersect.W || size.Y < intersect.H)
                    continue;

                int x = (intersect.X - MaxBoundary.Left()) / (size.X << 1);
                int y = (MaxBoundary.Top() - intersect.Y) / (size.Y << 1);
                var node = Nodes[depth][GetNodeId(depth, x, y)];

                for (var parent = node; parent != null; parent = parent.Parent)
                    if (parent.LooseBoundary.Contain(in intersect))
                        return parent;
            }

            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadNode GetNode(int depth, int x, int y) => Nodes[depth][GetNodeId(depth, x, y)];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNodeId(int depth, int x, int y) => x + (1 << depth) * y;

        public void Clean()
        {
            foreach (var nodes in Nodes)
            {
                foreach (var node in nodes)
                    node.Clean();
                nodes.Clean();
            }

            Nodes.Clean();
        }
        #endregion
    }
}