#if UNITY_EDITOR
using Eevee.Collection;
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
    /// 绘制搜索区域内的四叉树元素
    /// </summary>
    [AddComponentMenu("Eevee Editor/Quad Tree/Editor Draw Quad Tree Query")]
    internal sealed class EditorDrawQuadTreeQuery : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawQuadTreeQuery))]
        private sealed class EditorDrawQuadTreeQueryInspector : Editor
        {
            #region Property Path
            private const string Shape = nameof(_shape);
            private const string Center = nameof(_center);
            private const string Radius = nameof(_radius);
            private const string Extents = nameof(_extents);
            private const string Angle = nameof(_angle);
            private const string Polygon = nameof(_polygon);
            private const string TreeId = nameof(_treeId);
            private const string Height = nameof(_height);
            private const string DrawIndex = nameof(_drawIndex);
            private const string QueryColor = nameof(_queryColor);
            private const string ElementColor = nameof(_elementColor);
            private const string CacheIndex = nameof(_cacheIndex);
            #endregion

            private PropertyHandle _propertyHandle;
            private float _scale;
            private Vector2Int[] _polygon = Array.Empty<Vector2Int>();

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawProperties();
                serializedObject.ApplyModifiedProperties();
            }
            private void OnEnable() => _propertyHandle.Initialize(this);
            private void OnSceneGUI()
            {
                var behaviour = (Behaviour)target;
                if (!behaviour.enabled)
                    return;
                if (_scale == 0)
                    _scale = ((EditorDrawQuadTreeQuery)behaviour)._scale;
                DrawQueryShape();
                serializedObject.ApplyModifiedProperties();
            }
            private void OnDisable() => _propertyHandle.Dispose();

            private void DrawProperties()
            {
                _propertyHandle.DrawScript();
                _propertyHandle.Draw(Shape);

                switch (_propertyHandle.Get(Shape).enumValueFlag)
                {
                    case (int)QuadTreeShape.Point: _propertyHandle.Draw(Center); break;
                    case (int)QuadTreeShape.Circle: _propertyHandle.Draw(Center).Draw(Radius); break;
                    case (int)QuadTreeShape.AABB: _propertyHandle.Draw(Center).Draw(Extents); break;
                    case (int)QuadTreeShape.OBB: _propertyHandle.Draw(Center).Draw(Extents).Draw(Angle); break;
                    case (int)QuadTreeShape.Polygon: _propertyHandle.Draw(Polygon); break;
                    default: return;
                }

                _propertyHandle.EnumTreeFunc(TreeId);
                _propertyHandle.Draw(Height);
                _propertyHandle.Draw(DrawIndex);
                _propertyHandle.Draw(QueryColor);
                _propertyHandle.Draw(ElementColor);

                EditorGUILayout.BeginHorizontal();
                _propertyHandle.Draw(CacheIndex);
                DrawUseButton("使用缓存");
                EditorGUILayout.EndHorizontal();
            }
            private void DrawQueryShape()
            {
                var shapeProperty = _propertyHandle.Get(Shape);
                var centerProperty = _propertyHandle.Get(Center);
                var radiusProperty = _propertyHandle.Get(Radius);
                var extentsProperty = _propertyHandle.Get(Extents);
                var angleProperty = _propertyHandle.Get(Angle);
                var polygonProperty = _propertyHandle.Get(Polygon);
                var heightProperty = _propertyHandle.Get(Height);

                var shape = (QuadTreeShape)shapeProperty.enumValueFlag;
                var center = centerProperty.vector2IntValue;
                int radius = radiusProperty.intValue;
                var extents = extentsProperty.vector2IntValue;
                float angle = angleProperty.floatValue;
                var polygon = _polygon.Length == polygonProperty.arraySize ? _polygon : new Vector2Int[polygonProperty.arraySize];
                float height = heightProperty.floatValue;

                switch (shape)
                {
                    case QuadTreeShape.Point:
                        ShapeDraw.Point(ref center, _scale, height);
                        centerProperty.vector2IntValue = center;
                        break;

                    case QuadTreeShape.Circle:
                        ShapeDraw.Circle(ref center, ref radius, _scale, height);
                        centerProperty.vector2IntValue = center;
                        radiusProperty.intValue = radius;
                        break;

                    case QuadTreeShape.AABB:
                        ShapeDraw.AABB(ref center, ref extents, _scale, height);
                        centerProperty.vector2IntValue = center;
                        extentsProperty.vector2IntValue = extents;
                        break;

                    case QuadTreeShape.OBB:
                        ShapeDraw.OBB(ref center, ref extents, ref angle, _scale, height);
                        centerProperty.vector2IntValue = center;
                        extentsProperty.vector2IntValue = extents;
                        angleProperty.floatValue = angle;
                        break;

                    case QuadTreeShape.Polygon:
                        for (int i = 0; i < polygon.Length; ++i)
                            polygon[i] = polygonProperty.GetArrayElementAtIndex(i).vector2IntValue;
                        ShapeDraw.Polygon(ref polygon, _scale, height);
                        EditorUtils.SetArrayLength(polygonProperty, polygon.Length);
                        for (int i = 0; i < polygon.Length; ++i)
                            polygonProperty.GetArrayElementAtIndex(i).vector2IntValue = polygon[i];
                        _polygon = polygon;
                        break;

                    default: Debug.LogError($"[Editor][Quad] Shape:{shape}, not impl!"); break;
                }
            }
            private void DrawUseButton(string text)
            {
                if (!GUILayout.Button(text))
                    return;

                var quadShape = (QuadTreeShape)_propertyHandle.Get(Shape).enumValueFlag;
                int cacheIndex = _propertyHandle.Get(CacheIndex).intValue;
                object shape = ReadShape(quadShape, cacheIndex);
                switch (shape)
                {
                    case Vector2DInt point: _propertyHandle.Get(Center).vector2IntValue = point; break;

                    case CircleInt circle:
                        _propertyHandle.Get(Center).vector2IntValue = circle.Center();
                        _propertyHandle.Get(Radius).intValue = circle.R;
                        break;

                    case AABB2DInt aabb:
                        _propertyHandle.Get(Center).vector2IntValue = aabb.Center();
                        _propertyHandle.Get(Extents).vector2IntValue = aabb.HalfSize();
                        break;

                    case OBB2DInt obb:
                        _propertyHandle.Get(Center).vector2IntValue = obb.Center();
                        _propertyHandle.Get(Extents).vector2IntValue = obb.HalfSize();
                        _propertyHandle.Get(Angle).floatValue = (float)obb.A.Value;
                        break;

                    case PolygonInt polygon:
                        var polygonProperty = _propertyHandle.Get(Polygon);
                        int polygonCount = polygon.PointCount();
                        polygonProperty.arraySize = polygonCount;
                        for (int i = 0; i < polygonCount; ++i)
                            if (polygonProperty.GetArrayElementAtIndex(i) is { } elementProperty)
                                elementProperty.vector2IntValue = polygon[i];
                        break;

                    default: Debug.LogError($"[Editor][Quad] Use cache fail. Shape:{quadShape}, Type:{shape?.GetType().FullName ?? "null"}, Context:{shape?.ToString() ?? "null"}, CacheIndex:{cacheIndex}."); break;
                }
            }
        }
        #endregion

        #region 序列化字段
        [Header("四叉树设置（非Unity尺度）")] [SerializeField] private QuadTreeShape _shape = QuadTreeShape.Circle;
        [SerializeField] private Vector2Int _center;
        [SerializeField] private int _radius = 1;
        [SerializeField] private Vector2Int _extents = Vector2Int.one;
        [SerializeField] [Range(0, 359.999F)] private float _angle;
        [SerializeField] private Vector2Int[] _polygon = Array.Empty<Vector2Int>();

        [Header("渲染数据")] [SerializeField] private int _treeId;
        [SerializeField] private float _height;
        [SerializeField] private bool _drawIndex = true;
        [SerializeField] private Color _queryColor = Color.black;
        [SerializeField] private Color _elementColor = Color.blue;
        [SerializeField] private int _cacheIndex;
        #endregion

        #region 运行时缓存
        private const int PolygonSide = 3; // 多边形的最小边
        private static readonly Dictionary<QuadTreeShape, List<object>> _shapes = new(); // 缓存形状

        private QuadTreeManager _manager;
        private float _scale;
        private Vector2DInt[] _polygonRuntime = new Vector2DInt[PolygonSide];
        private readonly List<QuadTreeElement> _elements = new(); // 缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadTreeGetter.Proxy.Manager;
            _manager = manager;
            _scale = 1F / manager.Scale;
        }
        private void Update()
        {
            ReadyElements();
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;
            DrawQueryShape();
            DrawElements();
        }

        private void ReadyElements()
        {
            if (_polygonRuntime.Length < _polygon.Length)
                _polygonRuntime = new Vector2DInt[_polygon.Length << 1];
            for (int i = 0; i < _polygon.Length; ++i)
                _polygonRuntime[i] = _polygon[i];
            _elements.Clear();
            switch (_shape)
            {
                case QuadTreeShape.Point: _manager.QueryPoint(_treeId, _center, _elements); break;
                case QuadTreeShape.Circle: _manager.QueryCircle(_treeId, _center, _radius, true, _elements); break;
                case QuadTreeShape.AABB: _manager.QueryAABB(_treeId, _center, _extents, true, _elements); break;
                case QuadTreeShape.OBB: _manager.QueryOBB(_treeId, _center, _extents, _angle, true, _elements); break;
                case QuadTreeShape.Polygon: _manager.QueryPolygon(_treeId, new ReadOnlyArray<Vector2DInt>(_polygonRuntime, Math.Max(_polygon.Length, PolygonSide)), true, _elements); break;
                default: Debug.LogError($"[Editor][Quad] TreeId:{_treeId}, Shape:{_shape}, not impl!"); break;
            }
        }

        private void DrawQueryShape()
        {
            switch (_shape)
            {
                case QuadTreeShape.Point: ShapeDraw.Point(_center, _scale, _height, in _queryColor); break;
                case QuadTreeShape.Circle: ShapeDraw.Circle(new CircleInt(_center, _radius), _scale, _height, in _queryColor); break;
                case QuadTreeShape.AABB: ShapeDraw.AABB(new AABB2DInt(_center, _extents), _scale, _height, in _queryColor); break;
                case QuadTreeShape.OBB: ShapeDraw.OBB(new OBB2DInt(_center, _extents, _angle), _scale, _height, in _queryColor); break;
                case QuadTreeShape.Polygon: ShapeDraw.Polygon(_polygon, _scale, _height, in _queryColor); break;
                default: Debug.LogError($"[Editor][Quad] Shape:{_shape}, not impl!"); break;
            }
        }
        private void DrawElements()
        {
            var config = _manager.GetConfig(_treeId);
            foreach (var element in _elements)
                QuadTreeDraw.Element(config.Shape, _treeId, in element, _scale, _height, _drawIndex, in _elementColor);
        }

        #region 缓存
        internal static object ReadShape(QuadTreeShape shape, int index)
        {
            if (index < 0)
                return null;
            if (!_shapes.TryGetValue(shape, out var shapes))
                return null;
            if (index >= shapes.Count)
                return null;
            return shapes[index];
        }

        internal static void WriteShape(Vector2DInt shape) => WriteShape(QuadTreeShape.Point, shape);
        internal static void WriteShape(in CircleInt shape) => WriteShape(QuadTreeShape.Circle, shape);
        internal static void WriteShape(in AABB2DInt shape) => WriteShape(QuadTreeShape.AABB, shape);
        internal static void WriteShape(in OBB2DInt shape) => WriteShape(QuadTreeShape.OBB, shape);
        internal static void WriteShape(in PolygonInt shape) => WriteShape(QuadTreeShape.Polygon, new PolygonInt(new ReadOnlyArray<Vector2DInt>(shape.GetPoints().ToArray())));

        private static void WriteShape(QuadTreeShape shape, object content)
        {
            var contexts = _shapes.GetValueOrDefault(shape);
            var newContexts = contexts ?? new List<object>();
            newContexts.Add(content);
            _shapes[shape] = newContexts;
        }
        #endregion
    }
}
#endif