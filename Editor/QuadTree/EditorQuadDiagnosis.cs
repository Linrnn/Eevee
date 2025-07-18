#if UNITY_EDITOR
using Eevee.QuadTree;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorQuadDiagnosis : MonoBehaviour
    {
        [SerializeField] private bool _print;
        [SerializeField] private int[] _treeIds;
        [SerializeField] private int[] _indexes;

        private void OnEnable() => SetParam();
        private void OnValidate() => SetParam();
        private void OnDisable() => ResetParam();

        private void SetParam()
        {
            QuadDiagnosis.Print = _print;
            QuadDiagnosis.TreeIds = _treeIds;
            QuadDiagnosis.Indexes = _indexes;
        }
        private void ResetParam()
        {
            QuadDiagnosis.Print = false;
            QuadDiagnosis.TreeIds = null;
            QuadDiagnosis.Indexes = null;
        }
    }
}
#endif