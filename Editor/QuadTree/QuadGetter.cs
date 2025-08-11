#if UNITY_EDITOR
using Eevee.QuadTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    public interface IQuadDrawProxy
    {
        Type TreeEnum { get; } // Tree的枚举类型，null代表int类型
        QuadTreeManager Manager { get; } // 获得四叉树管理器

        int GetIndex(GameObject go); // 通过GO获得Index
        void GetIndexes(GameObject go, ICollection<int> indexes); // 通过GO获得Index
    }

    internal readonly struct QuadGetter
    {
        private static IQuadDrawProxy _proxy;
        internal static IQuadDrawProxy Proxy
        {
            get
            {
                if (_proxy is { } proxy)
                {
                    return proxy;
                }

                var proxyType = FindType.GetType(typeof(IQuadDrawProxy));
                if (proxyType is null)
                {
                    Debug.LogError("IQuadDrawProxy 未被继承！");
                    return null;
                }

                var proxyInstance = Activator.CreateInstance(proxyType) as IQuadDrawProxy;
                _proxy = proxyInstance;
                return proxyInstance;
            }
        }

        internal static void GetTrees(QuadTreeManager manager, ICollection<BasicQuadTree> trees)
        {
            trees.Clear();
            manager.GetTrees(trees);
        }
        internal static void GetTrees(QuadTreeManager manager, IDictionary<int, BasicQuadTree> trees)
        {
            trees.Clear();
            var values = new List<BasicQuadTree>();
            manager.GetTrees(values);
            foreach (var tree in values)
                trees.Add(tree.TreeId, tree);
        }

        internal static TCollection GetNodes<TCollection>(BasicQuadTree tree, TCollection nodes) where TCollection : ICollection<QuadNode>
        {
            nodes.Clear();
            tree.GetNodes(nodes);
            return nodes;
        }
        internal static TCollection GetNodes<TCollection>(BasicQuadTree tree, int depth, TCollection nodes) where TCollection : ICollection<QuadNode>
        {
            nodes.Clear();
            tree.GetNodes(depth, nodes);
            return nodes;
        }
    }
}
#endif