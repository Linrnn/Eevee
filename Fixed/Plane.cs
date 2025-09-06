using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的平面
    /// </summary>
    [Serializable]
    public struct Plane : IEquatable<Plane>, IComparable<Plane>, IFormattable
    {
        #region 字段/初始化
        public Vector3D Normal;
        public Fixed64 Distance;

        public Plane(in Vector3D inNormal, Fixed64 distance)
        {
            Normal = inNormal.Normalized();
            Distance = distance;
        }
        public Plane(in Vector3D inNormal, in Vector3D inPoint)
        {
            var normal = inNormal.Normalized();
            Normal = normal;
            Distance = -Vector3D.Dot(in normal, in inPoint);
        }
        /// <summary>
        /// 使用位于其中的三个点设置平面。向下看着平面的上表面时，这些点呈顺时针顺序
        /// </summary>
        public Plane(in Vector3D p0, in Vector3D p1, in Vector3D p2)
        {
            var normal = Vector3D.Cross(p1 - p0, p2 - p0).Normalized();
            Normal = normal;
            Distance = -Vector3D.Dot(in normal, in p0);
        }

        public void SetNormalAndPosition(in Vector3D inNormal, in Vector3D inPoint)
        {
            var normal = inNormal.Normalized();
            Normal = normal;
            Distance = -Vector3D.Dot(in normal, in inPoint);
        }
        /// <summary>
        /// 使用位于其中的三个点设置平面。向下看着平面的上表面时，这些点呈顺时针顺序
        /// </summary>
        public void Set3Points(in Vector3D p0, in Vector3D p1, in Vector3D p2)
        {
            var normal = Vector3D.Cross(p1 - p0, p2 - p0).Normalized();
            Normal = normal;
            Distance = -Vector3D.Dot(in normal, in p0);
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 反平面
        /// </summary>
        public readonly Plane Flipped() => new(-Normal, -Distance);
        /// <summary>
        /// 翻转
        /// </summary>
        public void Flip()
        {
            Normal = -Normal;
            Distance = -Distance;
        }

        /// <summary>
        /// 创建指定偏移的平面
        /// </summary>
        public readonly Plane Translated(in Vector3D translation) => new(in Normal, Distance + Vector3D.Dot(in Normal, in translation));
        /// <summary>
        /// 平移指定偏移
        /// </summary>
        public void Translate(in Vector3D translation) => Distance += Vector3D.Dot(in Normal, in translation);

        /// <summary>
        /// 返回从平面到点的带符号距离
        /// </summary>
        public readonly Fixed64 GetDistanceToPoint(in Vector3D point) => Distance + Vector3D.Dot(in Normal, in point);
        /// <summary>
        /// 点在平面的投影
        /// </summary>
        public readonly Vector3D ClosestPointOnPlane(in Vector3D point) => point - Normal * (Distance + Vector3D.Dot(in Normal, in point));

        /// <summary>
        /// 点是否处于平面的正向侧
        /// </summary>
        public readonly bool GetSide(in Vector3D point) => GetDistanceToPoint(in point).RawValue > 0;
        /// <summary>
        /// 两个点是否处于平面的相同侧
        /// </summary>
        public readonly bool SameSide(in Vector3D lhs, in Vector3D rhs) => GetSide(in lhs) == GetSide(in rhs);

        /// <summary>
        /// 射线与平面的交点
        /// </summary>
        public readonly bool RayCast(in Ray3D ray, out Fixed64 enter)
        {
            var dnDot = Vector3D.Dot(in ray.Direction, in Normal);
            if (dnDot.RawValue == 0)
            {
                enter = default;
                return false;
            }

            var onDot = Vector3D.Dot(in ray.Origin, in Normal);
            enter = (-onDot - Distance) / dnDot;
            return enter.RawValue > 0;
        }
        /// <summary>
        /// 射线与平面的交点
        /// </summary>
        public readonly bool RayCast(in Ray3D ray, out Vector3D point)
        {
            bool cast = RayCast(in ray, out Fixed64 enter);
            point = cast ? ray.GetPoint(enter) : Vector3D.Zero;
            return cast;
        }
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_5_3_OR_NEWER
        public static implicit operator Plane(in UnityEngine.Plane value) => new(value.normal, value.distance);
        public static explicit operator UnityEngine.Plane(in Plane value) => new((UnityEngine.Vector3)value.Normal, (float)value.Distance);
#endif

        public static implicit operator Plane(in System.Numerics.Plane value) => new(value.Normal, value.D);
        public static explicit operator System.Numerics.Plane(in Plane value) => new((System.Numerics.Vector3)value.Normal, (float)value.Distance);

        public static bool operator ==(in Plane lhs, in Plane rhs) => lhs.Normal == rhs.Normal && lhs.Distance == rhs.Distance;
        public static bool operator !=(in Plane lhs, in Plane rhs) => lhs.Normal != rhs.Normal || lhs.Distance != rhs.Distance;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Plane other && this == other;
        public readonly override int GetHashCode() => Normal.GetHashCode() ^ Distance.GetHashCode();
        public readonly bool Equals(Plane other) => this == other;
        public readonly int CompareTo(Plane other)
        {
            int match0 = Normal.CompareTo(other.Normal);
            if (match0 != 0)
                return match0;

            int match1 = Distance.CompareTo(other.Distance);
            if (match1 != 0)
                return match1;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"[Normal:{Normal.ToString(format, provider)}, Distance:{Distance.ToString(format, provider)}]";
        #endregion
    }
}