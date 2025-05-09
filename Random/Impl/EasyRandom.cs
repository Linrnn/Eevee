﻿using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.Random
{
    /// <summary>
    /// 便捷实现，子类需要实现“EasyRandom.GetInt()”
    /// </summary>
    public abstract class EasyRandom : IRandom
    {
        #region 接口实现
        public virtual int GetInt32(int minInclusive, int maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            int value = GetInt(minInclusive, maxExclusive);
            return value;
        }
        public virtual uint GetUInt32(uint minInclusive, uint maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            uint value = RandomUInt32(minInclusive, maxExclusive);
            return value;
        }
        public virtual long GetInt64(long minInclusive, long maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            long value = RandomInt64(minInclusive, maxExclusive);
            return value;
        }
        public virtual ulong GetUInt64(ulong minInclusive, ulong maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            ulong value = RandomUInt64(minInclusive, maxExclusive);
            return value;
        }
        public virtual Fixed64 GetFixed64(Fixed64 minInclusive, Fixed64 maxExclusive)
        {
            if (minInclusive == maxExclusive)
                return minInclusive;

            long value = RandomInt64(minInclusive.RawValue, maxExclusive.RawValue);
            return new Fixed64(value);
        }

        public virtual Vector2D GetVector2(in Vector2D p0, in Vector2D p1)
        {
            if (p0 == p1)
                return p0;

            var value = RandomVector2D(in p0, in p1);
            return value;
        }
        public virtual Vector3D GetVector3(in Vector3D p0, in Vector3D p1)
        {
            if (p0 == p1)
                return p0;

            var value = RandomVector3D(in p0, in p1);
            return value;
        }
        public virtual Vector2D InTriangle(in Vector2D p0, in Vector2D p1, in Vector2D p2)
        {
            var p3 = RandomVector2D(in p0, in p1);
            var p4 = RandomVector2D(in p2, in p3);
            return p4;
        }

        public virtual Vector2D OnUnitCircle() => RandomUnitCircle();
        public virtual Vector2D InCircle(Fixed64 radius)
        {
            var circle = RandomUnitCircle();
            var range = RandomFixed64(radius);
            return circle * range;
        }
        public virtual Vector3D OnUnitSphere()
        {
            var rad360 = RandomFixed64(Maths.Rad360);
            var rad180 = RandomFixed64(Maths.Rad180);
            var sin = Maths.Sin(rad180);

            var x = sin * Maths.Cos(rad360);
            var y = sin * Maths.Sin(rad360);
            var z = Maths.Cos(rad180);
            return new Vector3D(x, y, z);
        }
        public virtual Vector3D InSphere(Fixed64 radius)
        {
            var x = RandomFixed64(radius);
            var y = RandomFixed64(radius);
            var z = RandomFixed64(radius);
            var r = RandomFixed64(radius);
            var p = new Vector3D(x, y, z);
            return p.ClampMagnitude(r.Cube().Sqrt());
        }

        public virtual Vector3D GetEulerAngles()
        {
            var x = RandomFixed64(Maths.Deg360);
            var y = RandomFixed64(Maths.Deg360);
            var z = RandomFixed64(Maths.Deg360);
            return new Vector3D(x, y, z);
        }
        public virtual Quaternion GetQuaternion()
        {
            var number = RandomFixed64(Fixed64.One);
            var rad0 = RandomFixed64(Maths.Rad360);
            var rad1 = RandomFixed64(Maths.Rad360);
            var sqrt0 = (Fixed64.One - number).Sqrt();
            var sqrt1 = number.Sqrt();

            var x = sqrt0 * Maths.Sin(rad0);
            var y = sqrt0 * Maths.Cos(rad0);
            var z = sqrt1 * Maths.Sin(rad1);
            var w = sqrt1 * Maths.Cos(rad1);

            var magnitude = (x.Sqr() + y.Sqr() + z.Sqr() + w.Sqr()).Sqrt();
            return new Quaternion(x / magnitude, y / magnitude, z / magnitude, w / magnitude); // “Fixed64.Reciprocal()”在大数运算下，会丢失精度
        }
        #endregion

        /// <summary>
        /// 随机获取Int32<br/>
        /// 随机数的核心接口
        /// </summary>
        /// <param name="minInclusive">包括最小值</param>
        /// <param name="maxExclusive">不包括最大值</param>
        protected abstract int GetInt(int minInclusive, int maxExclusive);

        #region 工具方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint RandomUInt32(uint minInclusive, uint maxExclusive)
        {
            int min = (int)(minInclusive + int.MinValue);
            int max = (int)(maxExclusive + int.MinValue);
            int value = GetInt(min, max);
            return (uint)(value - (long)int.MinValue);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long RandomInt64(long minInclusive, long maxExclusive)
        {
            ulong min = L2Ul(minInclusive);
            ulong max = L2Ul(maxExclusive);
            ulong value = RandomUInt64(min, max);
            return Ul2L(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong RandomUInt64(ulong minInclusive, ulong maxExclusive)
        {
            ulong diff = maxExclusive - minInclusive;
            if (diff < uint.MaxValue)
            {
                uint value = RandomUInt32(0, (uint)diff);
                return minInclusive + value;
            }
            else
            {
                uint high = RandomUInt32(0, (uint)(diff >> 32));
                uint low = RandomUInt32(0, uint.MaxValue);
                ulong value = (ulong)high << 32 | low;
                return minInclusive + value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Fixed64 RandomFixed64(Fixed64 maxInclusive)
        {
            ulong value = RandomUInt64(0, (ulong)maxInclusive.RawValue + 1);
            return new Fixed64((long)value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2D RandomVector2D(in Vector2D from, in Vector2D to)
        {
            var percent = RandomFixed64(Fixed64.One);
            return Lerp.LinearUnClamp(in from, in to, percent);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3D RandomVector3D(in Vector3D from, in Vector3D to)
        {
            var percent = RandomFixed64(Fixed64.One);
            return Lerp.LinearUnClamp(in from, in to, percent);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2D RandomUnitCircle()
        {
            var rad = RandomFixed64(Maths.Rad360);
            return new Vector2D(Maths.Cos(rad), Maths.Sin(rad));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong L2Ul(long num) => num >= 0 ? (ulong)num + long.MaxValue + 1 : (ulong)(num - long.MinValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long Ul2L(ulong num) => num > long.MaxValue ? (long)(num - long.MaxValue - 1) : (long)num + long.MinValue;
        #endregion
    }
}