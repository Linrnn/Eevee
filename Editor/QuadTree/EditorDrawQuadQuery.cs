﻿#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    /// <summary>
    /// 绘制搜索区域内的四叉树元素
    /// </summary>
    internal sealed class EditorDrawQuadQuery : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawQuadQuery))]
        private sealed class EditorDrawQuadQueryInspector : Editor
        {
            private static readonly string[] _filedNames =
            {
                "m_Script",
                nameof(_quadShape),
                nameof(_position),
                nameof(_radius),
                nameof(_circleAccuracy),
                nameof(_extents),
                nameof(_angle),
                nameof(_treeId),
                nameof(_height),
                nameof(_color),
                nameof(_queries),
            };
            private readonly SerializedProperty[] _serializedProperties = new SerializedProperty[_filedNames.Length];

            private void OnEnable()
            {
                for (int i = 0; i < _filedNames.Length; ++i)
                    _serializedProperties[i] = serializedObject.FindProperty(_filedNames[i]);
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawClass();
                serializedObject.ApplyModifiedProperties();
            }
            private void OnDisable()
            {
                foreach (var serializedProperty in _serializedProperties)
                    serializedProperty.Dispose();
                _serializedProperties.Clean();
            }

            private void DrawClass()
            {
                DrawField(0, false);
                DrawField(1);
                DrawField(2);

                switch (_serializedProperties[1].enumValueFlag)
                {
                    case (int)QuadShape.Circle:
                        DrawField(3);
                        DrawField(4);
                        break;

                    case (int)QuadShape.AABB: DrawField(5); break;

                    case (int)QuadShape.OBB:
                        DrawField(5);
                        DrawField(6);
                        break;

                    default: return;
                }

                DrawField(7);
                DrawField(8);
                DrawField(9);
                DrawField(10, false);
            }
            private void DrawField(int index, bool allowSet = true)
            {
                if (allowSet)
                {
                    EditorGUILayout.PropertyField(_serializedProperties[index]);
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(_serializedProperties[index]);
                    EditorGUI.EndDisabledGroup();
                }
            }
        }
        #endregion

        #region 序列化字段
        [Header("四叉树设置")] [SerializeField] private QuadShape _quadShape = QuadShape.Circle;
        [SerializeField] private Vector2Int _position;
        [SerializeField] private int _radius = 1;
        [SerializeField] private int _circleAccuracy = 12;
        [SerializeField] private Vector2Int _extents = Vector2Int.one;
        [SerializeField] [Range(0, 360)] private float _angle;

        [Header("渲染数据")] [SerializeField] private int _treeId;
        [SerializeField] private float _height;
        [SerializeField] private Color _color = Color.blue;
        [SerializeField] private List<int> _queries = new();
        #endregion

        #region 运行时缓存
        private float _drawDuration;
        private float _scale;
        private readonly List<QuadElement> _elements = new(); // 缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadGetter.Proxy.Manager;
            if (manager is null)
                return;

            _drawDuration = Time.fixedDeltaTime;
            _scale = 1F / manager.Scale;
        }
        private void FixedUpdate()
        {
            _queries.Clear(); // 下一帧清除数据
            _elements.Clear();
            Query();
            BuildData();
            SetValue();
            Draw();
        }

        private void Query()
        {
            var manager = QuadGetter.Proxy.Manager;
            switch (_quadShape)
            {
                case QuadShape.Circle: manager.QueryCircle(_treeId, _position, _radius, false, _elements); break;
                case QuadShape.AABB: manager.QueryAABB(_treeId, _position, _extents, false, _elements); break;
                case QuadShape.OBB: manager.QueryOBB(_treeId, _position, _extents, _angle, false, _elements); break;
                default: LogRelay.Error($"[Editor][Quad] TreeId:{_treeId}, QuadShape:{_quadShape}, not impl!"); break;
            }
        }
        private void BuildData()
        {
            foreach (var element in _elements)
                _queries.Add(element.Index);
        }
        private void SetValue()
        {
            if (TryGetComponent<EditorDrawQuadElement>(out var component))
                component = gameObject.AddComponent<EditorDrawQuadElement>();
            component.SetDrawRangeCustom();
            component.SetTreeId(_treeId);
            component.SetCustomIndexes(_queries);
        }
        private void Draw()
        {
            switch (_quadShape)
            {
                case QuadShape.Circle: ShapeDraw.Circle(new CircleInt(_position, _radius), _circleAccuracy, _scale, _height, in _color, _drawDuration); break;
                case QuadShape.AABB: ShapeDraw.AABB(new AABB2DInt(_position, _extents), _scale, _height, in _color, _drawDuration); break;
                case QuadShape.OBB: ShapeDraw.OBB(new OBB2DInt(_position, _extents, _angle), _scale, _height, in _color, _drawDuration); break;
                default: LogRelay.Error($"[Editor][Quad] TreeId:{_treeId}, QuadShape:{_quadShape}, not impl!"); break;
            }
        }
    }
}
#endif