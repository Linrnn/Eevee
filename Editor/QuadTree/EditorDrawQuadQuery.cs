#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.QuadTree;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorDrawQuadQuery : MonoBehaviour
    {
        [Space] [SerializeField] private EditorQuadDrawAsset _boxAsset = new("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png");
        [SerializeField] private EditorQuadDrawAsset _circleAsset = new("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Circle.png");

        [Space] [SerializeField] private QueryEnum _queryEnum = QueryEnum.Circle;
        [SerializeField] private Vector2Int _war3Position;
        [SerializeField] private int _war3Radius = 1; // 圆半径
        [SerializeField] private Vector2Int _war3Size = Vector2Int.one; // x：半宽，y：半长
        [SerializeField] [Range(0, 360)] private float _direction;

        [Space] [SerializeField] [SingleEnum] private int _funcEnum;
        [SerializeField] [SingleEnum] private EditorQuadSubEnum _subEnum = EditorQuadSubEnum.Tree1;

        [Space] [SerializeField] private float _height = 0.01F;
        [SerializeField] private Color _color = Color.blue;
        [SerializeField] private List<int> _queries = new();

        private readonly List<int> _subTrees = new(1); // 缓存
        private readonly List<QuadElement> _elements = new(); // 缓存
        private GameObject _drawGo;

        private void Awake()
        {
            if (_drawGo)
                return;

            _drawGo = new GameObject($"{gameObject.name} Query");
            _drawGo.transform.SetParent(transform);

            if (!_drawGo.TryGetComponent<SpriteRenderer>(out _))
                _drawGo.AddComponent<SpriteRenderer>();
        }
        private void OnEnable()
        {
            _boxAsset.Load();
            _circleAsset.Load();
        }
        private void Update()
        {
            _queries.Clear(); // 下一帧清除数据
            _elements.Clear();
            _subTrees.Clear();
            _subTrees.Add(Log2((int)_subEnum));

            Query();
            BuildData();
            SetValue();
            Draw(_drawGo);
        }
        private void OnDestroy()
        {
            if (_drawGo)
            {
                Destroy(_drawGo);
                _drawGo = null;
            }
        }

        private int Log2(double num) => num > 0 ? (int)Math.Log(num, 2) : -1;
        private void Query()
        {
            // todo eevee
            //switch (_queryEnum)
            //{
            //    case QueryEnum.Circle:
            //        BATEntry.Data.quadTree.QueryCircle(_war3Position, _war3Radius, _funcEnum, _subTrees, _elements);
            //        break;

            //    case QueryEnum.Box:
            //        BATEntry.Data.quadTree.QueryBox(_war3Position, _war3Size, _funcEnum, _subTrees, _elements);
            //        break;

            //    case QueryEnum.DirBox:
            //        var center = new Vector2D(_war3Position.x, _war3Position.y);
            //        var extent = new Vector2D(_war3Size.x, _war3Size.y);
            //        float direction = _direction * Mathf.Deg2Rad;
            //        var dir = new Vector2D(Math.Cos(direction), -Math.Sin(direction));
            //        BATEntry.Data.quadTree.QueryRectangle(in center, in extent, in dir, _funcEnum, _subTrees, _elements);
            //        break;
            //}
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
            component.SetFuncEnum(_funcEnum);
            component.SetSubEnum(_subEnum);
            component.SetCustomEntityIds(_queries);
        }
        private void Draw(GameObject go)
        {
            bool isDirBox = _queryEnum == QueryEnum.DirBox;
            bool isCircle = _queryEnum == QueryEnum.Circle;

            float direction = isDirBox ? _direction : 0;
            var size = isCircle ? new Vector2Int(_war3Radius, _war3Radius) : _war3Size;
            var asset = isCircle ? _circleAsset : _boxAsset;

            go.transform.position = new Vector3(_war3Position.x / 128F, _height, _war3Position.y / 128F);
            go.transform.eulerAngles = new Vector3(90, direction, 0);
            go.transform.localScale = new Vector3(size.x / 64F, size.y / 64F, 1);

            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = asset.Asset;
            spriteRenderer.color = _color;
        }

        #region Type
        private enum QueryEnum : ushort
        {
            None = 0,
            Circle = 1, // 圆形
            Box = 2, // 无向矩阵
            DirBox = 3, // 有向矩阵
        }

        [CustomEditor(typeof(EditorDrawQuadQuery))]
        private sealed class EditorDrawQuadQueryInspector : UnityEditor.Editor
        {
            private static readonly string[] FiledNames = { "m_Script", nameof(_boxAsset), nameof(_circleAsset), nameof(_queryEnum), nameof(_war3Position), nameof(_war3Radius), nameof(_war3Size), nameof(_direction), nameof(_funcEnum), nameof(_subEnum), nameof(_height), nameof(_color), nameof(_queries), };

            private readonly SerializedProperty[] _serializedProperties = new SerializedProperty[FiledNames.Length];

            private void OnEnable()
            {
                for (int i = 0; i < FiledNames.Length; ++i)
                {
                    string filedName = FiledNames[i];
                    var serializedProperty = serializedObject.FindProperty(filedName);
                    _serializedProperties[i] = serializedProperty;
                }
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
                _serializedProperties.CleanAll();
            }

            private void DrawClass()
            {
                DrawField(0, false);
                DrawField(1);
                DrawField(2);
                DrawField(3);

                switch (_serializedProperties[3].enumValueFlag)
                {
                    case (int)QueryEnum.Circle:
                        DrawField(4);
                        DrawField(5);
                        break;

                    case (int)QueryEnum.Box:
                        DrawField(4);
                        DrawField(6);
                        break;

                    case (int)QueryEnum.DirBox:
                        DrawField(4);
                        DrawField(6);
                        DrawField(7);
                        break;

                    default: return;
                }

                DrawField(8);
                DrawField(9);
                DrawField(10);
                DrawField(11);
                DrawField(12, false);
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
    }
}
#endif