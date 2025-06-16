#if UNITY_EDITOR
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorDrawQuadTree : MonoBehaviour
    {
        [SerializeField] private bool _showEmptyNode; // 显示空节点
        [SerializeField] private bool _showNode = true; // 显示非空节点
        [SerializeField] private bool _showRoot = true; // 显示根节点
        [SerializeField] private int[] _indexes = Array.Empty<int>(); // 搜索对象所在的节点

        [Space] [SerializeField] private int _treeId;
        [SerializeField] private float _height = 1;

        private float _lineDuration;
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly List<QuadNode> _nodes = new(); // 临时缓存

        private void OnEnable()
        {
            var manager = QuadGetter.Proxy.Manager;
            if (manager is null)
                return;

            _lineDuration = Time.fixedDeltaTime;
            _scale = 1F / manager.Scale;
            QuadGetter.GetTrees(manager, _trees);
        }
        private void FixedUpdate()
        {
            if (_trees.TryGetValue(_treeId, out var tree))
                DrawTree(tree);
        }
        private void OnDisable() => _trees.Clear();

        private void DrawTree(BasicQuadTree tree)
        {
            if (_showEmptyNode)
                foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                    if (node.Elements.Count == 0)
                        EditorDraw.AABB(in node.Boundary, _scale, _height, Color.gray, _lineDuration);

            if (_showNode)
                foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                    if (node.Elements.Count > 0)
                        EditorDraw.AABB(in node.Boundary, _scale, _height, Color.green, _lineDuration);

            if (_showRoot)
                EditorDraw.AABB(tree.MaxBoundary, _scale, _height, Color.red, _lineDuration);

            if (_indexes.Length > 0)
                foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                foreach (var element in node.Elements)
                    if (_indexes.Contains(element.Index))
                        DrawNodeAndElement(node, in element);
        }
        private void DrawNodeAndElement(QuadNode node, in QuadElement element)
        {
            EditorDraw.AABB(in node.LooseBoundary, _scale, _height, Color.magenta, _lineDuration);
            EditorDraw.AABB(in node.Boundary, _scale, _height, Color.blue, _lineDuration);
            EditorDraw.AABB(in element.AABB, _scale, _height, Color.yellow, _lineDuration);
        }
    }
}
#endif