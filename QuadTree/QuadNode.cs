using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树节点
    /// </summary>
    public sealed class QuadNode
    {
        #region 迭代器
        internal readonly struct Iterator
        {
            private readonly IReadOnlyList<QuadNode> _enumerator;
            private readonly bool _checkNull;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Iterator(IReadOnlyList<QuadNode> enumerator, bool checkNull)
            {
                _enumerator = enumerator;
                _checkNull = checkNull;
            }
            public Enumerator GetEnumerator() => new(_enumerator, _checkNull);
        }

        internal struct Enumerator : IEnumerator<QuadNode>
        {
            private readonly IReadOnlyList<QuadNode> _enumerator;
            private bool _checkNull;
            private int _index;
            private QuadNode _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(IReadOnlyList<QuadNode> enumerator, bool checkNull)
            {
                _enumerator = enumerator;
                _checkNull = checkNull;
                _index = 0;
                _current = null;
            }

            #region IEnumerator`1
            public readonly QuadNode Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
            #endregion

            #region IEnumerator
            readonly object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_enumerator is null)
                {
                    _index = 1;
                    _current = null;
                    return false;
                }

                int count = _enumerator.Count;
                if (_index >= count)
                {
                    _index = count + 1;
                    _current = null;
                    return false;
                }

                return _checkNull ? MoveNextCanNull(count) : MoveNextNoNull();
            }
            public void Reset() => Dispose();
            #endregion

            #region IDisposable
            public void Dispose()
            {
                _checkNull = false;
                _index = 0;
                _current = null;
            }
            #endregion

            #region Helper
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool MoveNextCanNull(int count)
            {
                for (int i = _index; i < count; ++i)
                {
                    var current = _enumerator[i];
                    if (current is null)
                        continue;

                    _index = i + 1;
                    _current = current;
                    return true;
                }

                _index = count + 1;
                _current = null;
                return false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool MoveNextNoNull()
            {
                var current = _enumerator[_index];
                _current = current;
                ++_index;
                return true;
            }
            #endregion
        }
        #endregion

        #region 字段
        internal AABB2DInt Boundary; // 边界
        internal AABB2DInt LooseBoundary; // 松散四叉树的节点边界
        internal QuadIndex Index = QuadIndex.Invalid; // 节点所在层级的二维坐标
        internal int ChildId = -1; // 相对于父节点的编号

        internal QuadNode Parent; // 父节点
        internal readonly QuadNode[] Children = new QuadNode[QuadExt.ChildCount]; // 子节点
        internal readonly WeakOrderList<QuadElement> Elements = new(); // 存储元素的数组（禁止外部直接修改）
        internal int SumCount; // 当前节点及所有子节点存的数量（禁止外部直接修改）

        private bool _valid;
        #endregion

        #region 方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnAlloc(in AABB2DInt boundary, in AABB2DInt looseBoundary, int depth, int x, int y, int childId, QuadNode parent)
        {
            Boundary = boundary;
            LooseBoundary = looseBoundary;
            Index = new QuadIndex(depth, x, y);
            ChildId = childId;
            Parent = parent;
            _valid = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnRelease()
        {
            if (!_valid)
            {
                LogRelay.Warn($"[Quad] Index:{Index}, ChildId:{ChildId} _valid is false.");
                return;
            }

            Boundary = default;
            LooseBoundary = default;
            Index = QuadIndex.Invalid;
            ChildId = -1;
            Parent = null;
            Children.Clean();
            Elements.Clear();
            SumCount = 0;
            _valid = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(in QuadElement element)
        {
            Elements.Add(element);
            CountSumAdd();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Remove(in QuadElement element)
        {
            if (!Elements.Remove(element))
                return false;

            CountSumSub();
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveAt(int index)
        {
            Elements.RemoveAt(index);
            CountSumSub();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Update(in QuadElement perElement, in QuadElement tarElement)
        {
            for (int count = Elements.Count, i = 0; i < count; ++i)
            {
                if (perElement != Elements[i])
                    continue;

                Elements[i] = tarElement;
                return true;
            }

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Update(int index, in QuadElement element) => Elements[index] = element;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CountSumAdd()
        {
            for (var node = this; node is not null; node = node.Parent)
                ++node.SumCount;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CountSumSub()
        {
            for (var node = this; node is not null; node = node.Parent)
                --node.SumCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsEmpty() => SumCount == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int IndexOf(in QuadElement element) => Elements.IndexOf(element);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AABB2DInt CountChildBoundary(int childId) => childId switch // 计算子包围盒
        {
            0 => Boundary.LeftTopAABB(),
            1 => Boundary.RightTopAABB(),
            2 => Boundary.LeftBottomAABB(),
            3 => Boundary.RightBottomAABB(),
            _ => throw new IndexOutOfRangeException($"ChildId:{childId}，越界"),
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Iterator AsIterator(bool checkNull = false) => new(Children, checkNull);
        #endregion
    }
}