#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorDrawQuadElement : MonoBehaviour
    {
        #region Type
        private readonly struct DrawTree
        {
            internal readonly int TreeId;
            internal readonly QuadShape Shape;
            internal readonly Color Color;

            internal DrawTree(BasicQuadTree tree, float alpha)
            {
                int treeId = tree.TreeId;
                var color = QuadGetter.Proxy.GetElementColor(treeId);
                color.a = alpha;

                TreeId = treeId;
                Shape = tree.Shape;
                Color = color;
            }
        }

        [Serializable]
        private struct DrawElement : IComparable<DrawElement>
        {
            [SerializeField] internal int Index;
            internal readonly AABB2DInt AABB;
            [SerializeField] internal int TreeId;
            internal readonly QuadShape Shape;
            internal readonly Color Color;

            internal DrawElement(in QuadElement element, in DrawTree drawTree)
            {
                Index = element.Index;
                AABB = element.AABB;
                TreeId = drawTree.TreeId;
                Shape = drawTree.Shape;
                Color = drawTree.Color;
            }
            public readonly int CompareTo(DrawElement other)
            {
                int match0 = Index.CompareTo(other.Index);
                if (match0 != 0)
                    return match0;

                int match1 = AABB.Center().CompareTo(other.AABB.Center());
                if (match1 != 0)
                    return match1;

                int match2 = AABB.HalfSize().CompareTo(other.AABB.HalfSize());
                if (match2 != 0)
                    return match2;

                int match3 = TreeId.CompareTo(other.TreeId);
                if (match3 != 0)
                    return match3;

                return 0;
            }
        }

        private enum DrawRange
        {
            None,
            Single,
            Children,
            All,
            Custom,
        }
        #endregion

        [Space] [SerializeField] private int _circlePointCount = 12;
        [SerializeField] private DrawRange _drawRange = DrawRange.Children;
        [SerializeField] private int[] _treeIds;

        [Space] [SerializeField] private float _height;
        [SerializeField] [Range(0, 1)] private float _alpha = 0.25F;
        [SerializeField] private List<DrawElement> _drawElements = new();

        private float _lineDuration;
        private float _scale;
        private readonly Dictionary<int, BasicQuadTree> _trees = new();
        private readonly HashSet<int> _customIndexes = new();
        private readonly HashSet<int> _drawIndexes = new();
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
            ReadyEntity();
            ReadyTrees();
            DrawTrees();
        }
        private void OnDisable() => _trees.Clear();

        private void ReadyEntity()
        {
            _drawIndexes.Clear();

            switch (_drawRange)
            {
                case DrawRange.Single: _drawIndexes.Add(QuadGetter.Proxy.GetIndex(gameObject)); break;
                case DrawRange.Children: QuadGetter.Proxy.GetIndexes(gameObject, _drawIndexes); break;
                case DrawRange.All:
                    foreach (var pair in _trees)
                    foreach (var node in QuadGetter.GetNodes(pair.Value, _nodes))
                    foreach (var element in node.Elements)
                        _drawIndexes.Add(element.Index);
                    break;

                case DrawRange.Custom: _drawIndexes.UnionWith(_customIndexes); break;
            }
        }
        private void ReadyTrees()
        {
            _drawElements.Clear();
            if (_trees is null)
                return;
            if (_alpha <= 0)
                return;
            if (_drawIndexes.Count == 0)
                return;

            foreach (int treeId in _treeIds)
            {
                if (!_trees.TryGetValue(treeId, out var tree))
                    continue;
                var drawTree = new DrawTree(tree, _alpha);
                ReadyElements(tree, in drawTree);
            }
        }
        private void DrawTrees()
        {
            _drawElements.Sort();
            foreach (var element in _drawElements)
                DrawElements(element);
        }

        private void ReadyElements(BasicQuadTree tree, in DrawTree drawTree)
        {
            foreach (var node in QuadGetter.GetNodes(tree, _nodes))
            foreach (var element in node.Elements)
                if (_drawIndexes.Contains(element.Index))
                    _drawElements.Add(new DrawElement(in element, in drawTree));
        }
        private void DrawElements(in DrawElement element)
        {
            switch (element.Shape)
            {
                case QuadShape.Circle: ShapeDraw.Circle(Converts.AsCircleInt(in element.AABB), _circlePointCount, _scale, _height, in element.Color, _lineDuration); break;
                case QuadShape.AABB: ShapeDraw.AABB(in element.AABB, _scale, _height, in element.Color, _lineDuration); break;
            }
        }

        public void SetDrawRangeCustom() => _drawRange = DrawRange.Custom;
        public void SetTreeId(int value)
        {
            if (_treeIds.Length != 1)
                _treeIds = new int[1];
            _treeIds[0] = value;
        }
        public void SetCustomIndexes(IEnumerable<int> value)
        {
            _customIndexes.Clear();
            _customIndexes.UnionWith(value);
        }
    }
}
#endif