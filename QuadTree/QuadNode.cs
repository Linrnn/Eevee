using Eevee.Collection;
using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树节点
    /// </summary>
    public sealed class QuadNode
    {
        #region 字段
        public readonly AABB2DInt Boundary; // 边界
        public readonly AABB2DInt LooseBoundary; // 松散四叉树的节点边界
        public readonly int Depth; // 所在层的深度
        public readonly int ChildId; // 相对于父节点的编号
        public readonly int IdxX; // 所在层级的二维坐标X
        public readonly int IdxY; // 所在层级的二维坐标Y

        public readonly QuadNode Parent; // 父节点
        public QuadNode[] Children; // 子节点
        public readonly WeakOrderList<QuadElement> Elements = new(); // 存储元素的数组（禁止外部直接修改）
        public int SumCount; // 当前节点及所有子节点存的数量（禁止外部直接修改）
        #endregion

        #region 方法
        public QuadNode(in AABB2DInt boundary, in AABB2DInt looseBoundary, int depth, int childId, int x, int y, QuadNode parent)
        {
            Boundary = boundary;
            LooseBoundary = looseBoundary;
            Depth = depth;
            ChildId = childId;
            IdxX = x;
            IdxY = y;
            Parent = parent;
        }
        public AABB2DInt CountChildBoundary(int index) => index switch // 计算子包围盒
        {
            0 => Boundary.LeftTopAABB(), // 左上
            1 => Boundary.RightTopAABB(), // 右上
            2 => Boundary.LeftBottomAABB(), // 左下
            3 => Boundary.RightBottomAABB(), // 右下
            _ => throw new IndexOutOfRangeException($"index:{index}，越界"),
        };

        public void Add(in QuadElement element)
        {
            Elements.Add(element);
            CountSum(true);
        }
        public bool Remove(in QuadElement element)
        {
            if (!Elements.Remove(element))
                return false;

            CountSum(false);
            return true;
        }
        public bool RemoveAt(int index)
        {
            if (index < 0 && index >= Elements.Count)
            {
                LogRelay.Error($"[QuadTree] index < 0 && index >= Count, index:{index}, count:{Elements.Count}");
                return false;
            }

            Elements.RemoveAt(index);
            CountSum(false);
            return true;
        }

        public bool Update(in QuadElement perElement, in QuadElement tarElement)
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
        public void Update(int index, in QuadElement element) => Elements[index] = element;

        public bool IsEmpty() => SumCount == 0;
        public int IndexOf(in QuadElement element) => Elements.IndexOf(element);

        public void Clean()
        {
            Elements.Clear();
            SumCount = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CountSum(bool addOrRemove)
        {
            if (addOrRemove)
                for (var node = this; node != null; node = node.Parent)
                    ++node.SumCount;
            else
                for (var node = this; node != null; node = node.Parent)
                    --node.SumCount;
        }
        #endregion
    }
}