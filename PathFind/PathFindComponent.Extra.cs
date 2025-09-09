#if UNITY_EDITOR
using Eevee.Fixed;
using System.Collections.Generic;
using CollSize = System.SByte;
using MoveFunc = System.Byte;

namespace Eevee.PathFind
{
    public sealed partial class PathFindComponent
    {
        public Vector2DInt16 GetSize() => _size;
        internal PathFindObstacle[,] GetObstacleNodes() => _obstacleNodes;
        internal void GetPortals(ICollection<PathFindPortal> portals)
        {
            portals.Clear();
            foreach (var (_, portal) in _portals)
                portals.Add(portal);
        }
        internal int[,] GetMoveableNodes(MoveFunc moveType)
        {
            if (_moveTypeIndexes.TryGetValue(moveType, out var moveTypeInfo))
                return _moveableNodes[moveTypeInfo.GroupIndex];
            return null;
        }
        internal CollSize[,] GetPassNodes(MoveFunc moveType)
        {
            if (_moveTypeIndexes.TryGetValue(moveType, out var moveTypeInfo))
                return _passes[moveTypeInfo.TypeIndex];
            return null;
        }
        internal Dictionary<short, uint> GetAreaCount(MoveFunc moveType, CollSize coll) => GetMoveColl(moveType, coll).AreaCount;
        internal int GetAreaIdAllocator(MoveFunc moveType, CollSize coll) => GetMoveColl(moveType, coll).AreaIdAllocator;
        internal short[,] GetAreaIdNodes(MoveFunc moveType, CollSize coll) => GetMoveColl(moveType, coll).AreaIds;
        internal Dictionary<Vector2DInt16, List<PathFindJumpPointHandle>> GetJumpPoints(MoveFunc moveType, CollSize coll) => GetMoveColl(moveType, coll).JumpPoints;
        internal short[,,] GetNextJPs(MoveFunc moveType, CollSize coll) => GetMoveColl(moveType, coll).NextJPs;

        private PathFindMtCs GetMoveColl(MoveFunc moveType, CollSize coll)
        {
            if (!_moveTypeIndexes.TryGetValue(moveType, out var moveTypeInfo))
                return default;
            if (!_collisionIndexes.TryGetValue(coll, out int collIndex))
                return default;
            return _moveCollisions[moveTypeInfo.TypeIndex, collIndex];
        }
    }
}
#endif