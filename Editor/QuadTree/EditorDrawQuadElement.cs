#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    /// <summary>
    /// 绘制四叉树元素
    /// </summary>
    internal sealed class EditorDrawQuadElement : MonoBehaviour
    {
        #region 类型
        private readonly struct DrawTree
        {
            internal readonly int TreeId;
            internal readonly QuadShape Shape;
            internal readonly Color Color;
            internal DrawTree(BasicQuadTree tree)
            {
                int treeId = tree.TreeId;
                var color = QuadGetter.Proxy.GetElementColor(treeId);

                TreeId = treeId;
                Shape = tree.Shape;
                Color = color;
            }
        }

        [Serializable]
        private struct DrawElement : IComparable<DrawElement>
        {
            [SerializeField] internal int Index;
            internal readonly AABB2DInt Content;
            [SerializeField] internal int TreeId;
            [SerializeField] internal QuadShape Shape;
            internal readonly Color Color;
            internal DrawElement(in QuadElement element, in DrawTree drawTree)
            {
                Index = element.Index;
                Content = element.Shape;
                TreeId = drawTree.TreeId;
                Shape = drawTree.Shape;
                Color = drawTree.Color;
            }
            public readonly int CompareTo(DrawElement other)
            {
                int match0 = Index.CompareTo(other.Index);
                if (match0 != 0)
                    return match0;

                int match1 = Content.Center().CompareTo(other.Content.Center());
                if (match1 != 0)
                    return match1;

                int match2 = Content.HalfSize().CompareTo(other.Content.HalfSize());
                if (match2 != 0)
                    return match2;

                int match3 = TreeId.CompareTo(other.TreeId);
                if (match3 != 0)
                    return match3;

                return 0;
            }

            [CustomPropertyDrawer(typeof(DrawElement))]
            private sealed class DrawElementDrawer : PropertyDrawer
            {
                private const int HeightScale = 3;
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    var indexProperty = property.FindPropertyRelative(nameof(Index));
                    var treeIdProperty = property.FindPropertyRelative(nameof(TreeId));
                    var shapeProperty = property.FindPropertyRelative(nameof(Shape));

                    var size = new Vector2(position.size.x, position.size.y / HeightScale);
                    var indexPosition = new Rect(position.position, size);
                    var treeIdPosition = new Rect(position.x, position.y + size.y, size.x, size.y);
                    var shapePosition = new Rect(position.x, position.y + size.y * 2, size.x, size.y);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(indexPosition, indexProperty);
                    EditorGUI.PropertyField(treeIdPosition, treeIdProperty);
                    EditorGUI.PropertyField(shapePosition, shapeProperty);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    indexProperty.Dispose();
                    treeIdProperty.Dispose();
                    shapeProperty.Dispose();
                }
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label) * HeightScale;
            }
        }

        private enum DrawRange
        {
            None,
            Single,
            Children,
            All,
            Custom,
        }
        #endregion

        #region 序列化字段
        [Header("四叉树设置")] [SerializeField] private int _circleAccuracy = 12;
        [SerializeField] private DrawRange _drawRange = DrawRange.All;
        [SerializeField] private int[] _treeIds;

        [Header("渲染数据")] [SerializeField] private float _height;
        [SerializeField] private bool _draw = true;
        [SerializeField] private List<DrawElement> _elements = new();
        #endregion

        #region 运行时缓存
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly HashSet<int> _customIndexes = new();
        private readonly HashSet<int> _drawIndexes = new();
        private readonly List<QuadNode> _nodes = new(); // 临时缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadGetter.Proxy.Manager;
            if (manager is null)
                return;

            _scale = 1F / manager.Scale;
            QuadGetter.GetTrees(manager, _trees);
        }
        private void FixedUpdate()
        {
            ReadyEntity();
            ReadyTrees();
        }
        private void OnDrawGizmos()
        {
            DrawTrees();
        }
        private void OnDisable() => _elements.Clear();

        private void ReadyEntity()
        {
            _drawIndexes.Clear();

            switch (_drawRange)
            {
                case DrawRange.Single: _drawIndexes.Add(QuadGetter.Proxy.GetIndex(gameObject)); break;
                case DrawRange.Children: QuadGetter.Proxy.GetIndexes(gameObject, _drawIndexes); break;
                case DrawRange.All:
                    foreach (var pair in _trees)
                    foreach (var node in QuadGetter.GetNodes(pair.Value, _nodes))
                    foreach (var element in node.Elements)
                        _drawIndexes.Add(element.Index);
                    break;

                case DrawRange.Custom: _drawIndexes.UnionWith(_customIndexes); break;
            }
        }
        private void ReadyTrees()
        {
            _elements.Clear();
            if (_trees is null)
                return;
            if (_drawIndexes.Count == 0)
                return;
            foreach (int treeId in _treeIds)
                if (_trees.TryGetValue(treeId, out var tree))
                    ReadyElements(tree, new DrawTree(tree));
        }
        private void DrawTrees()
        {
            if (_draw)
                foreach (var element in _elements)
                    DrawElements(element);
        }

        private void ReadyElements(BasicQuadTree tree, in DrawTree drawTree)
        {
            foreach (var node in QuadGetter.GetNodes(tree, _nodes))
            foreach (var element in node.Elements)
                if (_drawIndexes.Contains(element.Index))
                    _elements.Add(new DrawElement(in element, in drawTree));
        }
        private void DrawElements(in DrawElement element)
        {
            ShapeDraw.Label(element.Content.Center(), _scale, _height, element.Index.ToString(), in element.Color);
            switch (element.Shape)
            {
                case QuadShape.Circle: ShapeDraw.Circle(Converts.AsCircleInt(in element.Content), _circleAccuracy, _scale, _height, in element.Color); break;
                case QuadShape.AABB: ShapeDraw.AABB(in element.Content, _scale, _height, in element.Color); break;
            }
        }

        internal void SetDrawRangeCustom() => _drawRange = DrawRange.Custom;
        internal void SetTreeId(int value)
        {
            if (_treeIds.Length != 1)
                _treeIds = new int[1];
            _treeIds[0] = value;
        }
        internal void SetCustomIndexes(IEnumerable<int> value)
        {
            _customIndexes.Clear();
            _customIndexes.UnionWith(value);
        }
    }
}
#endif