#if UNITY_EDITOR
using Eevee.QuadTree;
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
            private const string EnableLog = nameof(_enableLog);
            private const string TreeIds = nameof(_treeIds);
            private const string Indexes = nameof(_indexes);
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
                _propertyHandle.Draw(EnableLog);
                _propertyHandle.EnumTreeFunc(TreeIds);
                _propertyHandle.Draw(Indexes);
            }
        }
        #endregion

        [SerializeField] private bool _enableLog;
        [SerializeField] private int[] _treeIds;
        [SerializeField] private int[] _indexes;

        private void OnEnable() => SetParam();
        private void OnValidate() => SetParam();
        private void OnDisable() => ResetParam();

        private void SetParam()
        {
            QuadTreeDiagnosis.EnableLog = _enableLog;
            QuadTreeDiagnosis.TreeIds = _treeIds;
            QuadTreeDiagnosis.Indexes = _indexes;
        }
        private void ResetParam()
        {
            QuadTreeDiagnosis.EnableLog = false;
            QuadTreeDiagnosis.TreeIds = null;
            QuadTreeDiagnosis.Indexes = null;
        }
    }
}
#endif