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
        public readonly int DepthCount;
        public readonly AABB2DInt MaxBounds;
        private readonly Dictionary<int, QuadTreeConfig> _configs = new();
        private readonly Dictionary<int, MeshQuadTree> _trees = new();

        public QuadTreeManager(int depthCount, in AABB2DInt maxBounds, IList<QuadTreeConfig> configs)
        {
            DepthCount = depthCount;
            MaxBounds = maxBounds;
            BuildConfigs(configs);
            BuildTrees(depthCount, in maxBounds);
        }
        #endregion

        #region 预处理元素
        public bool PreCountElement(int treeId, int index, in Change<Vector2DInt> center, int extents, out QuadPreCache cache)
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
        public bool PreCountElement(int treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents, out QuadPreCache cache)
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

        public void PreUpdateElement(in QuadPreCache cache)
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

        #region 插入/删除元素
        public void InsertElement(int treeId, int index, Vector2DInt center, int extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            tree.Insert(in element);
        }
        public void InsertElement(int treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            tree.Insert(in element);
        }

        public bool RemoveElement(int treeId, int index, Vector2DInt center, int extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            bool success = tree.Remove(in element);
            return success;
        }
        public bool RemoveElement(int treeId, int index, Vector2DInt center, Vector2DInt extents)
        {
            var tree = _trees[treeId];
            var element = new QuadElement(index, new AABB2DInt(center, extents));
            bool success = tree.Remove(in element);
            return success;
        }
        #endregion

        #region 更新元素
        public void UpdateElement(int treeId, int index, in Change<Vector2DInt> center, int extents)
        {
            if (center.Equals())
                return;

            var tree = _trees[treeId];
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));
            tree.Update(in preEle, in tarEle);
        }
        public void UpdateElement(int treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents)
        {
            if (center.Equals())
                return;

            var tree = _trees[treeId];
            var preEle = new QuadElement(index, new AABB2DInt(center.Pre, extents));
            var tarEle = new QuadElement(index, new AABB2DInt(center.Tar, extents));
            tree.Update(in preEle, in tarEle);
        }

        public void UpdateElement(Change<int> treeId, int index, Vector2DInt center, int extents)
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
        public void UpdateElement(Change<int> treeId, int index, Vector2DInt center, Vector2DInt extents)
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

        public void UpdateElement(Change<int> treeId, int index, in Change<Vector2DInt> center, int extents)
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
        public void UpdateElement(Change<int> treeId, int index, in Change<Vector2DInt> center, Vector2DInt extents)
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

        #region 查询圆形区域内的元素
        public void QueryCircle(int treeId, Vector2DInt center, int extents, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new AABB2DInt(center, extents);
            tree.QueryCircle(in aabb, elements);
        }
        public void QueryCircle(int treeId, Vector2DInt center, Vector2DInt extents, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new AABB2DInt(center, extents);
            tree.QueryCircle(in aabb, elements);
        }

        public void QueryCircle(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, ICollection<QuadElement> elements)
        {
            var aabb = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryCircle(in aabb, elements);
            }
        }
        public void QueryCircle(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, ICollection<QuadElement> elements)
        {
            var aabb = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryCircle(in aabb, elements);
            }
        }
        #endregion

        #region 查询无向矩阵区域内的元素
        public void QueryBox(int treeId, Vector2DInt center, int extents, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new AABB2DInt(center, extents);
            tree.QueryBox(in aabb, elements);
        }
        public void QueryBox(int treeId, Vector2DInt center, Vector2DInt extents, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new AABB2DInt(center, extents);
            tree.QueryBox(in aabb, elements);
        }

        public void QueryBox(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, ICollection<QuadElement> elements)
        {
            var aabb = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryBox(in aabb, elements);
            }
        }
        public void QueryBox(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, ICollection<QuadElement> elements)
        {
            var aabb = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryBox(in aabb, elements);
            }
        }
        #endregion

        #region 查询有向矩形区域内的元素
        public void QueryRectangle(int treeId, Vector2DInt center, int extents, in Vector2D dir, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new AABB2DInt(center, extents);
            tree.QueryRectangle(in aabb, in dir, elements);
        }
        public void QueryRectangle(int treeId, Vector2DInt center, Vector2DInt extents, in Vector2D dir, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            var aabb = new AABB2DInt(center, extents);
            tree.QueryRectangle(in aabb, in dir, elements);
        }

        public void QueryRectangle(IReadOnlyList<int> treeIds, Vector2DInt center, int extents, in Vector2D dir, ICollection<QuadElement> elements)
        {
            var aabb = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryRectangle(in aabb, in dir, elements);
            }
        }
        public void QueryRectangle(IReadOnlyList<int> treeIds, Vector2DInt center, Vector2DInt extents, in Vector2D dir, ICollection<QuadElement> elements)
        {
            var aabb = new AABB2DInt(center, extents);
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryRectangle(in aabb, in dir, elements);
            }
        }
        #endregion

        #region 查询不规则四边形区域内的元素
        public void QueryQuadrangle(int treeId, in Vector2D lb, in Vector2D lt, in Vector2D rt, in Vector2D rb, ICollection<QuadElement> elements)
        {
            var tree = _trees[treeId];
            tree.QueryQuadrangle(in lb, in lt, in rt, in rb, elements);
        }
        public void QueryQuadrangle(IReadOnlyList<int> treeIds, in Vector2D lb, in Vector2D lt, in Vector2D rt, in Vector2D rb, ICollection<QuadElement> elements)
        {
            for (int count = treeIds.Count, i = 0; i < count; ++i)
            {
                int treeId = treeIds[i];
                var tree = _trees[treeId];
                tree.QueryQuadrangle(in lb, in lt, in rt, in rb, elements);
            }
        }
        #endregion

        #region 辅助方法
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