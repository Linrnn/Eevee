using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CollSize = System.SByte;
using Ground = System.Byte;
using MoveFunc = System.Byte;

namespace Eevee.PathFind
{
    /// <summary>
    /// 寻路网格组件
    /// </summary>
    public sealed partial class PathFindComponent
    {
        #region 数据
        private bool _allowBuildCache;
        private Vector2DInt16 _size; // 地图尺寸
        private PathFindPeek _maxColl; // 最大的碰撞尺寸
        private List<CollSize> _collisions; // 可通行的尺寸，等价CollType
        private Dictionary<MoveFunc, PathFindMoveTypeInfo> _moveTypeIndexes; // key：moveType
        private Dictionary<CollSize, int> _collisionIndexes; // key：coll；value：collIndex
        private IPathFindCollisionGetter _collisionGetter;
        private IPathFindObjectPoolGetter _objectPoolGetter;

        private Ground[,] _terrainNodes; // 默认的地形
        private PathFindObstacle[,] _obstacleNodes; // 不可移动对象，障碍物
        private int[][,] _moveableNodes; // 可移动对象，数组索引：moveGroupIndex，第2层索引：x坐标，第3层索引：y坐标
        private CollSize[][,] _passes; // 最大可通行的尺寸；第1层索引：moveTypeIndex，第2层索引：x坐标，第3层索引：y坐标
        private PathFindMtCs[,] _moveCollisions; // 俩个维度的缓存；第1层索引：moveTypeIndex，第2层索引：collIndex
        private readonly Dictionary<Vector2DInt16, PathFindPortal> _portals = new(); // key：传送门index；value：传送区域；传送门一定是跳点
        #endregion

        #region 初始化
        public PathFindComponent(Ground[,] terrains, IReadOnlyList<IEnumerable<MoveFunc>> moveTypeGroups, IEnumerable<CollSize> collisions, in PathFindGetters getters)
        {
            var size = new Vector2DInt16(terrains.GetLength(0), terrains.GetLength(1));
            var newCollisions = new List<CollSize>(collisions);
            newCollisions.Sort();
            InitData(size, getters.Collision.GetMax(newCollisions), moveTypeGroups, newCollisions, in getters);
            InitNode(terrains);
        }
        public void Initialize(bool allowBuild, bool buildPass, bool buildArea, bool buildJumpPoint)
        {
            var size = _size;
            var collIndexes = _collisionIndexes;
            var moveTypeIndexes = _moveTypeIndexes; // 依赖“InitData”

            _allowBuildCache = allowBuild;
            if (buildPass)
                InitPass(size, moveTypeIndexes);
            if (buildArea)
                InitArea(size, moveTypeIndexes, collIndexes); // “InitArea”依赖“Pass”
            if (buildJumpPoint)
                InitJumpPoint(size, moveTypeIndexes, collIndexes); // “JumpPoint”依赖“Pass”
        }

        private void InitData(Vector2DInt16 size, CollSize maxColl, IReadOnlyList<IEnumerable<MoveFunc>> moveTypeGroups, List<CollSize> collisions, in PathFindGetters getters)
        {
            int groupKind = moveTypeGroups.Count;
            var moveTypes = new List<(MoveFunc Type, int GroypIndex)>();
            for (int i = 0; i < groupKind; ++i)
                if (moveTypeGroups[i] is var moveTypeGroup)
                    foreach (MoveFunc moveType in moveTypeGroup)
                        moveTypes.Add((moveType, i));

            int moveKind = moveTypes.Count;
            int collKind = collisions.Count;
            var moveTypeIndexes = new Dictionary<MoveFunc, PathFindMoveTypeInfo>(moveKind);
            var collisionsIndexes = new Dictionary<CollSize, int>(collKind);
            int[][,] moveableNodes = new int[groupKind][,];
            CollSize[][,] passes = new CollSize[moveKind][,];
            var moveCollisions = new PathFindMtCs[moveKind, collKind];

            for (int i = 0; i < moveKind; ++i)
                if (moveTypes[i] is var moveType)
                    moveTypeIndexes.Add(moveType.Type, new PathFindMoveTypeInfo(moveType.GroypIndex, i));
            for (int i = 0; i < collKind; ++i)
                collisionsIndexes.Add(collisions[i], i);
            for (int i = 0; i < groupKind; ++i)
                moveableNodes[i] = new int[size.X, size.Y];
            for (int i = 0; i < moveKind; ++i)
                passes[i] = new CollSize[size.X, size.Y];
            for (int i = 0; i < moveKind; ++i)
            for (int j = 0; j < collKind; ++j)
                moveCollisions[i, j] = new PathFindMtCs(size);

            _size = size;
            _maxColl = PathFindExt.GetColl(getters.Collision, maxColl);
            _collisions = collisions;
            _moveTypeIndexes = moveTypeIndexes;
            _collisionIndexes = collisionsIndexes;
            _moveableNodes = moveableNodes;
            _passes = passes;
            _moveCollisions = moveCollisions;
            _collisionGetter = getters.Collision;
            _objectPoolGetter = getters.ObjectPool;
        }
        private void InitNode(Ground[,] terrains)
        {
            int width = terrains.GetLength(0);
            int height = terrains.GetLength(1);
            Ground[,] configs = new Ground[width, height];
            var statics = new PathFindObstacle[width, height];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Ground groupType = terrains[i, j];
                    configs[i, j] = groupType;
                    statics[i, j] = new PathFindObstacle(groupType, PathFindExt.EmptyIndex);
                }
            }

            _terrainNodes = configs;
            _obstacleNodes = statics;
        }

        private void InitArea(Vector2DInt16 size, Dictionary<MoveFunc, PathFindMoveTypeInfo> moveTypeIndexes, Dictionary<CollSize, int> collIndexes)
        {
            var tasks = new List<Task>(moveTypeIndexes.Count * collIndexes.Count);

            foreach (var (_, moveTypeInfo) in moveTypeIndexes)
            {
                int moveTypeIndex = moveTypeInfo.TypeIndex;
                foreach ((CollSize coll, int collIndex) in collIndexes)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        ref var moveColl = ref _moveCollisions[moveTypeIndex, collIndex];
                        var points = new Stack<Vector2DInt16>();
                        var areaProcessor = new AreaProcessor(this, moveTypeIndex, coll, collIndex);
                        for (int i = 0; i < size.X; ++i)
                        for (int j = 0; j < size.Y; ++j)
                            if (areaProcessor.DFS(true, moveColl.AreaIdAllocator, i, j, points))
                                ++moveColl.AreaIdAllocator;
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
        private void InitJumpPoint(Vector2DInt16 size, Dictionary<MoveFunc, PathFindMoveTypeInfo> moveTypeIndexes, Dictionary<CollSize, int> collIndexes)
        {
            int sizeCount = size.X * size.Y * PathFindExt.DirIndexCount;
            var range = new PathFindPeek(size.X - 1, size.Y - 1);
            var tasks = new List<Task>(moveTypeIndexes.Count * collIndexes.Count);

            foreach ((MoveFunc _, var moveTypeInfo) in moveTypeIndexes)
            {
                int moveTypeIndex = moveTypeInfo.TypeIndex;
                foreach ((CollSize coll, int collIndex) in collIndexes)
                {
                    unsafe
                    {
                        ref var moveColl = ref _moveCollisions[moveTypeIndex, collIndex];
                        fixed (short* jpPtr = moveColl.NextJPs)
                            new Span<short>(jpPtr, sizeCount).Fill(PathFindExt.InvalidDistance);
                    }

                    tasks.Add(new JumpPointProcessor(this, moveTypeIndex, coll, collIndex, range).AsyncBuild());
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
        private void InitPass(Vector2DInt16 size, Dictionary<MoveFunc, PathFindMoveTypeInfo> moveTypeIndexes)
        {
            var range = new PathFindPeek(size.X - 1, size.Y - 1);
            var tasks = new List<Task>(moveTypeIndexes.Count);

            foreach ((MoveFunc moveType, var moveTypeInfo) in moveTypeIndexes)
                tasks.Add(Task.Run(new PassProcessor(this, moveType, moveTypeInfo.TypeIndex, range).Build));

            Task.WaitAll(tasks.ToArray());
        }
        #endregion

        #region 寻路
        /// <summary>
        /// 长距离寻路（JPS+算法）
        /// </summary>
        public PathFindResult GetLongPath(in PathFindInput input, ref PathFindOutput output)
        {
            PathFindDiagnosis.RemoveProcess(JPSPlusProcessor.FindFunc, input.Index);

            if (!ArriveArea(input.MoveType, input.Coll, input.Point))
            {
                PathFindDiagnosis.SetPath(JPSPlusProcessor.FindFunc, input.Index, default, input.Point);
                PathFindDiagnosis.Log(output.Path, input.Index, input.Point);
                return PathFindResult.CantArrive;
            }

            if (!input.Point.NeedFind())
            {
                input.Point.GetPath(output.Path);
                PathFindDiagnosis.SetPath(JPSPlusProcessor.FindFunc, input.Index, output.Path, input.Point);
                PathFindDiagnosis.Log(output.Path, input.Index, input.Point);
                return PathFindResult.Success;
            }

            var processor = new JPSPlusProcessor(this, in input);
            var result = processor.GetPath(ref output);
            PathFindDiagnosis.Log(output.Path, input.Index, input.Point);
            PathFindDiagnosis.SetPath(JPSPlusProcessor.FindFunc, input.Index, output.Path, input.Point);
            return result;
        }

        /// <summary>
        /// 短距离寻路（A*算法）
        /// </summary>
        public PathFindResult GetShortPath(in PathFindInput input, ref PathFindOutput output)
        {
            PathFindDiagnosis.RemoveProcess(AStarProcessor.FindFunc, input.Index);

            if (!input.Point.NeedFind())
            {
                input.Point.GetPath(output.Path);
                PathFindDiagnosis.SetPath(AStarProcessor.FindFunc, input.Index, output.Path, input.Point);
                PathFindDiagnosis.Log(output.Path, input.Index, input.Point);
                return PathFindResult.Success;
            }

            var processor = new AStarProcessor(this, in input);
            var result = processor.GetPath(ref output);
            PathFindDiagnosis.Log(output.Path, input.Index, input.Point);
            PathFindDiagnosis.SetPath(AStarProcessor.FindFunc, input.Index, output.Path, input.Point);
            return result;
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新“不可移动对象”的网格占用
        /// </summary>
        public void SetObstacle(int index, Dictionary<Vector2DInt16, Ground> obstacles)
        {
            foreach ((var point, Ground groupType) in obstacles)
            {
                ref var obstacleNode = ref _obstacleNodes[point.X, point.Y];
                if (obstacleNode.Index == PathFindExt.EmptyIndex)
                {
                    obstacleNode.GroupType = groupType;
                    obstacleNode.Index = index;
                }
                else
                {
                    LogRelay.Error($"[PathFind] SetObstacle fail, Point:{point}, CacheIndex:{obstacleNode.Index}, NewIndex:{index}, GroupType:{groupType}");
                }
            }

            new ObstacleProcessor(this).Build(obstacles, _allowBuildCache);
        }
        /// <summary>
        /// 重置“不可移动对象”的网格占用
        /// </summary>
        public void ResetObstacle(int index, Dictionary<Vector2DInt16, Ground> obstacles)
        {
            foreach ((var point, Ground groupType) in obstacles)
            {
                ref var obstacleNode = ref _obstacleNodes[point.X, point.Y];
                if (obstacleNode.Index == index)
                {
                    obstacleNode.GroupType = _terrainNodes[point.X, point.Y];
                    obstacleNode.Index = PathFindExt.EmptyIndex;
                }
                else
                {
                    LogRelay.Error($"[PathFind] ResetObstacle fail, Point:{point}, CacheIndex:{obstacleNode.Index}, ResetIndex:{index}, GroupType:{groupType}");
                }
            }

            new ObstacleProcessor(this).Build(obstacles, _allowBuildCache);
        }

        /// <summary>
        /// 更新“可移动对象”的网格占用
        /// </summary>
        public void SetMoveable(int index, MoveFunc moveType, PathFindPeek coll)
        {
            var moveTypeInfo = _moveTypeIndexes[moveType];
            int[,] moveableNodes = _moveableNodes[moveTypeInfo.GroupIndex];
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            {
                for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                {
                    ref int moveableNode = ref moveableNodes[i, j];
                    Assert.Equal<InvalidOperationException, DiagnosisArgs<int, int, int, int, MoveFunc>, int>(moveableNode, PathFindExt.EmptyIndex, nameof(moveableNode), "SetMoveable fail, X:{0}, Y:{1}, Index:{2}, MoveableNode:{3}, MoveType:{4}", new DiagnosisArgs<int, int, int, int, MoveFunc>(i, j, index, moveableNode, moveType));
                    moveableNode = index;
                }
            }
        }
        /// <summary>
        /// 重置“可移动对象”的网格占用
        /// </summary>
        public void ResetMoveable(int index, MoveFunc moveType, PathFindPeek coll)
        {
            var moveTypeInfo = _moveTypeIndexes[moveType];
            int[,] moveableNodes = _moveableNodes[moveTypeInfo.GroupIndex];
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            {
                for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                {
                    ref int moveableNode = ref moveableNodes[i, j];
                    Assert.Equal<InvalidOperationException, DiagnosisArgs<int, int, int, int, MoveFunc>, int>(moveableNode, index, nameof(moveableNode), "SetMoveable fail, X:{0}, Y:{1}, Index:{2}, MoveableNode:{3}, MoveType:{4}", new DiagnosisArgs<int, int, int, int, MoveFunc>(i, j, index, moveableNode, moveType));
                    moveableNode = PathFindExt.EmptyIndex;
                }
            }
        }

        /// <summary>
        /// 添加传送门
        /// </summary>
        public void AddPortal(int index, in PathFindPoint point)
        {
            var start = point.Start;
            _portals.Add(start, new PathFindPortal(index, point));
            foreach ((MoveFunc _, var moveTypeInfo) in _moveTypeIndexes)
            foreach ((CollSize coll, int collIndex) in _collisionIndexes)
                new JumpPointProcessor(this, moveTypeInfo.TypeIndex, coll, collIndex, new PathFindPeek(start, start)).Remove().Build(start);
        }
        /// <summary>
        /// 移除传送门
        /// </summary>
        public void RemovePortal(int index)
        {
            foreach (var (start, portal) in _portals)
            {
                if (portal.Index != index)
                    continue;
                if (!_portals.Remove(start))
                    break;
                foreach ((MoveFunc _, var moveTypeInfo) in _moveTypeIndexes)
                foreach ((CollSize coll, int collIndex) in _collisionIndexes)
                    new JumpPointProcessor(this, moveTypeInfo.TypeIndex, coll, collIndex, new PathFindPeek(start, start)).Remove().Build(start);
                return;
            }

            LogRelay.Error($"[PathFind] isn't exist, Index:{index}.");
        }
        #endregion

        #region 格子是否可站立
        /// <summary>
        /// 可以放在格子里（检测障碍物）
        /// </summary>
        public bool CanStand(PathFindPeek coll) => CheckStand(coll);
        /// <summary>
        /// 可以放在格子里（检测障碍物）
        /// </summary>
        public bool CanStand(PathFindPeek coll, int ignoreIndex)
        {
            if (ignoreIndex == PathFindExt.EmptyIndex)
                return CheckStand(coll);
            if (BoundsIsOutOf(coll))
                return false;
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                if (_obstacleNodes[i, j].Index is var index && index != PathFindExt.EmptyIndex && index != ignoreIndex)
                    return false;
            return true;
        }
        /// <summary>
        /// 可以放在格子里（检测障碍物）
        /// </summary>
        public bool CanStand(PathFindPeek coll, ICollection<int> ignoreIndexes)
        {
            if (ignoreIndexes.IsNullOrEmpty())
                return CheckStand(coll);
            if (BoundsIsOutOf(coll))
                return false;
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                if (_obstacleNodes[i, j].Index is var index && index != PathFindExt.EmptyIndex && !ignoreIndexes.Contains(index))
                    return false;
            return true;
        }
        /// <summary>
        /// 可以放在格子里（检测障碍物）
        /// </summary>
        public bool CanStand(Vector2DInt16 point, MoveFunc moveType) => ObstacleCanStand(point.X, point.Y, moveType);

        /// <summary>
        /// 可以放在格子里（检测障碍物 + 移动对象）
        /// </summary>
        public bool CanStand(Vector2DInt16 point, MoveFunc moveType, CollSize coll, bool checkMoveable = true)
        {
            var collRange = PathFindExt.GetColl(_collisionGetter, point, coll);
            if (BoundsIsOutOf(collRange))
                return false;
            var moveTypeInfo = _moveTypeIndexes[moveType];
            if (!ObstacleCanStand(_passes[moveTypeInfo.TypeIndex], point.X, point.Y, coll))
                return false;
            if (!checkMoveable)
                return true;
            return MoveableCanStand(collRange, _moveableNodes[moveTypeInfo.GroupIndex]);
        }
        /// <summary>
        /// 可以放在格子里（检测障碍物 + 移动对象）
        /// </summary>
        public bool CanStand(Vector2DInt16 point, MoveFunc moveType, CollSize coll, int ignoreIndex, bool checkMoveable = true)
        {
            var collRange = PathFindExt.GetColl(_collisionGetter, point, coll);
            if (BoundsIsOutOf(collRange))
                return false;
            var moveTypeInfo = _moveTypeIndexes[moveType];
            if (!ObstacleCanStand(_passes[moveTypeInfo.TypeIndex], point.X, point.Y, coll))
                return false;
            if (!checkMoveable)
                return true;
            int[,] moveableNodes = _moveableNodes[moveTypeInfo.GroupIndex];
            if (ignoreIndex == PathFindExt.EmptyIndex)
                return MoveableCanStand(collRange, moveableNodes);
            return MoveableCanStand(collRange, moveableNodes, ignoreIndex);
        }
        /// <summary>
        /// 可以放在格子里（检测障碍物 + 移动对象）
        /// </summary>
        public bool CanStand(Vector2DInt16 point, MoveFunc moveType, CollSize coll, ICollection<int> ignoreIndexes, bool checkMoveable = true)
        {
            var collRange = PathFindExt.GetColl(_collisionGetter, point, coll);
            if (BoundsIsOutOf(collRange))
                return false;
            var moveTypeInfo = _moveTypeIndexes[moveType];
            if (!ObstacleCanStand(_passes[moveTypeInfo.TypeIndex], point.X, point.Y, coll))
                return false;
            if (!checkMoveable)
                return true;
            int[,] moveableNodes = _moveableNodes[moveTypeInfo.GroupIndex];
            if (ignoreIndexes.IsNullOrEmpty())
                return MoveableCanStand(collRange, moveableNodes);
            return MoveableCanStand(collRange, moveableNodes, ignoreIndexes);
        }
        #endregion

        #region 检测接口
        /// <summary>
        /// 检测区域id是否一致
        /// </summary>
        public bool CheckAreaIsSame(Vector2DInt16 lhs, Vector2DInt16 rhs, MoveFunc moveType, CollSize coll)
        {
            var moveTypeInfo = _moveTypeIndexes[moveType];
            int collIndex = _collisionIndexes[coll];
            ref var moveColl = ref _moveCollisions[moveTypeInfo.TypeIndex, collIndex];
            return moveColl.AreaIds[lhs.X, lhs.Y] == moveColl.AreaIds[rhs.X, rhs.Y];
        }

        /// <summary>
        /// 检测两个点是否可以直线到达
        /// </summary>
        public bool CheckStraightArrive(Vector2DInt16 lhs, Vector2DInt16 rhs, MoveFunc moveType, CollSize coll)
        {
            var moveTypeInfo = _moveTypeIndexes[moveType];
            CollSize[,] passes = _passes[moveTypeInfo.TypeIndex];
            for (var next = lhs; next != rhs;)
            {
                var dir = rhs - next;
                next += dir.Sign();
                if (!ObstacleCanStand(passes, next.X, next.Y, coll))
                    return false;
            }

            return true;
        }
        #endregion

        #region 工具方法
        private bool ArriveArea(MoveFunc moveType, CollSize coll, PathFindPoint point)
        {
            var moveTypeInfo = _moveTypeIndexes[moveType];
            ref var moveColl = ref _moveCollisions[moveTypeInfo.TypeIndex, _collisionIndexes[coll]];
            short[,] areas = moveColl.AreaIds;
            short startAreaId = areas[point.Start.X, point.Start.Y];
            short endAreaId = areas[point.End.X, point.End.Y];
            if (startAreaId == endAreaId)
                return true;
            if (_portals.IsEmpty())
                return false;

            int tpCount = 0;
            var tpAreas = (Span<Vector2DInt16>)stackalloc Vector2DInt16[_portals.Count];
            foreach (var (start, portal) in _portals)
            {
                var end = portal.Point.End;
                short psAreaId = areas[start.X, start.Y];
                short peAreaId = areas[end.X, end.Y];
                if (psAreaId == peAreaId)
                    continue;

                var tpArea = new Vector2DInt16(psAreaId, peAreaId);
                if (tpAreas.IndexOf(tpArea) < 0)
                    tpAreas[tpCount++] = tpArea;
            }

            if (tpCount == 0)
                return false;
            return ArriveArea(startAreaId, endAreaId, tpAreas[..tpCount]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ArriveArea(short startAreaId, short endAreaId, ReadOnlySpan<Vector2DInt16> tpAreas)
        {
            foreach (var tpArea in tpAreas)
            {
                if (startAreaId != tpArea.X)
                    continue;
                if (endAreaId == tpArea.Y)
                    return true;
                if (ArriveArea(tpArea.Y, endAreaId, tpAreas))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BoundsIsOutOf(int x, int y) => x < 0 || y < 0 || x >= _size.X || y >= _size.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BoundsIsOutOf(int x, int y, PathFindPeek range) => x < range.Min.X || y < range.Min.Y || x > range.Max.X || y > range.Max.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BoundsIsOutOf(PathFindPeek coll) => coll.Min.X < 0 || coll.Min.Y < 0 || coll.Max.X >= _size.X || coll.Max.Y >= _size.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ObstacleCanStand(int x, int y, MoveFunc moveType) => CheckStand(_obstacleNodes[x, y].GroupType, moveType);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ObstacleCanStand(CollSize[,] passes, int x, int y, CollSize coll) => passes[x, y] >= coll;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ObstacleCanStand(CollSize[,] passes, int x, int y, MoveFunc moveType, CollSize coll, int ignoreIndex)
        {
            var obstacleNode = _obstacleNodes[x, y];
            if (ignoreIndex == PathFindExt.EmptyIndex && !CheckStand(obstacleNode.GroupType, moveType))
                return false;
            if (ignoreIndex != PathFindExt.EmptyIndex && obstacleNode.Index != ignoreIndex && !CheckStand(obstacleNode.GroupType, moveType))
                return false;
            return passes[x, y] >= coll;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MoveableCanStand(PathFindPeek coll, int[,] moveableNodes)
        {
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                if (moveableNodes[i, j] != PathFindExt.EmptyIndex)
                    return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MoveableCanStand(PathFindPeek coll, int[,] moveableNodes, int ignoreIndex)
        {
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                if (moveableNodes[i, j] is var moveableNode && moveableNode != PathFindExt.EmptyIndex && moveableNode != ignoreIndex)
                    return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MoveableCanStand(PathFindPeek coll, int[,] moveableNodes, ICollection<int> ignoreIndexes)
        {
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                if (moveableNodes[i, j] is var moveableNode && moveableNode != PathFindExt.EmptyIndex && !ignoreIndexes.Contains(moveableNode))
                    return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckStand(Ground groupType, MoveFunc moveType) => (groupType & moveType) == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckStand(PathFindPeek coll)
        {
            if (BoundsIsOutOf(coll))
                return false;
            for (int i = coll.Min.X; i <= coll.Max.X; ++i)
            for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                if (_obstacleNodes[i, j].Index != PathFindExt.EmptyIndex)
                    return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PathFindPeek CountRange(Dictionary<Vector2DInt16, Ground> nodes, MoveFunc moveType)
        {
            short xMin = short.MaxValue;
            short yMin = short.MaxValue;
            short xMax = short.MinValue;
            short yMax = short.MinValue;

            foreach ((var point, Ground groupType) in nodes)
            {
                if (CheckStand(groupType, moveType))
                    continue;
                xMin = Math.Min(xMin, point.X);
                yMin = Math.Min(yMin, point.Y);
                xMax = Math.Max(xMax, point.X);
                yMax = Math.Max(yMax, point.Y);
            }

            if (xMin > xMax)
                return PathFindPeek.Invalid;
            if (yMin > yMax)
                return PathFindPeek.Invalid;
            return new PathFindPeek(xMin, yMin, xMax, yMax);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PathFindPeek CountRange(PathFindPeek range, PathFindPeek change)
        {
            var newRange = range.CountRange(change);
            return newRange.Limit(new PathFindPeek(_size - Vector2DInt16.One));
        }
        #endregion
    }
}