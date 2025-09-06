#if UNITY_EDITOR
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Point")]
    internal sealed class EditorDrawPathFindPoint : MonoBehaviour
    {
        [SerializeField] private Vector2Int[] _points;
        [SerializeField] private bool _drawPoint = true;
        [SerializeField] private Color _color = Color.gray;

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            DrawPoint();
        }

        private void DrawPoint()
        {
            if (_points is not { Length: > 0 } points)
                return;

            foreach (var point in points)
            {
                PathFindDraw.Grid(point, in _color);
                PathFindDraw.Label(point, in _color, _drawPoint);
            }
        }
    }
}
#endif