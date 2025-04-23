#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.QuadTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorDrawQuadCountElement : MonoBehaviour
    {
        #region Type
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

        private float _lineDuration;
        private IDictionary<int, MeshQuadTree[]> _trees;
        private MeshQuadTree[] _tree;

        [SingleEnum] [SerializeField] private int _funcEnum;
        [SerializeField] private bool _drawLineBall;
        [SerializeField] private float _height = 1;
        [SerializeField] private DepthCount[] _depthCounts = Array.Empty<DepthCount>();

        private void OnEnable()
        {
            _lineDuration = Time.fixedDeltaTime;

            // todo eevee
            //var manager = BATEntry.Data?.quadTree;
            //if (manager == null)
            //    return;

            //var treeFiled = manager.GetType().GetField(QuadTreeManager.TreeName, BindingFlags.Instance | BindingFlags.NonPublic);
            //_trees = treeFiled?.GetValue(manager) as IDictionary<int, MeshQuadTree[]>;
            //BuildTree();
        }
        private void OnValidate()
        {
            if (!enabled)
                return;

            BuildTree();
        }
        private void FixedUpdate()
        {
            _depthCounts.CleanAll();

            if (_tree.IsNullOrEmpty())
                return;

            foreach (var tree in _tree)
            {
                if (tree == null)
                    continue;

                for (int depthLength = _depthCounts.Length, depth = 0; depth < depthLength; ++depth)
                {
                    var nodes = tree.Nodes[depth];
                    var depthCount = _depthCounts[depth];
                    depthCount.Depth = depth;

                    foreach (var node in nodes)
                    {
                        depthCount.AddTotalCount(node.SumCount);

                        foreach (var element in node.Elements.AsReadOnlySpan())
                        {
                            int elementSqr = element.AABB.Size().SqrMagnitude();
                            int boundsSqr = node.LooseBounds.HalfSize().SqrMagnitude();
                            if (elementSqr >= boundsSqr)
                                continue;

                            float bits = MathF.Ceiling(MathF.Log(elementSqr, 2));
                            float space = MathF.Sqrt((1 << (int)bits) / (float)boundsSqr);
                            depthCount.AddLineBall();
                            depthCount.AddSpaceUtility(space);
                            if (_drawLineBall)
                                EditorHelper.DrawRect(in element.AABB, Color.magenta, _height, _lineDuration);
                        }
                    }

                    _depthCounts[depth] = depthCount;
                }
            }
        }

        private void BuildTree()
        {
            _depthCounts = Array.Empty<DepthCount>();

            if (_trees == null)
                return;

            _trees.TryGetValue(_funcEnum, out var cacheTree);
            if (cacheTree == null)
                return;

            _tree = cacheTree;
            foreach (var tree in cacheTree)
            {
                if (tree == null)
                    continue;

                _depthCounts = new DepthCount[tree.HalfBoundsSizes.Length - 1]; // 不计算最后一层
                break;
            }
        }
    }
}
#endif