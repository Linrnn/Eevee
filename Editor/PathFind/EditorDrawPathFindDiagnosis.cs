#if UNITY_EDITOR
using Eevee.PathFind;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Diagnosis")]
    internal sealed class EditorDrawPathFindDiagnosis : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindDiagnosis))]
        private sealed class EditorDrawPathFindDiagnosisInspector : Editor
        {
            #region Property Path
            private const string NextPoint = nameof(_nextPoint);
            private const string Path = nameof(_path);
            private const string Process = nameof(_process);
            private const string Log = nameof(_log);
            private const string Indexes = nameof(_indexes);
            private const string Height = nameof(_height);
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
                _propertyHandle.Draw(NextPoint);
                _propertyHandle.Draw(Path);
                _propertyHandle.Draw(Process);
                _propertyHandle.Draw(Log);
                if (_propertyHandle.Get(Log).boolValue)
                    _propertyHandle.Draw(Indexes);
                _propertyHandle.Draw(Height);
            }
        }
        #endregion

        [Header("路径参数")] [SerializeField] private bool _nextPoint;
        [SerializeField] private bool _path;
        [SerializeField] private bool _process;
        [Header("日志参数")] [SerializeField] private bool _log;
        [SerializeField] private int[] _indexes;
        [Header("绘制参数")] [SerializeField] private float _height;

        private void OnEnable() => SetParam();
        private void OnValidate() => SetParam();
        private void OnDisable() => ResetParam();

        private void SetParam()
        {
            PathFindDiagnosis.EnableNextPoint = _nextPoint;
            PathFindDiagnosis.EnablePath = _path;
            PathFindDiagnosis.EnableProcess = _process;
            PathFindDiagnosis.EnableLog = _log;
            PathFindDiagnosis.Indexes = _indexes.ToArray();
            PathFindDraw.Height = _height;
        }
        private void ResetParam()
        {
            PathFindDiagnosis.EnableNextPoint = default;
            PathFindDiagnosis.EnablePath = default;
            PathFindDiagnosis.EnableProcess = default;
            PathFindDiagnosis.EnableLog = default;
            PathFindDiagnosis.Indexes = Array.Empty<int>();
            PathFindDraw.Height = _height;
        }
    }
}
#endif