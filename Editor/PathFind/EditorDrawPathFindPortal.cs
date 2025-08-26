#if UNITY_EDITOR
using Eevee.PathFind;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Portal")]
    internal sealed class EditorDrawPathFindPortal : MonoBehaviour
    {
        [Header("输入参数")] [SerializeField] private bool _drawIndex;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private bool _drawLine;
        [Header("渲染参数")] [SerializeField] private Color _startColor = Color.gray;
        [SerializeField] private Color _lineColor = Color.gray.RGBScale(0.5F);
        [SerializeField] private Color _endColor = Color.gray.RGBScale(0.8F);
        [SerializeField] private float _height;

        private PathFindComponent _component;
        private Vector2 _minBoundary;
        private float _gridSize;
        private readonly List<PathFindPortal> _portals = new();

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _component = proxy.Component;
            _minBoundary = proxy.MinBoundary;
            _gridSize = proxy.GridSize;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            _component.GetPortals(_portals);
            DrawPortal();
        }

        private void DrawPortal()
        {
            foreach (var portal in _portals)
            {
                var start = portal.Point.Start;
                var end = portal.Point.End;
                string indexStr = _drawIndex ? portal.Index.ToString() : null;

                PathFindDraw.Grid(start.X, start.Y, _gridSize, _minBoundary, _height, in _startColor);
                PathFindDraw.Text(start.X, start.Y, _gridSize, _minBoundary, _height, in _startColor, _drawPoint, indexStr);
                if (_drawLine)
                    PathFindDraw.Line(start.X, start.Y, end.X, end.Y, _gridSize, _minBoundary, _height, in _lineColor);
                PathFindDraw.Grid(end.X, end.Y, _gridSize, _minBoundary, _height, in _endColor);
                PathFindDraw.Text(end.X, end.Y, _gridSize, _minBoundary, _height, in _endColor, _drawPoint, indexStr);
            }
        }
    }
}
#endif