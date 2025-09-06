#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.Fixed;
using Eevee.PathFind;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Result")]
    internal sealed class EditorDrawPathFindResult : MonoBehaviour
    {
        [Header("输入参数")] [SerializeField] private PathFindFunc _findFunc;
        [SerializeField] private int[] _indexes;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private bool _drawNext;
        [Header("渲染参数")] [SerializeField] private Color _startColor = Color.cyan;
        [SerializeField] private Color _lineColor = Color.cyan.RGBScale(0.5F);
        [SerializeField] private Color _endColor = Color.cyan.RGBScale(0.8F);
        [SerializeField] private Color _nextColor = Color.cyan.RGBScale(0.8F);

        private IPathFindDrawProxy _proxy;
        private Vector2 _minBoundary;
        private float _gridSize;
        private readonly List<Vector2DInt16> _points = new();

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _proxy = proxy;
            _minBoundary = proxy.MinBoundary;
            _gridSize = proxy.GridSize;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            DrawPath();
        }

        private void DrawPath()
        {
            if (_indexes.IsNullOrEmpty())
                return;

            foreach (int index in _indexes)
            {
                PathFindDiagnosis.GetPath(_findFunc, index, _points);
                if (_points.IsEmpty())
                    continue;

                var start = _points[0];
                PathFindDraw.Grid(start.X, start.Y, _gridSize, _minBoundary, in _startColor);
                PathFindDraw.Label(start.X, start.Y, _gridSize, _minBoundary, in _startColor, _drawPoint);

                for (int count = _points.Count - 1, i = 1; i < count; ++i)
                {
                    var point = _points[i];
                    PathFindDraw.Grid(point.X, point.Y, _gridSize, _minBoundary, in _lineColor);
                    PathFindDraw.Label(point.X, point.Y, _gridSize, _minBoundary, in _lineColor, _drawPoint);
                }

                var end = _points[^1];
                PathFindDraw.Grid(end.X, end.Y, _gridSize, _minBoundary, in _endColor);
                PathFindDraw.Label(end.X, end.Y, _gridSize, _minBoundary, in _endColor, _drawPoint);
                PathFindDraw.Arrow(end.X, end.Y, ((Vector2)(end - _points[^2])).normalized, _gridSize, _minBoundary, in _endColor);

                for (int i = 1; i < _points.Count; ++i)
                    if (_points[i - 1] is { } p0 && _points[i] is { } p1)
                        PathFindDraw.Line(p0.X, p0.Y, p1.X, p1.Y, _gridSize, _minBoundary, in _lineColor);

                if (_drawNext && _proxy.GetCurrentPoint(index) is { } curPoint && PathFindDiagnosis.GetNextPoint(_findFunc, index) is { } nextPoint)
                {
                    PathFindDraw.Grid(curPoint.x, curPoint.y, _gridSize, _minBoundary, in _nextColor);
                    PathFindDraw.Line(curPoint.x, curPoint.y, nextPoint.X, nextPoint.Y, _gridSize, _minBoundary, in _nextColor);
                    PathFindDraw.Arrow(nextPoint.X, nextPoint.Y, ((Vector2)(nextPoint - curPoint)).normalized, _gridSize, _minBoundary, in _nextColor);
                }
            }
        }
    }
}
#endif