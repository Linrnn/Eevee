#if UNITY_EDITOR
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    /// <summary>
    /// 绘制四叉树节点，目标所在的四叉树节点
    /// </summary>
    internal sealed class EditorDrawQuadTree : MonoBehaviour
    {
        #region 序列化字段
        [Header("四叉树设置")] [SerializeField] private int _treeId;
        [SerializeField] private int[] _indexes = Array.Empty<int>(); // 搜索对象所在的节点

        [Header("渲染设置")] [SerializeField] private Empty _; // 使“Header”特性正常绘制缩进
        [SerializeField] private ShowNode _emptyNode = new(false, Color.gray); // 空节点
        [SerializeField] private ShowNode _normalNode = new(true, Color.green); // 非空节点
        [SerializeField] private ShowNode _rootNode = new(false, Color.red); // 根节点

        [Header("渲染数据")] [SerializeField] private Color _looseColor = Color.magenta; // 搜索对象所在的节点的松散边界
        [SerializeField] private Color _boundaryColor = Color.blue; // 搜索对象所在的节点的边界
        [SerializeField] private Color _shapeColor = Color.black; // 搜索对象所在的节点实际的形状
        [SerializeField] private int _circleAccuracy = 12;
        [SerializeField] private float _height;
        #endregion

        #region 运行时缓存
        private QuadTreeManager _manager;
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly List<QuadNode> _nodes = new(); // 临时缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadGetter.Proxy.Manager;
            if (manager is null)
                return;

            _manager = manager;
            _scale = 1F / manager.Scale;
            QuadGetter.GetTrees(manager, _trees);
        }
        private void OnDrawGizmos()
        {
            if (!enabled)
                return;
            if (_trees.TryGetValue(_treeId, out var tree))
                DrawTree(tree);
        }
        private void OnDisable() => _trees.Clear();

        private void DrawTree(BasicQuadTree tree)
        {
            if (_emptyNode.Show)
                foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                    if (node.Elements.Count == 0)
                        ShapeDraw.AABB(in node.Boundary, _scale, _height, in _emptyNode.Color);

            if (_normalNode.Show)
                foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                    if (node.Elements.Count > 0)
                        ShapeDraw.AABB(in node.Boundary, _scale, _height, in _normalNode.Color);

            if (_rootNode.Show)
                ShapeDraw.AABB(tree.MaxBoundary, _scale, _height, in _rootNode.Color);

            if (_indexes.Length > 0)
                foreach (var node in QuadGetter.GetNodes(tree, _nodes))
                foreach (var element in node.Elements)
                    if (_indexes.Contains(element.Index))
                        DrawNodeAndElement(node, in element);
        }
        private void DrawNodeAndElement(QuadNode node, in QuadElement element)
        {
            var config = _manager.GetConfig(_treeId);
            ShapeDraw.AABB(in node.LooseBoundary, _scale, _height, in _looseColor);
            ShapeDraw.AABB(in node.Boundary, _scale, _height, in _boundaryColor);
            QuadDraw.Element(config.Shape, config.TreeId, in element, _circleAccuracy, _scale, _height, in _shapeColor);
        }
    }
}
#endif