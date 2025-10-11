using Eevee.Collection;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CollSize = System.SByte;

namespace Eevee.PathFind
{
    public sealed partial class PathFindComponent
    {
        /// <summary>
        /// 参考文档：https://blog.csdn.net/LIQIANGEASTSUN/article/details/118766080
        /// </summary>
        private struct JPSPlusProcessor
        {
            #region 字段
            internal const PathFindFunc FindFunc = PathFindFunc.JPSPlus;
            private readonly PathFindComponent _component;
            // 只读缓存
            private readonly PathFindInput _input;
            private readonly IPathFindObjectPoolGetter _objectPoolGetter;
            private CollSize[,] _passes;
            private Dictionary<Vector2DInt16, List<PathFindJumpPointHandle>> _jumpPoints;
            private short[,,] _nextJPs;
            private Dictionary<Vector2DInt16, PathFindPortal> _portals;
            // 变动缓存
            private PathFindOutput _output;
            private JPSPlusCache _cache;
            private int _findIdAllocator;
            #endregion

            internal JPSPlusProcessor(PathFindComponent component, in PathFindInput input)
            {
                _component = component;
                _input = input;
                _objectPoolGetter = component._getters.ObjectPool;
                _passes = default;
                _jumpPoints = default;
                _nextJPs = default;
                _portals = default;
                _output = default;
                _cache = default;
                _findIdAllocator = default;
            }
            internal PathFindResult GetPath(ref PathFindOutput output)
            {
                _output = output;

                Initialize();
                int? endFindId = CountEnd();
                if (endFindId.HasValue)
                    BuildPath(endFindId.Value);

                output = _output;
                _cache.Release(_objectPoolGetter);

                return endFindId.HasValue ? PathFindResult.Success : PathFindResult.NoEnd;
            }

            private void Initialize()
            {
                var moveTypeInfo = _component._moveTypeIndexes[_input.MoveType];
                int moveTypeIndex = moveTypeInfo.TypeIndex;
                int collIndex = _component._collisionIndexes[_input.Coll];
                ref var moveColl = ref _component._moveCollisions[moveTypeIndex, collIndex];
                _cache.Alloc(_objectPoolGetter);
                _passes = _component._passes[moveTypeIndex];
                _jumpPoints = moveColl.JumpPoints;
                _nextJPs = moveColl.NextJPs;
                _portals = _component._portals;

                // 将“Start”加入“Open”
                var start = _input.Point.Start;
                int findId = ++_findIdAllocator;
                PathFindExt.GetOctDirections(_objectPoolGetter, out var straightDirections, out var obliqueDirections);
                _cache.Opens.Add(_objectPoolGetter, start, OpenCreate(findId, 0, start, _input.Point.End, straightDirections, obliqueDirections));
                _cache.Parents.Add(findId, new JPSPlusFindIdHandle(0, 0, start));
            }
            private int? CountEnd() // 计算终点的“FindId”
            {
                ref var opens = ref _cache.Opens;
                while (true)
                {
                    opens.Get(out int openIndex, out var openHandle);
                    opens.RemoveAt(openIndex);
                    _cache.Closes.Add(openHandle.Point);

                    OctFind(new JPSPlusParam(openHandle.FindId, openHandle.Point), in openHandle);

                    openHandle.Release(_objectPoolGetter);
                    if (opens.TryGetValue(_input.Point.End, out var endOpenHandle))
                        return endOpenHandle.FindId;
                    if (!opens.IsEmpty())
                        continue;
                    return null;
                }
            }
            private readonly void BuildPath(int endFindId) // 构建寻路路径
            {
                var start = _input.Point.Start;
                var path = _output.Path;
                var portals = _output.Portals;
                if (_input.MergePath)
                {
                    for (int findId = endFindId;;)
                    {
                        var findIdHandle = _cache.Parents[findId];
                        var point = findIdHandle.Point;

                        if (PathFindExt.ValidPath(path) && PathFindExt.SameDir(point - path[0], point - path[1]))
                            path[0] = point; // 合并路径
                        else
                            path.Insert(0, point);

                        if (_portals.TryGetValue(point, out var portal))
                            portals.Add(portal.Index);
                        if (point == start)
                            break;
                        findId = findIdHandle.ParentId;
                    }
                }
                else
                {
                    for (int findId = endFindId;;)
                    {
                        var findIdHandle = _cache.Parents[findId];
                        var point = findIdHandle.Point;
                        path.Insert(0, point);

                        if (_portals.TryGetValue(point, out var portal))
                            portals.Add(portal.Index);
                        if (point == start)
                            break;
                        findId = findIdHandle.ParentId;
                    }
                }
            }

            #region 按方向搜索
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void OctFind(JPSPlusParam param, in JPSPlusOpenHandle openHandle)
            {
                var point = param.Point;
                foreach (var dir in openHandle.StraightDirections)
                    StraightFind(param, point, dir, Array.IndexOf(PathFindExt.StraightDirections, dir));

                foreach (var dir in openHandle.ObliqueDirections)
                {
                    int step = _input.Range.GetMaxStep(point, dir); // step替代越界判定
                    if (step <= 0)
                        continue;

                    int findId = ++_findIdAllocator;
                    var newParam = new JPSPlusParam(findId, point);
                    var xDir = new Vector2DInt16(dir.X, default);
                    var yDir = new Vector2DInt16(default, dir.Y);
                    int xDirIndex = xDir.X == 1 ? PathFindExt.DirIndexRight : PathFindExt.DirIndexLeft;
                    int yDirIndex = yDir.Y == 1 ? PathFindExt.DirIndexUp : PathFindExt.DirIndexDown;
                    FindIdBuild(param.FindId, findId, point);

                    for (var next = point + dir; step > 0; next += dir, --step)
                    {
                        PathFindDiagnosis.AddProcess(FindFunc, _input.Index, next);
                        if (FindPortal(param, next))
                            break;
                        StraightFind(newParam, next, xDir, xDirIndex);
                        StraightFind(newParam, next, yDir, yDirIndex);
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StraightFind(JPSPlusParam param, Vector2DInt16 point, Vector2DInt16 dir, int dirIndex)
            {
                var end = _input.Point.End;
                int findId = ++_findIdAllocator;
                var newParam = new JPSPlusParam(findId, point);
                FindIdBuild(param.FindId, findId, point);

                var peDir = (end - point).Sign();
                if (peDir == dir) // 终点在搜索方向
                {
                    for (var next = point + dir;;)
                    {
                        short nextJPs = _nextJPs[next.X, next.Y, dirIndex];
                        int move = nextJPs == PathFindExt.InvalidDistance ? 1 : nextJPs; // 往终点方向搜索时，未找到跳点，尝试移动1格
                        var tp = next + dir * move;
                        PathFindDiagnosis.AddProcess(FindFunc, _input.Index, tp);
                        if ((end - tp).Sign() != peDir && FindEnd(newParam, end)) // “tp-point”的方向与“point-end”不一致，意味着错过了终点
                            break;
                        if (_component.BoundsIsOutOf(tp.X, tp.Y, _input.Range))
                            break;
                        if (FindPortal(newParam, tp))
                            break;
                        if (FindJumpPoint(newParam, tp, dir))
                            break;
                        next = tp + dir;
                        if (_component.BoundsIsOutOf(next.X, next.Y, _input.Range))
                            break;
                    }
                }
                else
                {
                    for (var next = point + dir;;)
                    {
                        short move = _nextJPs[next.X, next.Y, dirIndex];
                        if (move == PathFindExt.InvalidDistance)
                            break;
                        var tp = next + dir * move;
                        PathFindDiagnosis.AddProcess(FindFunc, _input.Index, tp);
                        if (_component.BoundsIsOutOf(tp.X, tp.Y, _input.Range))
                            break;
                        if (FindPortal(newParam, tp))
                            break;
                        if (FindJumpPoint(newParam, tp, dir))
                            break;
                        next = tp + dir;
                        if (_component.BoundsIsOutOf(next.X, next.Y, _input.Range))
                            break;
                    }
                }
            }
            #endregion

            #region 修改“Opens”
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FindEnd(JPSPlusParam param, Vector2DInt16 point)
            {
                int findId = ++_findIdAllocator;
                var openHandle = OpenCreate(param.FindId, findId, param.Point, point, point, _input.Point.End, null, null);
                _cache.Opens.Add(_objectPoolGetter, point, openHandle);
                FindIdBuild(param.FindId, findId, point);
                return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool FindPortal(JPSPlusParam param, Vector2DInt16 point)
            {
                var end = _input.Point.End;
                if (point == end)
                    return FindEnd(param, end);
                if (!_component.ObstacleCanStand(_passes, point.X, point.Y, _input.MoveType, _input.Coll, _input.Target))
                    return true;
                ref var cache = ref _cache;
                if (cache.Closes.Contains(point))
                    return true;
                if (_portals.IsEmpty() || !_portals.TryGetValue(point, out var portal))
                    return false;

                int findId = ++_findIdAllocator;
                FindIdBuild(param.FindId, findId, point);
                PathFindExt.GetOctDirections(_objectPoolGetter, out var straightDirections, out var obliqueDirections);
                var portalEnd = portal.Point.End;
                var openHandle = OpenCreate(param.FindId, findId, param.Point, point, portalEnd, end, straightDirections, obliqueDirections);
                cache.Opens.Add(_objectPoolGetter, portalEnd, openHandle);
                return false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly bool FindJumpPoint(JPSPlusParam param, Vector2DInt16 point, Vector2DInt16 dir)
            {
                if (!_jumpPoints.TryGetValue(point, out var jumpPoints))
                    return false;

                var prevPoint = point - dir;
                var obliqueDirections = _objectPoolGetter.ListAlloc<Vector2DInt16>();
                foreach (var jumpPoint in jumpPoints)
                    if (jumpPoint.PrevPoint == prevPoint)
                        obliqueDirections.Add(jumpPoint.Direction);

                if (obliqueDirections.IsEmpty())
                {
                    _objectPoolGetter.Release(obliqueDirections);
                    return false;
                }

                var straightDirections = _objectPoolGetter.ListAlloc<Vector2DInt16>();
                foreach (var obliqueDir in obliqueDirections)
                {
                    straightDirections.SetAdd(new Vector2DInt16(obliqueDir.X, default));
                    straightDirections.SetAdd(new Vector2DInt16(default, obliqueDir.Y));
                }

                int findId = _findIdAllocator;
                var openHandle = OpenCreate(param.FindId, findId, param.Point, point, point, _input.Point.End, straightDirections, obliqueDirections);
                _cache.Opens.Add(_objectPoolGetter, point, openHandle);
                return true;
            }
            #endregion

            #region 开列表
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly JPSPlusOpenHandle OpenCreate(int findId, int g, Vector2DInt16 point, Vector2DInt16 end, List<Vector2DInt16> straightDirections, List<Vector2DInt16> obliqueDirections)
            {
                int h = PathFindExt.CountWeight(point, end);
                return new JPSPlusOpenHandle(findId, point, g, h, straightDirections, obliqueDirections);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly JPSPlusOpenHandle OpenCreate(int parentFindId, int findId, Vector2DInt16 parent, Vector2DInt16 portalStart, Vector2DInt16 portalEnd, Vector2DInt16 end, List<Vector2DInt16> straightDirections, List<Vector2DInt16> obliqueDirections)
            {
                int g = _cache.Parents[parentFindId].G + PathFindExt.CountWeight(parent, portalStart);
                int h = PathFindExt.CountWeight(portalEnd, end);
                return new JPSPlusOpenHandle(findId, portalEnd, g, h, straightDirections, obliqueDirections);
            }
            #endregion

            #region 父节点
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private readonly void FindIdBuild(int parentFindId, int findId, Vector2DInt16 point)
            {
                var parents = _cache.Parents;
                var parent = parents[parentFindId];
                int g = parent.G + PathFindExt.CountWeight(parent.Point, point);
                var handle = new JPSPlusFindIdHandle(parentFindId, g, point);
                parents.Add(findId, handle);
            }
            #endregion
        }

        #region Simple
        private readonly struct JPSPlusParam
        {
            internal readonly int FindId;
            internal readonly Vector2DInt16 Point; // 当前方向搜索的起点（不是寻路的起点）

            internal JPSPlusParam(int findId, Vector2DInt16 point)
            {
                FindId = findId;
                Point = point;
            }
        }

        private struct JPSPlusCache
        {
            internal PathFindOpenHandles<JPSPlusOpenHandle> Opens;
            internal HashSet<Vector2DInt16> Closes;
            internal Dictionary<int, JPSPlusFindIdHandle> Parents; // Key：FindId

            internal void Alloc(IPathFindObjectPoolGetter getter)
            {
                Opens.Alloc(getter);
                getter.Alloc(ref Closes);
                getter.Alloc(ref Parents);
            }
            internal void Release(IPathFindObjectPoolGetter getter)
            {
                Opens.Release(getter);
                getter.Release(ref Closes);
                getter.Release(ref Parents);
            }
        }

        private readonly struct JPSPlusFindIdHandle
        {
            internal readonly int ParentId;
            internal readonly int G;
            internal readonly Vector2DInt16 Point;

            internal JPSPlusFindIdHandle(int parentId, int g, Vector2DInt16 point)
            {
                ParentId = parentId;
                G = g;
                Point = point;
            }
        }

        private readonly struct JPSPlusOpenHandle : IPathFindOpenHandle
        {
            internal readonly Vector2DInt16 Point;
            internal readonly int FindId;
            internal readonly List<Vector2DInt16> StraightDirections; // 直方向
            internal readonly List<Vector2DInt16> ObliqueDirections; // 斜方向
            private readonly PathFindWeight _weight;

            public int G => _weight.G;
            public int H => _weight.H;
            public int F => _weight.F;
            public Vector2DInt16 Current => Point;

            internal JPSPlusOpenHandle(int findId, Vector2DInt16 point, int g, int h, List<Vector2DInt16> straightDirections, List<Vector2DInt16> obliqueDirections)
            {
                FindId = findId;
                Point = point;
                StraightDirections = straightDirections;
                ObliqueDirections = obliqueDirections;
                _weight = new PathFindWeight(g, h);
                // 加大“h”权重，减少“opens”数量，但不是最优路径
                //_weight = new PathFindWeight(g, h, g * 8 + h * 9);
            }
            public void Release(IPathFindObjectPoolGetter getter)
            {
                if (StraightDirections is { } straightDirections)
                    getter.Release(straightDirections);
                if (ObliqueDirections is { } obliqueDirections)
                    getter.Release(obliqueDirections);
            }
        }
        #endregion
    }
}