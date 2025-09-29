using Eevee.Collection;
using Eevee.Define;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CollSize = System.SByte;
using Ground = System.Byte;
using MoveFunc = System.Byte;

namespace Eevee.PathFind
{
    #region Enum/Getter
    /// <summary>
    /// 寻路结果类型
    /// </summary>
    public enum PathFindResult
    {
        Success, // 正常寻路
        CantArrive, // Area不可到达
        InvalidEnd, // 无效的终点
    }

    public readonly struct PathFindGetters
    {
        internal readonly IPathFindTerrainGetter Terrain;
        internal readonly IPathFindCollisionGetter Collision;
        internal readonly IPathFindObjectPoolGetter ObjectPool;

        public PathFindGetters(IPathFindTerrainGetter terrain, IPathFindCollisionGetter collision, IPathFindObjectPoolGetter objectPool)
        {
            Terrain = terrain;
            Collision = collision;
            ObjectPool = objectPool;
        }
    }

    public interface IPathFindTerrainGetter
    {
        int Width { get; }
        int Height { get; }
        Ground Get(int x, int y);
    }

    public interface IPathFindCollisionGetter
    {
        CollSize GetNull();
        CollSize GetMax(IReadOnlyList<CollSize> collisions);
        PathFindPeek Get(CollSize coll);
        PathFindPeek Get(int x, int y, CollSize coll);
    }

    public interface IPathFindObjectPoolGetter
    {
        List<T> AllocList<T>(bool needLock);
        void ReleaseList<T>(List<T> collection);

        Stack<T> AllocStack<T>();
        void ReleaseStack<T>(Stack<T> collection);

        HashSet<T> AllocSet<T>();
        void ReleaseSet<T>(HashSet<T> collection);

        Dictionary<TKey, TValue> AllocMap<TKey, TValue>(bool needLock);
        void ReleaseMap<TKey, TValue>(Dictionary<TKey, TValue> collection, bool needLock);
    }
    #endregion

    #region Basic
    internal struct PathFindObstacle
    {
        internal Ground GroupType; // 占用后的地表类型，即允许的移动类型
        internal int Index; // 当前格子占用的对象index

        internal PathFindObstacle(Ground groupType)
        {
            GroupType = groupType;
            Index = PathFindExt.EmptyIndex;
        }
    }

    /// <summary>
    /// 移动类型与碰撞尺寸<br/>
    /// Mt：MoveType / 移动类型<br/>
    /// Cs：CollSize / 碰撞尺寸与移动类型
    /// </summary>
    internal struct PathFindMtCs
    {
        internal readonly Dictionary<Vector2DInt16, List<PathFindJumpPointHandle>> JumpPoints; // Key：跳点
        internal readonly short[,,] NextJPs; // 距离下一个跳点的距离；第1层索引：x坐标，第2层索引：y坐标，第3层索引：方向索引（顺序按照“PathFindExt.StraightDirections”）
        internal readonly short[,] AreaIds; // 区域编号，第1层索引：x坐标，第2层索引：y坐标
        internal readonly Dictionary<short, uint> AreaCounts; // 区域编号，Key：区域编号，Value：区域数量
        internal short AreaIdAllocator;

        internal PathFindMtCs(Vector2DInt16 size)
        {
            JumpPoints = new Dictionary<Vector2DInt16, List<PathFindJumpPointHandle>>();
            NextJPs = new short[size.X, size.Y, PathFindExt.DirIndexCount];
            AreaIds = new short[size.X, size.Y];
            AreaCounts = new Dictionary<short, uint>();
            AreaIdAllocator = 1;
        }
    }

    internal readonly struct PathFindMoveTypeInfo
    {
        internal readonly int GroupIndex;
        internal readonly int TypeIndex;

        internal PathFindMoveTypeInfo(int groupIndex, int typeIndex)
        {
            GroupIndex = groupIndex;
            TypeIndex = typeIndex;
        }
    }

    internal readonly struct PathFindPortal
    {
        internal readonly int Index; // 传送门对象index
        internal readonly PathFindPoint Point; // 传送门：起点/终点

        internal PathFindPortal(int index, PathFindPoint point)
        {
            Index = index;
            Point = point;
        }
    }
    #endregion

    #region Input/Output
    public readonly struct PathFindInput
    {
        public readonly int Index; // 寻路对象index
        public readonly int Target; // 寻路目标index
        public readonly bool MergePath; // 合并路径
        public readonly MoveFunc MoveType; // 移动方式
        public readonly CollSize Coll; // 碰撞区域
        public readonly PathFindPeek Range; // 寻路区域
        public readonly PathFindPoint Point; // 起点-终点

        public PathFindInput(int index, int target, bool mergePath, MoveFunc moveType, CollSize coll, PathFindPeek range, PathFindPoint point)
        {
            Index = index;
            Target = target;
            MergePath = mergePath;
            MoveType = moveType;
            Coll = coll;
            Range = range;
            Point = point;
        }
    }

    public readonly struct PathFindOutput
    {
        public readonly IList<Vector2DInt16> Path; // 路径
        public readonly ICollection<int> Portals; // 传送门，沿途的传送门

        public PathFindOutput(IList<Vector2DInt16> path)
        {
            Path = path;
            Portals = null;
        }
        public PathFindOutput(IList<Vector2DInt16> path, ICollection<int> portals)
        {
            Path = path;
            Portals = portals;
        }
    }
    #endregion

    #region Simple
    public readonly struct PathFindPoint
    {
        public readonly Vector2DInt16 Start;
        public readonly Vector2DInt16 End;

        public PathFindPoint(Vector2DInt16 start, Vector2DInt16 end)
        {
            Start = start;
            End = end;
        }
        public PathFindPoint(int sx, int sy, int ex, int ey)
        {
            Start = new Vector2DInt16(sx, sy);
            End = new Vector2DInt16(ex, ey);
        }

        internal bool NeedFind() => Start != End;
        internal Vector2DInt16 Dir() => End - Start;
        internal void GetPath(ICollection<Vector2DInt16> path)
        {
            path.Add(Start);
            path.Add(End);
        }

        public override string ToString() => $"[Start:{Start}, End:{End}]";
    }

    public readonly struct PathFindPeek
    {
        public static readonly PathFindPeek Invalid = new(-1, -1, default); // 无效值

        public readonly Vector2DInt16 Min; // 最小点
        public readonly Vector2DInt16 Max; // 最大点

        private PathFindPeek(int w, int h, bool _) // 不安全的构建
        {
            Min = default;
            Max = new Vector2DInt16(w, h);
        }
        public PathFindPeek(int w, int h)
        {
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int>, int>(0, w, nameof(w), "0 > w:{0}，参数异常！", new DiagnosisArgs<int>(w));
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int>, int>(0, h, nameof(h), "0 > h:{0}，参数异常！", new DiagnosisArgs<int>(h));
            Min = default;
            Max = new Vector2DInt16(w, h);
        }
        public PathFindPeek(Vector2DInt16 size)
        {
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int>, int>(0, size.X, nameof(size), "0 > size.X:{0}，参数异常！", new DiagnosisArgs<int>(size.X));
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int>, int>(0, size.Y, nameof(size), "0 > size.Y:{0}，参数异常！", new DiagnosisArgs<int>(size.Y));
            Min = default;
            Max = size;
        }
        public PathFindPeek(int xMin, int yMin, int xMax, int yMax)
        {
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int, int>, int>(xMin, xMax, nameof(xMin), "xMin:{0} > xMax:{1}，参数异常！", new DiagnosisArgs<int, int>(xMin, xMax));
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int, int>, int>(yMin, yMax, nameof(yMin), "yMin:{0} > yMax:{1}，参数异常！", new DiagnosisArgs<int, int>(yMin, yMax));
            Min = new Vector2DInt16(xMin, yMin);
            Max = new Vector2DInt16(xMax, yMax);
        }
        public PathFindPeek(Vector2DInt16 min, Vector2DInt16 max)
        {
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int, int>, int>(min.X, max.X, nameof(min), "min.X:{0} > max.X:{1}，参数异常！", new DiagnosisArgs<int, int>(min.X, max.X));
            Assert.LessEqual<InvalidOperationException, DiagnosisArgs<int, int>, int>(min.Y, max.Y, nameof(max), "min.Y:{0} > max.Y:{1}，参数异常！", new DiagnosisArgs<int, int>(min.Y, max.Y));
            Min = min;
            Max = max;
        }

        public bool IsValid() => Max.X >= Min.X && Max.Y >= Min.Y;
        public Vector2DInt16 Size() => new(Max.X - Min.X, Max.Y - Min.Y);
        public bool Contain(Vector2DInt16 point) => point.X >= Min.X && point.Y >= Min.Y && point.X <= Max.X && point.Y <= Max.Y;
        public PathFindPeek CountRange(int change)
        {
            var range = new PathFindPeek(Min.X - change, Min.Y - change, Max.X + change, Max.Y + change);
            return range;
        }
        public PathFindPeek CountRange(PathFindPeek change)
        {
            var range = new PathFindPeek(Min.X - change.Max.X - 1, Min.Y - change.Max.Y - 1, Max.X - change.Min.X + 1, Max.Y - change.Min.Y + 1);
            return range;
        }
        public PathFindPeek Limit(PathFindPeek limit)
        {
            var min = new Vector2DInt16(Math.Max(Min.X, limit.Min.X), Math.Max(Min.Y, limit.Min.Y));
            var max = new Vector2DInt16(Math.Min(Max.X, limit.Max.X), Math.Min(Max.Y, limit.Max.Y));
            return new PathFindPeek(min, max);
        }
        internal int GetMaxStep(Vector2DInt16 point, Vector2DInt16 direction, bool autoOffset = true) // 获取最大步数
        {
            int step = (direction.X, direction.Y) switch
            {
                (-1, 0) => point.X - Min.X,
                (0, 1) => Max.Y - point.Y,
                (1, 0) => Max.X - point.X,
                (0, -1) => point.Y - Min.Y,
                (-1, 1) => Math.Min(point.X - Min.X, Max.Y - point.Y),
                (1, 1) => Math.Min(Max.X - point.X, Max.Y - point.Y),
                (1, -1) => Math.Min(Max.X - point.X, point.Y - Min.Y),
                (-1, -1) => Math.Min(point.X - Min.X, point.Y - Min.Y),
                _ => throw new IndexOutOfRangeException($"direction:{direction}, is illegal!"),
            };
            return autoOffset ? step - 1 : step;
        }
    }

    internal readonly struct PathFindWeight
    {
        internal readonly int G; // 起点 -> 节点，实际的权重
        internal readonly int H; // 节点 -> 终点，预估的权重
        internal readonly int F; // F = G + H

        internal PathFindWeight(int g, int h, int f)
        {
            G = g;
            H = h;
            F = f;
        }
    }
    #endregion

    #region Handle
    public struct StandPositionProcessor
    {
        private readonly Vector2DInt16 _limit;
        private Vector2DInt16 _min;
        private Vector2DInt16 _max;

        public StandPositionProcessor(Vector2DInt16 start, Vector2DInt16 limit)
        {
            _limit = limit;
            _min = start;
            _max = start;
        }
        public bool ExpandInvalid()
        {
            Expand();
            return Invalid();
        }
        public void Expand()
        {
            _min.X = (short)Math.Max(-1, _min.X - 1);
            _min.Y = (short)Math.Max(-1, _min.Y - 1);
            _max.X = (short)Math.Min(_limit.X, _max.X + 1);
            _max.Y = (short)Math.Min(_limit.Y, _max.Y + 1);
        }
        public readonly bool Invalid() => _min is { X: < 0, Y: < 0 } && _max.X >= _limit.X && _max.Y >= _limit.Y;
        public readonly void GetPoints(ICollection<Vector2DInt16> points, bool clear = true)
        {
            if (clear)
                points.Clear();

            if (_min.Y >= 0)
                for (int i = _min.X; i < _max.X; ++i)
                    points.Add(new Vector2DInt16(i, _min.Y));

            if (_min.X >= 0)
                for (int j = _min.Y; j < _max.Y; ++j)
                    points.Add(new Vector2DInt16(_min.X, j));

            if (_max.Y < _limit.Y)
                for (int i = _min.X; i < _max.X; ++i)
                    points.Add(new Vector2DInt16(i, _max.Y));

            if (_max.X < _limit.X)
                for (int j = _min.Y; j < _max.Y; ++j)
                    points.Add(new Vector2DInt16(_max.X, j));

            if (points is not ISet<Vector2DInt16>)
                points.Remove(_min); // 删除重复的点
            points.Add(_max);
        }
    }

    internal interface IPathFindOpenHandle
    {
        int G { get; }
        int H { get; }
        int F { get; }
        Vector2DInt16 Current { get; }
        void Release(IPathFindObjectPoolGetter getter);
    }

    internal struct PathFindOpenHandles<TOpenHandle> where TOpenHandle : struct, IPathFindOpenHandle
    {
        private List<TOpenHandle> _order; // 按照F值，从大到小排序的列表
        private Dictionary<Vector2DInt16, TOpenHandle> _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly void Get(out int index, out TOpenHandle handle)
        {
            CheckCount();
            index = _order.Count - 1;
            var point = _order[index];
            handle = _data[point.Current];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly bool TryGetValue(Vector2DInt16 point, out TOpenHandle handle)
        {
            CheckCount();
            return _data.TryGetValue(point, out handle);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly bool Contains(Vector2DInt16 point)
        {
            CheckCount();
            return _data.ContainsKey(point);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly bool IsEmpty()
        {
            CheckCount();
            return _order.IsEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly void Add(IPathFindObjectPoolGetter getter, Vector2DInt16 point, in TOpenHandle newHandle)
        {
            CheckCount();
            if (_data.TryGetValue(point, out var oldHandle))
            {
                if (newHandle.F < oldHandle.F)
                {
                    OrderRemove(point);
                    OrderAdd(in newHandle);
                    _data[point] = newHandle;
                    oldHandle.Release(getter);
                }
                else
                {
                    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                    newHandle.Release(getter);
                }
            }
            else
            {
                OrderAdd(in newHandle);
                _data.Add(point, newHandle);
            }
            CheckCount();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly void RemoveAt(int index)
        {
            CheckCount();
            var handle = _order[index];
            _order.RemoveAt(index);
            _data.Remove(handle.Current);
            CheckCount();
        }

        internal void Alloc(IPathFindObjectPoolGetter getter)
        {
            getter.Alloc(ref _order);
            getter.Alloc(ref _data);
        }
        internal void Release(IPathFindObjectPoolGetter getter)
        {
            foreach (var handle in _order)
                handle.Release(getter);
            getter.Release(ref _order);
            getter.Release(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void OrderAdd(in TOpenHandle handle)
        {
            int left = 0;
            int right = _order.Count;
            int hwf = handle.F;

            while (left < right)
            {
                int mid = left + right >> 1;
                int owf = _order[mid].F;
                if (owf < hwf)
                    right = mid;
                else
                    left = mid + 1;
            }

            _order.Insert(left, handle);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void OrderRemove(Vector2DInt16 point)
        {
            for (int count = _order.Count, i = 0; i < count; ++i)
            {
                if (_order[i].Current != point)
                    continue;
                _order.RemoveAt(i);
                break;
            }
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void CheckCount() => Assert.Equal<InvalidOperationException, DiagnosisArgs<int, int>, int>(_order.Count, _order.Count, nameof(_order.Count), "Count fail, _data.Count:{0} != _order.Count:{1}", new DiagnosisArgs<int, int>(_order.Count, _order.Count));
    }

    internal readonly struct PathFindJumpPointHandle : IEquatable<PathFindJumpPointHandle>
    {
        internal readonly Vector2DInt16 PrevPoint; // 跳点的上一个点
        internal readonly Vector2DInt16 Direction;

        internal PathFindJumpPointHandle(Vector2DInt16 prevPoint, Vector2DInt16 direction)
        {
            PrevPoint = prevPoint;
            Direction = direction;
        }

        public bool Equals(PathFindJumpPointHandle other) => PrevPoint.Equals(other.PrevPoint) && Direction.Equals(other.Direction);
        public override bool Equals(object obj) => obj is PathFindJumpPointHandle other && this == other;
        public override int GetHashCode() => PrevPoint.GetHashCode() ^ Direction.GetHashCode();

        public static bool operator ==(PathFindJumpPointHandle lhs, PathFindJumpPointHandle rhs) => lhs.PrevPoint == rhs.Direction && lhs.PrevPoint == rhs.Direction;
        public static bool operator !=(PathFindJumpPointHandle lhs, PathFindJumpPointHandle rhs) => lhs.PrevPoint != rhs.Direction || lhs.PrevPoint != rhs.Direction;
    }
    #endregion
}