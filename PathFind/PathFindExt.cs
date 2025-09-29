using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CollSize = System.SByte;
using Fix64 = Eevee.Fixed.Fixed64;
using Ground = System.Byte;
using MoveFunc = System.Byte;

namespace Eevee.PathFind
{
    public readonly struct PathFindExt
    {
        #region 配置
        internal const short CantStand = -1; // 无法站立的areaId
        internal const short UnDisposed = 0; // 未处理的areaId
        public const int EmptyIndex = 0; // 空对象index
        internal const short InvalidDistance = -1; // 无效距离
        internal const short JumpPointDistance = 0; // 自身是跳点
        internal const int DirIndexLeft = 0;
        internal const int DirIndexUp = 1;
        internal const int DirIndexRight = 2;
        internal const int DirIndexDown = 3;
        internal const int DirIndexCount = 4;
        internal const int StraightWeight = 10;
        internal const int ObliqueWeight = 14;

        internal static readonly Vector2DInt16[] StraightDirections = // 直方向
        {
            Vector2DInt16.Left,
            Vector2DInt16.Up,
            Vector2DInt16.Right,
            Vector2DInt16.Down,
        };
        internal static readonly Vector2DInt16[] ObliqueDirections = // 斜方向
        {
            new(-1, 1),
            Vector2DInt16.One,
            new(1, -1),
            -Vector2DInt16.One,
        };
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Stand(Ground groupType, MoveFunc moveType) => (groupType & moveType) == moveType;

        internal static bool ValidPath<T>(ICollection<T> path) => path is { Count: >= 2 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool SameDir(Vector2DInt16 lhs, Vector2DInt16 rhs)
        {
            if (lhs.X == 0 && rhs.X == 0)
                return Math.Sign(lhs.Y) == Math.Sign(rhs.Y);
            if (lhs.X == 0 || rhs.X == 0)
                return false;
            var lk = (Fix64)lhs.Y / lhs.X;
            var rk = (Fix64)rhs.Y / rhs.X;
            return lk == rk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetOctDirections(IPathFindObjectPoolGetter objectPoolGetter, out List<Vector2DInt16> straightDirections, out List<Vector2DInt16> obliqueDirections)
        {
            straightDirections = objectPoolGetter.ListAlloc<Vector2DInt16>(StraightDirections.Length);
            straightDirections.AddRange(StraightDirections);

            obliqueDirections = objectPoolGetter.ListAlloc<Vector2DInt16>(ObliqueDirections.Length);
            obliqueDirections.AddRange(ObliqueDirections);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CountWeight(Vector2DInt16 lhs, Vector2DInt16 rhs) // 计算地图权重
        {
            int xOffset = Math.Abs(lhs.X - rhs.X);
            int yOffset = Math.Abs(lhs.Y - rhs.Y);
            return Math.Min(xOffset, yOffset) * ObliqueWeight + Math.Abs(xOffset - yOffset) * StraightWeight;
            // 避免开方计算，计算结果如下图
            // start
            //       *
            //         *
            //           *
            //             * ----------------------- end
            // “*”：Math.Min(xOffset, yOffset) * √2 -> Math.Min(xOffset, yOffset) * 14
            // “-”：Math.Abs(xOffset - yOffset) -> Math.Abs(xOffset - yOffset) * 10
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PathFindPeek GetColl(IPathFindCollisionGetter getter, CollSize coll) => getter.Get(coll);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PathFindPeek GetColl(IPathFindCollisionGetter getter, int x, int y, CollSize coll) => getter.Get(x, y, coll);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PathFindPeek GetColl(IPathFindCollisionGetter getter, Vector2DInt16 point, CollSize coll) => GetColl(getter, point.X, point.Y, coll);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PathFindPeek GetColl(Vector2DInt16 point, PathFindPeek coll)
        {
            var min = new Vector2DInt16(point.X + coll.Min.X, point.Y + coll.Min.Y);
            var max = new Vector2DInt16(point.X + coll.Max.X, point.Y + coll.Max.Y);
            return new PathFindPeek(min, max);
        }
    }

    internal static class PathFindObjectPoolExt
    {
        internal static List<T> ListAlloc<T>(this IPathFindObjectPoolGetter getter) => getter.AllocList<T>(false);
        internal static List<T> ListAlloc<T>(this IPathFindObjectPoolGetter getter, bool needLock) => getter.AllocList<T>(needLock);
        internal static List<T> ListAlloc<T>(this IPathFindObjectPoolGetter getter, int capacity)
        {
            var collection = getter.AllocList<T>(false);
            if (collection.Capacity < capacity)
                collection.Capacity = capacity;
            return collection;
        }
        internal static void Alloc<T>(this IPathFindObjectPoolGetter getter, ref List<T> collection) => collection = getter.AllocList<T>(false);
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, List<T> collection)
        {
            collection.Clear();
            getter.ReleaseList(collection);
        }
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, ref List<T> collection)
        {
            collection.Clear();
            getter.ReleaseList(collection);
            collection = null;
        }

        internal static Stack<T> StackAlloc<T>(this IPathFindObjectPoolGetter getter) => getter.AllocStack<T>();
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, Stack<T> collection)
        {
            collection.Clear();
            getter.ReleaseStack(collection);
        }

        internal static void Alloc<T>(this IPathFindObjectPoolGetter getter, ref HashSet<T> collection) => collection = getter.AllocSet<T>();
        internal static void Release<T>(this IPathFindObjectPoolGetter getter, ref HashSet<T> collection)
        {
            collection.Clear();
            getter.ReleaseSet(collection);
            collection = null;
        }

        internal static Dictionary<TKey, TValue> MapAlloc<TKey, TValue>(this IPathFindObjectPoolGetter getter, bool needLock) => getter.AllocMap<TKey, TValue>(needLock);
        internal static void Alloc<TKey, TValue>(this IPathFindObjectPoolGetter getter, ref Dictionary<TKey, TValue> collection) => collection = getter.AllocMap<TKey, TValue>(false);
        internal static void Release<TKey, TValue>(this IPathFindObjectPoolGetter getter, Dictionary<TKey, TValue> collection, bool needLock)
        {
            collection.Clear();
            getter.ReleaseMap(collection, needLock);
        }
        internal static void Release<TKey, TValue>(this IPathFindObjectPoolGetter getter, ref Dictionary<TKey, TValue> collection)
        {
            collection.Clear();
            getter.ReleaseMap(collection, false);
            collection = null;
        }
    }
}