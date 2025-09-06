#if UNITY_EDITOR
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
    }
}
#endif