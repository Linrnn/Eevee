#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    /// <summary>
    /// 绘制四叉树节点，目标所在的四叉树节点
    /// </summary>
    [AddComponentMenu("Eevee Editor/Quad Tree/Editor Draw Quad Tree Node")]
    internal sealed class EditorDrawQuadTreeNode : MonoBehaviour
    {
        #region 类型
        [CustomEditor(typeof(EditorDrawQuadTreeNode))]
        private sealed class EditorDrawQuadTreeNodeInspector : Editor
        {
            #region Property Path
            private const string TreeId = nameof(_treeId);
            private const string Indexes = nameof(_indexes);
            private const string DrawIndex = nameof(_drawIndex);
            private const string EmptyNode = nameof(_emptyNode);
            private const string NormalNode = nameof(_normalNode);
            private const string RootNode = nameof(_rootNode);
            private const string LooseColor = nameof(_looseColor);
            private const string BoundaryColor = nameof(_boundaryColor);
            private const string ShapeColor = nameof(_shapeColor);
            #endregion

            private PropertyHandle _propertyHandle;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawProperties();
                serializedObject.ApplyModifiedProperties();
            }
            private void OnEnable() => _propertyHandle.Initialize(this);
            private void OnDisable() => _propertyHandle.Dispose();

            private void DrawProperties()
            {
                _propertyHandle.DrawScript();
                _propertyHandle.EnumTreeFunc(TreeId);
                _propertyHandle.Draw(Indexes);
                _propertyHandle.Draw(DrawIndex);
                _propertyHandle.Draw(EmptyNode);
                _propertyHandle.Draw(NormalNode);
                _propertyHandle.Draw(RootNode);
                _propertyHandle.Draw(LooseColor);
                _propertyHandle.Draw(BoundaryColor);
                _propertyHandle.Draw(ShapeColor);
            }
        }
        #endregion

        #region 序列化字段
        [Header("渲染数据")] [SerializeField] private int _treeId;
        [SerializeField] private int[] _indexes = Array.Empty<int>(); // 搜索对象所在的节点
        [SerializeField] private bool _drawIndex = true;

        [Header("渲染设置")] [SerializeField] private ColorSetting _emptyNode = new(false, Color.gray); // 空节点
        [SerializeField] private ColorSetting _normalNode = new(true, Color.green); // 非空节点
        [SerializeField] private ColorSetting _rootNode = new(false, Color.red); // 根节点

        [Header("渲染参数")] [SerializeField] private Color _looseColor = Color.magenta; // 搜索对象所在的节点的松散边界
        [SerializeField] private Color _boundaryColor = Color.blue; // 搜索对象所在的节点的边界
        [SerializeField] private Color _shapeColor = Color.black; // 搜索对象所在的节点实际的形状
        #endregion

        #region 运行时缓存
        private QuadTreeManager _manager;
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly List<QuadTreeNode> _nodes = new(); // 临时缓存
        #endregion

        private void OnEnable()
        {
            var manager = QuadTreeGetter.Proxy.Manager;
            _manager = manager;
            _scale = 1F / manager.Scale;
            QuadTreeGetter.GetTrees(manager, _trees);
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
            float height = QuadTreeDraw.Height;

            if (_emptyNode.Show)
                foreach (var node in QuadTreeGetter.GetNodes(tree, _nodes))
                    if (node.Elements.Count == 0)
                        ShapeDraw.AABB(in node.Boundary, _scale, height, in _emptyNode.Color);

            if (_normalNode.Show)
                foreach (var node in QuadTreeGetter.GetNodes(tree, _nodes))
                    if (node.Elements.Count > 0)
                        ShapeDraw.AABB(in node.Boundary, _scale, height, in _normalNode.Color);

            if (_rootNode.Show)
                ShapeDraw.AABB(tree.MaxBoundary, _scale, height, in _rootNode.Color);

            if (_indexes.Length > 0)
                foreach (var node in QuadTreeGetter.GetNodes(tree, _nodes))
                foreach (var element in node.Elements)
                    if (_indexes.Has(element.Index))
                        DrawNodeAndElement(node, in element);
        }
        private void DrawNodeAndElement(QuadTreeNode node, in QuadTreeElement element)
        {
            var config = _manager.GetConfig(_treeId);
            float height = QuadTreeDraw.Height;
            ShapeDraw.AABB(in node.LooseBoundary, _scale, height, in _looseColor);
            ShapeDraw.AABB(in node.Boundary, _scale, height, in _boundaryColor);
            QuadTreeDraw.Element(config.Shape, config.TreeId, in element, _scale, _drawIndex, in _shapeColor);
        }
    }
}
#endif