#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.PathFind;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CollSize = System.SByte;
using MoveFunc = System.Byte;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Jump Point")]
    internal sealed class EditorDrawPathFindJumpPoint : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindJumpPoint))]
        private sealed class EditorDrawPathFindJumpPointInspector : Editor
        {
            #region Property Path
            private const string MoveType = nameof(_moveType);
            private const string CollType = nameof(_collType);
            private const string DrawPoint = nameof(_drawPoint);
            private const string DrawPrev = nameof(_drawPrev);
            private const string DrawNavPoint = nameof(_drawNavPoint);
            private const string NavInterval = nameof(_navInterval);
            private const string Color = nameof(_color);
            private const string PrevColor = nameof(_prevColor);
            private const string NextColor = nameof(_nextColor);
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
                _propertyHandle.EnumMoveType(MoveType);
                _propertyHandle.EnumCollType(CollType);
                _propertyHandle.Draw(DrawPoint);
                _propertyHandle.Draw(DrawPrev);
                _propertyHandle.Draw(DrawNavPoint);
                if (_propertyHandle.Get(DrawNavPoint).boolValue)
                    _propertyHandle.Draw(NavInterval);
                _propertyHandle.Draw(Color);
                _propertyHandle.Draw(PrevColor);
                _propertyHandle.Draw(NextColor);
            }
        }
        #endregion

        [Header("输入参数")] [SerializeField] private MoveFunc _moveType;
        [SerializeField] private CollSize _collType;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private bool _drawPrev;
        [SerializeField] private bool _drawNavPoint;
        [SerializeField] private int _navInterval = 5;
        [Header("渲染参数")] [SerializeField] private Color _color = Color.yellow;
        [SerializeField] private Color _prevColor = Color.yellow.RGBScale(0.8F);
        [SerializeField] private Color _nextColor = Color.yellow.RGBScale(0.5F);

        private PathFindComponent _component;
        private readonly HashSet<Vector2DInt16> _points = new();
        private readonly List<(int sx, int sy, int ex, int ey)> _lines = new();

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _component = proxy.Component;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            ReadyNavPoints();
            DrawJumpPoint();
        }

        private void ReadyNavPoints()
        {
            _lines.Clear();
            if (!_drawNavPoint)
                return;

            short[,,] navPoints = _component.GetNavPoints(_moveType, _collType);
            if (navPoints is null)
                return;

            var size = _component.GetSize();
            for (int max = size.X - 1, j = 0; j < size.Y; ++j)
            for (int i = max; i >= 0; --i)
                BuildNavLine(navPoints, ref i, ref j, PathFindExt.DirIndexLeft, -1);
            for (int i = 0; i < size.X; ++i)
            for (int j = 0; j < size.Y; ++j)
                BuildNavLine(navPoints, ref i, ref j, PathFindExt.DirIndexUp, size.Y);
            for (int j = 0; j < size.Y; ++j)
            for (int i = 0; i < size.X; ++i)
                BuildNavLine(navPoints, ref i, ref j, PathFindExt.DirIndexRight, size.X);
            for (int max = size.Y - 1, i = 0; i < size.X; ++i)
            for (int j = max; j >= 0; --j)
                BuildNavLine(navPoints, ref i, ref j, PathFindExt.DirIndexDown, -1);
        }
        private void DrawJumpPoint()
        {
            var jumpPoints = _component.GetJumpPoints(_moveType, _collType);
            if (jumpPoints is null)
                return;

            _points.Clear();
            foreach (var (point, handles) in jumpPoints)
            {
                if (_drawPrev)
                {
                    foreach (var jumpPoint in handles)
                    {
                        var prevPoint = jumpPoint.PrevPoint;
                        PathFindDraw.ObliqueArrow(prevPoint, jumpPoint.Direction, in _prevColor);
                        if (_points.Add(prevPoint))
                            PathFindDraw.Grid(prevPoint, in _prevColor);
                    }
                }

                PathFindDraw.Grid(point, in _color);
                PathFindDraw.Label(point, in _color, _drawPoint);
            }

            if (_drawNavPoint)
            {
                _points.Clear();
                foreach (var line in _lines)
                {
                    var dir = new Vector2DInt16(line.ex - line.sx, line.ey - line.sy).Sign();
                    var ro = (Vector2)dir.Perpendicular() * -0.25F; // 直线靠右，所以“Perpendicular”乘以负数
                    var offset = ro - (Vector2)dir * 0.5F;
                    var sp = new Vector2(line.sx + offset.x, line.sy + offset.y);
                    var ep = new Vector2(line.ex + offset.x, line.ey + offset.y);
                    PathFindDraw.Line(sp, ep, in _nextColor);
                    PathFindDraw.Arrow(ep, dir, in _nextColor);

                    if (_navInterval > 0)
                    {
                        var start = new Vector2DInt16(line.sx, line.sy);
                        var next = new Vector2DInt16(line.ex, line.ey) - _navInterval * dir;
                        int distance = _navInterval;
                        while ((next - start).Sign() == dir)
                        {
                            if (_points.Add(next))
                                PathFindDraw.Grid(next, in _nextColor);
                            PathFindDraw.Label(next + ro, in _nextColor, _drawPoint, distance.ToString());
                            next -= _navInterval * dir;
                            distance += _navInterval;
                        }
                    }
                }
            }
        }

        private void BuildNavLine(short[,,] navPoints, ref int i, ref int j, int dirIndex, int limit)
        {
            var dir = PathFindExt.StraightDirections[dirIndex];
            for (int fi = i, fj = j, fn = navPoints[i, j, dirIndex]; i != limit && j != limit; i += dir.X, j += dir.Y)
            {
                int next = navPoints[i, j, dirIndex];
                if (next == PathFindExt.CantStand)
                    break;
                int diff = fn - next;
                if (diff != Math.Abs(i - fi) && diff != Math.Abs(j - fj))
                    break;
                if (next != PathFindExt.JumpPointDistance && i + dir.X != limit && j != limit + dir.Y)
                    continue;
                if (fi == i && fj == j)
                    break;
                _lines.Add((fi, fj, i, j));
                break;
            }
        }
    }
}
#endif