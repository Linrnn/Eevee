#if UNITY_EDITOR
using Eevee.QuadTree;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    [AddComponentMenu("Eevee Editor/Quad Tree/Editor Draw Quad Tree Diagnosis")]
    internal sealed class EditorDrawQuadTreeDiagnosis : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawQuadTreeDiagnosis))]
        private sealed class EditorDrawQuadTreeDiagnosisInspector : Editor
        {
            #region Property Path
            private const string Log = nameof(_log);
            private const string TreeIds = nameof(_treeIds);
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
                _propertyHandle.Draw(Log);
                if (_propertyHandle.Get(Log).boolValue)
                {
                    _propertyHandle.EnumTreeFunc(TreeIds);
                    _propertyHandle.Draw(Indexes);
                }
                _propertyHandle.Draw(Height);
            }
        }
        #endregion

        [Header("日志参数")] [SerializeField] private bool _log;
        [SerializeField] private int[] _treeIds = Array.Empty<int>();
        [SerializeField] private int[] _indexes = Array.Empty<int>();
        [Header("绘制参数")] [SerializeField] private float _height;

        private void OnEnable() => SetParam();
        private void OnValidate() => SetParam();
        private void OnDisable() => ResetParam();

        private void SetParam()
        {
            QuadTreeDiagnosis.EnableLog = _log;
            QuadTreeDiagnosis.TreeIds = _treeIds.ToArray();
            QuadTreeDiagnosis.Indexes = _indexes.ToArray();
            QuadTreeDraw.Height = _height;
        }
        private void ResetParam()
        {
            QuadTreeDiagnosis.EnableLog = default;
            QuadTreeDiagnosis.TreeIds = Array.Empty<int>();
            QuadTreeDiagnosis.Indexes = Array.Empty<int>();
            QuadTreeDraw.Height = default;
        }
    }
}
#endif