using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using Eevee.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    public sealed class QuadTreeManager
    {
        #region 字段/构造方法
        public readonly int Scale; // 缩放比例，(int)(Fixed64 * Scale) = int
        public readonly Fixed64 Reciprocal; // = 1 / Scale
        public readonly int DepthCount;
        public readonly AABB2DInt MaxBoundary;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();

        public QuadTreeManager(int scale, int depthCount, in AABB2DInt maxBoundary, IReadOnlyList<QuadTreeConfig> configs)
        {
            Scale = scale;
            Reciprocal = Fixed64.One / scale;
            DepthCount = depthCount;
            MaxBoundary = maxBoundary;
            BuildTrees(depthCount, in maxBoundary, configs);
        }
        #endregion

        #region 预处理
        public bool PreCount(int treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents, out QuadPreCache cache)
        {
            if (center.Equals())
            {
                cache = default;
                return false;
            }

            var tree = _trees[treeId];
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));

            tree.TryGetNodeIndex(in preEle.AABB, QuadExt.CountMode, out var preNodeIndex);
            tree.TryGetNodeIndex(in tarEle.AABB, QuadExt.CountMode, out var tarNodeIndex);
            var preNode = tree.GetOrAddNode(preNodeIndex.Depth, preNodeIndex.X, preNodeIndex.Y, false);
            int preIndex = preNode?.IndexOf(in preEle) ?? -1;

            cache = new QuadPreCache(in preEle, in tarEle, in preNodeIndex, in tarNodeIndex, preIndex, tree.TreeId);
            if (QuadDebug.CheckIndex(treeId, index))
                LogRelay.Info($"[Quad] PreCountElement, NodeEqual:{preNodeIndex == tarNodeIndex}, TreeId:{tree.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            return true;
        }

        public void PreUpdate(in QuadPreCache cache)
        {
            var tree = _trees[cache.TreeId];
            var preEle = cache.PreEle;
            var tarEle = cache.TarEle;
            var preNode = tree.GetOrAddNode(cache.PreNodeIndex.Depth, cache.PreNodeIndex.X, cache.PreNodeIndex.Y, false);
            var tarNode = tree.GetOrAddNode(cache.TarNodeIndex.Depth, cache.TarNodeIndex.X, cache.TarNodeIndex.Y, true);
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
                if (preNode.IsEmpty())
                    tree.RemoveNode(preNode);
                tarNode.Add(in tarEle);
            }

            if (hasError)
                LogRelay.Error($"[Quad] PreUpdateElement Fail, TreeId:{cache.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            else if (QuadDebug.Check())
                LogRelay.Info($"[Quad] PreUpdateElement Success, NodeEqual:{preNode == tarNode}, UsePre:{usePre}, TreeId:{cache.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
        }
        #endregion

        #region 插入/删除
        public void Insert(int treeId, int index, Vector2DInt center, int extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            tree.Insert(in element);
        }
        public void Insert(int treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            tree.Insert(in element);
        }

        public bool Remove(int treeId, int index, Vector2DInt center, int extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            bool success = tree.Remove(in element);
            return success;
        }
        public bool Remove(int treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
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
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));
            tree.Update(in preEle, in tarEle);
        }
        public void Update(int treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents)
        {
            if (center.Equals())
                return;

            var tree = _trees[treeId];
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));
            tree.Update(in preEle, in tarEle);
        }

        public void Update(Change<int> treeId, int index, Vector2DInt center, int extents)
        {
            if (treeId.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var element = new QuadElement(index, new AABB2DInt(center, extents));

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
            var element = new QuadElement(index, new AABB2DInt(center, extents));

            if (!preTree.Remove(in element))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:{center}");
            tarTree.Insert(in element);
        }

        public void Update(Change<int> treeId, int index, in Change<Vector2DInt> center, int extents)
        {
            if (treeId.Equals() && center.Equals())
                return;

            var preTree = _trees[treeId.Pre];
            var tarTree = _trees[treeId.Tar];
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));

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
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));

            if (!preTree.Remove(in preEle))
                LogRelay.Error($"[Quad] RemoveElement Fail, TreeId:({treeId}), Index:{index}, Center:[{center}]");
            tarTree.Insert(in tarEle);
        }
        #endregion

        #region 查询Point
        public void QueryPoint(int treeId, Vector2DInt center, ICollection<QuadElement> elements)
        {
            QueryShape(treeId, center, elements);
        }
        public void QueryPoint(IReadOnlyList<int> treeIds, Vector2DInt center, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], center, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, Vector2DInt center, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryPoint(center, elements);
        }
        #endregion

        #region 查询Circle区域
        public void QueryCircle(int treeId, Vector2DInt center, int radius, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new CircleInt(center, radius);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryCircle(int treeId, in CircleInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryCircle(IReadOnlyList<int> treeIds, Vector2DInt center, int radius, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new CircleInt(center, radius);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryCircle(IReadOnlyList<int> treeIds, in CircleInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in CircleInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryCircle(in shape, checkRoot, elements);
        }
        #endregion

        #region 查询AABB区域
        public void QueryAABB(int treeId, Vector2DInt center, int extents, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryAABB(int treeId, Vector2DInt center, Vector2DInt extents, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryAABB(int treeId, in AABB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryAABB(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryAABB(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryAABB(IReadOnlyList<int> treeIds, in AABB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in AABB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryAABB(in shape, checkRoot, elements);
        }
        #endregion

        #region 查询OBB区域
        public void QueryOBB(int treeId, Vector2DInt center, int extents, Fixed64 angle, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryOBB(int treeId, Vector2DInt center, Vector2DInt extents, Fixed64 angle, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryOBB(int treeId, in OBB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryOBB(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, Fixed64 angle, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryOBB(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, Fixed64 angle, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new OBB2DInt(center, extents, angle);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryOBB(IReadOnlyList<int> treeIds, in OBB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in OBB2DInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryOBB(in shape, checkRoot, elements);
        }
        #endregion

        #region 查询Polygon区域
        public void QueryPolygon(int treeId, in ReadOnlyArray<Vector2DInt> points, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new PolygonInt(points);
            QueryShape(treeId, in shape, checkRoot, elements);
        }
        public void QueryPolygon(int treeId, in PolygonInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            QueryShape(treeId, in shape, checkRoot, elements);
        }

        public void QueryPolygon(IReadOnlyList<int> treeIds, in ReadOnlyArray<Vector2DInt> points, bool checkRoot, ICollection<QuadElement> elements)
        {
            var shape = new PolygonInt(points);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }
        public void QueryPolygon(IReadOnlyList<int> treeIds, in PolygonInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
                QueryShape(treeIds[i], in shape, checkRoot, elements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QueryShape(int treeId, in PolygonInt shape, bool checkRoot, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryPolygon(in shape, checkRoot, elements);
        }
        #endregion

        #region 辅助方法
        public int F2I(Fixed64 value) => Fixed64ToInt32(value);
        public Vector2DInt F2I(in Vector2D value) => new(Fixed64ToInt32(value.X), Fixed64ToInt32(value.Y));
        public CircleInt F2I(in Circle value) => new(Fixed64ToInt32(value.X), Fixed64ToInt32(value.Y), Fixed64ToInt32(value.R));
        public AABB2DInt F2I(in AABB2D value) => new(Fixed64ToInt32(value.X), Fixed64ToInt32(value.Y), Fixed64ToInt32(value.W), Fixed64ToInt32(value.H));
        public OBB2DInt F2I(in OBB2D value) => new(Fixed64ToInt32(value.X), Fixed64ToInt32(value.Y), Fixed64ToInt32(value.W), Fixed64ToInt32(value.H), value.A);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Fixed64ToInt32(Fixed64 value) => (int)(value * Scale);

        public Fixed64 I2F(int value) => Int32ToFixed64(value);
        public Vector2D I2F(Vector2DInt value) => new(Int32ToFixed64(value.X), Int32ToFixed64(value.Y));
        public Circle I2F(in CircleInt value) => new(Int32ToFixed64(value.X), Int32ToFixed64(value.Y), Int32ToFixed64(value.R));
        public AABB2D I2F(in AABB2DInt value) => new(Int32ToFixed64(value.X), Int32ToFixed64(value.Y), Int32ToFixed64(value.W), Int32ToFixed64(value.H));
        public OBB2D I2F(in OBB2DInt value) => new(Int32ToFixed64(value.X), Int32ToFixed64(value.Y), Int32ToFixed64(value.W), Int32ToFixed64(value.H), value.A);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Fixed64 Int32ToFixed64(int value) => value * Reciprocal;

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
                tree.OnCreate(config.TreeId, config.Shape, depth, in maxBoundary);
                _trees.Add(config.TreeId, tree);
            }
        }
        public void Clean()
        {
            foreach (var pair in _trees)
                pair.Value.OnDestroy();
            _trees.Clear();
        }
        #endregion
    }
}