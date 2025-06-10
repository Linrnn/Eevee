using Eevee.Diagnosis;
using Eevee.Fixed;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树基类
    /// </summary>
    public abstract class QuadTreeBasic
    {
        #region 数据
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
            CountNodeIndex(in element.AABB, QuadExt.CountMode, out var index);
            var node = this.GetNode(in index);
            node.Add(in element);

            if (QuadDebug.CheckIndex(_treeId, element.Index))
                LogRelay.Log($"[Quad] InsertElement Success, TreeId:{_treeId}, Ele:{element}");
        }
        public bool Remove(in QuadElement element)
        {
            CountNodeIndex(in element.AABB, QuadExt.CountMode, out var index);
            var node = this.GetNode(in index);
            bool remove = node.Remove(in element);

            if (remove && QuadDebug.CheckIndex(_treeId, element.Index))
                LogRelay.Log($"[Quad] RemoveElement Success, TreeId:{_treeId}, Ele:{element}");
            if (!remove && QuadDebug.CheckIndex(_treeId, element.Index))
                LogRelay.Warn($"[Quad] RemoveElement Fail, TreeId:{_treeId}, Ele:{element}");
            return remove;
        }
        public void Update(in QuadElement preElement, in QuadElement tarElement)
        {
            CountNodeIndex(in preElement.AABB, QuadExt.CountMode, out var preIndex);
            CountNodeIndex(in tarElement.AABB, QuadExt.CountMode, out var tarIndex);
            var preNode = this.GetNode(in preIndex);
            var tarNode = this.GetNode(in tarIndex);

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
                case QuadShape.Circle: IterateStart(new IQuadPoint2CircleChecker(shape), Converts.AsAABB2DInt(shape), elements); break;
                case QuadShape.AABB: IterateStart(new IQuadPoint2AABBChecker(shape), Converts.AsAABB2DInt(shape), elements); break;
                default: throw QuadExt.ShapeNotImplementException(_treeId, _shape);
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
                IterateOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle: IterateStart(new IQuadCircle2CircleChecker(in shape), Converts.AsAABB2DInt(in shape), elements); break;
                case QuadShape.AABB: IterateStart(new IQuadCircle2AABBChecker(in shape), Converts.AsAABB2DInt(in shape), elements); break;
                default: throw QuadExt.ShapeNotImplementException(_treeId, _shape);
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
                IterateOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle: IterateStart(new IQuadAABB2CircleChecker(in shape), in shape, elements); break;
                case QuadShape.AABB: IterateStart(new IQuadAABB2AABBChecker(in shape), in shape, elements); break;
                default: throw QuadExt.ShapeNotImplementException(_treeId, _shape);
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
                IterateOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle:
                {
                    var checker = new IQuadOBB2CircleChecker(in shape);
                    IterateStart(in checker, in checker.Shape, elements);
                    break;
                }

                case QuadShape.AABB:
                {
                    var checker = new IQuadOBB2AABBChecker(in shape);
                    IterateStart(in checker, in checker.Shape, elements);
                    break;
                }

                default: throw QuadExt.ShapeNotImplementException(_treeId, _shape);
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
                IterateOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadShape.Circle:
                {
                    var checker = new IQuadPolygon2CircleChecker(in shape);
                    IterateStart(in checker, in checker.Shape, elements);
                    checker.Checker.Dispose();
                    break;
                }

                case QuadShape.AABB:
                {
                    var checker = new IQuadPolygon2AABBChecker(in shape);
                    IterateStart(in checker, in checker.Shape, elements);
                    checker.Checker.Dispose();
                    break;
                }

                default: throw QuadExt.ShapeNotImplementException(_treeId, _shape);
            }
        }
        #endregion

        #region 工具
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IterateOnly(QuadNode node, ICollection<QuadElement> elements)
        {
            if (node.IsEmpty())
                return;

            foreach (var element in node.Elements.AsReadOnlySpan())
                elements.Add(element);

            if (node.Children is { Length: > 0 } children)
                foreach (var child in children)
                    IterateOnly(child, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IterateStart<TChecker>(in TChecker checker, in AABB2DInt aabb, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker
        {
            CountNodeIndex(in aabb, QuadExt.CountMode, out var index);
            Iterate(in checker, index, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IterateParent<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker
        {
            for (var parent = node; parent is not null; parent = parent.Parent)
                foreach (var element in parent.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.AABB))
                        elements.Add(element);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IterateChildren<TChecker>(in TChecker checker, QuadNode node, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker
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

            if (node.Children is { Length: > 0 } children)
                foreach (var child in children)
                    IterateChildren(in checker, child, elements);
        }
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
        internal virtual void OnDestroy() => _halfBoundaries = null;

        internal abstract QuadNode CreateNode(in AABB2DInt boundary, int depth, int childId, int x, int y, QuadNode parent);
        internal abstract QuadNode GetNode(int depth, int x, int y);
        internal abstract bool CountNodeIndex(in AABB2DInt aabb, QuadCountNodeMode mode, out QuadIndex index);

        internal abstract void GetNodes(ICollection<QuadNode> nodes);
        internal abstract void GetNodes(int depth, ICollection<QuadNode> nodes);

        protected abstract void Iterate<TChecker>(in TChecker checker, in QuadIndex index, ICollection<QuadElement> elements) where TChecker : struct, IQuadChecker;
        #endregion
    }
}