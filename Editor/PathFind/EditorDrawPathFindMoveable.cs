#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.PathFind;
using UnityEditor;
using UnityEngine;
using MoveFunc = System.Byte;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Moveable")]
    internal sealed class EditorDrawPathFindMoveable : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindMoveable))]
        private sealed class EditorDrawPathFindMoveableInspector : Editor
        {
            #region Property Path
            private const string MoveType = nameof(_moveType);
            private const string Indexes = nameof(_indexes);
            private const string DrawIndex = nameof(_drawIndex);
            private const string DrawPoint = nameof(_drawPoint);
            private const string DrawDir = nameof(_drawDir);
            private const string Color = nameof(_color);
            private const string DirColor = nameof(_dirColor);
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
                _propertyHandle.Draw(Indexes);
                _propertyHandle.Draw(DrawIndex);
                _propertyHandle.Draw(DrawPoint);
                _propertyHandle.Draw(DrawDir);
                _propertyHandle.Draw(Color);
                _propertyHandle.Draw(DirColor);
            }
        }
        #endregion

        [Header("输入参数")] [SerializeField] private MoveFunc _moveType;
        [SerializeField] private int[] _indexes;
        [Header("调试参数")] [SerializeField] private bool _drawIndex;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private bool _drawDir;
        [Header("渲染参数")] [SerializeField] private Color _color = Color.blue;
        [SerializeField] private Color _dirColor = Color.blue.RGBScale(0.8F);

        private IPathFindDrawProxy _proxy;
        private PathFindComponent _component;
        private Vector2 _minBoundary;
        private float _gridSize;
        private readonly PathFindBoundaryProcessor<int, int> _moveableProcessor = new(node => node != PathFindExt.EmptyIndex, node => node);

        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _proxy = proxy;
            _component = proxy.Component;
            _minBoundary = proxy.MinBoundary;
            _gridSize = proxy.GridSize;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            ReadyMoveable();
            DrawMoveable();
        }

        private void ReadyMoveable()
        {
            int[,] nodes = _component.GetMoveableNodes(_moveType);
            var size = _component.GetSize();
            _moveableProcessor.Build(nodes, size);
        }
        private void DrawMoveable()
        {
            foreach ((int index, var boundary) in _moveableProcessor)
            {
                if (!_indexes.IsNullOrEmpty() && !_indexes.Has(index))
                    continue;
                foreach (var side in boundary.Sides())
                    PathFindDraw.Side(side.x, side.y, PathFindExt.StraightDirections[side.z], _gridSize, _minBoundary, in _color);
                var center = boundary.Center();
                string indexStr = _drawIndex ? index.ToString() : null;
                PathFindDraw.Text(center.x, center.y, _gridSize, _minBoundary, in _color, _drawPoint, indexStr);
                if (_drawDir && _proxy.GetMoveDirection(index) is { normalized: var moveDir })
                    PathFindDraw.Arrow(center.x + moveDir.x, center.y + moveDir.y, moveDir, _gridSize, _minBoundary, in _dirColor);
            }
        }
    }
}
#endif