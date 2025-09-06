#if UNITY_EDITOR
using Eevee.PathFind;
using UnityEditor;
using UnityEngine;
using CollSize = System.SByte;
using MoveFunc = System.Byte;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Area")]
    internal sealed class EditorDrawPathFindArea : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindArea))]
        private sealed class EditorDrawPathFindAreaInspector : Editor
        {
            #region Property Path
            private const string MoveType = nameof(_moveType);
            private const string CollType = nameof(_collType);
            private const string AllocatorId = nameof(_allocatorId);
            private const string DrawPoint = nameof(_drawPoint);
            private const string Color = nameof(_color);
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
                _propertyHandle.Draw(AllocatorId, true);
                _propertyHandle.Draw(DrawPoint);
                _propertyHandle.Draw(Color);
            }
        }
        #endregion

        [SerializeField] private MoveFunc _moveType;
        [SerializeField] private CollSize _collType;
        [SerializeField] private int _allocatorId;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private Color _color = Color.red;

        private PathFindComponent _component;
        private Vector2 _minBoundary;
        private float _gridSize;
        private readonly PathFindBoundaryProcessor<int, short> _areaProcessor = new(areaId => areaId != PathFindExt.CantStand, areaId => areaId);

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

            ReadyArea();
            DrawArea();
        }

        private void ReadyArea()
        {
            short[,] areaIds = _component.GetAreaIdNodes(_moveType, _collType);
            var size = _component.GetSize();
            _areaProcessor.Build(areaIds, size);
            _allocatorId = _component.GetAreaIdAllocator(_moveType, _collType);
        }
        private void DrawArea()
        {
            foreach ((int areaId, var boundary) in _areaProcessor)
            {
                foreach (var side in boundary.Sides())
                    PathFindDraw.Side(side.x, side.y, PathFindExt.StraightDirections[side.z], _gridSize, _minBoundary, in _color);
                foreach (var point in boundary.Girds())
                    PathFindDraw.Label(point.x, point.y, _gridSize, _minBoundary, in _color, _drawPoint, areaId.ToString());
            }
        }
    }
}
#endif