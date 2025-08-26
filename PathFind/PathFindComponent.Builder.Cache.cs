using Eevee.Collection;
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
    public sealed partial class PathFindComponent
    {
        private readonly struct ObstacleProcessor
        {
            #region 字段
            private readonly PathFindComponent _component;
            private readonly IPathFindCollisionGetter _collisionGetter;
            private readonly IPathFindObjectPoolGetter _objectPoolGetter;
            #endregion

            #region 方法
            internal ObstacleProcessor(PathFindComponent component)
            {
                _component = component;
                _collisionGetter = component._collisionGetter;
                _objectPoolGetter = component._objectPoolGetter;
            }
            internal void Build(Dictionary<Vector2DInt16, Ground> obstacles, bool allowBuild)
            {
                var rangePoints = _objectPoolGetter.StackAlloc<Vector2DInt16>();

                foreach ((MoveFunc moveType, var moveTypeInfo) in _component._moveTypeIndexes)
                {
                    int moveTypeIndex = moveTypeInfo.TypeIndex;
                    var obstaclesRange = _component.CountRange(obstacles, moveType);
                    var mtRange = _component.CountRange(obstaclesRange, _component._maxColl);
                    new PassProcessor(_component, moveType, moveTypeIndex, mtRange).Build();

                    if (!allowBuild)
                        continue;

                    foreach ((CollSize coll, int collIndex) in _component._collisionIndexes)
                    {
                        ref var collMove = ref _component._moveCollisions[moveTypeIndex, collIndex];
                        var mtCsRange = _component.CountRange(obstaclesRange, PathFindExt.GetColl(_collisionGetter, coll));
                        new JumpPointProcessor(_component, moveTypeIndex, coll, collIndex, mtCsRange).Remove().Build(); // “JumpPoint”构建依赖“Pass”
                        new AreaProcessor(_component, moveTypeIndex, coll, collIndex, mtCsRange).Remove().Build(ref collMove.AreaIdAllocator, mtCsRange.Size(), rangePoints); // “AreaId”构建依赖“Pass”
                    }
                }

                _objectPoolGetter.Release(rangePoints);
            }
            #endregion
        }

        private readonly struct PassProcessor
        {
            #region 字段
            private readonly PathFindComponent _component;
            private readonly MoveFunc _moveType;
            private readonly PathFindPeek _range;
            private readonly IList<CollSize> _collisions;
            private readonly IPathFindCollisionGetter _collisionGetter;
            private readonly CollSize[,] _passes;
            #endregion

            #region 方法
            internal PassProcessor(PathFindComponent component, MoveFunc moveType, int moveTypeIndex, PathFindPeek range)
            {
                _component = component;
                _moveType = moveType;
                _range = range;
                _collisions = component._collisions;
                _collisionGetter = component._collisionGetter;
                _passes = component._passes[moveTypeIndex];
            }
            internal void Build()
            {
                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                    _passes[i, j] = CountColl(i, j);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private CollSize CountColl(int x, int y)
            {
                for (int i = _collisions.Count - 1; i >= 0; --i) // 从大到小进行遍历
                    if (_collisions[i] is var coll && CheckStand(PathFindExt.GetColl(_collisionGetter, x, y, coll)))
                        return coll;
                return _collisionGetter.GetNull();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool CheckStand(PathFindPeek coll)
            {
                if (_component.BoundsIsOutOf(coll))
                    return false;
                for (int i = coll.Min.X; i <= coll.Max.X; ++i)
                for (int j = coll.Min.Y; j <= coll.Max.Y; ++j)
                    if (!_component.ObstacleCanStand(i, j, _moveType))
                        return false;
                return true;
            }
            #endregion
        }

        private readonly struct JumpPointProcessor
        {
            #region 字段
            private readonly PathFindComponent _component;
            private readonly Vector2DInt16 _size;
            private readonly CollSize _coll;
            private readonly PathFindPeek _range;
            private readonly IPathFindCollisionGetter _collisionGetter;
            private readonly IPathFindObjectPoolGetter _objectPoolGetter;
            private readonly CollSize[,] _passes;
            private readonly Dictionary<Vector2DInt16, List<PathFindJumpPointHandle>> _jumpPoints;
            private readonly short[,,] _nextJPs;
            private readonly bool _enableThread;
            #endregion

            #region 方法
            internal JumpPointProcessor(PathFindComponent component, int moveTypeIndex, CollSize coll, int collIndex, PathFindPeek range)
            {
                ref var moveColl = ref component._moveCollisions[moveTypeIndex, collIndex];
                _component = component;
                _size = component._size;
                _coll = coll;
                _range = range;
                _collisionGetter = component._collisionGetter;
                _objectPoolGetter = component._objectPoolGetter;
                _passes = component._passes[moveTypeIndex];
                _jumpPoints = moveColl.JumpPoints;
                _nextJPs = moveColl.NextJPs;
                _enableThread = false;
            }
            private JumpPointProcessor(in JumpPointProcessor other, bool enableThread = false)
            {
                this = other;
                _enableThread = enableThread;
            }
            internal JumpPointProcessor Remove()
            {
                var jumpPoints = _jumpPoints;
                short[,,] nextJPs = _nextJPs;

                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                {
                    if (jumpPoints.Remove(new Vector2DInt16(i, j), out var handles))
                        _objectPoolGetter.Release(handles);
                    for (int k = 0; k < PathFindExt.DirIndexCount; ++k)
                        nextJPs[i, j, k] = PathFindExt.InvalidDistance;
                }

                int xMin = _range.Min.X - 1;
                int yMin = _range.Min.Y - 1;
                int xMax = _range.Max.X + 1;
                int yMax = _range.Max.Y + 1;

                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                for (int i = xMax; i < _size.X; ++i)
                    if (CleanNextJPs(i, j, PathFindExt.DirIndexLeft))
                        break;

                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                for (int i = xMin; i >= 0; --i)
                    if (CleanNextJPs(i, j, PathFindExt.DirIndexRight))
                        break;

                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = yMax; j < _size.Y; ++j)
                    if (CleanNextJPs(i, j, PathFindExt.DirIndexDown))
                        break;

                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = yMin; j >= 0; --j)
                    if (CleanNextJPs(i, j, PathFindExt.DirIndexUp))
                        break;

                return this;
            }
            internal Task AsyncBuild(Vector2DInt16? forceJumpPoint = null)
            {
                var processor = new JumpPointProcessor(in this, true);
                return Task.Run(() => processor.Build(forceJumpPoint));
            }
            internal void Build(Vector2DInt16? forceJumpPoint = null)
            {
                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                    if (_component.ObstacleCanStand(_passes, i, j, _coll))
                        foreach (var dir in PathFindExt.ObliqueDirections)
                            TryAddJumpPoints(i, j, dir.X, dir.Y);

                var rangeJumpPoints = _objectPoolGetter.MapAlloc<Vector2DInt16, int?>();
                GetJumpPoints(rangeJumpPoints);
                if (forceJumpPoint.HasValue)
                    rangeJumpPoints[forceJumpPoint.Value] = null;
                else
                    foreach (var (start, _) in _component._portals)
                        if (_range.Contain(start))
                            rangeJumpPoints[start] = null;

                foreach ((var point, int? dir) in rangeJumpPoints)
                {
                    if (dir.HasValue)
                    {
                        CountNextJPs(point, -PathFindExt.StraightDirections[dir.Value], dir.Value);
                    }
                    else
                    {
                        CountNextJPs(point, Vector2DInt16.Down, PathFindExt.DirIndexUp);
                        CountNextJPs(point, Vector2DInt16.Up, PathFindExt.DirIndexDown);
                        CountNextJPs(point, Vector2DInt16.Left, PathFindExt.DirIndexRight);
                        CountNextJPs(point, Vector2DInt16.Right, PathFindExt.DirIndexLeft);
                    }
                }

                _objectPoolGetter.Release(rangeJumpPoints);
            }

            private bool CleanNextJPs(int i, int j, int dirIndex)
            {
                var point = new Vector2DInt16(i, j);
                if (!_component.ObstacleCanStand(_passes, i, j, _coll))
                    return true;
                if (_jumpPoints.ContainsKey(point))
                    return true;
                _nextJPs[i, j, dirIndex] = PathFindExt.InvalidDistance;
                return false;
            }
            private void GetJumpPoints(IDictionary<Vector2DInt16, int?> jumpPoints)
            {
                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                    TryGetJumpPoint(jumpPoints, i, j, null);

                int xMin = _range.Min.X - 1;
                int yMin = _range.Min.Y - 1;
                int xMax = _range.Max.X + 1;
                int yMax = _range.Max.Y + 1;

                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                for (int i = xMax; i < _size.X; ++i)
                    if (TryGetJumpPoint(jumpPoints, i, j, PathFindExt.DirIndexRight))
                        break;

                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                for (int i = xMin; i >= 0; --i)
                    if (TryGetJumpPoint(jumpPoints, i, j, PathFindExt.DirIndexLeft))
                        break;

                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = yMax; j < _size.Y; ++j)
                    if (TryGetJumpPoint(jumpPoints, i, j, PathFindExt.DirIndexUp))
                        break;

                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = yMin; j >= 0; --j)
                    if (TryGetJumpPoint(jumpPoints, i, j, PathFindExt.DirIndexDown))
                        break;
            }
            private bool TryGetJumpPoint(IDictionary<Vector2DInt16, int?> jumpPoints, int x, int y, int? dirIndex)
            {
                if (_component.BoundsIsOutOf(x, y))
                    return true;
                if (!_component.ObstacleCanStand(_passes, x, y, _coll))
                    return true;
                var point = new Vector2DInt16(x, y);
                if (!_jumpPoints.ContainsKey(point))
                    return false;
                jumpPoints.Add(point, dirIndex);
                return true;
            }
            private void CountNextJPs(Vector2DInt16 point, Vector2DInt16 dir, int dirIndex)
            {
                for (int k = 0; k < PathFindExt.DirIndexCount; ++k)
                    _nextJPs[point.X, point.Y, k] = PathFindExt.JumpPointDistance;

                short distance = 0;
                for (var next = point + dir;; next += dir)
                {
                    if (_component.BoundsIsOutOf(next.X, next.Y))
                        break;
                    if (!_component.ObstacleCanStand(_passes, next.X, next.Y, _coll))
                        break;

                    if (_jumpPoints.ContainsKey(next))
                    {
                        for (int k = 0; k < PathFindExt.DirIndexCount; ++k)
                            _nextJPs[next.X, next.Y, k] = PathFindExt.JumpPointDistance;
                        break;
                    }

                    ++distance;
                    _nextJPs[next.X, next.Y, dirIndex] = distance;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void TryAddJumpPoints(int xCheck, int yCheck, int xDir, int yDir)
            {
                var check = new Vector2DInt16(xCheck, yCheck);
                var dir = new Vector2DInt16(xDir, yDir);

                var parent0 = new Vector2DInt16(xCheck - xDir, yCheck);
                if (CheckJumpPoint(parent0.X, parent0.Y, xCheck, yCheck, xDir, yDir))
                    AddJumpPoint(check, new PathFindJumpPointHandle(parent0, dir));

                var parent1 = new Vector2DInt16(xCheck, yCheck - yDir);
                if (CheckJumpPoint(parent1.X, parent1.Y, xCheck, yCheck, xDir, yDir))
                    AddJumpPoint(check, new PathFindJumpPointHandle(parent1, dir));
            }
            /// <summary>
            /// 跳点检测，target = parent + dir<br/>
            /// parent obstacle <br/>
            /// check  target <br/>
            /// obstacle是障碍物，parent前往target，最短路径必须经过check，所以check是跳点
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool CheckJumpPoint(int xParent, int yParent, int xCheck, int yCheck, int xDir, int yDir)
            {
                int xTarget = xParent + xDir;
                int yTarget = yParent + yDir;
                int xObstacle = xParent == xCheck ? xTarget : xParent;
                int yObstacle = yParent == yCheck ? yTarget : yParent;

                if (_component.BoundsIsOutOf(PathFindExt.GetColl(_collisionGetter, xObstacle, yObstacle, _coll)))
                    return false;
                if (_component.ObstacleCanStand(_passes, xObstacle, yObstacle, _coll))
                    return false;
                if (!_component.ObstacleCanStand(_passes, xParent, yParent, _coll))
                    return false;
                if (!_component.ObstacleCanStand(_passes, xTarget, yTarget, _coll))
                    return false;

                return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AddJumpPoint(Vector2DInt16 point, PathFindJumpPointHandle handle)
            {
                if (_jumpPoints.TryGetValue(point, out var oldHandles))
                {
                    oldHandles.SetAdd(handle);
                }
                else
                {
                    var newHandles = _objectPoolGetter.ListAlloc<PathFindJumpPointHandle>();
                    newHandles.Add(handle);
                    _jumpPoints.Add(point, newHandles);
                }
            }
            #endregion
        }

        private readonly struct AreaProcessor
        {
            #region 类型
            private struct StandPoint
            {
                internal readonly short AreaId;
                internal readonly int StartIndex;
                internal readonly Vector2DInt16 Start;
                internal int EndIndex;
                internal Vector2DInt16 End;
                internal int Count;

                internal StandPoint(short[,] areaIds, int startIndex, Vector2DInt16 start)
                {
                    AreaId = areaIds[start.X, start.Y];
                    StartIndex = startIndex;
                    Start = start;
                    EndIndex = -1;
                    End = default;
                    Count = 1;
                }
                internal StandPoint(short[,] areaIds, int startIndex, Vector2DInt16 start, int endIndex, Vector2DInt16 end, int count)
                {
                    AreaId = areaIds[start.X, start.Y];
                    StartIndex = startIndex;
                    Start = start;
                    EndIndex = endIndex;
                    End = end;
                    Count = count;
                }

                internal readonly short GetAreaId(short[,] areaIds) => areaIds[Start.X, Start.Y];
                internal void SetEnd(int index, Vector2DInt16 point)
                {
                    EndIndex = index;
                    End = point;
                }
                internal void AddCount() => ++Count;
            }
            #endregion

            #region 字段
            private readonly PathFindComponent _component;
            private readonly CollSize _coll;
            private readonly PathFindPeek _range;
            private readonly CollSize[,] _passes;
            private readonly short[,] _areaIds;
            #endregion

            #region 方法
            internal AreaProcessor(PathFindComponent component, int moveTypeIndex, CollSize coll, int collIndex, PathFindPeek range = default)
            {
                ref var moveColl = ref component._moveCollisions[moveTypeIndex, collIndex];
                _component = component;
                _coll = coll;
                _range = range;
                _passes = component._passes[moveTypeIndex];
                _areaIds = moveColl.AreaIds;
            }
            internal AreaProcessor Remove()
            {
                int xMin = _range.Min.X + 1;
                int yMin = _range.Min.Y + 1;
                for (int i = xMin; i < _range.Max.X; ++i)
                for (int j = yMin; j < _range.Max.Y; ++j)
                    _areaIds[i, j] = _component.ObstacleCanStand(_passes, i, j, _coll) ? PathFindExt.UnDisposed : PathFindExt.CantStand;
                return this;
            }
            internal void Build(ref short areaIdAllocator, Vector2DInt16 size, Stack<Vector2DInt16> rangePoints)
            {
                int capacity = size.X + size.Y << 1;
                var boundaryPoints = (Span<Vector2DInt16>)stackalloc Vector2DInt16[capacity];
                var standPoints = (Span<StandPoint>)stackalloc StandPoint[capacity];

                BuildBoundaryPoints(in boundaryPoints);
                var sliceStandPoints = BuildStandPoints(boundaryPoints, in standPoints);
                SetAreaId(ref areaIdAllocator, boundaryPoints, in sliceStandPoints, rangePoints);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool DFS(bool checkUnDisposed, short newAreaId, int x, int y, Stack<Vector2DInt16> points)
            {
                points.Push(new Vector2DInt16(x, y));
                bool added = false;

                while (points.TryPop(out var point))
                {
                    if (_component.BoundsIsOutOf(point.X, point.Y))
                        continue;
                    ref short areaId = ref _areaIds[point.X, point.Y];
                    if (areaId == PathFindExt.CantStand)
                        continue;
                    if (checkUnDisposed && areaId != PathFindExt.UnDisposed)
                        continue;
                    if (areaId == newAreaId)
                        continue;

                    bool stand = _component.ObstacleCanStand(_passes, point.X, point.Y, _coll);
                    areaId = stand ? newAreaId : PathFindExt.CantStand;
                    if (stand)
                        added = true;
                    else
                        continue;

                    foreach (var dir in PathFindExt.StraightDirections)
                        points.Push(point + dir);
                }

                return added;
            }

            private void BuildBoundaryPoints(in Span<Vector2DInt16> points)
            {
                // 严格按照顺序遍历
                int index = -1;
                for (int i = _range.Min.X; i < _range.Max.X; ++i)
                    points[++index] = new Vector2DInt16(i, _range.Min.Y);
                for (int j = _range.Min.Y; j < _range.Max.Y; ++j)
                    points[++index] = new Vector2DInt16(_range.Max.X, j);
                for (int i = _range.Max.X; i > _range.Min.X; --i)
                    points[++index] = new Vector2DInt16(i, _range.Max.Y);
                for (int j = _range.Max.Y; j > _range.Min.Y; --j)
                    points[++index] = new Vector2DInt16(_range.Min.X, j);
            }
            private ReadOnlySpan<StandPoint> BuildStandPoints(in ReadOnlySpan<Vector2DInt16> points, in Span<StandPoint> standPoints)
            {
                // 遍历障碍物边缘的点，如果都可以Stand：区域连通
                // 想象边缘的点是一个封闭的图案，不可Stand的点是缺口；如果有且只有一个连续缺口，区域连通
                // 其他情况：区域不一定位连通

                int standPointCount = 0;
                bool start = false;
                StandPoint standPoint = default;
                for (int count = points.Length, i = 0; i < count; ++i)
                {
                    var point = points[i];
                    bool end = false;
                    if (_component.ObstacleCanStand(_passes, point.X, point.Y, _coll))
                    {
                        if (start)
                        {
                            standPoint.AddCount();
                            if (i + 1 == count)
                            {
                                i = count;
                                end = true;
                            }
                        }
                        else
                        {
                            start = true;
                            standPoint = new StandPoint(_areaIds, i, point);
                        }
                    }
                    else if (start)
                    {
                        end = true;
                    }

                    if (end)
                    {
                        int endIndex = i - 1;
                        start = false;
                        standPoint.SetEnd(endIndex, points[endIndex]);
                        standPoints[standPointCount] = standPoint;
                        ++standPointCount;
                    }
                }

                if (standPointCount > 1)
                {
                    var first = standPoints[0];
                    var last = standPoints[standPointCount - 1];
                    if (first.Start == points[0] && last.End == points[^1])
                    {
                        --standPointCount;
                        standPoints[0] = new StandPoint(_areaIds, points.IndexOf(last.Start), last.Start, points.IndexOf(first.End), first.End, first.Count + last.Count);
                    }
                }

                var sliceStandPoints = standPoints[..standPointCount];
                for (int count = sliceStandPoints.Length, i = 0; i < count; ++i)
                {
                    ref var max = ref sliceStandPoints[i];
                    for (int j = i + 1; j < count; ++j)
                    {
                        ref var sp = ref sliceStandPoints[j];
                        if (sp.Count > max.Count)
                            (sp, max) = (max, sp);
                        else if (sp.Count == max.Count && sp.GetAreaId(_areaIds) < max.GetAreaId(_areaIds))
                            (max, sp) = (sp, max);
                    }
                }
                return sliceStandPoints;
            }
            private void SetAreaId(ref short areaIdAllocator, in ReadOnlySpan<Vector2DInt16> boundaryPoints, in ReadOnlySpan<StandPoint> standPoints, Stack<Vector2DInt16> rangePoints)
            {
                int count = standPoints.Length;
                if (count == 0)
                {
                    return;
                }

                if (count > 1)
                {
                    short maxAreaId = areaIdAllocator;
                    for (int i = 0; i < count; ++i)
                    {
                        var standPoint = standPoints[i];
                        if (standPoint.AreaId != standPoint.GetAreaId(_areaIds)) // 已经设置AreaId，跳过修改
                            continue;

                        short areaId = i == 0 ? standPoint.GetAreaId(_areaIds) : areaIdAllocator++;
                        if (standPoint.StartIndex <= standPoint.EndIndex)
                        {
                            RangeDFS(standPoint.StartIndex, standPoint.EndIndex, areaId, in boundaryPoints, rangePoints);
                        }
                        else
                        {
                            RangeDFS(0, standPoint.EndIndex, areaId, in boundaryPoints, rangePoints);
                            RangeDFS(standPoint.StartIndex, boundaryPoints.Length - 1, areaId, in boundaryPoints, rangePoints);
                        }
                    }

                    for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                    for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                        if (_areaIds[i, j] < maxAreaId && DFS(true, areaIdAllocator, i, j, rangePoints))
                            ++areaIdAllocator;
                    return;
                }

                short firstAreaId = standPoints[0].GetAreaId(_areaIds);
                for (int i = _range.Min.X; i <= _range.Max.X; ++i)
                for (int j = _range.Min.Y; j <= _range.Max.Y; ++j)
                    DFS(false, firstAreaId, i, j, rangePoints);
            }
            private void RangeDFS(int startIndex, int endIndex, short areaId, in ReadOnlySpan<Vector2DInt16> boundaryPoints, Stack<Vector2DInt16> rangePoints)
            {
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    var point = boundaryPoints[i];
                    foreach (var dir in PathFindExt.StraightDirections)
                        rangePoints.Push(point + dir);
                    DFS(false, areaId, point.X, point.Y, rangePoints);
                }
            }
            #endregion
        }
    }
}