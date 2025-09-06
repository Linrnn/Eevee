using Eevee.Collection;
using Eevee.Define;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eevee.PathFind
{
    public enum PathFindFunc
    {
        AStar,
        JPSPlus,
    }

    public readonly struct PathFindDiagnosis
    {
        public static bool EnableNextPoint = false;
        public static bool EnablePath = false;
        public static bool EnableProcess = false;
        public static bool EnableLog = false;
        public static int[] Indexes = Array.Empty<int>(); // 需要检测的Index，null/Empty代表不限制检测

        private static readonly Dictionary<Vector2DInt16, Vector2DInt16> _nextPoints = new();
        private static readonly Dictionary<Vector2DInt16, ICollection<Vector2DInt16>> _paths = new();
        private static readonly Dictionary<Vector2DInt16, ICollection<Vector2DInt16>> _processes = new();

        internal static Vector2DInt16? GetNextPoint(PathFindFunc func, int index)
        {
            var key = GetKey(func, index);
            return _nextPoints.TryGetValue(key, out var nextPoint) ? nextPoint : null;
        }
        [Conditional(Macro.Editor)]
        public static void SetNextPoint(PathFindFunc func, int index, Vector2DInt16 nextPoint)
        {
            if (!EnableNextPoint)
                return;
            if (!CheckTargets(index))
                return;
            var key = GetKey(func, index);
            _nextPoints[key] = nextPoint;
        }
        [Conditional(Macro.Editor)]
        public static void RemoveNextPoint(PathFindFunc func, int index)
        {
            if (!EnableNextPoint)
                return;
            if (!CheckTargets(index))
                return;
            var key = GetKey(func, index);
            _nextPoints.Remove(key);
        }

        internal static void GetPath(PathFindFunc func, int index, ICollection<Vector2DInt16> path)
        {
            var key = GetKey(func, index);
            var cachePath = _paths.GetValueOrDefault(key);
            path.UpdateLowGC(cachePath);
        }
        [Conditional(Macro.Editor)]
        internal static void SetPath(PathFindFunc func, int index, ICollection<Vector2DInt16> path, PathFindPoint point)
        {
            if (!EnablePath)
                return;
            if (!CheckTargets(index))
                return;
            var key = GetKey(func, index);
            if (PathFindExt.ValidPath(path))
            {
                if (_paths.TryGetValue(key, out var points))
                    points.UpdateLowGC(path);
                else
                    _paths.Add(key, new List<Vector2DInt16>(path));
            }
            else if (_paths.TryGetValue(key, out var points))
            {
                points.Clear();
                point.GetPath(points);
            }
            else
            {
                var newPath = new List<Vector2DInt16>();
                point.GetPath(newPath);
                _paths.Add(key, newPath);
            }
        }
        [Conditional(Macro.Editor)]
        public static void RemovePath(PathFindFunc func, int index)
        {
            if (!EnablePath)
                return;
            if (!CheckTargets(index))
                return;
            var key = GetKey(func, index);
            _paths.Remove(key);
        }

        internal static void GetProcess(PathFindFunc func, int index, ICollection<Vector2DInt16> processes)
        {
            var key = GetKey(func, index);
            var cacheProcesses = _processes.GetValueOrDefault(key);
            processes.UpdateLowGC(cacheProcesses);
        }
        [Conditional(Macro.Editor)]
        internal static void AddProcess(PathFindFunc func, int index, Vector2DInt16 point)
        {
            if (!EnableProcess)
                return;
            if (!CheckTargets(index))
                return;
            var key = GetKey(func, index);
            if (_processes.TryGetValue(key, out var points))
                points.Add(point);
            else
                _processes.Add(key, new List<Vector2DInt16>
                {
                    point,
                });
        }
        [Conditional(Macro.Editor)]
        internal static void RemoveProcess(PathFindFunc func, int index)
        {
            if (!EnableProcess)
                return;
            if (!CheckTargets(index))
                return;
            var key = GetKey(func, index);
            if (_processes.TryGetValue(key, out var points))
                points.Clear();
        }

        [Conditional(Macro.Editor)]
        internal static void Log(ICollection<Vector2DInt16> path, int index, PathFindPoint point)
        {
            if (!EnableLog)
                return;
            if (!CheckTargets(index))
                return;
            if (PathFindExt.ValidPath(path))
                LogRelay.Info($"[PathFind] Success, Index:{index}, {point}");
            else
                LogRelay.Error($"[PathFind] Fail, Index:{index}, {point}");
        }

        private static bool CheckTargets(int id)
        {
            ICollection<int> targets = Indexes;
            return targets.IsNullOrEmpty() || targets.Contains(id);
        }
        private static Vector2DInt16 GetKey(PathFindFunc func, int index) => new((int)func, index);
    }
}