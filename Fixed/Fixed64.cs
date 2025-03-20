using Eevee.Define;
using Eevee.Log;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 有限小数，不包括无限循环小数<br/>
    /// 默认实现：1位符号 + 31位整数 + 32位小数<br/>
    /// 通过 “Const.FractionalPlaces” 修改小数位数
    /// </summary>
    [Serializable]
    public struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>, IFormattable
    {
        #region 字段/构造函数
        public static readonly Fixed64 Zero = default; // 数字：0
        public static readonly Fixed64 Half = new(Const.Half); // 数字：0.5
        public static readonly Fixed64 One = new(Const.One); // 数字：1
        public static readonly Fixed64 MinValue = new(Const.MinValue); // 最小值
        public static readonly Fixed64 MaxValue = new(Const.MaxValue); // 最大值
        public static readonly Fixed64 Infinitesimal = new(Const.Infinitesimal); // 无穷小
        public static readonly Fixed64 Infinity = new(Const.Infinity); // 无穷大
        public static readonly Fixed64 NaN = new(Const.MinPeak); // 非数，不存在的数

        public long RawValue;
        public Fixed64(long rawValue) => RawValue = rawValue;
        #endregion

        #region 基础方法
        /// <summary>
        /// 符号<br/>
        /// 大于0，返回1<br/>
        /// 等于0，返回0<br/>
        /// 小于0，返回-1
        /// </summary>
        public readonly int Sign() => Math.Sign(RawValue);
        /// <summary>
        /// 符号<br/>
        /// 大于等于0，返回1<br/>
        /// 小于0，返回-1
        /// </summary>
        public readonly int No0Sign() => RawValue >= 0 ? 1 : -1;
        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Fixed64 Abs() => RawValue >= 0 ? this : -this;

        /// <summary>
        /// 向上取整
        /// </summary>
        public readonly Fixed64 Ceiling() => (RawValue & Const.FractionalPart) == 0 ? this : Floor() + One;
        /// <summary>
        /// 向下取整
        /// </summary>
        public readonly Fixed64 Floor() => new(RawValue & Const.IntegerPart);
        /// <summary>
        /// 四舍五入到最接近的整数值<br/>
        /// 如果一个数在偶数和奇数中间，则返回最近的偶数（如：3.5 -> 4，4.5 -> 4）
        /// </summary>
        public readonly Fixed64 Round() => (RawValue & Const.FractionalPart) switch
        {
            < Const.Half => Floor(),
            > Const.Half => Ceiling(),
            _ => (RawValue & Const.One) == 0 ? Floor() : Ceiling(),
        };
        /// <summary>
        /// 得到小数部分
        /// </summary>
        public readonly Fixed64 FractionalPart() => new(RawValue & Const.FractionalPart);

        /// <summary>
        /// 倒数，-1次方
        /// </summary>
        public readonly Fixed64 Reciprocal() => One / this;
        /// <summary>
        /// 平方根，即开方，1/2次方
        /// </summary>
        public readonly Fixed64 Sqrt()
        {
            switch (RawValue)
            {
                case < 0:
                {
                    LogRelay.Fail($"[Fixed] Fixed64.Sqrt()，value：{RawValue}是负数，无法开方");
                    return NaN;
                }

                case <= Const.MaxValue >> Const.FractionalBits: // 先偏移，后开方，精度更高
                {
                    long offset = RawValue << Const.FractionalBits;
                    long sqrt = SquareRoot.Count(offset);
                    return new Fixed64(sqrt);
                }

                default:
                {
                    long sqrt = SquareRoot.Count(RawValue);
                    long offset = sqrt << (Const.FractionalBits >> 1);
                    return new Fixed64(offset);
                }
            }
        }
        /// <summary>
        /// 平方，2次方
        /// </summary>
        public readonly Fixed64 Sqr() => this * this;
        /// <summary>
        /// 立方，3次方
        /// </summary>
        public readonly Fixed64 Cube() => this * this * this;

        /// <summary>
        /// 区间[0, 1]值更正（如果出区间，值取最近）
        /// </summary>
        public readonly Fixed64 Clamp01() => RawValue switch
        {
            < Const.Zero => Zero,
            > Const.One => One,
            _ => this,
        };
        /// <summary>
        /// 区间值更正（如果出区间，值取最近）
        /// </summary>
        public readonly Fixed64 Clamp(Fixed64 min, Fixed64 max)
        {
            if (RawValue < min.RawValue)
                return min;

            if (RawValue > max.RawValue)
                return max;

            return this;
        }

        /// <summary>
        /// 较小值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Min(Fixed64 lsh, Fixed64 rsh) => lsh < rsh ? lsh : rsh;
        /// <summary>
        /// 较小值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Min(Fixed64 lsh, Fixed64 msh, Fixed64 rsh)
        {
            var value = lsh < msh ? lsh : msh;
            return value < rsh ? value : rsh;
        }

        /// <summary>
        /// 较大值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Max(Fixed64 lsh, Fixed64 rsh) => lsh > rsh ? lsh : rsh;
        /// <summary>
        /// 较大值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Max(Fixed64 lsh, Fixed64 msh, Fixed64 rsh)
        {
            var value = lsh > msh ? lsh : msh;
            return value > rsh ? value : rsh;
        }

        /// <summary>
        /// 尝试解析，将字符串转成Fixed64
        /// </summary>
        public static bool TryParse(string str, out Fixed64 result)
        {
            result = Zero;
            if (string.IsNullOrWhiteSpace(str))
                return false;

            int length = str.Length;
            int start = 0;
            int end = length - 1;
            while (start < length && char.IsWhiteSpace(str[start])) // 跳过前导空格
                ++start;
            while (end >= start && char.IsWhiteSpace(str[end])) // 跳过尾部空格
                --end;
            if (start > end) // 如果全是空格，返回 false
                return false;

            int i = start;
            if (str[start] is '+' or '-') // 跳过正负号
                ++i;

            bool hasFractional = false;
            int integerPart = 0;
            int fractionalPart = 0;
            int fractionalDivisor = 1;
            for (; i < length; ++i)
            {
                char ch = str[i];
                if (char.IsNumber(ch)) // 处理数字
                {
                    int digit = ch - '0';
                    if (hasFractional)
                    {
                        fractionalPart = fractionalPart * 10 + digit;
                        fractionalDivisor *= 10;
                    }
                    else
                    {
                        integerPart = integerPart * 10 + digit;
                    }
                }
                else if (ch == '.' && !hasFractional) // 处理小数点
                {
                    hasFractional = true;
                }
                else
                {
                    return false; // 非法字符
                }
            }

            if (str[start] == '-')
                result = -integerPart - (Fixed64)fractionalPart / fractionalDivisor;
            else
                result = integerPart + (Fixed64)fractionalPart / fractionalDivisor;
            return true;
        }
        /// <summary>
        /// 尝试解析，将字符串转成Fixed64
        /// </summary>
        public static bool TryParse(IList<char> str, out Fixed64 result)
        {
            result = Zero;

            int length = str.Count;
            int start = 0;
            int end = length - 1;
            while (start < length && char.IsWhiteSpace(str[start])) // 跳过前导空格
                ++start;
            while (end >= start && char.IsWhiteSpace(str[end])) // 跳过尾部空格
                --end;
            if (start > end) // 如果全是空格，返回 false
                return false;

            int i = start;
            if (str[start] is '+' or '-') // 跳过正负号
                ++i;

            bool hasFractional = false;
            int integerPart = 0;
            int fractionalPart = 0;
            int fractionalDivisor = 1;
            for (; i < length; ++i)
            {
                char ch = str[i];
                if (char.IsNumber(ch)) // 处理数字
                {
                    int digit = ch - '0';
                    if (hasFractional)
                    {
                        fractionalPart = fractionalPart * 10 + digit;
                        fractionalDivisor *= 10;
                    }
                    else
                    {
                        integerPart = integerPart * 10 + digit;
                    }
                }
                else if (ch == '.' && !hasFractional) // 处理小数点
                {
                    hasFractional = true;
                }
                else
                {
                    return false; // 非法字符
                }
            }

            if (str[start] == '-')
                result = -integerPart - (Fixed64)fractionalPart / fractionalDivisor;
            else
                result = integerPart + (Fixed64)fractionalPart / fractionalDivisor;
            return true;
        }

        public readonly bool IsInfinity() => RawValue is Const.Infinitesimal or Const.Infinity;
        public readonly bool IsNaN() => RawValue == Const.MinPeak;
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static implicit operator Fixed64(long value) => new(value << Const.FractionalBits);
        public static implicit operator Fixed64(float value) => new((long)((decimal)value * Const.One)); // 处理大数丢失整数部分
        public static implicit operator Fixed64(double value) => new((long)((decimal)value * Const.One)); // 处理大数丢失整数部分
        public static implicit operator Fixed64(in decimal value) => new((long)(value * Const.One));

        public static explicit operator short(Fixed64 value) => (short)(value.RawValue >> Const.FractionalBits);
        public static explicit operator int(Fixed64 value) => (int)(value.RawValue >> Const.FractionalBits);
        public static explicit operator long(Fixed64 value) => value.RawValue >> Const.FractionalBits;
        public static explicit operator ushort(Fixed64 value) => (ushort)(value.RawValue >> Const.FractionalBits);
        public static explicit operator uint(Fixed64 value) => (uint)(value.RawValue >> Const.FractionalBits);
        public static explicit operator ulong(Fixed64 value) => (ulong)(value.RawValue >> Const.FractionalBits);
        public static explicit operator float(Fixed64 value) => value.RawValue / (float)Const.One;
        public static explicit operator double(Fixed64 value) => value.RawValue / (double)Const.One;
        public static explicit operator decimal(Fixed64 value) => value.RawValue / (decimal)Const.One;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator >>(Fixed64 lhs, int rhs) => new(lhs.RawValue >> rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator <<(Fixed64 lhs, int rhs) => new(lhs.RawValue << rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator ~(Fixed64 lhs) => new(~lhs.RawValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator +(Fixed64 value) => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator -(Fixed64 value) => new(-value.RawValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator ++(Fixed64 value) => new(value.RawValue + Const.One);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator --(Fixed64 value) => new(value.RawValue - Const.One);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator +(Fixed64 lhs, Fixed64 rhs) => new(lhs.RawValue + rhs.RawValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator -(Fixed64 lhs, Fixed64 rhs) => new(lhs.RawValue - rhs.RawValue);
        public static Fixed64 operator *(Fixed64 lhs, Fixed64 rhs)
        {
            long la = Math.Abs(lhs.RawValue);
            long ra = Math.Abs(rhs.RawValue);
            long li = la >> Const.FractionalBits;
            long lf = la & Const.FractionalPart;
            long ri = ra >> Const.FractionalBits;
            long rf = ra & Const.FractionalPart;

            long ii = li * ri << Const.FractionalBits;
            long fi = li * rf + lf * ri;
            ulong ff = (ulong)lf * (ulong)rf >> Const.FractionalBits; // lf*rf可能会溢出，所以转成ulong
            return lhs.Sign() == rhs.Sign() ? new Fixed64(ii + fi + (long)ff) : new Fixed64(-ii - fi - (long)ff);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator *(Fixed64 lhs, long rhs) => new(lhs.RawValue * rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator *(long lhs, Fixed64 rhs) => new(lhs * rhs.RawValue);
        public static Fixed64 operator /(Fixed64 lhs, Fixed64 rhs)
        {
            if (lhs.RawValue is >= Const.MinPeak >> Const.FractionalBits and <= Const.MaxPeak >> Const.FractionalBits)
            {
                return new Fixed64((lhs.RawValue << Const.FractionalBits) / rhs.RawValue);
            }

            long remainder = lhs.RawValue % rhs.RawValue;
            if (remainder is >= Const.MinPeak >> Const.FractionalBits and <= Const.MaxPeak >> Const.FractionalBits)
            {
                long div = lhs.RawValue / rhs.RawValue << Const.FractionalBits;
                long mod = (remainder << Const.FractionalBits) / rhs.RawValue;
                return new Fixed64(div + mod);
            }

            long dividend = Math.Abs(lhs.RawValue); // 被除数
            long divisor = Math.Abs(rhs.RawValue); // 除数
            long quotient = 0; // 商
            for (int remainBits = Const.FractionalBits, moveBits = Const.FractionalBits >> 1;;)
            {
                #region 检测
                if (remainBits > 0 && remainBits < moveBits)
                    moveBits = remainBits;
                bool inRange = dividend >= Const.MinPeak >> moveBits && dividend <= Const.MaxPeak >> moveBits;
                bool canMove;
                int movBit;

                if (inRange)
                {
                    canMove = true;
                    movBit = moveBits;
                }
                else if (dividend >= divisor)
                {
                    canMove = true;
                    movBit = 0;
                }
                else if (moveBits > 1)
                {
                    moveBits >>= 1;
                    continue;
                }
                else
                {
                    canMove = false;
                    movBit = 0;
                }
                #endregion

                #region 计算
                if (canMove)
                {
                    long div = dividend << movBit;
                    dividend = div % divisor;
                    remainBits -= movBit;

                    if (remainBits >= 0)
                    {
                        quotient += div / divisor << remainBits;
                    }
                    else
                    {
                        long quot = div / divisor >> -remainBits;
                        if (quot == 0)
                            break;

                        quotient += quot;
                    }
                    if (dividend == 0)
                        break;
                }
                else // dividend同时满足下列条件：小于divisor，小于Const.MinPeak/2，大于Const.MaxPeak/2
                {
                    dividend += dividend - divisor;
                    --remainBits;

                    if (remainBits >= 0)
                        quotient += 1L << remainBits;
                    else
                        break;
                    if (dividend == 0)
                        break;
                }
                #endregion
            }
            bool sameSign = Math.Sign(lhs.RawValue) == Math.Sign(rhs.RawValue);
            return new Fixed64(sameSign ? quotient : -quotient);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator /(Fixed64 lhs, long rhs) => new(lhs.RawValue / rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 operator %(Fixed64 lhs, Fixed64 rhs) => new(lhs.RawValue % rhs.RawValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fixed64 lhs, Fixed64 rhs) => lhs.RawValue == rhs.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fixed64 lhs, Fixed64 rhs) => lhs.RawValue != rhs.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Fixed64 lhs, Fixed64 rhs) => lhs.RawValue >= rhs.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Fixed64 lhs, Fixed64 rhs) => lhs.RawValue <= rhs.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Fixed64 lhs, Fixed64 rhs) => lhs.RawValue > rhs.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Fixed64 lhs, Fixed64 rhs) => lhs.RawValue < rhs.RawValue;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Fixed64 other && other.RawValue == RawValue;
        public readonly override int GetHashCode() => RawValue.GetHashCode();
        public readonly bool Equals(Fixed64 other) => RawValue == other.RawValue;
        public readonly int CompareTo(Fixed64 other) => RawValue.CompareTo(other.RawValue);

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => ((double)this).ToString(format, provider);
        #endregion
    }
}