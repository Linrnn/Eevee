﻿using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的三维射线
    /// </summary>
    [Serializable]
    public struct Ray3D : IEquatable<Ray3D>, IComparable<Ray3D>, IFormattable
    {
        #region 字段/初始化
        public Vector3D Origin;
        public Vector3D Direction; // 必须是单位向量

        public Ray3D(in Vector3D origin, in Vector3D direction)
        {
            Check.NotZero(in direction);
            Check.Normal(in direction);

            Origin = origin;
            Direction = direction;
        }
        #endregion

        #region 基础方法
        public readonly Vector3D GetPoint(Fixed64 distance) => Origin + Direction * distance;
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_5_3_OR_NEWER
        public static implicit operator Ray3D(in UnityEngine.Ray value) => new(value.origin, value.direction);
        public static explicit operator UnityEngine.Ray(in Ray3D value) => new((UnityEngine.Vector3)value.Origin, (UnityEngine.Vector3)value.Direction);
#endif

        public static bool operator ==(in Ray3D lhs, in Ray3D rhs) => lhs.Origin == rhs.Origin && lhs.Direction == rhs.Direction;
        public static bool operator !=(in Ray3D lhs, in Ray3D rhs) => lhs.Origin != rhs.Origin || lhs.Direction != rhs.Direction;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Ray3D other && this == other;
        public readonly override int GetHashCode() => Origin.GetHashCode() ^ Direction.GetHashCode();
        public readonly bool Equals(Ray3D other) => this == other;
        public readonly int CompareTo(Ray3D other)
        {
            int match0 = Origin.CompareTo(Origin);
            if (match0 != 0)
                return match0;

            int match1 = Direction.CompareTo(Direction);
            if (match1 != 0)
                return match1;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"[Origin:{Origin.ToString(format, provider)}, Direction:{Direction.ToString(format, provider)})]";
        #endregion
    }
}