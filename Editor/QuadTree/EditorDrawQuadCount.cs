#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    /// <summary>
    /// 统计四叉树性能数据
    /// </summary>
    internal sealed class EditorDrawQuadCount : MonoBehaviour
    {
        #region 类型
        [Serializable]
        private struct DepthCount
        {
            [ReadOnly] [SerializeField] internal int Depth;
            [ReadOnly] [SerializeField] internal int TotalCount;

            [Space] [ReadOnly] [SerializeField] internal int LineBallCount;
            [ReadOnly] [SerializeField] internal float LineBallRatio; // AABB压线率，越低越好

            internal float SpaceUtility;
            [ReadOnly] [SerializeField] internal float SpaceUtilityRatio; // 压线情况下的AABB空间利用率，越高越好

            internal void AddTotalCount(int count)
            {
                TotalCount += count;
                LineBallRatio = LineBallCount / (float)TotalCount;
            }
            internal void AddLineBall()
            {
                ++LineBallCount;
                LineBallRatio = LineBallCount / (float)TotalCount;
                SpaceUtilityRatio = SpaceUtility / LineBallCount;
            }
            internal void AddSpaceUtility(float count)
            {
                SpaceUtility += count;
                SpaceUtilityRatio = SpaceUtility / LineBallCount;
            }
        }
        #endregion

        #region 序列化字段
        [SerializeField] private int[] _treeIds;
        [SerializeField] private bool _drawLineBall;
        [SerializeField] private float _height;
        [SerializeField] private DepthCount[] _depthCounts = Array.Empty<DepthCount>();
        #endregion

        #region 运行时缓存
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly List<QuadNode> _nodes = new(); // 临时缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadGetter.Proxy.Manager;
            if (manager is null)
                return;

            _scale = 1F / manager.Scale;
            QuadGetter.GetTrees(manager, _trees);
            BuildTree();
        }
        private void OnValidate()
        {
            if (enabled)
                BuildTree();
        }
        private void OnDrawGizmos()
        {
            _depthCounts.Clean();
            if (_treeIds.Length == 0)
                return;

            foreach (int treeId in _treeIds)
            {
                if (!_trees.TryGetValue(treeId, out var tree))
                    continue;

                for (int length = _depthCounts.Length, depth = 0; depth < length; ++depth)
                {
                    var nodes = QuadGetter.GetNodes(tree, depth, _nodes);
                    var depthCount = _depthCounts[depth];
                    depthCount.Depth = depth;

                    foreach (var node in nodes)
                    {
                        depthCount.AddTotalCount(node.SumCount);

                        foreach (var element in node.Elements)
                        {
                            int elementSqr = element.Shape.Size().SqrMagnitude();
                            int boundarySqr = node.LooseBoundary.HalfSize().SqrMagnitude();
                            if (elementSqr >= boundarySqr)
                                continue;

                            float bits = MathF.Ceiling(MathF.Log(elementSqr, 2));
                            float space = MathF.Sqrt((1 << (int)bits) / (float)boundarySqr);
                            depthCount.AddLineBall();
                            depthCount.AddSpaceUtility(space);
                            if (_drawLineBall)
                                ShapeDraw.AABB(in element.Shape, _scale, _height, Color.magenta);
                        }
                    }

                    _depthCounts[depth] = depthCount;
                }
            }
        }

        private void BuildTree()
        {
            if (_trees.Count == 0)
                return;

            int maxDepth = 0;
            foreach (int treeId in _treeIds)
                if (_trees.TryGetValue(treeId, out var tree))
                    maxDepth = Math.Max(maxDepth, tree.MaxDepth);
            _depthCounts = new DepthCount[maxDepth]; // 不计算最后一层
        }
    }
}
#endif