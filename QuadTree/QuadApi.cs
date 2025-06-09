using Eevee.Diagnosis;
using Eevee.Fixed;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 检测器
    /// </summary>
    public interface IQuadChecker
    {
        bool CheckNode(in AABB2DInt boundary);
        bool CheckElement(in AABB2DInt boundary);
    }

    /// <summary>
    /// 四叉树的形状
    /// </summary>
    public enum QuadShape : byte
    {
        Circle, // 圆形
        AABB, // 轴对齐包围盒
        OBB, // 定向包围盒
        Polygon, // 凸多边形
    }

    /// <summary>
    /// 计算四叉树节点的方式
    /// </summary>
    public enum QuadCountNodeMode : byte
    {
        NotIntersect, // 不和四叉树边界做检测
        OnlyIntersect, // 和四叉树边界做检测，但是输入的aabb不做偏移
        IntersectOffset, // 和四叉树边界做检测，且输入的aabb做偏移
    }

    /// <summary>
    /// 四叉树配置
    /// </summary>
    public readonly struct QuadTreeConfig
    {
        public readonly int TreeId;
        public readonly QuadShape Shape; // 目前只支持“Circle/AABB”
        public readonly Vector2DInt Extents; // 节点理想的最小尺寸，必须是2的幂
        public readonly Type TreeType; // 树的类型，必须继承“QuadTreeBasic”

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private QuadTreeConfig(int treeId, QuadShape shape, Vector2DInt extents, Type treeType)
        {
            TreeId = treeId;
            Shape = shape;
            Extents = extents;
            TreeType = treeType;
        }
        public static QuadTreeConfig Build<TTree>(int treeId, QuadShape shape, Vector2DInt extents) where TTree : QuadTreeBasic, new()
        {
            Assert.True<ArgumentException, AssertArgs<int>>(Maths.IsPowerOf2(extents.X), nameof(extents.X), "{0} isn't power of 2", new AssertArgs<int>(extents.X));
            Assert.True<ArgumentException, AssertArgs<int>>(Maths.IsPowerOf2(extents.Y), nameof(extents.Y), "{0} isn't power of 2", new AssertArgs<int>(extents.Y));
            return new QuadTreeConfig(treeId, shape, extents, typeof(TTree));
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
        public readonly int PreIndex; // “Element”在“Node”的索引位置
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