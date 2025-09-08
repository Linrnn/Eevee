#if UNITY_EDITOR
using Eevee.Fixed;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Point")]
    internal sealed class EditorDrawPathFindPoint : MonoBehaviour
    {
        private static EditorDrawPathFindPoint _instance;

        [SerializeField] private Vector2Int[] _points;
        [SerializeField] private bool _drawPoint = true;
        [SerializeField] private Color _color = Color.gray;

        private void OnEnable()
        {
            _instance = this;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            DrawPoint();
        }
        private void OnDisable()
        {
            _instance = null;
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

        #region Change
        internal static void SetPoint(Vector2DInt16 point) => _instance._points = new[]
        {
            (Vector2Int)point,
        };
        internal static void SetPoint(Vector2DInt point) => _instance._points = new[]
        {
            (Vector2Int)point,
        };
        internal static void SetPoint(Vector2Int point) => _instance._points = new[]
        {
            point,
        };

        internal static void AddPoint(Vector2DInt16 point)
        {
            var points = _instance._points.ToList();
            points.Add(point);
            _instance._points = points.ToArray();
        }
        internal static void AddPoint(Vector2DInt point)
        {
            var points = _instance._points.ToList();
            points.Add(point);
            _instance._points = points.ToArray();
        }
        internal static void AddPoint(Vector2Int point)
        {
            var points = _instance._points.ToList();
            points.Add(point);
            _instance._points = points.ToArray();
        }

        internal static void SetPoints(IEnumerable<Vector2DInt16> points) => _instance._points = points.Select(point => (Vector2Int)point).ToArray();
        internal static void SetPoints(IEnumerable<Vector2DInt> points) => _instance._points = points.Select(point => (Vector2Int)point).ToArray();
        internal static void SetPoints(IEnumerable<Vector2Int> points) => _instance._points = points.ToArray();

        internal static void AddPoints(IEnumerable<Vector2DInt16> points)
        {
            var newPoints = _instance._points.ToList();
            newPoints.AddRange(points.Select(point => (Vector2Int)point));
            _instance._points = newPoints.ToArray();
        }
        internal static void AddPoints(IEnumerable<Vector2DInt> points)
        {
            var newPoints = _instance._points.ToList();
            newPoints.AddRange(points.Select(point => (Vector2Int)point));
            _instance._points = newPoints.ToArray();
        }
        internal static void AddPoints(IEnumerable<Vector2Int> points)
        {
            var newPoints = _instance._points.ToList();
            newPoints.AddRange(points.Select(point => point));
            _instance._points = newPoints.ToArray();
        }
        #endregion
    }
}
#endif