﻿using Eevee.Fixed;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CollSize = System.SByte;

namespace Eevee.PathFind
{
    public sealed partial class PathFindComponent
    {
        private struct AStarProcessor
        {
            #region 字段
            internal const PathFindFunc FindFunc = PathFindFunc.AStar;
            private readonly PathFindComponent _component;
            // 只读缓存
            private readonly PathFindInput _input;
            private readonly int _stepLimit;
            private readonly IPathFindCollisionGetter _collisionGetter;
            private readonly IPathFindObjectPoolGetter _objectPoolGetter;
            private int[,] _moveableNodes;
            private CollSize[,] _passes;
            // 变动缓存
            private PathFindOutput _output;
            private AStarPlusCache _cache;
            private int _stepCount;
            #endregion

            internal AStarProcessor(PathFindComponent component, in PathFindInput input, int stepLimit)
            {
                _component = component;
                _input = input;
                _stepLimit = stepLimit;
                _collisionGetter = component._getters.Collision;
                _objectPoolGetter = component._getters.ObjectPool;
                _moveableNodes = default;
                _passes = default;
                _output = default;
                _cache = default;
                _stepCount = default;
            }
            internal PathFindResult GetPath(ref PathFindOutput output)
            {
                _output = output;

                Initialize();
                var endPoint = CountEnd();
                if (endPoint.HasValue)
                    BuildPath(endPoint.Value);

                output = _output;
                _cache.Release(_objectPoolGetter);
                return endPoint.HasValue ? PathFindResult.Success : PathFindResult.CantArrive;
            }

            private void Initialize()
            {
                _cache.Alloc(_objectPoolGetter);
                var moveTypeInfo = _component._moveTypeIndexes[_input.MoveType];
                _moveableNodes = _component._moveableNodes[moveTypeInfo.GroupIndex];
                _passes = _component._passes[moveTypeInfo.TypeIndex];
                _cache.IgnoreIndexes.Add(_input.Index);
                if (_input.Target != PathFindExt.EmptyIndex)
                    _cache.IgnoreIndexes.Add(_input.Target);

                // 将“Start”加入“Open”
                var start = _input.Point.Start;
                _cache.Opens.Add(_objectPoolGetter, start, new AStarOpenHandle(start, 0, PathFindExt.CountWeight(start, _input.Point.End)));
            }
            private Vector2DInt16? CountEnd() // 计算终点
            {
                ref var opens = ref _cache.Opens;
                while (true)
                {
                    opens.Get(out int openIndex, out var openHandle);
                    opens.RemoveAt(openIndex);
                    _cache.Closes.Add(openHandle.Point);
                    var endOpenHandle = StraightFind(in openHandle);

                    if (endOpenHandle.HasValue)
                        return endOpenHandle.Value.Point;
                    if (_stepCount >= _stepLimit)
                        break;
                    if (opens.IsEmpty())
                        break;
                }

                Vector2DInt16? endPoint = null;
                int checkWeight = int.MaxValue;
                foreach (var (point, handle) in _cache.Parents) // 找到离终点最近的点
                {
                    int weight = handle.H;
                    if (weight >= checkWeight)
                        continue;
                    endPoint = point;
                    checkWeight = weight;
                }

                return endPoint;
            }
            private readonly void BuildPath(Vector2DInt16 end) // 构建寻路路径
            {
                var start = _input.Point.Start;
                var path = _output.Path;
                for (var point = end;;)
                {
                    if (PathFindExt.ValidPath(path) && PathFindExt.SameDir(point - path[0], point - path[1]))
                        path[0] = point; // 合并路径
                    else
                        path.Insert(0, point);

                    if (point == start)
                        break;
                    point = _cache.Parents[point].Point;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private AStarOpenHandle? StraightFind(in AStarOpenHandle openHandle)
            {
                var closes = _cache.Closes;
                ref var opens = ref _cache.Opens;
                var end = _input.Point.End;

                foreach (var dir in PathFindExt.StraightDirections)
                {
                    var next = openHandle.Point + dir;
                    if (closes.Contains(next))
                        continue;
                    if (opens.Contains(next))
                        continue;
                    if (_component.BoundsIsOutOf(next.X, next.Y, _input.Range))
                        continue;
                    if (!_component.ObstacleCanStand(_passes, next.X, next.Y, _input.MoveType, _input.Coll, _input.Target) && closes.Add(next))
                        continue;
                    var collRange = PathFindExt.GetColl(_collisionGetter, next, _input.Coll);
                    if (!_component.MoveableCanStand(collRange, _moveableNodes, _cache.IgnoreIndexes) && closes.Add(next))
                        continue;

                    int g = openHandle.G + PathFindExt.StraightWeight; // “StraightWeight”等价“CountWeight(openHandle.Point, next)”
                    int h = PathFindExt.CountWeight(next, end);
                    var nextOpenHandle = new AStarOpenHandle(next, g, h);
                    opens.Add(_objectPoolGetter, next, nextOpenHandle);
                    _cache.Parents.Add(next, openHandle);
                    ++_stepCount;
                    PathFindDiagnosis.AddProcess(FindFunc, _input.Index, next);
                    if (next == end)
                        return nextOpenHandle;
                }

                return null;
            }
        }

        #region Simple
        private readonly struct AStarOpenHandle : IPathFindOpenHandle
        {
            internal readonly Vector2DInt16 Point;
            private readonly PathFindWeight _weight;

            public int G => _weight.G;
            public int H => _weight.H;
            public int F => _weight.F;
            public Vector2DInt16 Current => Point;

            internal AStarOpenHandle(Vector2DInt16 point, int g, int h)
            {
                Point = point;
                _weight = new PathFindWeight(g, h, g + h);
            }
            public void Release(IPathFindObjectPoolGetter getter) { }
        }

        private struct AStarPlusCache
        {
            internal PathFindOpenHandles<AStarOpenHandle> Opens;
            internal HashSet<Vector2DInt16> Closes;
            internal Dictionary<Vector2DInt16, AStarOpenHandle> Parents;
            internal List<int> IgnoreIndexes;

            internal void Alloc(IPathFindObjectPoolGetter getter)
            {
                Opens.Alloc(getter);
                getter.Alloc(ref Closes);
                getter.Alloc(ref Parents);
                getter.Alloc(ref IgnoreIndexes);
            }
            internal void Release(IPathFindObjectPoolGetter getter)
            {
                Opens.Release(getter);
                getter.Release(ref Closes);
                getter.Release(ref Parents);
                getter.Release(ref IgnoreIndexes);
            }
        }
        #endregion
    }
}