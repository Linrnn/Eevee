#if UNITY_EDITOR
using Eevee.Collection;
using Eevee.Fixed;
using Eevee.QuadTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal sealed class EditorDrawQuadElement : MonoBehaviour
    {
        #region Type
        private class ObjectPool
        {
            private readonly HashSet<GameObject> _objects = new();
            private GameObject _root;

            internal GameObject Pop()
            {
                if (_root == null)
                {
                    _root = new GameObject(nameof(EditorDrawQuadElement))
                    {
                        transform =
                        {
                            position = Vector3.zero,
                        },
                    };
                    _objects.Clear();
                }

                if (_objects.IsEmpty())
                {
                    var go = new GameObject();
                    go.AddComponent<SpriteRenderer>();
                    go.transform.SetParent(_root.transform);
                    return go;
                }
                else
                {
                    var go = _objects.GetFirst0GC();
                    _objects.Remove(go);
                    go.SetActive(true);
                    return go;
                }
            }
            internal void Push(GameObject go)
            {
                go.SetActive(false);
                go.transform.position = Vector3.zero;
                _objects.Add(go);
            }
        }

        private readonly struct TreeInfo
        {
            internal readonly int FuncEnum;
            internal readonly Color Color;
            internal readonly Sprite Asset;

            internal TreeInfo(int funcEnum, in EditorQuadDrawAsset boxAsset, in EditorQuadDrawAsset circleAsset, float alpha)
            {
                // todo eevee
                //bool circle = (funcEnum & QuadTreeManager.CircleEnum) > 0;
                bool circle = false;
                var color = GetColor(funcEnum);
                color.a = alpha;

                FuncEnum = funcEnum;
                Color = color;
                Asset = circle ? circleAsset.Asset : boxAsset.Asset;
            }

            private static Color GetColor(int func)
            {
                return func switch
                {
                    // todo eevee
                    //int.None => Color.clear,
                    //int.GuardBox => Color.green,
                    //int.GuardArea => Color.red,
                    //int.Region => Color.yellow,
                    //int.Shop => Color.white,
                    //int.InRange => Color.yellow,
                    //int.WidgetBox => Color.black,
                    //int.EffectBox => Color.blue,
                    _ => Color.clear,
                };
            }
        }

        private readonly struct SubTreeInfo
        {
            internal readonly int FuncEnum;
            internal readonly int SubTree;
            internal readonly Color Color;
            internal readonly Sprite Asset;

            internal SubTreeInfo(in TreeInfo treeInfo, int subTree)
            {
                FuncEnum = treeInfo.FuncEnum;
                SubTree = subTree;
                Color = treeInfo.Color;
                Asset = treeInfo.Asset;
            }
        }

        [Serializable]
        private struct DrawElement : IComparable<DrawElement>
        {
            [SerializeField] internal int EntityId;
            [SerializeField] internal AABB2DInt Box;
            [SerializeField] internal int FuncEnum;
            [SerializeField] internal int SubTree;
            internal readonly Color Color;
            internal readonly Sprite Asset;

            internal DrawElement(in QuadElement element, in SubTreeInfo treeInfo)
            {
                EntityId = element.Index;
                Box = element.AABB;
                FuncEnum = treeInfo.FuncEnum;
                SubTree = treeInfo.SubTree;
                Color = treeInfo.Color;
                Asset = treeInfo.Asset;
            }
            public readonly int CompareTo(DrawElement other)
            {
                int match0 = Box.W - other.Box.W;
                if (match0 != 0)
                    return match0;

                int match1 = Box.H - other.Box.H;
                if (match1 != 0)
                    return match1;

                int match2 = FuncEnum - other.FuncEnum;
                if (match2 != 0)
                    return match2;

                int match3 = SubTree - other.SubTree;
                if (match3 != 0)
                    return match3;

                int match4 = EntityId - other.EntityId;
                if (match4 != 0)
                    return match4;

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

        [SerializeField] private EditorQuadDrawAsset _boxAsset = new("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png");
        [SerializeField] private EditorQuadDrawAsset _circleAsset = new("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Circle.png");

        [Space] [SerializeField] private DrawRange _drawRange = DrawRange.Children;
        [SerializeField] private int _funcEnum;
        [SerializeField] private EditorQuadSubEnum _subEnum = (EditorQuadSubEnum)ushort.MaxValue;

        [Space] [SerializeField] private float _height = 0.01F;
        [SerializeField] [Range(0, 1)] private float _alpha = 0.25F;
        [SerializeField] private List<DrawElement> _drawElements = new();

        private readonly HashSet<int> _customEntityIds = new();
        private readonly HashSet<int> _drawEntityIds = new();
        private readonly HashSet<GameObject> _objects = new();
        private IDictionary<int, MeshQuadTree[]> _trees;

        private static readonly ObjectPool Pool = new();

        private void OnEnable()
        {
            _boxAsset.Load();
            _circleAsset.Load();

            // todo eevee
            //var manager = BATEntry.Data?.quadTree;
            //if (manager == null)
            //    return;

            //var treeFiled = manager.GetType().GetField(QuadTreeManager.TreeName, BindingFlags.Instance | BindingFlags.NonPublic);
            //_trees = treeFiled?.GetValue(manager) as IDictionary<int, MeshQuadTree[]>;
        }
        private void Update()
        {
            ReadyEntity();
            ReadyTrees();
            DrawTrees();
        }
        private void OnDisable()
        {
            PushObject();
            _trees = null;
        }

        private void ReadyEntity()
        {
            _drawEntityIds.Clear();

            switch (_drawRange)
            {
                // todo eevee
                //case DrawRange.Single:
                //{
                //    var component = gameObject.GetComponent<UnitParentMonoComponent>();
                //    if (component)
                //        _drawEntityIds.Add(component.entityIdx);
                //    break;
                //}

                //case DrawRange.Children:
                //{
                //    var components = gameObject.GetComponentsInChildren<UnitParentMonoComponent>(true);
                //    foreach (var component in components)
                //        _drawEntityIds.Add(component.entityIdx);
                //    break;
                //}

                case DrawRange.All:
                {
                    foreach (var pair in _trees)
                    foreach (var tree in pair.Value)
                        if (tree != null)
                            foreach (var nodes in tree.Nodes)
                            foreach (var node in nodes)
                            foreach (var element in node.Elements.AsReadOnlySpan())
                                _drawEntityIds.Add(element.Index);
                    break;
                }

                case DrawRange.Custom:
                {
                    _drawEntityIds.UnionWith(_customEntityIds);
                    break;
                }
            }
        }
        private void ReadyTrees()
        {
            _drawElements.Clear();

            if (_trees == null)
                return;

            if (_alpha <= 0)
                return;

            if (_drawEntityIds.IsNullOrEmpty())
                return;

            int treeEnum = (int)_subEnum;
            foreach (var pair in _trees)
            {
                var trees = pair.Value;
                if ((pair.Key & _funcEnum) == 0)
                    continue;

                var treeInfo = new TreeInfo(pair.Key, in _boxAsset, in _circleAsset, _alpha);
                for (int length = trees.Length, i = 0; i < length; ++i)
                    if ((1 << i & treeEnum) > 0)
                        ReadyElements(trees[i], new SubTreeInfo(in treeInfo, i));
            }
        }
        private void DrawTrees()
        {
            PushObject();
            _drawElements.Sort();

            for (int i = 0; i < _drawElements.Count; ++i)
            {
                var element = _drawElements[i];
                var go = Pool.Pop();

                DrawElements(go, in element, i);
                _objects.Add(go);
            }
        }

        private void PushObject()
        {
            foreach (var go in _objects)
                Pool.Push(go);
            _objects.Clear();
        }
        private void ReadyElements(MeshQuadTree tree, in SubTreeInfo treeInfo)
        {
            if (tree == null)
                return;

            foreach (var nodes in tree.Nodes)
            foreach (var node in nodes)
            foreach (var element in node.Elements.AsReadOnlySpan())
                if (_drawEntityIds.Contains(element.Index))
                    _drawElements.Add(new DrawElement(in element, in treeInfo));
        }
        private void DrawElements(GameObject go, in DrawElement element, int i)
        {
            go.name = $"{element.FuncEnum} Sub:{element.SubTree} Id:{element.EntityId} i:{i}";

            go.transform.SetSiblingIndex(i);
            go.transform.position = new Vector3(element.Box.X / 128F, _height, element.Box.Y / 128F);
            go.transform.eulerAngles = new Vector3(90, 0, 0);
            go.transform.localScale = new Vector3(element.Box.W / 64F, element.Box.H / 64F, 1);

            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = element.Asset;
            spriteRenderer.color = element.Color;
            spriteRenderer.sortingOrder = 10000 - i;
        }

        public void SetDrawRangeCustom()
        {
            _drawRange = DrawRange.Custom;
        }
        public void SetFuncEnum(int value)
        {
            _funcEnum = value;
        }
        public void SetSubEnum(EditorQuadSubEnum value)
        {
            _subEnum = value;
        }
        public void SetCustomEntityIds(IEnumerable<int> value)
        {
            _customEntityIds.Clear();
            _customEntityIds.UnionWith(value);
        }
    }
}
#endif