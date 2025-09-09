#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.Fixed;
using Eevee.PathFind;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Process")]
    internal sealed class EditorDrawPathFindProcess : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindProcess))]
        private sealed class EditorDrawPathFindProcessInspector : Editor
        {
            #region Property Path
            private const string FindFunc = nameof(_findFunc);
            private const string Index = nameof(_index);
            private const string RepeatedRate = nameof(_repeatedRate);
            private const string TotalPointCount = nameof(_totalPointCount);
            private const string PointCount = nameof(_pointCount);
            private const string RepeatedDetail = nameof(_repeatedDetail);
            private const string Step = nameof(_step);
            private const string Process = nameof(_process);
            private const string Pause = nameof(_pause);
            private const string Loop = nameof(_loop);
            private const string Color = nameof(_color);
            private const string Decrease = nameof(_decrease);
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
                var stepProperty = _propertyHandle.Get(Step);
                var processProperty = _propertyHandle.Get(Process);
                var pauseProperty = _propertyHandle.Get(Pause);

                _propertyHandle.DrawScript();
                _propertyHandle.Draw(FindFunc);
                _propertyHandle.Draw(Index);
                _propertyHandle.Draw(RepeatedRate, true);
                _propertyHandle.Draw(TotalPointCount, true);
                _propertyHandle.Draw(PointCount, true);
                _propertyHandle.Draw(RepeatedDetail, true);
                _propertyHandle.Draw(Step);
                _propertyHandle.Draw(Process);
                EditorGUILayout.BeginHorizontal();
                _propertyHandle.Draw(Pause);
                if (pauseProperty.boolValue && GUILayout.Button("Prev"))
                    processProperty.intValue -= stepProperty.intValue;
                if (pauseProperty.boolValue && GUILayout.Button("Next"))
                    processProperty.intValue += stepProperty.intValue;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                _propertyHandle.Draw(Loop);
                if (GUILayout.Button("Restart"))
                    processProperty.intValue = 0;
                EditorGUILayout.EndHorizontal();
                _propertyHandle.Draw(Color);
                _propertyHandle.Draw(Decrease);
            }
        }

        [Serializable]
        private struct Repeated
        {
            private static Comparison<Repeated> _comparison = (lhs, rhs) => lhs._times.CompareTo(rhs._times);
            [SerializeField] private int _times;
            [SerializeField] private int _count;

            private Repeated(int times, int count)
            {
                _times = times;
                _count = count;
            }
            internal static void Add(List<Repeated> repeatedDetail, int times)
            {
                for (int i = 0; i < repeatedDetail.Count; ++i)
                {
                    var repeated = repeatedDetail[i];
                    if (repeated._times != times)
                        continue;

                    repeatedDetail[i] = new Repeated(times, repeated._count + 1);
                    return;
                }

                repeatedDetail.Add(new Repeated(times, 1));
                repeatedDetail.Sort(_comparison);
            }

            [CustomPropertyDrawer(typeof(Repeated))]
            private sealed class RepeatedDrawer : PropertyDrawer
            {
                private static readonly GUIContent _timesLabel = new(nameof(_times));
                private static readonly GUIContent _countLabel = new(nameof(_count));

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    const int scale = 2;
                    const float spaceWidth = EditorUtils.SpaceWidth / scale;
                    const float propertyHeight = EditorUtils.PropertyHeight;

                    var min = position.position;
                    float labelWidth = EditorGUIUtility.labelWidth;
                    float width = (position.size.x - spaceWidth) / scale;
                    var timesPosition = new Rect(min.x, min.y, width - spaceWidth, propertyHeight);
                    var countPosition = new Rect(min.x + width + spaceWidth, min.y, width - spaceWidth, propertyHeight);

                    var timesProperty = property.FindPropertyRelative(nameof(_times));
                    var countProperty = property.FindPropertyRelative(nameof(_count));

                    EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(_timesLabel) + spaceWidth;
                    timesProperty.intValue = EditorGUI.IntField(timesPosition, timesProperty.displayName, timesProperty.intValue);
                    EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(_countLabel) + spaceWidth;
                    countProperty.intValue = EditorGUI.IntField(countPosition, countProperty.displayName, countProperty.intValue);
                    EditorGUIUtility.labelWidth = labelWidth;
                }
            }
        }
        #endregion

        [Header("输入参数")] [SerializeField] private PathFindFunc _findFunc;
        [SerializeField] private int _index;
        [Header("性能参数")] [SerializeField] private float _repeatedRate;
        [SerializeField] private int _totalPointCount;
        [SerializeField] private int _pointCount;
        [SerializeField] private List<Repeated> _repeatedDetail = new();
        [Header("调试参数")] [SerializeField] private int _step = 10;
        [SerializeField] private int _process;
        [SerializeField] private bool _pause;
        [SerializeField] private bool _loop;
        [Header("渲染参数")] [SerializeField] private Color _color = Color.white;
        [SerializeField] private float _decrease = 0.2F;

        private readonly List<Vector2DInt16> _points = new();
        private readonly Dictionary<Vector2DInt16, int> _pointDrawTimes = new();

        private void OnDrawGizmos()
        {
            DrawRepeated();

            if (!enabled)
                return;

            DrawProcess();
        }

        private void DrawRepeated()
        {
            PathFindDiagnosis.GetProcess(_findFunc, _index, _points);
            if (_points.Count == 0)
            {
                _repeatedRate = 0;
            }
            else
            {
                _pointDrawTimes.Clear();
                foreach (var point in _points)
                    _pointDrawTimes[point] = _pointDrawTimes.GetValueOrDefault(point) + 1;
                _repeatedRate = _points.Count / (float)_pointDrawTimes.Count - 1;
            }

            _totalPointCount = _points.Count;
            _pointCount = _pointDrawTimes.Count;

            _repeatedDetail.Clear();
            foreach ((_, int times) in _pointDrawTimes)
                Repeated.Add(_repeatedDetail, times);
        }
        private void DrawProcess()
        {
            if (_step < 0)
                return;

            PathFindDiagnosis.GetProcess(_findFunc, _index, _points);
            if (_points.IsEmpty())
                return;

            if (!_pause)
                _process += _step;
            if (_loop)
                _process = (_process % _points.Count + _points.Count) % _points.Count;
            else
                _process = Math.Clamp(_process, 0, _points.Count - 1);

            _pointDrawTimes.Clear();
            for (int i = 0; i <= _process; ++i)
            {
                var point = _points[i];
                int count = _pointDrawTimes.GetValueOrDefault(point);
                _pointDrawTimes[point] = count + 1;
                PathFindDraw.Grid(point, 1 - count * _decrease, in _color);
            }
        }
    }
}
#endif