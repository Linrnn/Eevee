﻿using Eevee.Log;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 有限小数，不包括无限循环小数<br/>
    /// 默认实现：1位符号 + 31位整数 + 32位小数<br/>
    /// 通过 “Const.FractionalPlaces” 修改小数位数
    /// </summary>
    [Serializable]
    public readonly struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>, IFormattable
    {
        #region 字段 & 构造函数
        public static readonly Fixed64 Zero = default; // 数字：0
        public static readonly Fixed64 Half = new(Const.Half); // 数字：0.5
        public static readonly Fixed64 One = new(Const.One); // 数字：1
        public static readonly Fixed64 MinValue = new(Const.MinValue); // 最小值
        public static readonly Fixed64 MaxValue = new(Const.MaxValue); // 最大值
        public static readonly Fixed64 Infinitesimal = new(Const.Infinitesimal); // 无穷小
        public static readonly Fixed64 Infinity = new(Const.Infinity); // 无穷大
        public static readonly Fixed64 NaN = new(Const.MinPeak); // 非数，不存在的数

        private static readonly Fixed64 _en2 = One / 100L;

        public static readonly Fixed64 Log2Max = new(Const.Log2Max);
        public static readonly Fixed64 Log2Min = new(Const.Log2Min);
        public static readonly Fixed64 Ln2 = new(Const.LogE2);

        public readonly long RawValue;
        public Fixed64(long rawValue) => RawValue = rawValue;
        #endregion

        #region 数字转换
        /// <summary>
        /// 符号<br/>
        /// 大于0，返回1<br/>
        /// 等于0，返回0<br/>
        /// 小于0，返回-1
        /// </summary>
        public int Sign() => Math.Sign(RawValue);
        /// <summary>
        /// 绝对值
        /// </summary>
        public Fixed64 Abs() => RawValue >= 0L ? this : -this;

        /// <summary>
        /// 向上取整
        /// </summary>
        public Fixed64 Ceiling() => (RawValue & Const.FractionalPart) == 0 ? this : Floor() + One;
        /// <summary>
        /// 向下取整
        /// </summary>
        public Fixed64 Floor() => new(RawValue & Const.IntegerPart);
        /// <summary>
        /// 四舍五入到最接近的整数值<br/>
        /// 如果一个数在偶数和奇数中间，则返回最近的偶数（如：3.5 -> 4，4.5 -> 4）
        /// </summary>
        public Fixed64 Round() => (RawValue & Const.FractionalPart) switch
        {
            < Const.Half => Floor(),
            > Const.Half => Ceiling(),
            _ => (RawValue & Const.One) == 0L ? Floor() : Ceiling(),
        };

        /// <summary>
        /// 倒数，-1次方
        /// </summary>
        public Fixed64 Reciprocal() => One / this;
        /// <summary>
        /// 平方根，即开方，1/2次方
        /// </summary>
        public Fixed64 Sqrt()
        {
            switch (RawValue)
            {
                case < 0L:
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
        public Fixed64 Sqr() => this * this;
        #endregion

        #region 三角函数
        /// <summary>
        /// Returns the arctan of of the specified number, calculated using Euler series
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static Fixed64 Atan(Fixed64 z)
        {
            if (z.RawValue == 0)
                return Zero;

            // Force positive values for argument
            // Atan(-z) = -Atan(z).
            var neg = z.RawValue < 0;
            if (neg)
            {
                z = -z;
            }

            Fixed64 two = 2;
            Fixed64 three = 3;

            bool invert = z > One;
            if (invert)
                z = z.Reciprocal();

            var result = One;
            var term = One;

            var zSq = z * z;
            var zSq2 = zSq * two;
            var zSqPlusOne = zSq + One;
            var zSq12 = zSqPlusOne * two;
            var dividend = zSq2;
            var divisor = zSqPlusOne * three;

            for (var i = 2; i < 30; ++i)
            {
                term *= dividend / divisor;
                result += term;

                dividend += zSq2;
                divisor += zSq12;

                if (term.RawValue == 0)
                    break;
            }

            result = result * z / zSqPlusOne;

            if (invert)
            {
                result = Maths.PiOver2 - result;
            }

            if (neg)
            {
                result = -result;
            }

            return result;
        }

        public static Fixed64 Atan2(Fixed64 y, Fixed64 x)
        {
            var yl = y.RawValue;
            var xl = x.RawValue;
            if (xl == 0)
            {
                if (yl > 0)
                {
                    return Maths.PiOver2;
                }

                if (yl == 0)
                {
                    return Zero;
                }

                return -Maths.PiOver2;
            }

            Fixed64 atan;
            var z = y / x;

            Fixed64 sm = _en2 * 28;
            // Deal with overflow
            if (One + sm * z * z == MaxValue)
            {
                return y < Zero ? -Maths.PiOver2 : Maths.PiOver2;
            }

            if (z.Abs() < One)
            {
                atan = z / (One + sm * z * z);
                if (xl < 0)
                {
                    if (yl < 0)
                    {
                        return atan - Maths.Rad180;
                    }

                    return atan + Maths.Rad180;
                }
            }
            else
            {
                atan = Maths.PiOver2 - z / (z * z + sm);
                if (yl < 0)
                {
                    return atan - Maths.Rad180;
                }
            }

            return atan;
        }
        #endregion

        #region 判断
        public float AsFloat() => (float)this;
        public double AsDouble() => (double)this;
        public decimal AsDecimal() => (decimal)this;

        public bool IsInfinity() => RawValue is Const.Infinitesimal or Const.Infinity;
        public bool IsNaN() => RawValue == Const.MinPeak;
        #endregion

        #region 隐式/显示转换
        public static implicit operator Fixed64(long value) => new(value << Const.FractionalBits);
        public static implicit operator Fixed64(float value) => new((long)(value * Const.One));
        public static implicit operator Fixed64(double value) => new((long)(value * Const.One));
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
        #endregion

        #region 运算符重载
        public static Fixed64 operator +(Fixed64 value) => value;
        public static Fixed64 operator -(Fixed64 value) => new(-value.RawValue);
        public static Fixed64 operator ++(Fixed64 value) => new(value.RawValue + Const.One);
        public static Fixed64 operator --(Fixed64 value) => new(value.RawValue - Const.One);

        public static Fixed64 operator >>(Fixed64 left, int right) => new(left.RawValue >> right);
        public static Fixed64 operator <<(Fixed64 left, int right) => new(left.RawValue << right);
        public static Fixed64 operator +(Fixed64 left, Fixed64 right) => new(left.RawValue + right.RawValue);
        public static Fixed64 operator -(Fixed64 left, Fixed64 right) => new(left.RawValue - right.RawValue);
        public static Fixed64 operator %(Fixed64 left, Fixed64 right) => new(left.RawValue % right.RawValue);

        public static Fixed64 operator *(Fixed64 left, Fixed64 right)
        {
            long li = left.RawValue >> Const.FractionalBits;
            long lf = left.RawValue & Const.FractionalPart;
            long ri = right.RawValue >> Const.FractionalBits;
            long rf = right.RawValue & Const.FractionalPart;

            long ii = li * ri << Const.FractionalBits;
            long fi = li * rf + lf * ri;
            ulong ff = (ulong)lf * (ulong)rf >> Const.FractionalBits; // lf*lf可能会溢出，所以转成ulong
            return new Fixed64(ii + fi + (long)ff);
        }
        public static Fixed64 operator *(Fixed64 left, long right) => new(left.RawValue * right);
        public static Fixed64 operator *(long left, Fixed64 right) => new(left * right.RawValue);
        public static Fixed64 operator /(Fixed64 left, Fixed64 right)
        {
            if (left.RawValue is >= Const.MinPeak >> Const.FractionalBits and <= Const.MaxPeak >> Const.FractionalBits)
            {
                return new Fixed64((left.RawValue << Const.FractionalBits) / right.RawValue);
            }

            long remainder = left.RawValue % right.RawValue;
            if (remainder is >= Const.MinPeak >> Const.FractionalBits and <= Const.MaxPeak >> Const.FractionalBits)
            {
                long div = left.RawValue / right.RawValue << Const.FractionalBits;
                long mod = (remainder << Const.FractionalBits) / right.RawValue;
                return new Fixed64(div + mod);
            }

            long dividend = Math.Abs(left.RawValue); // 被除数
            long divisor = Math.Abs(right.RawValue); // 除数
            long quotient = 0L; // 商
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
                        if (quot == 0L)
                            break;

                        quotient += quot;
                    }
                    if (dividend == 0L)
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
                    if (dividend == 0L)
                        break;
                }
                #endregion
            }
            bool sameSign = Math.Sign(left.RawValue) == Math.Sign(right.RawValue);
            return new Fixed64(sameSign ? quotient : -quotient);
        }
        public static Fixed64 operator /(Fixed64 left, long right) => new(left.RawValue / right);

        public static bool operator ==(Fixed64 left, Fixed64 right) => left.RawValue == right.RawValue;
        public static bool operator !=(Fixed64 left, Fixed64 right) => left.RawValue != right.RawValue;
        public static bool operator >=(Fixed64 left, Fixed64 right) => left.RawValue >= right.RawValue;
        public static bool operator <=(Fixed64 left, Fixed64 right) => left.RawValue <= right.RawValue;
        public static bool operator >(Fixed64 left, Fixed64 right) => left.RawValue > right.RawValue;
        public static bool operator <(Fixed64 left, Fixed64 right) => left.RawValue < right.RawValue;
        #endregion

        #region 继承重载
        public override bool Equals(object obj) => obj is Fixed64 number && number.RawValue == RawValue;
        public override int GetHashCode() => RawValue.GetHashCode();
        public bool Equals(Fixed64 other) => RawValue == other.RawValue;
        public int CompareTo(Fixed64 other) => RawValue.CompareTo(other.RawValue);

        public override string ToString() => ((float)this).ToString();
        public string ToString(string format) => ((float)this).ToString(format);
        public string ToString(IFormatProvider provider) => ((float)this).ToString(provider);
        public string ToString(string format, IFormatProvider provider) => ((float)this).ToString(format, provider);
        #endregion

        #region override object func
        /// <summary>
        /// 溢出检测相加
        /// 以操作数的符号确定溢出取最大或最小值
        /// </summary>
        public static Fixed64 OverflowAdd(Fixed64 x, Fixed64 y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if (((~(xl ^ yl) & (xl ^ sum)) & Const.MinPeak) != 0)
            {
                sum = xl > 0 ? Const.MaxPeak : Const.MinPeak;
            }

            return new Fixed64(sum);
        }

        /// <summary>
        /// 相加。
        /// 无溢出检测
        /// </summary>
        public static Fixed64 FastAdd(Fixed64 x, Fixed64 y) => new(x.RawValue + y.RawValue);

        /// <summary>
        /// 溢出检测相减
        /// 以操作数的符号确定溢出取最大或最小值
        /// </summary>
        public static Fixed64 OverflowSub(Fixed64 x, Fixed64 y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if (((xl ^ yl) & (xl ^ diff) & Const.MinPeak) != 0)
            {
                diff = xl < 0 ? Const.MinPeak : Const.MaxPeak;
            }

            return new Fixed64(diff);
        }

        /// <summary>
        /// 相减
        /// 无溢出检测
        /// </summary>
        public static Fixed64 FastSub(Fixed64 x, Fixed64 y)
        {
            return new Fixed64(x.RawValue - y.RawValue);
        }

        static long AddOverflowHelper(long x, long y, ref bool overflow)
        {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & Const.MinPeak) != 0;
            return sum;
        }

        /// <summary>
        /// 溢出检测相乘
        /// 保证乘积值不会溢出
        /// </summary>
        public static Fixed64 OverflowMul(Fixed64 x, Fixed64 y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;

            var xlo = (ulong)(xl & Const.FractionalPart);
            var xhi = xl >> Const.FractionalBits;
            var ylo = (ulong)(yl & Const.FractionalPart);
            var yhi = yl >> Const.FractionalBits;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> Const.FractionalBits;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << Const.FractionalBits;

            bool overflow = false;
            var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            bool opSignsEqual = ((xl ^ yl) & Const.MinPeak) == 0;

            // 如果操作数的符号相等而结果的符号为负，则乘法溢出为正，反之亦然
            if (opSignsEqual)
            {
                if (sum < 0 || overflow && xl > 0)
                {
                    return MaxValue;
                }
            }
            else
            {
                if (sum > 0)
                {
                    return MinValue;
                }
            }

            // 如果hihi（高32位乘积）的前32位(未在结果中使用)不是全0或全1，那么这意味着结果溢出。
            var topCarry = hihi >> Const.FractionalBits;
            if (topCarry != 0 && topCarry != -1)
            {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // 如果符号不同，两个操作数的大小都大于1，并且结果大于负操作数，则存在负溢出。
            if (!opSignsEqual)
            {
                long posOp, negOp;
                if (xl > yl)
                {
                    posOp = xl;
                    negOp = yl;
                }
                else
                {
                    posOp = yl;
                    negOp = xl;
                }

                if (sum > negOp && negOp < -Const.One && posOp > Const.One)
                {
                    return MinValue;
                }
            }

            return new Fixed64(sum);
        }

        /// <summary>
        /// 相乘
        /// 无溢出检测（对于确保值不溢出的运算效率更高）
        /// </summary>
        public static Fixed64 FastMul(Fixed64 x, Fixed64 y)
        {
            long xl = x.RawValue;
            long yl = y.RawValue;

            ulong xlo = (ulong)(xl & Const.FractionalPart);
            long xhi = xl >> Const.FractionalBits;
            ulong ylo = (ulong)(yl & Const.FractionalPart);
            long yhi = yl >> Const.FractionalBits;

            ulong lolo = xlo * ylo;
            long lohi = (long)xlo * yhi;
            long hilo = xhi * (long)ylo;
            long hihi = xhi * yhi;

            ulong loResult = lolo >> Const.FractionalBits;
            long midResult1 = lohi;
            long midResult2 = hilo;
            long hiResult = hihi << Const.FractionalBits;

            return new Fixed64((long)loResult + midResult1 + midResult2 + hiResult);
        }
        #endregion
    }
}