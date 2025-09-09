#if UNITY_EDITOR
using Eevee.PathFind;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using CollSize = System.SByte;
using MoveFunc = System.Byte;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Area")]
    internal sealed class EditorDrawPathFindArea : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindArea))]
        private sealed class EditorDrawPathFindAreaInspector : Editor
        {
            #region Property Path
            private const string MoveType = nameof(_moveType);
            private const string CollType = nameof(_collType);
            private const string Check = nameof(_check);
            private const string Counts = nameof(_counts);
            private const string IdAllocator = nameof(_idAllocator);
            private const string Draw = nameof(_draw);
            private const string DrawPoint = nameof(_drawPoint);
            private const string Color = nameof(_color);
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
                _propertyHandle.DrawScript();
                _propertyHandle.EnumMoveType(MoveType);
                _propertyHandle.EnumCollType(CollType);
                _propertyHandle.Draw(Check);
                _propertyHandle.Draw(Counts, true);
                _propertyHandle.Draw(IdAllocator, true);
                _propertyHandle.Draw(Draw);
                _propertyHandle.Draw(DrawPoint);
                _propertyHandle.Draw(Color);
            }
        }

        [Serializable]
        private struct AreaCount
        {
            private static Comparison<AreaCount> _comparison = (lhs, rhs) => lhs.Id.CompareTo(rhs.Id);
            [SerializeField] internal int Id;
            [SerializeField] internal int Count;

            internal AreaCount(int id, uint count)
            {
                Id = id;
                Count = (int)count;
            }
            internal static void Set(List<AreaCount> output, Dictionary<short, uint> input)
            {
                output.Clear();
                foreach ((short areaId, uint count) in input)
                    output.Add(new AreaCount(areaId, count));
                output.Sort(_comparison);
            }

            [CustomPropertyDrawer(typeof(AreaCount))]
            private sealed class AreaCountDrawer : PropertyDrawer
            {
                private static readonly GUIContent _idLabel = new(nameof(Id));
                private static readonly GUIContent _countLabel = new(nameof(Count));

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    const int scale = 2;
                    const float spaceWidth = EditorUtils.SpaceWidth / scale;
                    const float propertyHeight = EditorUtils.PropertyHeight;

                    var min = position.position;
                    float labelWidth = EditorGUIUtility.labelWidth;
                    float width = (position.size.x - spaceWidth) / scale;
                    var idPosition = new Rect(min.x, min.y, width - spaceWidth, propertyHeight);
                    var countPosition = new Rect(min.x + width + spaceWidth, min.y, width - spaceWidth, propertyHeight);

                    var idProperty = property.FindPropertyRelative(nameof(Id));
                    var countProperty = property.FindPropertyRelative(nameof(Count));

                    EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(_idLabel) + spaceWidth;
                    idProperty.intValue = EditorGUI.IntField(idPosition, idProperty.displayName, idProperty.intValue);
                    EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(_countLabel) + spaceWidth;
                    countProperty.longValue = EditorGUI.LongField(countPosition, countProperty.displayName, countProperty.longValue);
                    EditorGUIUtility.labelWidth = labelWidth;
                }
            }
        }
        #endregion

        [Header("输入参数")] [SerializeField] private MoveFunc _moveType;
        [SerializeField] private CollSize _collType;
        [SerializeField] private bool _check = true; // 检测“AreaCount”
        [Header("只读参数")] [SerializeField] private List<AreaCount> _counts;
        [SerializeField] private int _idAllocator;
        [Header("绘制参数")] [SerializeField] private bool _draw = true;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private Color _color = Color.red;

        private PathFindComponent _component;
        private readonly PathFindBoundaryProcessor<int, short> _areaProcessor = new(areaId => areaId != PathFindExt.CantStand, areaId => areaId);
        private readonly Dictionary<short, uint> _checkCount = new();
        private readonly StringBuilder _checkStringBuilder = new();

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _component = proxy.Component;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            ReadyArea();
            DrawArea();
            Check();
        }

        private void ReadyArea()
        {
            short[,] areaIds = _component.GetAreaIdNodes(_moveType, _collType);
            var size = _component.GetSize();
            _areaProcessor.Build(areaIds, size);
            AreaCount.Set(_counts, _component.GetAreaCount(_moveType, _collType));
            _idAllocator = _component.GetAreaIdAllocator(_moveType, _collType);
        }
        private void DrawArea()
        {
            if (!_draw)
                return;

            foreach ((int areaId, var boundary) in _areaProcessor)
            {
                foreach (var side in boundary.Sides())
                    PathFindDraw.Side((Vector2Int)side, PathFindExt.StraightDirections[side.z], in _color);
                foreach (var point in boundary.Girds())
                    PathFindDraw.Label(point, in _color, _drawPoint, areaId.ToString());
            }
        }
        private void Check()
        {
            if (!_draw)
                return;
            _checkCount.Clear();
            _checkStringBuilder.Clear();

            short[,] areaIds = _component.GetAreaIdNodes(_moveType, _collType);
            foreach (short areaId in areaIds)
                if (areaId != PathFindExt.CantStand)
                    _checkCount[areaId] = _checkCount.GetValueOrDefault(areaId) + 1;

            if (_checkCount.TryGetValue(PathFindExt.UnDisposed, out uint unDisposedCount))
                _checkStringBuilder.AppendLine($"error# exist unDisposed, count:{unDisposedCount} > 0");
            _checkCount.Remove(PathFindExt.UnDisposed);
            if (_counts.Count != _checkCount.Count)
                _checkStringBuilder.AppendLine($"error# total, data.count:{_counts.Count} != check.count:{_checkCount.Count}");
            foreach (var areaCount in _counts)
                if (!_checkCount.TryGetValue((short)areaCount.Id, out uint count))
                    _checkStringBuilder.AppendLine($"error# areaId:{areaCount.Id}, get areaId fail");
                else if (areaCount.Count != count)
                    _checkStringBuilder.AppendLine($"error# areaId:{areaCount.Id}, data.count:{areaCount.Count} != check.count:{count}");

            if (_checkStringBuilder.Length > 0)
                Debug.LogError(_checkStringBuilder);
        }
    }
}
#endif