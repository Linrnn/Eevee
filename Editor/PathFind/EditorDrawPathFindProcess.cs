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
                if (GUILayout.Button("Prev"))
                    ButtonPrev();
                if (GUILayout.Button("Next"))
                    ButtonNext();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                _propertyHandle.Draw(Loop);
                if (GUILayout.Button("Restart"))
                    ButtonRestart();
                EditorGUILayout.EndHorizontal();
                _propertyHandle.Draw(Color);
                _propertyHandle.Draw(Decrease);
            }

            private void ButtonPrev()
            {
                var stepProperty = _propertyHandle.Get(Step);
                var processProperty = _propertyHandle.Get(Process);
                var pauseProperty = _propertyHandle.Get(Pause);

                if (pauseProperty.boolValue)
                    processProperty.intValue = Math.Max(0, processProperty.intValue + stepProperty.intValue);
            }
            private void ButtonNext()
            {
                var stepProperty = _propertyHandle.Get(Step);
                var processProperty = _propertyHandle.Get(Process);
                var pauseProperty = _propertyHandle.Get(Pause);
                var loopProperty = _propertyHandle.Get(Loop);
                int pointCount = ((EditorDrawPathFindProcess)target)._points.Count;

                if (!pauseProperty.boolValue)
                    return;
                if (loopProperty.boolValue && processProperty.intValue + 1 >= pointCount)
                    processProperty.intValue = 0;
                else
                    processProperty.intValue = Math.Min(processProperty.intValue + stepProperty.intValue, pointCount);
            }
            private void ButtonRestart()
            {
                var processProperty = _propertyHandle.Get(Process);
                processProperty.intValue = 0;
            }
        }

        [Serializable]
        private struct Repeated
        {
            private static Comparison<Repeated> _comparison = (lhs, rhs) => lhs.Times.CompareTo(rhs.Times);
            [SerializeField] [ReadOnly] internal int Times;
            [SerializeField] [ReadOnly] internal int Count;

            private Repeated(int times, int count)
            {
                Times = times;
                Count = count;
            }
            internal static void Add(List<Repeated> repeatedDetail, int times)
            {
                for (int i = 0; i < repeatedDetail.Count; ++i)
                {
                    var repeated = repeatedDetail[i];
                    if (repeated.Times != times)
                        continue;

                    repeatedDetail[i] = new Repeated(times, repeated.Count + 1);
                    return;
                }

                repeatedDetail.Add(new Repeated(times, 1));
                repeatedDetail.Sort(_comparison);
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

        private Vector2 _minBoundary;
        private float _gridSize;
        private readonly List<Vector2DInt16> _points = new();
        private readonly Dictionary<Vector2DInt16, int> _pointDrawTimes = new();

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _minBoundary = proxy.MinBoundary;
            _gridSize = proxy.GridSize;
        }
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
                if (_loop && _process + 1 >= _points.Count)
                    _process = 0;
                else
                    _process = Math.Min(_process + _step, _points.Count);

            _pointDrawTimes.Clear();
            for (int i = 0; i < _process; ++i)
            {
                var point = _points[i];
                int count = _pointDrawTimes.GetValueOrDefault(point);
                _pointDrawTimes[point] = count + 1;
                PathFindDraw.Grid(point.X, point.Y, _gridSize, _gridSize * (1 - count * _decrease), _minBoundary, in _color);
            }
        }
    }
}
#endif