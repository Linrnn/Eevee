#if UNITY_EDITOR
using Eevee.QuadTree;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EeveeEditor.QuadTree
{
    internal readonly struct QuadHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TCollection GetNodes<TCollection>(BasicQuadTree tree, TCollection returnNodes) where TCollection : ICollection<QuadNode>
        {
            returnNodes.Clear();
            tree.GetNodes(returnNodes);
            return returnNodes;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TCollection GetNodes<TCollection>(BasicQuadTree tree, int depth, TCollection returnNodes) where TCollection : ICollection<QuadNode>
        {
            returnNodes.Clear();
            tree.GetNodes(depth, returnNodes);
            return returnNodes;
        }
    }
}
#endif