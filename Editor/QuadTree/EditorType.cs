#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    [Flags]
    internal enum EditorQuadSubEnum : ushort
    {
        None = 0,
        Tree1 = 1,
        Tree2 = 1 << 1,
        Tree3 = 1 << 2,
        Tree4 = 1 << 3,
        Tree5 = 1 << 4,
        Tree6 = 1 << 5,
        Tree7 = 1 << 6,
        Tree8 = 1 << 7,
        Tree9 = 1 << 8,
        Tree10 = 1 << 9,
        Tree11 = 1 << 10,
        Tree12 = 1 << 11,
        Tree13 = 1 << 12,
        Tree14 = 1 << 13,
        Tree15 = 1 << 14,
        Tree16 = 1 << 15,
    }

    [Serializable]
    internal struct EditorQuadDrawAsset
    {
        [SerializeField] internal string Path;
        [SerializeField] [ReadOnly] internal Sprite Asset;

        internal EditorQuadDrawAsset(string path)
        {
            Path = path;
            Asset = null;
        }
        internal void Load()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return;

            Asset = AssetDatabase.LoadAssetAtPath<Sprite>(Path);
        }
    }
}
#endif