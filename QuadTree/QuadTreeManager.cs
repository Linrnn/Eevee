using Eevee.Collection;
using Eevee.Define;
using Eevee.Diagnosis;
using Eevee.Fixed;
using Eevee.Pool;
using Eevee.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树左下角：(0, 0)
    /// </summary>
    public sealed class QuadTreeManager
    {
        #region 字段/构造方法
        public readonly int Scale; // 缩放比例，引擎尺寸 * Scale = 四叉树尺寸
        public readonly Fixed64 ScaleReciprocal; // 1 / Scale
        public readonly int DepthCount;
        public readonly AABB2DInt MaxBoundary;
        private readonly Dictionary<int, QuadTreeConfig> _configs;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly ObjectDelPool<QuadTreeNode> _nodePool = new(() => new QuadTreeNode(), null, element => element.OnRelease(), null, Macro.HasCheckRelease);
        private readonly ArrayPool<QuadTreeElement> _elementPool = ArrayPool<QuadTreeElement>.Shared;

        public QuadTreeManager(int scale, int depthCount, in AABB2DInt maxBoundary, IReadOnlyList<QuadTreeConfig> configs)
        {
            int width = 1 << Maths.Log2(maxBoundary.W) + (Maths.IsPowerOf2(maxBoundary.W) ? 0 : 1);
            int height = 1 << Maths.Log2(maxBoundary.H) + (Maths.IsPowerOf2(maxBoundary.H) ? 0 : 1);
            var realMaxBoundary = new AABB2DInt(maxBoundary.X, maxBoundary.Y, width, height);
            var treeConfigs = new Dictionary<int, QuadTreeConfig>(configs.Count);
            for (int count = configs.Count, i = 0; i < count; ++i)
                if (configs[i] is { } config)
                    treeConfigs.Add(config.TreeId, config);

            Scale = scale;
            ScaleReciprocal = Fixed64.One / scale;
            DepthCount = depthCount;
            MaxBoundary = realMaxBoundary;
            _configs = treeConfigs;
            BuildTrees(depthCount, in realMaxBoundary, configs);
        }
        #endregion

        #region 预处理
        public bool PreCount(int treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents, out QuadTreePreCache cache)
        {
            if (center.Equals())
            {
                cache = default;
                return false;
            }

            var tree = _trees[treeId];
            var preEle = new QuadTreeElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadTreeElement(index, new AABB2DInt(center.Tar, extents));

            tree.TryGetNodeIndex(in preEle.Shape, QuadTreeNodeExt.ElementCountMode, out var preNodeIndex);
            tree.TryGetNodeIndex(in tarEle.Shape, QuadTreeNodeExt.ElementCountMode, out var tarNodeIndex);
            var preNode = tree.GetNode(preNodeIndex.Depth, preNodeIndex.X, preNodeIndex.Y);
            int preIndex = preNode?.IndexOf(in preEle) ?? -1;

            cache = new QuadTreePreCache(in preEle, in tarEle, in preNodeIndex, in tarNodeIndex, preIndex, tree.TreeId);
            return true;
        }

        public void PreUpdate(in QuadTreePreCache cache)
        {
            var tree = _trees[cache.TreeId];
            var preEle = cache.PreEle;
            var tarEle = cache.TarEle;
            var preNode = tree.GetNode(cache.PreNodeIndex.Depth, cache.PreNodeIndex.X, cache.PreNodeIndex.Y);
            var tarNode = tree.GetOrAddNode(cache.TarNodeIndex.Depth, cache.TarNodeIndex.X, cache.TarNodeIndex.Y);
            int index = cache.PreElementIndex;
            bool usePre = index < preNode.Elements.Count && preNode.Elements[index] == preEle;
            bool hasError = false;

            if (preNode == tarNode)
            {
                if (usePre)
                    tarNode.Update(index, in tarEle);
                else
                    hasError = !tarNode.Update(in preEle, in tarEle);
            }
            else
            {
                if (usePre)
                    preNode.RemoveAt(index);
                else
                    hasError = !preNode.Remove(in preEle);
                tarNode.Add(in tarEle);
                tree.TryRemoveNode(preNode); // “tarNode”可能是“preNode”的子节点，所以要先“Add”，后“RemoveNode”
            }

            if (hasError)
                LogRelay.Error($"[Quad] PreUpdateElement Fail, TreeId:{cache.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            else
                QuadTreeDiagnosis.LogIndex(cache.TreeId, preEle.Index, LogType.Info, "[Quad] PreUpdateElement Success, NodeEqual:{0}, UsePre:{1}, TreeId:{2}, PreEle:{3}, TarEle:{4}", new DiagnosisArgs<bool, bool, int, QuadTreeElement, QuadTreeElement>(preNode == tarNode, usePre, cache.TreeId, preEle, tarEle));
        }
        #endregion

        #region 插入/删除
        public void Insert(int treeId, int index, Vector2DInt center, int extents)
        {
            var tree = _trees[treeId];
            var element = new QuadTreeElement(index, new AABB2DInt(center, extents));
            tree.Insert(in element);
        }
        public void Insert(int treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            var tree = _trees[treeId];
            var element = new QuadTreeElement(index, new AABB2DInt(center, extents));
            tree.Insert(in element);
        }
        public void Insert(int treeId, int index, in AABB2DInt shape)
        {
            var tree = _trees[treeId];
            var element = new QuadTreeElement(index, in shape);
            tree.Insert(in element);
        }

        public bool Remove(int treeId, int index, Vector2DInt center, int extents)
        {
            var tree = _trees[treeId];
            var element = new QuadTreeElement(index, new AABB2DInt(center, extents));
            bool success = tree.Remove(in element);
            return success;
        }
        public bool Remove(int treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            var tree = _trees[treeId];
            var element = new QuadTreeElement(index, new AABB2DInt(center, extents));
            bool success = tree.Remove(in element);
            return success;
        }
        public bool Remove(int treeId, int index, in AABB2DInt shape)
        {
            var tree = _trees[treeId];
            var element = new QuadTreeElement(index, in shape);
            bool success = tree.Remove(in element);
            return success;
        }
        #endregion

        #region 更新
        public void Update(int treeId, int index, in Change<Vector2DInt> center, int extents)
        {
            if (center.Equals())
                return;

            var tree = _trees[treeId];
            var preEle = new QuadTreeElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadTreeElement(index, new AABB2DInt(center.Tar, extents));
            tree.Update(in preEle, in tarEle);
        }
        public void Update(int treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents)
        {
            if (center.Equals())
                return;

            var tree = _trees[treeId];
            var preEle = new QuadTreeElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadTreeElement(index, new AABB2DInt(center.Tar, extents));
            tree.Update(in preEle, in tarEle);
        }

        public void Update(Change<int> treeId, int index, Vector2DInt center, int extents)
        {
            if (treeId.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var element = new QuadTreeElement(index, new AABB2DInt(center, extents));

            if (!preTree.Remove(in element))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:{center}");
            tarTree.Insert(in element);
        }
        public void Update(Change<int> treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            if (treeId.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var element = new QuadTreeElement(index, new AABB2DInt(center, extents));

            if (!preTree.Remove(in element))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:{center}");
            tarTree.Insert(in element);
        }
        public void Update(Change<int> treeId, int index, in AABB2DInt shape)
        {
            if (treeId.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var element = new QuadTreeElement(index, in shape);

            if (!preTree.Remove(in element))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:{shape.Center()}");
            tarTree.Insert(in element);
        }

        public void Update(Change<int> treeId, int index, in Change<Vector2DInt> center, int extents)
        {
            if (treeId.Equals() && center.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var preEle = new QuadTreeElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadTreeElement(index, new AABB2DInt(center.Tar, extents));

            if (!preTree.Remove(in preEle))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:[{center}]");
            tarTree.Insert(in tarEle);
        }
        public void Update(Change<int> treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents)
        {
            if (treeId.Equals() && center.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var preEle = new QuadTreeElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadTreeElement(index, new AABB2DInt(center.Tar, extents));

            if (!preTree.Remove(in preEle))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:[{center}]");
            tarTree.Insert(in tarEle);
        }
        #endregion

        #region 查询Point
        public void QueryPoint(int treeId, Vector2DInt center, ICollection<QuadTreeElement> elements)
        {
            QueryShape(treeId, center, elements);
        }
        public void QueryPoint(IReadOnlyList<int> treeIds, Vector2DInt center, ICollection<QuadTreeElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], center, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, Vector2DInt center, ICollection<QuadTreeElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryPoint(center, elements);
        }
        #endregion

        #region 查询Circle区域
        public void QueryCircle(int treeId, Vector2DInt center, int radius, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new CircleInt(center, radius);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryCircle(int treeId, in CircleInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryCircle(IReadOnlyList<int> treeIds, Vector2DInt center, int radius, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new CircleInt(center, radius);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryCircle(IReadOnlyList<int> treeIds, in CircleInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in CircleInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryCircle(in shape, checkRoot, elements);
        }
        #endregion

        #region 查询AABB区域
        public void QueryAABB(int treeId, Vector2DInt center, int extents, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryAABB(int treeId, Vector2DInt center, Vector2DInt extents, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryAABB(int treeId, in AABB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryAABB(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryAABB(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryAABB(IReadOnlyList<int> treeIds, in AABB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in AABB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryAABB(in shape, checkRoot, elements);
        }
        #endregion

        #region 查询OBB区域
        public void QueryOBB(int treeId, Vector2DInt center, int extents, Fixed64 angle, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryOBB(int treeId, Vector2DInt center, Vector2DInt extents, Fixed64 angle, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryOBB(int treeId, in OBB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryOBB(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, Fixed64 angle, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryOBB(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, Fixed64 angle, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryOBB(IReadOnlyList<int> treeIds, in OBB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in OBB2DInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryOBB(in shape, checkRoot, elements);
        }
        #endregion

        #region 查询Polygon区域
        public void QueryPolygon(int treeId, in ReadOnlyArray<Vector2DInt> points, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new PolygonInt(points);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryPolygon(int treeId, in PolygonInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryPolygon(IReadOnlyList<int> treeIds, in ReadOnlyArray<Vector2DInt> points, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var shape = new PolygonInt(points);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryPolygon(IReadOnlyList<int> treeIds, in PolygonInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in PolygonInt shape, bool checkRoot, ICollection<QuadTreeElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryPolygon(in shape, checkRoot, elements);
        }
        #endregion

        #region 辅助方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildTrees(int depthCount, in AABB2DInt maxBoundary, IReadOnlyList<QuadTreeConfig> configs)
        {
            for (int maxExtents = Math.Max(maxBoundary.W, maxBoundary.H), count = configs.Count, i = 0; i < count; ++i)
            {
                var config = configs[i];
                int depth = depthCount;
                for (int extents = Math.Max(config.Extents.X, config.Extents.Y) << depthCount - 1; depth > 1 && extents > maxExtents; extents >>= 1)
                    --depth;
                var tree = (BasicQuadTree)Activator.CreateInstance(config.TreeType);
                (tree as IDynamicQuadTree)?.Inject(_nodePool);
                tree.OnCreate(config.TreeId, config.Shape, depth, in maxBoundary, _elementPool);
                _trees.Add(config.TreeId, tree);
            }
        }

        internal QuadTreeConfig GetConfig(int treeId) => _configs.GetValueOrDefault(treeId);
        internal void GetTrees(ICollection<BasicQuadTree> trees)
        {
            foreach (var pair in _trees)
                trees.Add(pair.Value);
        }

        public void RemoveEmptyNode(int treeId)
        {
            var tree = _trees[treeId];
            (tree as IDynamicQuadTree)?.RemoveEmptyNode();
        }
        public void RemoveEmptyNode()
        {
            foreach (var pair in _trees)
                if (pair.Value is IDynamicQuadTree tree)
                    tree.RemoveEmptyNode();
        }

        public void Clean()
        {
            foreach (var pair in _trees)
                pair.Value.OnDestroy();
            LogRelay.Debug($"[Quad] Pool, CountRef:{_nodePool.CountRef}, HistoryCapacity:{_nodePool.HistoryCapacity}");
            _trees.Clear();
            _nodePool.Clean();
        }
        #endregion
    }
}