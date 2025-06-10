#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorDrawQuadTree : MonoBehaviour
    {
        private readonly List<QuadNode> _nodes = new(); // 临时缓存
        private float _lineDuration;
        private IDictionary<int, QuadTreeBasic[]> _trees;
        private QuadTreeBasic _tree;

        [SerializeField] private bool _showEmptyNode = false; // 显示空节点
        [SerializeField] private bool _showNode = true; // 显示非空节点
        [SerializeField] private bool _showRoot = true; // 显示根节点
        [SerializeField] private int[] _entityIds = Array.Empty<int>(); // 搜索单位所在的节点

        [Space] [SerializeField] [SingleEnum] private int _funcEnum;
        [SerializeField] [SingleEnum] private EditorQuadSubEnum _subEnum = EditorQuadSubEnum.Tree1;
        [SerializeField] private float _height = 1;

        private void OnEnable()
        {
            _lineDuration = Time.fixedDeltaTime;

            // todo eevee
            //var manager = BATEntry.Data?.quadTree;
            //if (manager is null)
            //    return;

            //var treeFiled = manager.GetType().GetField(QuadTreeManager.TreeName, BindingFlags.Instance | BindingFlags.NonPublic);
            //_trees = treeFiled?.GetValue(manager) as IDictionary<int, MeshQuadTree[]>;
            //if (_trees is not null)
            //    _tree = GetTree();
        }
        private void OnValidate()
        {
            if (_trees is null)
                return;

            if (!enabled)
                return;

            _tree = GetTree();
        }
        private void FixedUpdate()
        {
            DrawTree(_tree);
        }
        private void OnDisable()
        {
            _trees = null;
            _tree = null;
        }

        private void DrawTree(QuadTreeBasic tree)
        {
            if (tree is null)
                return;

            if (_showEmptyNode)
                foreach (var node in QuadHelper.GetNodes(tree, _nodes))
                    if (node.Elements.Count == 0)
                        EditorHelper.DrawRect(in node.Boundary, Color.gray, _height, _lineDuration);

            if (_showNode)
                foreach (var node in QuadHelper.GetNodes(tree, _nodes))
                    if (node.Elements.Count > 0)
                        EditorHelper.DrawRect(in node.Boundary, Color.green, _height, _lineDuration);

            if (_showRoot)
                EditorHelper.DrawRect(tree.MaxBoundary, Color.red, _height, _lineDuration);

            if (_entityIds.Length > 0)
                foreach (var node in QuadHelper.GetNodes(tree, _nodes))
                    if (node.Elements.Count > 0)
                        foreach (var element in node.Elements.AsReadOnlySpan())
                            if (_entityIds.Contains(element.Index))
                                DrawRect((node.LooseBoundary, Color.magenta), (node.Boundary, Color.blue), (element.AABB, Color.yellow));
        }
        private void DrawRect(params (AABB2DInt aabb, Color color)[] array)
        {
            foreach (var (aabb, color) in array)
                EditorHelper.DrawRect(in aabb, in color, _height, _lineDuration);
        }

        private QuadTreeBasic GetTree() => _trees[_funcEnum][Log2((double)_subEnum)];
        private int Log2(double num) => num > 0 ? (int)Math.Log(num, 2) : -1;
    }
}
#endif