using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    public sealed class QuadTreeManager
    {
        #region 类型
        private readonly struct Size
        {
            internal readonly int HalfWidth; // 节点理想的最小尺寸，必须是2的幂
            internal readonly int HalfHeight; // 节点理想的最小尺寸，必须是2的幂
            internal readonly int SubTreeCount; // 子树的数量

            internal Size(int width, int height, int count)
            {
                HalfWidth = width;
                HalfHeight = height;
                SubTreeCount = count;
            }
        }
        #endregion

        #region 字段 & 构造方法
        public const int CircleEnum = 0;
        public const int DefaultSubTree = 0;

        public readonly AABB2DInt MaxBounds;
        public readonly int DepthCount;
        private readonly Dictionary<int, MeshQuadTree[]> _trees = new();
        private readonly Dictionary<int, Size> _nodeSizes = new() // 节点理想的最小尺寸，必须是2的幂；子树的尺寸
            { };

        public QuadTreeManager(in AABB2DInt maxBounds, int depthCount)
        {
            MaxBounds = maxBounds;
            DepthCount = depthCount;
            BuildTrees();
        }
        #endregion

        #region 插入元素，坐标和尺寸都是War3尺度
        public void InsertElement(Vector2DInt center, int radius, int idx, int funcEnum)
        {
            var tree = GetTree(funcEnum);
            var element = new QuadElement(new AABB2DInt(center, radius), idx);

            tree.Insert(in element);
        }
        public void InsertElement(in Vector2D center, in Vector2D extent, int idx, int funcEnum)
        {
            var tree = GetTree(funcEnum);
            var element = new QuadElement(new AABB2DInt(in center, in extent), idx);

            tree.Insert(in element);
        }

        public void InsertElement(Vector2DInt center, int radius, int idx, int funcEnum, int subTree)
        {
            var tree = GetTree(funcEnum, subTree);
            var element = new QuadElement(new AABB2DInt(center, radius), idx);

            tree.Insert(in element);
        }
        #endregion

        #region 删除元素，坐标和尺寸都是War3尺度
        public bool RemoveElement(Vector2DInt center, int radius, int idx, int funcEnum)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return false;

            var element = new QuadElement(new AABB2DInt(center, radius), idx);
            return tree.Remove(in element);
        }
        public bool RemoveElement(in Vector2D center, in Vector2D extent, int idx, int funcEnum)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return false;

            var element = new QuadElement(new AABB2DInt(in center, in extent), idx);
            return tree.Remove(in element);
        }

        public bool RemoveElement(Vector2DInt center, int radius, int idx, int funcEnum, int subTree)
        {
            var tree = GetTree(funcEnum, subTree);
            if (tree == null)
                return false;

            var element = new QuadElement(new AABB2DInt(center, radius), idx);
            return tree.Remove(in element);
        }
        #endregion

        #region 更新元素，坐标和尺寸都是War3尺度
        public void UpdateElement(Vector2DInt preCenter, Vector2DInt tarCenter, int radius, int idx, int funcEnum)
        {
            if (preCenter == tarCenter)
                return;

            var tree = GetTree(funcEnum);
            var preEle = new QuadElement(new AABB2DInt(preCenter, radius), idx);
            var tarEle = new QuadElement(new AABB2DInt(tarCenter, radius), idx);

            tree.Update(in preEle, in tarEle);
        }

        public void UpdateElement(Vector2DInt preCenter, Vector2DInt tarCenter, int radius, int idx, int funcEnum, int subTree)
        {
            if (preCenter == tarCenter)
                return;

            var tree = GetTree(funcEnum, subTree);
            var preEle = new QuadElement(new AABB2DInt(preCenter, radius), idx);
            var tarEle = new QuadElement(new AABB2DInt(tarCenter, radius), idx);

            tree.Update(in preEle, in tarEle);
        }
        public void UpdateElement(Vector2DInt center, int radius, int idx, int funcEnum, int preSubTree, int tarSubTree)
        {
            if (preSubTree == tarSubTree)
                return;

            var trees = _trees[funcEnum];
            var preTree = GetTree(trees, preSubTree);
            var tarTree = GetTree(trees, tarSubTree);
            var element = new QuadElement(new AABB2DInt(center, radius), idx);

            if (!preTree.Remove(in element))
                LogRelay.Error($"[Quad] RemoveElement Fail, FuncEnum:{funcEnum}, PreSubTree:{preSubTree}, TarSubTree:{tarSubTree}, Ele:{element}");
            tarTree.Insert(in element);
        }
        public void UpdateElement(Vector2DInt preCenter, Vector2DInt tarCenter, int radius, int idx, int funcEnum, int preSubTree, int tarSubTree)
        {
            if (preCenter == tarCenter && preSubTree == tarSubTree)
                return;

            var trees = _trees[funcEnum];
            var preTree = GetTree(trees, preSubTree);
            var tarTree = GetTree(trees, tarSubTree);

            var preEle = new QuadElement(new AABB2DInt(preCenter, radius), idx);
            var tarEle = new QuadElement(new AABB2DInt(tarCenter, radius), idx);

            if (!preTree.Remove(in preEle))
                LogRelay.Error($"[Quad] RemoveElement Fail, FuncEnum:{funcEnum}, PreSubTree:{preSubTree}, TarSubTree:{tarSubTree}, PreEle:{preEle}, tarEle:{tarEle}");
            tarTree.Insert(in tarEle);
        }
        #endregion

        #region 预处理元素，坐标和尺寸都是War3尺度
        public bool PreUpdateElement(Vector2DInt preCenter, Vector2DInt tarCenter, int radius, int idx, int funcEnum, out QuadPreCache cache)
        {
            if (preCenter == tarCenter)
            {
                cache = default;
                return false;
            }

            var tree = GetTree(funcEnum);
            var preEle = new QuadElement(new AABB2DInt(preCenter, radius), idx);
            var tarEle = new QuadElement(new AABB2DInt(tarCenter, radius), idx);
            var preNode = tree.GetNode(in preEle.AABB);
            var tarNode = tree.GetNode(in tarEle.AABB);
            int preIndex = preNode.IndexOf(in preEle);
            cache = new QuadPreCache(in preEle, in tarEle, preNode, tarNode, preIndex, tree.TreeId);

            if (MeshQuadTree.ShowLog)
                LogRelay.Info($"[Quad] PreUpdateElement, NodeEqual:{preNode == tarNode}, TreeId:{tree.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            return true;
        }
        public bool PreUpdateElement(Vector2DInt preCenter, Vector2DInt tarCenter, int radius, int idx, int funcEnum, int subTree, out QuadPreCache cache)
        {
            if (preCenter == tarCenter)
            {
                cache = default;
                return false;
            }

            var tree = GetTree(funcEnum, subTree);
            var preEle = new QuadElement(new AABB2DInt(preCenter, radius), idx);
            var tarEle = new QuadElement(new AABB2DInt(tarCenter, radius), idx);
            var preNode = tree.GetNode(in preEle.AABB);
            var tarNode = tree.GetNode(in tarEle.AABB);
            int preIndex = preNode.IndexOf(in preEle);
            cache = new QuadPreCache(in preEle, in tarEle, preNode, tarNode, preIndex, tree.TreeId);

            if (MeshQuadTree.ShowLog)
                LogRelay.Info($"[Quad] PreUpdateElement, NodeEqual:{preNode == tarNode}, TreeId:{tree.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
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
                LogRelay.Error($"[Quad] UpdateElement Fail, TreeId:{cache.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
            else if (MeshQuadTree.ShowLog)
                LogRelay.Info($"[Quad] RemoveElement Success, NodeEqual:{preNode == tarNode}, UsePre:{usePre}, TreeId:{cache.TreeId}, PreEle:{preEle}, TarEle:{tarEle}");
        }
        #endregion

        #region 查询圆形区域内的元素，坐标和尺寸都是War3尺度
        public void QueryCircle(Vector2DInt center, Fixed64 radius, int funcEnum, ICollection<QuadElement> elements)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return;

            var aabb = new AABB2DInt(center, radius);
            tree.QueryCircle(in aabb, elements);
        }
        public void QueryCircle(in Vector2D center, Fixed64 radius, int funcEnum, ICollection<QuadElement> elements)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return;

            var aabb = new AABB2DInt(in center, radius);
            tree.QueryCircle(in aabb, elements);
        }

        public void QueryCircle(Vector2DInt center, int radius, int funcEnum, int subTree, ICollection<QuadElement> elements)
        {
            var tree = GetTree(funcEnum, subTree);
            if (tree == null)
                return;

            var aabb = new AABB2DInt(center, radius);
            tree.QueryCircle(in aabb, elements);
        }
        public void QueryCircle(Vector2DInt center, int radius, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];
            var aabb = new AABB2DInt(center, radius);

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryCircle(in aabb, elements);
            }
        }
        public void QueryCircle(in Vector2D center, Fixed64 radius, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];
            var aabb = new AABB2DInt(in center, radius);

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryCircle(in aabb, elements);
            }
        }
        #endregion

        #region 查询无向矩阵区域内的元素，坐标和尺寸都是War3尺度
        public void QueryBox(Vector2DInt center, int radius, int funcEnum, ICollection<QuadElement> elements)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return;

            var aabb = new AABB2DInt(center, radius);
            tree.QueryBox(in aabb, elements);
        }
        public void QueryBox(Vector2DInt center, Vector2DInt extent, int funcEnum, ICollection<QuadElement> elements)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return;

            var aabb = new AABB2DInt(center, extent);
            tree.QueryBox(in aabb, elements);
        }
        public void QueryBox(in Vector2D center, in Vector2D extent, int funcEnum, ICollection<QuadElement> elements)
        {
            var tree = GetTree(funcEnum);
            if (tree == null)
                return;

            var aabb = new AABB2DInt(in center, in extent);
            tree.QueryBox(in aabb, elements);
        }

        public void QueryBox(Vector2DInt center, int radius, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];
            var aabb = new AABB2DInt(center, radius);

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryBox(in aabb, elements);
            }
        }
        public void QueryBox(Vector2DInt center, Vector2DInt extent, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];
            var aabb = new AABB2DInt(center, extent);

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryBox(in aabb, elements);
            }
        }
        public void QueryBox(in Vector2D center, in Vector2D extent, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];
            var aabb = new AABB2DInt(in center, in extent);

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryBox(in aabb, elements);
            }
        }
        #endregion

        #region 查询有向矩形区域内的元素，坐标和尺寸都是War3尺度
        public void QueryRectangle(in Vector2D center, in Vector2D extent, in Vector2D dir, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];
            var aabb = new AABB2DInt(in center, in extent);

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryRectangle(in aabb, in dir, elements);
            }
        }
        #endregion

        #region 查询不规则四边形区域内的元素，坐标和尺寸都是War3尺度
        public void QueryQuadrangle(in Vector2D leftBottom, in Vector2D leftTop, in Vector2D rightTop, in Vector2D rightBottom, int funcEnum, IReadOnlyList<int> subTrees, ICollection<QuadElement> elements)
        {
            var trees = _trees[funcEnum];

            for (int count = subTrees.Count, i = 0; i < count; ++i)
            {
                int subTree = subTrees[i];
                var tree = GetTree(trees, subTree);
                tree.QueryQuadrangle(in leftBottom, in leftTop, in rightTop, in rightBottom, elements);
            }
        }
        #endregion

        #region 辅助方法
        public static string TreeName => nameof(_trees);
        public void Clean()
        {
            foreach (var pair in _trees)
            {
                foreach (var tree in pair.Value)
                    tree.Clean();

                pair.Value.CleanAll();
            }
        }

        private void BuildTrees()
        {
            foreach (var pair in _nodeSizes)
            {
                var func = pair.Key;
                var size = pair.Value;
                bool circle = (func & CircleEnum) != 0;
                var trees = new MeshQuadTree[size.SubTreeCount];
                _trees.Add(func, trees);

                int depthCount = DepthCount;
                int maxSize = Math.Max(size.HalfWidth, size.HalfHeight) << depthCount - 1;
                int boundsSize = Math.Max(MaxBounds.W, MaxBounds.H);
                while (depthCount > 1 && maxSize > boundsSize)
                {
                    --depthCount;
                    maxSize >>= 1;
                }

                for (int i = 0; i < size.SubTreeCount; ++i)
                {
                    var tree = new MeshQuadTree(in MaxBounds, depthCount, circle, (int)func * 1000 + i);
                    trees[i] = tree;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MeshQuadTree GetTree(int funcEnum, int subTree = DefaultSubTree) => _trees[funcEnum][subTree];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MeshQuadTree GetTree(IReadOnlyList<MeshQuadTree> trees, int subTree = DefaultSubTree) => trees[subTree];
        #endregion
    }
}