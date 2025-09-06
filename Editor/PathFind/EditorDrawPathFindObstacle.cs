#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.PathFind;
using UnityEditor;
using UnityEngine;
using Ground = System.Byte;

namespace EeveeEditor.PathFind
{
    [AddComponentMenu("Eevee Editor/Path Find/Editor Draw Path Find Obstacle")]
    internal sealed class EditorDrawPathFindObstacle : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawPathFindObstacle))]
        private sealed class EditorDrawPathFindObstacleInspector : Editor
        {
            #region Property Path
            private const string ObstacleType = nameof(_obstacleType);
            private const string Indexes = nameof(_indexes);
            private const string DrawIndex = nameof(_drawIndex);
            private const string DrawPoint = nameof(_drawPoint);
            private const string DrawTerrain = nameof(_drawTerrain);
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
                _propertyHandle.EnumGroupType(ObstacleType);
                _propertyHandle.Draw(Indexes);
                _propertyHandle.Draw(DrawIndex);
                _propertyHandle.Draw(DrawPoint);
                _propertyHandle.Draw(DrawTerrain);
                _propertyHandle.Draw(Color);
            }
        }
        #endregion

        [Header("输入参数")] [SerializeField] private Ground _obstacleType;
        [SerializeField] private int[] _indexes;
        [Header("调试参数")] [SerializeField] private bool _drawIndex;
        [SerializeField] private bool _drawPoint;
        [SerializeField] private bool _drawTerrain;
        [Header("渲染参数")] [SerializeField] private Color _color = Color.magenta;

        private PathFindComponent _component;
        private PathFindBoundaryProcessor<int, PathFindObstacle> _obstacleProcessor;

        private void Awake()
        {
            _obstacleProcessor = new PathFindBoundaryProcessor<int, PathFindObstacle>(node => (node.GroupType & (Ground)_obstacleType) != 0, node => node.Index);
        }
        private void OnEnable()
        {
            var proxy = PathFindGetter.Proxy;
            _component = proxy.Component;
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            ReadyObstacle();
            DrawObstacle();
        }

        private void ReadyObstacle()
        {
            var nodes = _component.GetObstacleNodes();
            var size = _component.GetSize();
            _obstacleProcessor.Build(nodes, size);
        }
        private void DrawObstacle()
        {
            foreach ((int index, var boundary) in _obstacleProcessor)
            {
                if (!_indexes.IsNullOrEmpty() && !_indexes.Has(index))
                    continue;
                foreach (var side in boundary.Sides())
                    PathFindDraw.Side((Vector2Int)side, PathFindExt.StraightDirections[side.z], in _color);
                var center = boundary.Center();
                string indexStr = _drawIndex ? index.ToString() : null;
                if (index != PathFindExt.EmptyIndex)
                    PathFindDraw.Label(center, in _color, _drawPoint, indexStr);
                else if (_drawTerrain)
                    foreach (var point in boundary.Girds())
                        PathFindDraw.Label(point, in _color, _drawPoint, indexStr);
            }
        }
    }
}
#endif