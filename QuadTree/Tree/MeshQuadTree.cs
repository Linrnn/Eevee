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
    /// 四叉树 + 网格法
    /// </summary>
    public sealed class MeshQuadTree
    {
        #region 数据/构造方法
        private const int ChildCount = 4;
        public const bool ShowLog = false;
        private static readonly int[] _logCheckIds = Array.Empty<int>(); // 需要检测的EntityId，null/空数组：不限制检测

        public readonly AABB2DInt MaxBounds; // 最大包围盒
        public readonly Vector2DInt[] HalfBoundsSizes; // 每一层的边界尺寸（四分之一的面积，方便后续计算）
        public readonly QuadNode[][] Nodes; // 所有节点
        public readonly QuadNode Root; // 根节点
        public readonly int MaxDepth; // 最大深度
        public readonly bool IsCircle;
        public readonly int TreeId;

        public MeshQuadTree(in AABB2DInt maxBounds, int depthCount, bool circle, int treeId)
        {
            var root = new QuadNode(in maxBounds, in maxBounds, 0, 0, 0, 0, null); // 先屏蔽松散四叉树，搜索有问题

            MaxBounds = maxBounds;
            HalfBoundsSizes = new Vector2DInt[depthCount];
            Nodes = new QuadNode[depthCount][];
            Root = root;
            MaxDepth = depthCount - 1;
            IsCircle = circle;
            TreeId = treeId;

            HalfBoundsSizes[0] = root.Bounds.HalfSize();
            Nodes[0] = new[] { root, };

            int left = maxBounds.Left();
            int top = maxBounds.Top();
            for (int depth = 1; depth < depthCount; ++depth)
            {
                int length = 1 << depth << depth; // length = 4^depth
                var nodes = new QuadNode[length];

                HalfBoundsSizes[depth] = HalfBoundsSizes[depth - 1] / 2;
                Nodes[depth] = nodes;

                for (int i = 0; i < length; i += ChildCount)
                {
                    var parent = Nodes[depth - 1][i / ChildCount];
                    parent.Children = new QuadNode[ChildCount];

                    for (int childId = 0; childId < ChildCount; ++childId)
                    {
                        var bounds = parent.CountChildBounds(childId);
                        int x = (bounds.X - left) / (bounds.W << 1);
                        int y = (top - bounds.Y) / (bounds.H << 1);
                        var child = new QuadNode(in bounds, in bounds, depth, childId, x, y, parent); // 先屏蔽松散四叉树，搜索有问题

                        parent.Children[childId] = child;
                        nodes[x + (1 << depth) * y] = child;
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
        public void QueryBox(in AABB2DInt box, ICollection<QuadElement> elements) // 查询单个包围盒
        {
            if (Root.IsEmpty())
                return;

            if (IsCircle)
                RecursiveQuery(new QueryBoxNodeCircleChecker(in box), in box, elements);
            else
                RecursiveQuery(new QueryBoxNodeBoxChecker(in box), in box, elements);
        }
        public void QueryCircle(in AABB2DInt circle, ICollection<QuadElement> elements) // 查询单个圆
        {
            if (Root.IsEmpty())
                return;

            if (circle.Contain(in MaxBounds))
                RecursiveAdd(Root, elements);
            else if (IsCircle)
                RecursiveQuery(new QueryCircleNodeCircleChecker(in circle), in circle, elements);
            else
                RecursiveQuery(new QueryCircleNodeBoxChecker(in circle), in circle, elements);
        }
        public void QueryRectangle(in AABB2DInt rect, in Vector2D dir, ICollection<QuadElement> elements) // 搜索单个有向矩形
        {
            if (Root.IsEmpty())
                return;

            GetRectangleCorner(in rect, in dir, out var lb, out var b, out var lt, out var rt);
            var width = Fixed64.Max((rt.X - lb.X).Abs(), (lt.X - b.X).Abs());
            var height = Fixed64.Max((rt.Y - lb.Y).Abs(), (lt.Y - b.Y).Abs());

            var aabb = new AABB2DInt(rect.Center(), new Vector2DInt((int)(width >> 1), (int)(height >> 1)));
            var checker = new QueryRectangleChecker(in aabb, in lb, in b, in rt, in lt);
            RecursiveQuery(in checker, in rect, elements);
        }
        public void QueryQuadrangle(in Vector2D lb, in Vector2D lt, in Vector2D rt, in Vector2D rb, ICollection<QuadElement> elements) // 搜索单个四边形
        {
            if (Root.IsEmpty())
                return;

            var minX = Fixed64.Min(lb.X, lt.X, rb.X, rt.X);
            var maxX = Fixed64.Max(lb.X, lt.X, rb.X, rt.X);
            var minY = Fixed64.Min(lb.Y, lt.Y, rb.Y, rt.Y);
            var maxY = Fixed64.Max(lb.Y, lt.Y, rb.Y, rt.Y);

            var aabb = new AABB2DInt((int)minX, (int)maxY, (int)maxX, (int)minY);
            var checker = new QueryQuadrangleChecker(in aabb, in lb, in rb, in rt, in lt);
            RecursiveQuery(in checker, in aabb, elements);
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
        private void RecursiveQuery<TChecker>(in TChecker checker, in AABB2DInt aabb, ICollection<QuadElement> elements) where TChecker : struct, IQueryIntersectChecker
        {
            var node = GetNode(in aabb);
            if (node == null) // 不存在的节点，不遍历树结构
                return;

            RecursiveQueryParent(in checker, node.Parent, elements);
            RecursiveQueryChildren(in checker, node, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecursiveQueryParent<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, IQueryIntersectChecker
        {
            for (var point = node; point != null; point = point.Parent)
                foreach (var element in node.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.AABB))
                        elements.Add(element);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecursiveQueryChildren<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, IQueryIntersectChecker
        {
            if (node.IsEmpty())
                return;

            if (node.Elements.Count > 0)
            {
                if (!checker.CheckNode(in node.LooseBounds))
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
        public QuadNode GetNode(in AABB2DInt aabb)
        {
            var intersect = aabb.Intersect(in MaxBounds); // 处理边界，减少触发LooseBounds.Contain的次数
            if (intersect.W < 0 || intersect.H < 0)
                return null;

            for (int depth = MaxDepth; depth >= 0; --depth)
            {
                var size = HalfBoundsSizes[depth];
                if (size.X < intersect.W || size.Y < intersect.H)
                    continue;

                int x = (intersect.X - MaxBounds.Left()) / (size.X << 1);
                int y = (MaxBounds.Top() - intersect.Y) / (size.Y << 1);
                var node = Nodes[depth][x + (1 << depth) * y];

                for (var point = node; point != null; point = point.Parent)
                    if (point.LooseBounds.Contain(in intersect))
                        return point;
            }

            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetRectangleCorner(in AABB2DInt box, in Vector2D dir, out Vector2D lb, out Vector2D rb, out Vector2D lt, out Vector2D rt) // 获得矩形的四个角
        {
            // 计算方向向量的垂直向量
            var dir3D = new Vector3D(dir.X, dir.Y);
            var normal = Vector3D.Cross(in dir3D, in Vector3D.Forward);
            normal.Normalize();

            // 先计算xy的乘积
            var xw = dir.X * box.W;
            var yw = dir.Y * box.W;
            var xh = normal.X * box.H;
            var yh = normal.Y * box.H;

            // 计算矩形的四个角坐标
            lb = new Vector2D(box.X - xw - xh, box.Y - yw - yh);
            rb = new Vector2D(box.X + xw - xh, box.Y + yw - yh);
            lt = new Vector2D(box.X - xw + xh, box.Y - yw + yh);
            rt = new Vector2D(box.X + xw + xh, box.Y + yw + yh);
        }

        public void Clean()
        {
            foreach (var nodes in Nodes)
            {
                foreach (var node in nodes)
                    node.Clean();
                nodes.CleanAll();
            }

            Nodes.CleanAll();
        }
        #endregion
    }
}