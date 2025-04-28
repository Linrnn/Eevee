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
        public readonly AABB2DInt MaxBounds;
        private readonly Dictionary<int, QuadTreeConfig> _configs = new();
        private readonly Dictionary<int, MeshQuadTree> _trees = new();

        public QuadTreeManager(int scale, int depthCount, in AABB2DInt maxBounds, IList<QuadTreeConfig> configs)
        {
            Scale = scale;
            Reciprocal = Fixed64.One / scale;
            DepthCount = depthCount;
            MaxBounds = maxBounds;
            BuildConfigs(configs);
            BuildTrees(depthCount, in maxBounds);
        }
        #endregion

        #region 预处理
        public bool PreCount(int treeId, int index, in Change<Vector2DInt> center, int extents, out QuadPreCache cache)
        {
            if (center.Equals())
            {
                cache = default;
                return false;
            }

            var tree = _trees[treeId];
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));
            var preNode = tree.GetNode(in preEle.AABB);
            var tarNode = tree.GetNode(in tarEle.AABB);
            int preIndex = preNode.IndexOf(in preEle);
            cache = new QuadPreCache(in preEle, in tarEle, preNode, tarNode, preIndex, tree.TreeId);

            if (MeshQuadTree.ShowLog)
                LogRelay.Info($"[Quad] PreCountElement, NodeEqual:{preNode == tarNode}, TreeId:{tree.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            return true;
        }
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
            var preNode = tree.GetNode(in preEle.AABB);
            var tarNode = tree.GetNode(in tarEle.AABB);
            int preIndex = preNode.IndexOf(in preEle);
            cache = new QuadPreCache(in preEle, in tarEle, preNode, tarNode, preIndex, tree.TreeId);

            if (MeshQuadTree.ShowLog)
                LogRelay.Info($"[Quad] PreCountElement, NodeEqual:{preNode == tarNode}, TreeId:{tree.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            return true;
        }

        public void PreUpdate(in QuadPreCache cache)
        {
            var preEle = cache.PreEle;
            var tarEle = cache.TarEle;
            var preNode = cache.PreNode;
            var tarNode = cache.TarNode;
            int index = cache.PreIndex;
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
            }

            if (hasError)
                LogRelay.Error($"[Quad] PreUpdateElement Fail, TreeId:{cache.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            else if (MeshQuadTree.ShowLog)
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

        #region 查询点
        public void QueryPoint(int treeId, Vector2DInt center, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryPoint(center, elements);
        }
        public void QueryPoint(IReadOnlyList<int> treeIds, Vector2DInt center, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryPoint(center, elements);
            }
        }
        #endregion

        #region 查询圆形区域
        public void QueryCircle(int treeId, Vector2DInt center, int radius, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var area = new CircleInt(center, radius);
            tree.QueryCircle(in area, elements);
        }
        public void QueryCircle(IReadOnlyList<int> treeIds, Vector2DInt center, int radius, ICollection<QuadElement> elements)
        {
            var area = new CircleInt(center, radius);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryCircle(in area, elements);
            }
        }
        #endregion

        #region 查询AABB区域
        public void QueryAABB(int treeId, Vector2DInt center, int extents, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var area = new AABB2DInt(center, extents);
            tree.QueryAABB(in area, elements);
        }
        public void QueryAABB(int treeId, Vector2DInt center, Vector2DInt extents, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var area = new AABB2DInt(center, extents);
            tree.QueryAABB(in area, elements);
        }

        public void QueryAABB(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, ICollection<QuadElement> elements)
        {
            var area = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryAABB(in area, elements);
            }
        }
        public void QueryAABB(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, ICollection<QuadElement> elements)
        {
            var area = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryAABB(in area, elements);
            }
        }
        #endregion

        #region 查询OBB区域
        public void QueryOBB(int treeId, Vector2DInt center, int extents, Fixed64 angle, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new OBB2DInt(center, extents, angle);
            tree.QueryOOB(in aabb, elements);
        }
        public void QueryOBB(int treeId, Vector2DInt center, Vector2DInt extents, Fixed64 angle, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var area = new OBB2DInt(center, extents, angle);
            tree.QueryOOB(in area, elements);
        }

        public void QueryOBB(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, Fixed64 angle, ICollection<QuadElement> elements)
        {
            var area = new OBB2DInt(center, extents, angle);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryOOB(in area, elements);
            }
        }
        public void QueryOBB(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, Fixed64 angle, ICollection<QuadElement> elements)
        {
            var area = new OBB2DInt(center, extents, angle);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryOOB(in area, elements);
            }
        }
        #endregion

        #region 查询多边形区域
        public void QueryPolygon(int treeId, in Vector2D lb, in Vector2D lt, in Vector2D rt, in Vector2D rb, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryPolygon(in lb, in lt, in rt, in rb, elements);
        }
        public void QueryPolygon(IReadOnlyList<int> treeIds, in Vector2D lb, in Vector2D lt, in Vector2D rt, in Vector2D rb, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryPolygon(in lb, in lt, in rt, in rb, elements);
            }
        }
        #endregion

        #region 辅助方法
        public int F2I(Fixed64 value) => (int)(value * Scale);
        public Vector2DInt F2I(in Vector2D value) => new(F2I(value.X), F2I(value.Y));
        public CircleInt F2I(in Circle value) => new(F2I(value.X), F2I(value.Y), F2I(value.R));
        public AABB2DInt F2I(in AABB2D value) => new(F2I(value.X), F2I(value.Y), F2I(value.W), F2I(value.H));

        public Fixed64 I2F(int value) => value * Reciprocal;
        public Vector2D I2F(Vector2DInt value) => new(I2F(value.X), I2F(value.Y));
        public Circle I2F(in CircleInt value) => new(I2F(value.X), I2F(value.Y), I2F(value.R));
        public AABB2D I2F(in AABB2DInt value) => new(I2F(value.X), I2F(value.Y), I2F(value.W), I2F(value.H));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildConfigs(IList<QuadTreeConfig> sizes)
        {
            for (int count = sizes.Count, i = 0; i < count; ++i)
                _configs.Add(sizes[i].TreeId, sizes[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildTrees(int depthCount, in AABB2DInt maxBounds)
        {
            foreach ((int treeId, var config) in _configs)
            {
                int depth = depthCount;
                int size = Math.Max(config.Size.X, config.Size.Y) << depthCount - 1;
                int bounds = Math.Max(maxBounds.W, maxBounds.H);

                while (depth > 1 && size > bounds)
                {
                    --depth;
                    size >>= 1;
                }

                _trees.Add(treeId, new MeshQuadTree(treeId, config.Shape, depth, in maxBounds));
            }
        }
        public void Clean()
        {
            _configs.Clear();
            foreach (var pair in _trees)
                pair.Value.Clean();
            _trees.Clear();
        }
        #endregion
    }
}