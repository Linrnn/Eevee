using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    // todo eevee 代码优化“OnCreate()”，减少重复代码
    // todo eevee 代码优化“CountNode()”，减少重复代码
    // todo eevee 性能优化“CountNode()”循环，采用二分法
    // todo eevee 松散四叉树“Query”未处理
    /// <summary>
    /// 四叉树基类
    /// </summary>
    public abstract class QuadTreeBasic
    {
        #region 数据
        protected const int ChildCount = 4;
        protected const QuadCountNodeMode CountMode = QuadCountNodeMode.NotIntersect;
        protected int _treeId; // 四叉树的编号
        protected int _maxDepth; // 四叉树的最大深度
        protected QuadShape _shape; // 四叉树节点的形状（暂时只支持“Circle”和“AABB”）
        protected AABB2DInt _maxBoundary; // 最大包围盒
        protected Vector2DInt[] _halfBoundaries; // 每一层的边界尺寸（四分之一的面积，方便后续计算）
        protected QuadNode _root; // 根节点

        public int TreeId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _treeId;
        }
        public int MaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxDepth;
        }
        public QuadShape Shape
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _shape;
        }
        public AABB2DInt MaxBoundary
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxBoundary;
        }
        internal Vector2DInt[] HalfBoundaries
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _halfBoundaries;
        }
        internal QuadNode Root
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _root;
        }
        #endregion

        #region 操作
        public void Insert(in QuadElement element)
        {
            var node = CountNode(in element.AABB);
            node.Add(in element);

            if (QuadDebug.CheckIndex(_treeId, element.Index))
                LogRelay.Log($"[Quad] InsertElement Success, TreeId:{_treeId}, Ele:{element}");
        }
        public bool Remove(in QuadElement element)
        {
            var node = CountNode(in element.AABB);
            bool remove = node.Remove(in element);

            if (remove && QuadDebug.CheckIndex(_treeId, element.Index))
                LogRelay.Log($"[Quad] RemoveElement Success, TreeId:{_treeId}, Ele:{element}");
            if (!remove && QuadDebug.CheckIndex(_treeId, element.Index))
                LogRelay.Warn($"[Quad] RemoveElement Fail, TreeId:{_treeId}, Ele:{element}");
            return remove;
        }
        public void Update(in QuadElement preElement, in QuadElement tarElement)
        {
            var preNode = CountNode(in preElement.AABB);
            var tarNode = CountNode(in tarElement.AABB);

            if (preNode == tarNode)
            {
                if (!preNode.Update(in preElement, in tarElement))
                    LogRelay.Error($"[Quad] UpdateElement Fail, TreeId:{_treeId}, PreEle:{preElement}, TarEle:{tarElement}");
                else if (QuadDebug.CheckIndex(_treeId, preElement.Index))
                    LogRelay.Log($"[Quad] UpdateElement Success, TreeId:{_treeId}, PreEle:{preElement}, TarEle:{tarElement}");
            }
            else
            {
                if (!preNode.Remove(in preElement))
                    LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:{_treeId}, PreEle:{preElement}, TarEle:{tarElement}");
                tarNode.Add(in tarElement);
            }
        }
        #endregion

        #region 查询
        public void QueryPoint(Vector2DInt shape, ICollection<QuadElement> elements)
        {
            if (_root.IsEmpty())
                return;

            switch (_shape)
            {
                case QuadShape.Circle: RecursiveQuery(new IQuadPoint2CircleChecker(shape), Converts.AsAABB2DInt(shape), elements); break;
                case QuadShape.AABB: RecursiveQuery(new IQuadPoint2AABBChecker(shape), Converts.AsAABB2DInt(shape), elements); break;
                default: throw ShapeNotImplementException();
            }
        }
        public void QueryCircle(in CircleInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            if (_root.IsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                RecursiveAdd(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle: RecursiveQuery(new IQuadCircle2CircleChecker(in shape), Converts.AsAABB2DInt(in shape), elements); break;
                case QuadShape.AABB: RecursiveQuery(new IQuadCircle2AABBChecker(in shape), Converts.AsAABB2DInt(in shape), elements); break;
                default: throw ShapeNotImplementException();
            }
        }
        public void QueryAABB(in AABB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            if (_root.IsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                RecursiveAdd(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle: RecursiveQuery(new IQuadAABB2CircleChecker(in shape), in shape, elements); break;
                case QuadShape.AABB: RecursiveQuery(new IQuadAABB2AABBChecker(in shape), in shape, elements); break;
                default: throw ShapeNotImplementException();
            }
        }
        public void QueryOBB(in OBB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            if (_root.IsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                RecursiveAdd(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle:
                {
                    var checker = new IQuadOBB2CircleChecker(in shape);
                    RecursiveQuery(in checker, in checker.Shape, elements);
                    break;
                }

                case QuadShape.AABB:
                {
                    var checker = new IQuadOBB2AABBChecker(in shape);
                    RecursiveQuery(in checker, in checker.Shape, elements);
                    break;
                }

                default: throw ShapeNotImplementException();
            }
        }
        public void QueryPolygon(in PolygonInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            if (_root.IsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                RecursiveAdd(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle:
                {
                    var checker = new IQuadPolygon2CircleChecker(in shape);
                    RecursiveQuery(in checker, in checker.Shape, elements);
                    checker.Checker.Dispose();
                    break;
                }

                case QuadShape.AABB:
                {
                    var checker = new IQuadPolygon2AABBChecker(in shape);
                    RecursiveQuery(in checker, in checker.Shape, elements);
                    checker.Checker.Dispose();
                    break;
                }

                default: throw ShapeNotImplementException();
            }
        }
        #endregion

        #region 工具
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RecursiveAdd(QuadNode node, ICollection<QuadElement> elements)
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
        protected void RecursiveQuery<TChecker>(in TChecker checker, in AABB2DInt aabb, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker
        {
            var node = CountNode(in aabb);
            if (node == null)
                return;

            RecursiveQueryParent(in checker, node.Parent, elements);
            RecursiveQueryChildren(in checker, node, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RecursiveQueryParent<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker
        {
            for (var parent = node; parent != null; parent = parent.Parent)
                foreach (var element in parent.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.AABB))
                        elements.Add(element);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RecursiveQueryChildren<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker
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
        protected bool CountArea(in AABB2DInt aabb, QuadCountNodeMode mode, out AABB2DInt area)
        {
            if (mode == QuadCountNodeMode.NotIntersect)
            {
                area = aabb;
                return true;
            }

            if (Geometry.UnsafeIntersect(in aabb, in _maxBoundary, out var intersect)) // 处理边界，减少触发“LooseBoundary.Contain()”的次数
            {
                area = intersect;
                return true;
            }

            switch (mode)
            {
                case QuadCountNodeMode.OnlyIntersect:
                    area = default;
                    return false;

                case QuadCountNodeMode.IntersectOffset:
                    area = (intersect.W >= 0, intersect.H >= 0) switch
                    {
                        (false, false) => new AABB2DInt(intersect.X - intersect.W, intersect.Y - intersect.H, 0, 0),
                        (false, true) => new AABB2DInt(intersect.X - intersect.W, intersect.Y, 0, intersect.H),
                        (true, false) => new AABB2DInt(intersect.X, intersect.Y - intersect.H, intersect.W, 0),
                        _ => intersect,
                    };
                    return true;

                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, "Error!");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetNodeId(int depth, int x, int y) => x + (1 << depth) * y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Exception ShapeNotImplementException() => new NotImplementedException($"TreeId:{_treeId}, Shape:{_shape} not implement.");
        #endregion

        #region 待继承
        internal virtual void OnCreate(int treeId, QuadShape shape, int depthCount, in AABB2DInt maxBoundary)
        {
            var root = CreateNode(in maxBoundary, 0, 0, 0, 0, null);
            var halfBoundaries = new Vector2DInt[depthCount];
            halfBoundaries[0] = maxBoundary.HalfSize();

            _treeId = treeId;
            _maxDepth = depthCount - 1;
            _shape = shape;
            _maxBoundary = maxBoundary;
            _halfBoundaries = halfBoundaries;
            _root = root;
        }
        internal virtual void OnDestroy() => _halfBoundaries.Clean();

        internal virtual QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent) => new(in boundary, in boundary, depth, childId, x, y, parent);
        internal abstract QuadNode CountNode(in AABB2DInt aabb, QuadCountNodeMode mode = CountMode);
        internal abstract QuadNode GetNode(int depth, int x, int y);

        internal abstract TCollection GetNodes<TCollection>(TCollection collection) where TCollection : ICollection<QuadNode>;
        internal abstract TCollection GetNodes<TCollection>(int depth, TCollection collection) where TCollection : ICollection<QuadNode>;
        #endregion
    }
}