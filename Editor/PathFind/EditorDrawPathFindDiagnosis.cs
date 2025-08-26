#if UNITY_EDITOR
using Eevee.PathFind;
using System.Linq;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Diagnosis")]
    internal sealed class EditorDrawPathFindDiagnosis : MonoBehaviour
    {
        [SerializeField] private bool _nextPoint;
        [SerializeField] private bool _path;
        [SerializeField] private bool _process;
        [SerializeField] private bool _log;
        [SerializeField] private int[] _indexes;

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
        }
        private void ResetParam()
        {
            PathFindDiagnosis.EnableNextPoint = false;
            PathFindDiagnosis.EnablePath = false;
            PathFindDiagnosis.EnableProcess = false;
            PathFindDiagnosis.EnableLog = false;
            PathFindDiagnosis.Indexes = null;
        }
    }
}
#endif