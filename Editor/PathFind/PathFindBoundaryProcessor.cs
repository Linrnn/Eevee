#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.Fixed;
using Eevee.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    internal readonly struct PathFindBoundaryProcessor<TKey, TValue> where TKey : IEquatable<TKey>
    {
        internal readonly struct Boundary
        {
            private readonly List<Vector3Int> _sides;
            private readonly HashSet<Vector2Int> _grids;

            internal bool IsEmpty() => _sides.IsEmpty();
            internal Vector2 Center()
            {
                var sum = default(Vector2);
                foreach (var side in _sides)
                    sum += (Vector2Int)side;
                return sum / _sides.Count;
            }
            internal List<Vector3Int> Sides() => _sides;
            internal HashSet<Vector2Int> Girds()
            {
                _grids.Clear();
                foreach (var side in _sides)
                    _grids.Add((Vector2Int)side);
                return _grids;
            }

            internal Boundary(int x, int y, int dirIndex, HashSet<Vector2Int> grids)
            {
                _sides = new List<Vector3Int>();
                _grids = grids;
                Add(x, y, dirIndex);
            }
            internal void Add(int x, int y, int dirIndex) => _sides.Add(new Vector3Int(x, y, dirIndex));
            internal void Clean() => _sides.Clear();
        }

        private readonly Dictionary<TKey, Boundary> _elements; // key:T, value:(x:x, y:y, z:dirIndex)
        private readonly HashSet<Vector2Int> _grids;
        private readonly List<TKey> _empty;
        private readonly Func<TValue, bool> _checker;
        private readonly Func<TValue, TKey> _getter;

        internal PathFindBoundaryProcessor(Func<TValue, bool> check, Func<TValue, TKey> getter)
        {
            _elements = new Dictionary<TKey, Boundary>();
            _grids = new HashSet<Vector2Int>();
            _empty = new List<TKey>();
            _checker = check;
            _getter = getter;
        }
        internal void Build(TValue[,] elements, Vector2DInt16 size)
        {
            foreach (var (_, sides) in _elements)
                sides.Clean();
            if (elements is null)
                return;

            for (int i = 0; i < size.X; ++i)
            for (int j = 0; j < size.Y; ++j)
                if (elements[i, j] is { } element && _getter(element) is { } equatable && _checker(element))
                    for (int k = 0; k < PathFindExt.DirIndexCount; ++k)
                        if (!(PathFindExt.StraightDirections[k] is var dir))
                            continue;
                        else if (i + dir.X is var px && (px < 0 || px >= size.X))
                            continue;
                        else if (j + dir.Y is var py && (py < 0 || py >= size.Y))
                            continue;
                        else if (elements[px, py] is { } pe && _checker(pe) && equatable.Equals(_getter(pe)))
                            continue;
                        else if (_elements.TryGetValue(equatable, out var oldBoundary))
                            oldBoundary.Add(i, j, k);
                        else
                            _elements.Add(equatable, new Boundary(i, j, k, _grids));

            _empty.Clear();
            foreach (var (element, sides) in _elements)
                if (sides.IsEmpty())
                    _empty.Add(element);
            foreach (var element in _empty)
                _elements.Remove(element);
        }
        public Dictionary<TKey, Boundary>.Enumerator GetEnumerator() => _elements.GetEnumerator();
    }
}
#endif