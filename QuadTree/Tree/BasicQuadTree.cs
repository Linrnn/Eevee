using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树基类
    /// </summary>
    public abstract class BasicQuadTree
    {
        #region 数据
        protected int _treeId; // 四叉树的编号
        protected int _maxDepth = QuadTreeIndex.Invalid.Depth; // 四叉树的最大深度
        protected QuadTreeShape _shape; // 四叉树节点的形状（暂时只支持“Circle”和“AABB”）
        protected AABB2DInt _maxBoundary; // 最大包围盒
        protected ArrayPool<QuadTreeElement> _elementPool;
        protected QuadTreeNode _root; // 根节点

        public int TreeId => _treeId;
        public int MaxDepth => _maxDepth;
        public QuadTreeShape Shape => _shape;
        public AABB2DInt MaxBoundary => _maxBoundary;
        #endregion

        #region 操作
        public void Insert(in QuadTreeElement element)
        {
            TryGetNodeIndex(in element.Shape, QuadTreeNodeExt.ElementCountMode, out var index);
            var node = GetOrAddNode(index.Depth, index.X, index.Y);
            node.Add(in element);

            if (QuadTreeDiagnosis.CheckIndex(_treeId, element.Index))
                LogRelay.Log($"[Quad] InsertElement Success, TreeId:{_treeId}, Ele:{element}");
        }
        public bool Remove(in QuadTreeElement element)
        {
            TryGetNodeIndex(in element.Shape, QuadTreeNodeExt.ElementCountMode, out var index);
            var node = GetNode(index.Depth, index.X, index.Y);
            bool remove = node.Remove(in element);
            TryRemoveNode(node);

            if (remove && QuadTreeDiagnosis.CheckIndex(_treeId, element.Index))
                LogRelay.Log($"[Quad] RemoveElement Success, TreeId:{_treeId}, Ele:{element}");
            if (!remove && QuadTreeDiagnosis.CheckIndex(_treeId, element.Index))
                LogRelay.Warn($"[Quad] RemoveElement Fail, TreeId:{_treeId}, Ele:{element}");
            return remove;
        }
        public void Update(in QuadTreeElement preElement, in QuadTreeElement tarElement)
        {
            TryGetNodeIndex(in preElement.Shape, QuadTreeNodeExt.ElementCountMode, out var preIndex);
            TryGetNodeIndex(in tarElement.Shape, QuadTreeNodeExt.ElementCountMode, out var tarIndex);
            var preNode = GetNode(preIndex.Depth, preIndex.X, preIndex.Y);
            var tarNode = GetOrAddNode(tarIndex.Depth, tarIndex.X, tarIndex.Y);

            if (preNode == tarNode)
            {
                if (!preNode.Update(in preElement, in tarElement))
                    LogRelay.Error($"[Quad] UpdateElement Fail, TreeId:{_treeId}, PreEle:{preElement}, TarEle:{tarElement}");
                else if (QuadTreeDiagnosis.CheckIndex(_treeId, preElement.Index))
                    LogRelay.Log($"[Quad] UpdateElement Success, TreeId:{_treeId}, PreEle:{preElement}, TarEle:{tarElement}");
            }
            else
            {
                if (!preNode.Remove(in preElement))
                    LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:{_treeId}, PreEle:{preElement}, TarEle:{tarElement}");
                tarNode.Add(in tarElement);
                TryRemoveNode(preNode); // “tarNode”可能是“preNode”的子节点，所以要先“Add”，后“RemoveNode”
            }
        }
        #endregion

        #region 查询
        public void QueryPoint(Vector2DInt shape, ICollection<QuadTreeElement> elements)
        {
            if (_root.SumIsEmpty())
                return;

            switch (_shape)
            {
                case QuadTreeShape.Circle: IterateQueryStart(new QuadTreePoint2CircleChecker(shape), Converts.AsAABB2DInt(shape), elements); break;
                case QuadTreeShape.AABB: IterateQueryStart(new QuadTreePoint2AABBChecker(shape), Converts.AsAABB2DInt(shape), elements); break;
                default: throw QuadTreeNodeExt.BuildShapeException(_treeId, _shape);
            }
        }
        public void QueryCircle(in CircleInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            if (_root.SumIsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                IterateQueryOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadTreeShape.Circle: IterateQueryStart(new QuadTreeCircle2CircleChecker(in shape), Converts.AsAABB2DInt(in shape), elements); break;
                case QuadTreeShape.AABB: IterateQueryStart(new QuadTreeCircle2AABBChecker(in shape), Converts.AsAABB2DInt(in shape), elements); break;
                default: throw QuadTreeNodeExt.BuildShapeException(_treeId, _shape);
            }
        }
        public void QueryAABB(in AABB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            if (_root.SumIsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                IterateQueryOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadTreeShape.Circle: IterateQueryStart(new QuadTreeAABB2CircleChecker(in shape), in shape, elements); break;
                case QuadTreeShape.AABB: IterateQueryStart(new QuadTreeAABB2AABBChecker(in shape), in shape, elements); break;
                default: throw QuadTreeNodeExt.BuildShapeException(_treeId, _shape);
            }
        }
        public void QueryOBB(in OBB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            if (_root.SumIsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                IterateQueryOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadTreeShape.Circle:
                {
                    var checker = new QuadTreeOBB2CircleChecker(in shape);
                    IterateQueryStart(in checker, in checker.Shape, elements);
                    break;
                }

                case QuadTreeShape.AABB:
                {
                    var checker = new QuadTreeOBB2AABBChecker(in shape);
                    IterateQueryStart(in checker, in checker.Shape, elements);
                    break;
                }

                default: throw QuadTreeNodeExt.BuildShapeException(_treeId, _shape);
            }
        }
        public void QueryPolygon(in PolygonInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            if (_root.SumIsEmpty())
            {
                return;
            }

            if (checkRoot && Geometry.Contain(in shape, in _maxBoundary))
            {
                IterateQueryOnly(_root, elements);
                return;
            }

            switch (_shape)
            {
                case QuadTreeShape.Circle:
                {
                    var checker = new QuadTreePolygon2CircleChecker(in shape);
                    IterateQueryStart(in checker, in checker.Shape, elements);
                    checker.Checker.Dispose();
                    break;
                }

                case QuadTreeShape.AABB:
                {
                    var checker = new QuadTreePolygon2AABBChecker(in shape);
                    IterateQueryStart(in checker, in checker.Shape, elements);
                    checker.Checker.Dispose();
                    break;
                }

                default: throw QuadTreeNodeExt.BuildShapeException(_treeId, _shape);
            }
        }
        #endregion

        #region 工具
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IterateQueryOnly(QuadTreeNode node, ICollection<QuadTreeElement> elements)
        {
            if (node.SumIsEmpty())
                return;

            foreach (var element in node.Elements.AsReadOnlySpan())
                elements.Add(element);

            if (node.HasChild())
                foreach (var child in node.ChildAsIterator())
                    IterateQueryOnly(child, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IterateQueryStart<TChecker>(in TChecker checker, in AABB2DInt shape, ICollection<QuadTreeElement> elements) where TChecker : struct, IQuadTreeChecker
        {
            if (TryGetNodeIndex(in shape, QuadTreeNodeExt.QueryCountMode, out var index))
                IterateQuery(in checker, index, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IterateQueryParent<TChecker>(in TChecker checker, QuadTreeNode node, ICollection<QuadTreeElement> elements) where TChecker : struct, IQuadTreeChecker
        {
            for (var parent = node; parent is not null; parent = parent.Parent)
                foreach (var element in parent.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.Shape))
                        elements.Add(element);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IterateQueryParentCheckRepeat<TChecker>(in TChecker checker, QuadTreeNode node, ref StackAllocSet<QuadTreeNode> iterated, ICollection<QuadTreeElement> elements) where TChecker : struct, IQuadTreeChecker
        {
            for (var parent = node; parent is not null; parent = parent.Parent)
                if (iterated.Add(parent))
                    foreach (var element in parent.Elements.AsReadOnlySpan())
                        if (checker.CheckElement(in element.Shape))
                            elements.Add(element);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IterateQueryChildren<TChecker>(in TChecker checker, QuadTreeNode node, ICollection<QuadTreeElement> elements) where TChecker : struct, IQuadTreeChecker
        {
            if (node.SumIsEmpty())
                return;

            if (node.Elements.Count > 0)
            {
                if (!checker.CheckNode(in node.LooseBoundary))
                    return;

                foreach (var element in node.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.Shape))
                        elements.Add(element);
            }

            if (node.HasChild())
                foreach (var child in node.ChildAsSpan())
                    IterateQueryChildren(in checker, child, elements);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void IterateQueryChildrenCheckNull<TChecker>(in TChecker checker, QuadTreeNode node, ICollection<QuadTreeElement> elements) where TChecker : struct, IQuadTreeChecker
        {
            if (node is null || node.SumIsEmpty())
                return;

            if (node.Elements.Count > 0)
            {
                if (!checker.CheckNode(in node.LooseBoundary))
                    return;

                foreach (var element in node.Elements.AsReadOnlySpan())
                    if (checker.CheckElement(in element.Shape))
                        elements.Add(element);
            }

            if (node.HasChild())
                foreach (var child in node.ChildAsIterator())
                    IterateQueryChildrenCheckNull(in checker, child, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TryRemoveNode(QuadTreeNode node)
        {
            if (this is not IDynamicQuadTree { } quadDynamic)
                return;
            if (!node.SumIsEmpty())
                return;
            if (node == _root)
                return;
            quadDynamic.RemoveNode(node);
        }
        #endregion

        #region 待继承
        internal virtual void OnCreate(int treeId, QuadTreeShape shape, int depthCount, in AABB2DInt maxBoundary, ArrayPool<QuadTreeElement> pool)
        {
            _treeId = treeId;
            _maxDepth = depthCount - 1;
            _shape = shape;
            _maxBoundary = maxBoundary;
            _elementPool = pool;
            _root = CreateRoot(); // “CreateRoot()”依赖“_elementPool”
        }
        internal virtual void OnDestroy()
        {
            _treeId = 0;
            _maxDepth = QuadTreeIndex.Invalid.Depth;
            _shape = QuadTreeShape.None;
            _maxBoundary = default;
            _elementPool = null;
            _root = null;
        }

        internal abstract QuadTreeNode CreateRoot(); // 创建根节点
        internal abstract QuadTreeNode CreateNode(int depth, int x, int y, QuadTreeNode parent); // 创建单个节点（此接口的父节点Children字段不会绑定子节点，但是子节点Parent字段会绑定父节点）
        internal abstract QuadTreeNode GetOrAddNode(int depth, int x, int y); // 获取节点/创建单个节点（如果父节点不存在，会创建父节点）
        internal abstract QuadTreeNode GetNode(int depth, int x, int y);

        internal abstract void GetNodes(ICollection<QuadTreeNode> nodes);
        internal abstract void GetNodes(int depth, ICollection<QuadTreeNode> nodes);

        internal abstract bool TryGetNodeIndex(in AABB2DInt shape, QuadTreeCountNodeMode mode, out QuadTreeIndex index);
        protected abstract void IterateQuery<TChecker>(in TChecker checker, in QuadTreeIndex index, ICollection<QuadTreeElement> elements) where TChecker : struct, IQuadTreeChecker;
        #endregion
    }
}