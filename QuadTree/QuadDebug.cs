using Eevee.Define;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树调试相关
    /// </summary>
    public readonly struct QuadDebug
    {
        public static bool Print = false; // 是否输出日志
        public static readonly int[] TreeIds; // 需要检测的TreeId，null/Empty代表不限制检测
        public static readonly int[] Indexes; // 需要检测的Index，null/Empty代表不限制检测

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Check()
        {
            if (!CheckAllow())
                return false;
            return Print;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CheckTreeId(int treeId)
        {
            if (!CheckAllow())
                return false;
            if (!Print)
                return false;
            if (!CheckTargets(TreeIds, treeId))
                return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CheckIndex(int index)
        {
            if (!CheckAllow())
                return false;
            if (!Print)
                return false;
            if (!CheckTargets(Indexes, index))
                return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CheckIndex(int treeId, int index)
        {
            if (!CheckAllow())
                return false;
            if (!Print)
                return false;
            if (!CheckTargets(TreeIds, treeId))
                return false;
            if (!CheckTargets(Indexes, index))
                return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckAllow()
        {
            bool allow = false;
            SetAllow(ref allow);
            return allow;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckTargets(ICollection<int> targets, int id) => targets == null || targets.Contains(id);
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAllow(ref bool allow) => allow = true;
    }
}