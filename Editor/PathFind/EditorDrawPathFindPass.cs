#if UNITY_EDITOR
using Eevee.PathFind;
using UnityEditor;
using UnityEngine;
using CollSize = System.SByte;
using MoveFunc = System.Byte;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Pass")]
    internal sealed class EditorDrawPathFindPass : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindPass))]
        private sealed class EditorDrawPathFindPassInspector : Editor
        {
            #region Property Path
            private const string MoveType = nameof(_moveType);
            private const string DrawPoint = nameof(_drawPoint);
            private const string Color = nameof(_color);
            private const string Height = nameof(_height);
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
                _propertyHandle.Draw(DrawPoint);
                _propertyHandle.Draw(Color);
                _propertyHandle.Draw(Height);
            }
        }
        #endregion

        [SerializeField] private MoveFunc _moveType;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private Color _color = Color.green;
        [SerializeField] private float _height;

        private PathFindComponent _component;
        private Vector2 _minBoundary;
        private float _gridSize;
        private PathFindBoundaryProcessor<int, CollSize> _passProcessor;

        private void Awake()
        {
            var proxy = PathFindGetter.Proxy;
            _passProcessor = new PathFindBoundaryProcessor<int, CollSize>(proxy.ValidColl, node => node);
        }
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

            ReadyPass();
            DrawPass();
        }

        private void ReadyPass()
        {
            CollSize[,] nodes = _component.GetPassNodes(_moveType);
            var size = _component.GetSize();
            _passProcessor.Build(nodes, size);
        }
        private void DrawPass()
        {
            foreach ((int coll, var boundary) in _passProcessor)
            {
                foreach (var side in boundary.Sides())
                    PathFindDraw.Side(side.x, side.y, PathFindExt.StraightDirections[side.z], _gridSize, _minBoundary, _height, in _color);
                foreach (var point in boundary.Girds())
                    PathFindDraw.Text(point.x, point.y, _gridSize, _minBoundary, _height, in _color, _drawPoint, ((CollSize)coll).ToString());
            }
        }
    }
}
#endif