using Eevee.Diagnosis;
using Eevee.Fixed;
using System;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 检测器
    /// </summary>
    internal interface IIntersectChecker
    {
        bool CheckNode(in AABB2DInt bounds);
        bool CheckElement(in AABB2DInt bounds);
    }

    /// <summary>
    /// 树的形状
    /// </summary>
    public enum QuadShape : byte
    {
        None,
        Circle, // 圆形
        AABB, // 轴对齐包围盒
        OBB, // 定向包围盒
        Polygon, // 任意多边形（仅包含凸多边形）
    }

    /// <summary>
    /// 配置
    /// </summary>
    public readonly struct QuadTreeConfig
    {
        public readonly int TreeId; // 节点理想的最小尺寸，必须是2的幂
        public readonly QuadShape Shape; // 是否是圆形的四叉树
        public readonly Vector2DInt Size; // 节点理想的最小尺寸，必须是2的幂

        public QuadTreeConfig(int treeId, Vector2DInt size, QuadShape shape)
        {
            Assert.True<ArgumentException, AssertArgs<int>>(Maths.IsPowerOf2(size.X), nameof(size.X), "{0} isn't power of 2", new AssertArgs<int>(size.X));
            Assert.True<ArgumentException, AssertArgs<int>>(Maths.IsPowerOf2(size.Y), nameof(size.Y), "{0} isn't power of 2", new AssertArgs<int>(size.Y));
            TreeId = treeId;
            Shape = shape;
            Size = size;
        }
    }

    /// <summary>
    /// 预处理缓存
    /// </summary>
    public readonly struct QuadPreCache
    {
        public readonly QuadElement PreEle;
        public readonly QuadElement TarEle;
        public readonly QuadNode PreNode;
        public readonly QuadNode TarNode;
        public readonly int PreIndex;
        public readonly int TreeId;

        public QuadPreCache(in QuadElement preEle, in QuadElement tarEle, QuadNode preNode, QuadNode tarNode, int preIndex, int treeId)
        {
            PreEle = preEle;
            TarEle = tarEle;
            PreNode = preNode;
            TarNode = tarNode;
            PreIndex = preIndex;
            TreeId = treeId;
        }
    }
}