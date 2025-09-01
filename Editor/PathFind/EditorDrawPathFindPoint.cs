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

        private Vector2 _minBoundary;
        private float _gridSize;

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _minBoundary = proxy.MinBoundary;
            _gridSize = proxy.GridSize;
        }
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
                PathFindDraw.Grid(point.x, point.y, _gridSize, _minBoundary, in _color);
                PathFindDraw.Text(point.x, point.y, _gridSize, _minBoundary, in _color, _drawPoint);
            }
        }
    }
}
#endif