#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
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
        [CustomEditor(typeof(EditorDrawQuadElement))]
        private sealed class EditorDrawQuadElementInspector : Editor
        {
            #region Property Path
            private const string Range = nameof(_range);
            private const string TreeIds = nameof(_treeIds);
            private const string Indexes = nameof(_indexes);
            private const string Color = nameof(_color);
            private const string Height = nameof(_height);
            private const string Draw = nameof(_draw);
            private const string DrawIndex = nameof(_drawIndex);
            #endregion

            private PropertyHandle _propertyHandle;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawProperties();
                serializedObject.ApplyModifiedProperties();
            }
            private void OnEnable() => _propertyHandle.Initialize(this);
            private void OnDisable() => _propertyHandle.Dispose();

            private void DrawProperties()
            {
                var rangeProperty = _propertyHandle.Get(Range);

                _propertyHandle.DrawScript();
                _propertyHandle.Draw(Range);

                switch (rangeProperty.enumValueIndex)
                {
                    case (int)DrawRange.Single:
                    case (int)DrawRange.Children:
                    case (int)DrawRange.All: _propertyHandle.DrawEnumQuadFunc(TreeIds); break;
                    case (int)DrawRange.Custom:
                        _propertyHandle.DrawEnumQuadFunc(TreeIds);
                        _propertyHandle.Draw(Indexes);
                        break;
                }

                _propertyHandle.Draw(Color);
                _propertyHandle.Draw(Height);
                _propertyHandle.Draw(Draw);
                _propertyHandle.Draw(DrawIndex);
            }
        }

        [Serializable]
        private struct DrawElement : IComparable<DrawElement>
        {
            [SerializeField] internal int Index;
            internal readonly AABB2DInt Content;
            [SerializeField] internal int TreeId;
            [SerializeField] internal QuadShape Shape;

            internal DrawElement(in QuadElement element, in BasicQuadTree tree)
            {
                Index = element.Index;
                Content = element.Shape;
                TreeId = tree.TreeId;
                Shape = tree.Shape;
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
                    var size = new Vector2(position.size.x, position.size.y / HeightScale);
                    var indexPosition = new Rect(position.position, size);
                    var treeIdPosition = new Rect(position.x, position.y + size.y, size.x, size.y);
                    var shapePosition = new Rect(position.x, position.y + size.y * 2, size.x, size.y);

                    var indexProperty = property.FindPropertyRelative(nameof(Index));
                    var treeIdProperty = property.FindPropertyRelative(nameof(TreeId));
                    var shapeProperty = property.FindPropertyRelative(nameof(Shape));

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
        [Header("渲染数据")] [SerializeField] private DrawRange _range = DrawRange.All;
        [SerializeField] private int[] _treeIds;
        [SerializeField] private int[] _indexes;

        [Header("渲染参数")] [SerializeField] private Color _color = Color.green;
        [SerializeField] private float _height;
        [SerializeField] private bool _draw = true;
        [SerializeField] private bool _drawIndex = true;
        #endregion

        #region 运行时缓存
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly HashSet<int> _drawIndexes = new();
        private readonly List<DrawElement> _elements = new();
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
        private void Update()
        {
            ReadyIndexes();
            ReadyTrees();
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;
            DrawTrees();
        }

        private void ReadyIndexes()
        {
            _drawIndexes.Clear();
            switch (_range)
            {
                case DrawRange.Single: _drawIndexes.Add(QuadGetter.Proxy.GetIndex(gameObject)); break;
                case DrawRange.Children: QuadGetter.Proxy.GetIndexes(gameObject, _drawIndexes); break;
                case DrawRange.All:
                    foreach (var (_, tree) in _trees)
                    foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                    foreach (var element in node.Elements)
                        _drawIndexes.Add(element.Index);
                    break;
                case DrawRange.Custom: _drawIndexes.UnionWith(_indexes); break;
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
                    ReadyElements(tree);
        }
        private void ReadyElements(BasicQuadTree tree)
        {
            foreach (var node in QuadGetter.GetNodes(tree, _nodes))
            foreach (var element in node.Elements)
                if (_drawIndexes.Contains(element.Index))
                    _elements.Add(new DrawElement(in element, tree));
        }

        private void DrawTrees()
        {
            if (_draw)
                foreach (var element in _elements)
                    QuadDraw.Element(element.Shape, element.TreeId, new QuadElement(element.Index, in element.Content), _scale, _height, _drawIndex, in _color);
        }
    }
}
#endif