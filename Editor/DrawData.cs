#if UNITY_EDITOR
using UnityEngine;

namespace EeveeEditor
{
    internal readonly struct DrawData
    {
        internal readonly float OffsetX;
        internal readonly float OffsetY;
        internal readonly float Scale;
        internal readonly float Height;

        internal DrawData(float scale, float height)
        {
            OffsetX = default;
            OffsetY = default;
            Scale = scale;
            Height = height;
        }
        internal DrawData(float ox, float oy, float scale, float height)
        {
            OffsetX = ox;
            OffsetY = oy;
            Scale = scale;
            Height = height;
        }
        internal DrawData(Vector2 offset, float scale, float height)
        {
            OffsetX = offset.x;
            OffsetY = offset.y;
            Scale = scale;
            Height = height;
        }
        internal DrawData(Vector2 leftBottom, float offset, float scale, float height)
        {
            OffsetX = leftBottom.x + offset;
            OffsetY = leftBottom.y + offset;
            Scale = scale;
            Height = height;
        }

        internal Vector2 GetOffset() => new(OffsetX, OffsetY);
        internal DrawData OnlyScale() => new(Scale, default);
    }
}
#endif