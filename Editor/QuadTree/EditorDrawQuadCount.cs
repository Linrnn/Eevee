#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using UnityEditor;
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
            [SerializeField] internal int Depth;
            [SerializeField] internal int SumCross; // 层级以下的压线数总和
            [SerializeField] internal int Cross; // 压线数
            [SerializeField] internal int Count; // 节点数量
            [SerializeField] private float _crossRatio; // 压线率，越低越好
            [SerializeField] internal int Used;
            [SerializeField] internal long Space;
            [SerializeField] private float _usedRatio; // 压线情况下的空间利用率，越高越好

            internal void CountData()
            {
                _crossRatio = Count == 0 ? 0 : Cross / (float)Count;
                _usedRatio = Space == 0 ? 1 : MathF.Sqrt(Used / (float)Space);
            }

            [CustomPropertyDrawer(typeof(DepthCount))]
            internal sealed class DepthCountDrawer : PropertyDrawer
            {
                private const int HeightScale = 5;
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    var size = new Vector2(position.size.x, position.size.y / HeightScale);
                    var depthPosition = new Rect(position.position, size);
                    var sumCrossPosition = new Rect(position.x, position.y + size.y, size.x, size.y);
                    var crossPosition = new Rect(position.x, position.y + size.y * 2, size.x, size.y);
                    var crossRatioPosition = new Rect(position.x, position.y + size.y * 3, size.x, size.y);
                    var usedRatioPosition = new Rect(position.x, position.y + size.y * 4, size.x, size.y);

                    var depthProperty = property.FindPropertyRelative(nameof(Depth));
                    var sumCrossProperty = property.FindPropertyRelative(nameof(SumCross));
                    var crossProperty = property.FindPropertyRelative(nameof(Cross));
                    var crossRatioProperty = property.FindPropertyRelative(nameof(_crossRatio));
                    var usedRatioProperty = property.FindPropertyRelative(nameof(_usedRatio));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(depthPosition, depthProperty);
                    EditorGUI.PropertyField(sumCrossPosition, sumCrossProperty);
                    EditorGUI.PropertyField(crossPosition, crossProperty);
                    EditorGUI.TextField(crossRatioPosition, crossRatioProperty.displayName, crossRatioProperty.floatValue.ToString("p"));
                    EditorGUI.TextField(usedRatioPosition, usedRatioProperty.displayName, usedRatioProperty.floatValue.ToString("p"));
                    EditorGUI.EndDisabledGroup();

                    depthProperty.Dispose();
                    crossProperty.Dispose();
                    crossRatioProperty.Dispose();
                    sumCrossProperty.Dispose();
                    usedRatioProperty.Dispose();
                }
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label) * HeightScale;
            }
        }
        #endregion

        #region 序列化字段
        [SerializeField] private int[] _treeIds;
        [SerializeField] private ColorSetting _lineBall = new(true, Color.magenta); // 压线的元素
        [SerializeField] private float _height;
        [SerializeField] private bool _drawIndex = true;
        [SerializeField] private DepthCount[] _depthCounts = Array.Empty<DepthCount>();
        #endregion

        #region 运行时缓存
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly List<QuadNode> _nodes = new(); // 临时缓存
        private readonly List<QuadElement> _elements = new(); // 临时缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadGetter.Proxy.Manager;
            if (manager is null)
                return;

            _scale = 1F / manager.Scale;
            QuadGetter.GetTrees(manager, _trees);
            ReadyTree();
        }
        private void OnValidate()
        {
            if (!enabled)
                return;
            ReadyTree();
        }
        private void Update()
        {
            ReadyData();
        }
        private void OnDrawGizmos()
        {
            if (!_lineBall.Show)
                return;
            if (!enabled)
                return;
            DrawElements();
        }
        private void OnDisable() => _depthCounts = Array.Empty<DepthCount>();

        private void ReadyTree()
        {
            if (_trees.Count == 0)
                return;

            int maxDepth = 0;
            foreach (int treeId in _treeIds)
                if (_trees.TryGetValue(treeId, out var tree))
                    maxDepth = Math.Max(maxDepth, tree.MaxDepth);

            _depthCounts = new DepthCount[maxDepth + 1]; // depth从0开始，所以+1
        }
        private void ReadyData()
        {
            _depthCounts.Clean();
            _elements.Clear();

            if (_treeIds.Length == 0)
                return;

            foreach (int treeId in _treeIds)
            {
                if (!_trees.TryGetValue(treeId, out var tree))
                    continue;

                for (int lastIndex = _depthCounts.Length - 1, depth = lastIndex; depth >= 0; --depth)
                {
                    var nodes = QuadGetter.GetNodes(tree, depth, _nodes);
                    ref var depthCount = ref _depthCounts[depth];
                    depthCount.Depth = depth;

                    foreach (var node in nodes)
                    {
                        int boundarySqr = node.Boundary.HalfSize().SqrMagnitude();
                        depthCount.Count += node.Elements.Count;

                        foreach (var element in node.Elements)
                        {
                            if (element.Shape.W << 1 >= node.Boundary.W || element.Shape.H << 1 >= node.Boundary.H)
                                continue;

                            int elementSqr = element.Shape.Size().SqrMagnitude();
                            int pow = Mathf.CeilToInt(MathF.Log(elementSqr, 2));

                            ++depthCount.Cross;
                            depthCount.Used += 1 << pow;
                            depthCount.Space += boundarySqr;
                            if (_lineBall.Show)
                                _elements.Add(element);
                        }
                    }

                    for (int childDepth = lastIndex; childDepth >= depth; --childDepth)
                        depthCount.SumCross += _depthCounts[childDepth].Cross;
                    depthCount.CountData();
                }
            }
        }

        private void DrawElements()
        {
            if (_drawIndex)
            {
                foreach (var element in _elements)
                {
                    ShapeDraw.Label(element.Shape.Center(), _scale, _height, element.Index.ToString(), in _lineBall.Color);
                    ShapeDraw.AABB(in element.Shape, _scale, _height, in _lineBall.Color);
                }
            }
            else
            {
                foreach (var element in _elements)
                {
                    ShapeDraw.AABB(in element.Shape, _scale, _height, in _lineBall.Color);
                }
            }
        }
    }
}
#endif