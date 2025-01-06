using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 有限小数，不包括无限循环小数<br/>
    /// 默认实现：1位符号 + 31位整数 + 32位小数<br/>
    /// 通过 “Const.FractionalPlaces” 修改小数位数
    /// </summary>
    [Serializable]
    public struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>
    {
        private long _rawValue; // 当前值

        public static readonly Fixed64 MaxValue = new(Const.MaxValue - 1);
        public static readonly Fixed64 MinValue = new(Const.MinValue + 2);
        public static readonly Fixed64 Zero = new();
        public static readonly Fixed64 One = new(Const.One);
        public static readonly Fixed64 Half = new(Const.Half);
        public static readonly Fixed64 NegativeInfinity = new(Const.MinValue + 1); // 负无穷大
        public static readonly Fixed64 Infinity = new(Const.MaxValue); // 无穷大
        public static readonly Fixed64 NaN = new(Const.MinValue);

        private static readonly Fixed64 _en1 = One / 10;
        private static readonly Fixed64 _en2 = One / 100;
        private static readonly Fixed64 _en3 = One / 1000;
        private static readonly Fixed64 _en4 = One / 10000;
        private static readonly Fixed64 _en5 = One / 100000;
        private static readonly Fixed64 _en6 = One / 1000000;
        private static readonly Fixed64 _en7 = One / 10000000;
        private static readonly Fixed64 _en8 = One / 100000000;
        public static readonly Fixed64 Epsilon = _en3;

        public static readonly Fixed64 LutInterval = (Const.TableSize - 1) / new Fixed64(Const.PiOver2);
        public static readonly Fixed64 Log2Max = new(Const.Log2Max);
        public static readonly Fixed64 Log2Min = new(Const.Log2Min);
        public static readonly Fixed64 Ln2 = new(Const.LogE2);

        #region Constructor
        internal long RawValue => _rawValue;

        public Fixed64(long rawValue)
        {
            _rawValue = rawValue;
        }
        public Fixed64(int value)
        {
            _rawValue = (long)value << Const.FractionalPlaces;
        }
        #endregion

        #region override Math Func
        /// <summary>
        /// 返回一个数字，指示一个FixFloat数字的符号。
        /// 如果值为正，返回1;如果值为0，返回0;如果值为负，返回-1。
        /// </summary>
        public static int Sign(Fixed64 value)
        {
            return value._rawValue < 0 ? -1 : value._rawValue > 0 ? 1 : 0;
        }

        /// <summary>
        /// 绝对值。
        /// Note: Abs(FixFloat.MinValue) == FixFloat.MaxValue.
        /// </summary>
        public static Fixed64 Abs(Fixed64 value)
        {
            if (value._rawValue == MinValue._rawValue)
            {
                return MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value._rawValue >> 63;
            Fixed64 result;
            result._rawValue = (value._rawValue + mask) ^ mask;
            return result;
        }

        /// <summary>
        /// 绝对值
        /// FastAbs(Fix64.MinValue) is undefined.
        /// </summary>
        public static Fixed64 FastAbs(Fixed64 value)
        {
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value._rawValue >> 63;
            Fixed64 result;
            result._rawValue = (value._rawValue + mask) ^ mask;
            return result;
        }

        /// <summary>
        /// 向下取整
        /// </summary>
        public static Fixed64 Floor(Fixed64 value)
        {
            // 分数化0
            Fixed64 result;
            result._rawValue = (long)((ulong)value._rawValue & 0xFFFFFFFF00000000);
            return result;
        }

        /// <summary>
        /// 向上取整
        /// </summary>
        public static Fixed64 Ceiling(Fixed64 value)
        {
            var hasFractionalPart = (value._rawValue & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? Floor(value) + One : value;
        }

        /// <summary>
        /// 四舍五入到最接近的整数值。
        /// 如果一个数在偶数和奇数中间，则返回最近的偶数（如3.5 -> 4， 4.5 -> 4）
        /// </summary>
        public static Fixed64 Round(Fixed64 value)
        {
            var fractionalPart = value._rawValue & 0x00000000FFFFFFFF;
            var integralPart = Floor(value);
            if (fractionalPart < 0x80000000)
            {
                return integralPart;
            }

            if (fractionalPart > 0x80000000)
            {
                return integralPart + One;
            }

            // 在偶数和奇数中间，则返回最近的偶数 
            return (integralPart._rawValue & Const.One) == 0 ? integralPart : integralPart + One;
        }

        /// <summary>
        /// 平方根
        /// </summary>
        public static Fixed64 Sqrt(Fixed64 x)
        {
            var xl = x._rawValue;
            if (xl < 0)
            {
                //无法用单倍和双倍来表示无穷大，当x为负数时抛出异常。
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", $"{x}");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << Const.NumBits - 2;

            while (bit > num)
            {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i)
            {
                // First we get the top 48 bits of the answer.
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result >>= 1;
                    }

                    bit >>= 2;
                }

                if (i == 0)
                {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (Const.NumBits >> 1)) - 1)
                    {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (Const.NumBits >> 1)) - 0x80000000UL;
                        result = (result << (Const.NumBits >> 1)) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= (Const.NumBits >> 1);
                        result <<= (Const.NumBits >> 1);
                    }

                    bit = 1UL << (Const.NumBits >> 1) - 2;
                }
            }

            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result)
            {
                ++result;
            }

            Fixed64 r;
            r._rawValue = (long)result;
            return r;
        }

        /// <summary>
        /// Returns the Sine of x.
        /// This function has about 9 decimals of accuracy for small values of x.
        /// It may lose accuracy as the value of x grows.
        /// Performance: about 25% slower than Math.Sin() in x64, and 200% slower in x86.
        /// </summary>
        public static Fixed64 Sin(Fixed64 x)
        {
            var clampedL = ClampSinValue(x._rawValue, out var flipHorizontal, out var flipVertical);
            var clamped = new Fixed64(clampedL);

            // Find the two closest values in the LUT and perform linear interpolation
            // This is what kills the performance of this function on x86 - x64 is fine though
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = 0; //FastSub(rawIndex, roundedIndex);

            var nearestValue = new Fixed64(Sine.Get(flipHorizontal ? Sine.Length - 1 - (int)roundedIndex : (int)roundedIndex));
            var secondNearestValue = new Fixed64(Sine.Get(flipHorizontal ? Sine.Length - 1 - (int)roundedIndex - Sign(indexError) : (int)roundedIndex + Sign(indexError)));

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue)))._rawValue;
            var interpolatedValue = nearestValue._rawValue + (flipHorizontal ? -delta : delta);
            var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;

            Fixed64 a2;
            a2._rawValue = finalValue;
            return a2;
        }

        /// <summary>
        /// Returns a rough approximation of the Sine of x.
        /// This is at least 3 times faster than Sin() on x86 and slightly faster than Math.Sin(),
        /// however its accuracy is limited to 4-5 decimals, for small enough values of x.
        /// </summary>
        public static Fixed64 FastSin(Fixed64 x)
        {
            var clampedL = ClampSinValue(x._rawValue, out var flipHorizontal, out var flipVertical);

            // Here we use the fact that the LUTSin.table table has a number of entries
            // equal to (PiOver2 >> 15) to use the angle to index directly into it
            var rawIndex = (uint)(clampedL >> 15);
            if (rawIndex >= Const.TableSize)
            {
                rawIndex = Const.TableSize - 1;
            }

            var nearestValue = Sine.Get(flipHorizontal ? Sine.Length - 1 - (int)rawIndex : (int)rawIndex);

            Fixed64 result;
            result._rawValue = flipVertical ? -nearestValue : nearestValue;
            return result;
        }

        public static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical)
        {
            // Clamp value to 0 - 2*Pi using modulo; this is very slow but there's no better way AFAIK
            var clamped2Pi = angle % Const.PiTimes2;
            if (angle < 0)
            {
                clamped2Pi += Const.PiTimes2;
            }

            // The LUT contains values for 0 - Math.PIOver2; every other value must be obtained by
            // vertical or horizontal mirroring
            flipVertical = clamped2Pi >= Const.Pi;
            // obtain (angle % Pi) from (angle % 2PI) - much faster than doing another modulo
            var clampedPi = clamped2Pi;
            while (clampedPi >= Const.Pi)
            {
                clampedPi -= Const.Pi;
            }

            flipHorizontal = clampedPi >= Const.PiOver2;
            // obtain (angle % PiOver2) from (angle % Pi) - much faster than doing another modulo
            var clampedPiOver2 = clampedPi;
            if (clampedPiOver2 >= Const.PiOver2)
            {
                clampedPiOver2 -= Const.PiOver2;
            }

            return clampedPiOver2;
        }

        /// <summary>
        /// Returns the cosine of x.
        /// See Sin() for more details.
        /// </summary>
        public static Fixed64 Cos(Fixed64 x)
        {
            var xl = x._rawValue;
            var rawAngle = xl + (xl > 0 ? -Const.Pi - Const.PiOver2 : Const.PiOver2);
            Fixed64 a2 = Sin(new Fixed64(rawAngle));
            return a2;
        }

        /// <summary>
        /// Returns a rough approximation of the cosine of x.
        /// See FastSin for more details.
        /// </summary>
        public static Fixed64 FastCos(Fixed64 x)
        {
            var xl = x._rawValue;
            var rawAngle = xl + (xl > 0 ? -Const.Pi - Const.PiOver2 : Const.PiOver2);
            return FastSin(new Fixed64(rawAngle));
        }

        /// <summary>
        /// Returns the tangent of x.
        /// </summary>
        /// <remarks>
        /// This function is not well-tested. It may be wildly inaccurate.
        /// </remarks>
        public static Fixed64 Tan(Fixed64 x)
        {
            var clampedPi = x._rawValue % Const.Pi;
            var flip = false;
            if (clampedPi < 0)
            {
                clampedPi = -clampedPi;
                flip = true;
            }

            if (clampedPi > Const.PiOver2)
            {
                flip = !flip;
                clampedPi = Const.PiOver2 - (clampedPi - Const.PiOver2);
            }

            var clamped = new Fixed64(clampedPi);

            // Find the two closest values in the LUT and perform linear interpolation
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = FastSub(rawIndex, roundedIndex);

            var nearestValue = new Fixed64(Tangent.Get(roundedIndex));
            var secondNearestValue = new Fixed64(Tangent.Get((int)roundedIndex + Sign(indexError)));

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue)))._rawValue;
            var interpolatedValue = nearestValue._rawValue + delta;
            var finalValue = flip ? -interpolatedValue : interpolatedValue;
            Fixed64 a2 = new Fixed64(finalValue);
            return a2;
        }

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

            Fixed64 result;
            var two = (Fixed64)2;
            var three = (Fixed64)3;

            bool invert = z > One;
            if (invert)
                z = One / z;

            result = One;
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
                result = Maths.PIOver2 - result;
            }

            if (neg)
            {
                result = -result;
            }

            return result;
        }

        public static Fixed64 Atan2(Fixed64 y, Fixed64 x)
        {
            var yl = y._rawValue;
            var xl = x._rawValue;
            if (xl == 0)
            {
                if (yl > 0)
                {
                    return Maths.PIOver2;
                }

                if (yl == 0)
                {
                    return Zero;
                }

                return -Maths.PIOver2;
            }

            Fixed64 atan;
            var z = y / x;

            Fixed64 sm = Fixed64._en2 * 28;
            // Deal with overflow
            if (One + sm * z * z == MaxValue)
            {
                return y < Zero ? -Maths.PIOver2 : Maths.PIOver2;
            }

            if (Abs(z) < One)
            {
                atan = z / (One + sm * z * z);
                if (xl < 0)
                {
                    if (yl < 0)
                    {
                        return atan - Maths.PI;
                    }

                    return atan + Maths.PI;
                }
            }
            else
            {
                atan = Maths.PIOver2 - z / (z * z + sm);
                if (yl < 0)
                {
                    return atan - Maths.PI;
                }
            }

            return atan;
        }

        public static Fixed64 Asin(Fixed64 value)
        {
            return FastSub(Maths.PIOver2, Acos(value));
        }

        /// <summary>
        /// Returns the arccos of of the specified number, calculated using Atan and Sqrt
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static Fixed64 Acos(Fixed64 x)
        {
            if (x < -One || x > One)
            {
                throw new ArgumentOutOfRangeException("Must between -FP.One and FP.One", "x");
            }

            if (x.RawValue == 0)
                return Maths.PIOver2;

            var result = Atan(Sqrt(One - x * x) / x);
            return x.RawValue < 0 ? result + Maths.PI : result;
        }
        #endregion

        #region override operator
        /// <summary>
        /// 相加。
        /// 无溢出检测
        /// </summary>
        public static Fixed64 operator +(Fixed64 x, Fixed64 y)
        {
            Fixed64 result;
            result._rawValue = x._rawValue + y._rawValue;
            return result;
        }

        /// <summary>
        /// 相减。
        /// 无溢出检测
        /// </summary>
        public static Fixed64 operator -(Fixed64 x, Fixed64 y)
        {
            Fixed64 result;
            result._rawValue = x._rawValue - y._rawValue;
            return result;
        }

        /// <summary>
        /// 相乘
        /// 无溢出检测
        /// </summary>
        public static Fixed64 operator *(Fixed64 x, Fixed64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> Const.FractionalPlaces;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> Const.FractionalPlaces;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> Const.FractionalPlaces;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << Const.FractionalPlaces;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            Fixed64 result;
            result._rawValue = sum;
            return result;
        }

        /// <summary>
        /// 溢出检测相除
        /// </summary>
        public static Fixed64 operator /(Fixed64 x, Fixed64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;

            if (yl == 0)
            {
                return Const.MaxValue;
            }

            var remainder = (ulong)(xl >= 0 ? xl : -xl);
            var divider = (ulong)(yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = (Const.NumBits >> 1) + 1;

            // 除数可被2^n整除
            while ((divider & 0xF) == 0 && bitPos >= 4)
            {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0)
            {
                int shift = CountLeadingZeroes(remainder);
                if (shift > bitPos)
                {
                    shift = bitPos;
                }

                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder %= divider;
                quotient += div << bitPos;

                // 溢出检测
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
                {
                    return ((xl ^ yl) & Const.MinValue) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // 四舍五入
            ++quotient;
            var result = (long)(quotient >> 1);
            if (((xl ^ yl) & Const.MinValue) != 0)
            {
                result = -result;
            }

            Fixed64 r;
            r._rawValue = result;
            return r;
        }

        /// <summary>
        /// 取余
        /// </summary>
        public static Fixed64 operator %(Fixed64 x, Fixed64 y)
        {
            Fixed64 result;
            result._rawValue = x._rawValue == Const.MinValue & y._rawValue == -1 ? 0 : x._rawValue % y._rawValue;
            return result;
        }

        public static Fixed64 operator -(Fixed64 x)
        {
            return x._rawValue == Const.MinValue ? MaxValue : new Fixed64(-x._rawValue);
        }

        public static bool operator ==(Fixed64 x, Fixed64 y)
        {
            return x._rawValue == y._rawValue;
        }

        public static bool operator !=(Fixed64 x, Fixed64 y)
        {
            return x._rawValue != y._rawValue;
        }

        public static bool operator >(Fixed64 x, Fixed64 y)
        {
            return x._rawValue > y._rawValue;
        }

        public static bool operator <(Fixed64 x, Fixed64 y)
        {
            return x._rawValue < y._rawValue;
        }

        public static bool operator >=(Fixed64 x, Fixed64 y)
        {
            return x._rawValue >= y._rawValue;
        }

        public static bool operator <=(Fixed64 x, Fixed64 y)
        {
            return x._rawValue <= y._rawValue;
        }
        #endregion

        #region override object func
        /// <summary>
        /// 溢出检测相加
        /// 以操作数的符号确定溢出取最大或最小值
        /// </summary>
        public static Fixed64 OverflowAdd(Fixed64 x, Fixed64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if (((~(xl ^ yl) & (xl ^ sum)) & Const.MinValue) != 0)
            {
                sum = xl > 0 ? Const.MaxValue : Const.MinValue;
            }

            Fixed64 result;
            result._rawValue = sum;
            return result;
        }

        /// <summary>
        /// 相加。
        /// 无溢出检测
        /// </summary>
        public static Fixed64 FastAdd(Fixed64 x, Fixed64 y)
        {
            Fixed64 result;
            result._rawValue = x._rawValue + y._rawValue;
            return result;
        }

        /// <summary>
        /// 溢出检测相减
        /// 以操作数的符号确定溢出取最大或最小值
        /// </summary>
        public static Fixed64 OverflowSub(Fixed64 x, Fixed64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if ((((xl ^ yl) & (xl ^ diff)) & Const.MinValue) != 0)
            {
                diff = xl < 0 ? Const.MinValue : Const.MaxValue;
            }

            Fixed64 result;
            result._rawValue = diff;
            return result;
        }

        /// <summary>
        /// 相减
        /// 无溢出检测
        /// </summary>
        public static Fixed64 FastSub(Fixed64 x, Fixed64 y)
        {
            return new Fixed64(x._rawValue - y._rawValue);
        }

        static long AddOverflowHelper(long x, long y, ref bool overflow)
        {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & Const.MinValue) != 0;
            return sum;
        }

        /// <summary>
        /// 溢出检测相乘
        /// 保证乘积值不会溢出
        /// </summary>
        public static Fixed64 OverflowMul(Fixed64 x, Fixed64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> Const.FractionalPlaces;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> Const.FractionalPlaces;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> Const.FractionalPlaces;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << Const.FractionalPlaces;

            bool overflow = false;
            var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            bool opSignsEqual = ((xl ^ yl) & Const.MinValue) == 0;

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
            var topCarry = hihi >> Const.FractionalPlaces;
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

            Fixed64 result;
            result._rawValue = sum;
            return result;
        }

        /// <summary>
        /// 相乘
        /// 无溢出检测（对于确保值不溢出的运算效率更高）
        /// </summary>
        public static Fixed64 FastMul(Fixed64 x, Fixed64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> Const.FractionalPlaces;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> Const.FractionalPlaces;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> Const.FractionalPlaces;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << Const.FractionalPlaces;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            Fixed64 result; // = default(FP);
            result._rawValue = sum;
            return result;
        }

        static int CountLeadingZeroes(ulong x)
        {
            int result = 0;
            while ((x & 0xF000000000000000) == 0)
            {
                result += 4;
                x <<= 4;
            }

            while ((x & 0x8000000000000000) == 0)
            {
                result += 1;
                x <<= 1;
            }

            return result;
        }
        #endregion

        #region override type convert
        public static implicit operator Fixed64(long value)
        {
            Fixed64 result;
            result._rawValue = value * Const.One;
            return result;
        }

        public static explicit operator long(Fixed64 value)
        {
            return value._rawValue >> Const.FractionalPlaces;
        }

        public static implicit operator Fixed64(float value)
        {
            Fixed64 result;
            result._rawValue = (long)(value * Const.One);
            return result;
        }

        public static explicit operator float(Fixed64 value)
        {
            return (float)value._rawValue / Const.One;
        }

        public static implicit operator Fixed64(double value)
        {
            Fixed64 result;
            result._rawValue = (long)(value * Const.One);
            return result;
        }

        public static explicit operator double(Fixed64 value)
        {
            return (double)value._rawValue / Const.One;
        }

        public static explicit operator Fixed64(decimal value)
        {
            Fixed64 result;
            result._rawValue = (long)(value * Const.One);
            return result;
        }

        public static implicit operator Fixed64(int value)
        {
            Fixed64 result;
            result._rawValue = value * Const.One;
            return result;
        }

        public static explicit operator decimal(Fixed64 value)
        {
            return (decimal)value._rawValue / Const.One;
        }

        public float AsFloat()
        {
            return (float)this;
        }

        public int AsInt()
        {
            return (int)this;
        }

        public long AsLong()
        {
            return (long)this;
        }

        public double AsDouble()
        {
            return (double)this;
        }

        public decimal AsDecimal()
        {
            return (decimal)this;
        }

        public static float ToFloat(Fixed64 value)
        {
            return (float)value;
        }

        public static int ToInt(Fixed64 value)
        {
            return (int)value;
        }

        public static Fixed64 FromFloat(float value)
        {
            return (Fixed64)value;
        }

        public static bool IsInfinity(Fixed64 value)
        {
            return value == NegativeInfinity || value == Infinity;
        }

        public static bool IsNaN(Fixed64 value)
        {
            return value == NaN;
        }

        public override bool Equals(object obj)
        {
            return obj is Fixed64 && ((Fixed64)obj)._rawValue == _rawValue;
        }

        public override int GetHashCode()
        {
            return _rawValue.GetHashCode();
        }

        public bool Equals(Fixed64 other)
        {
            return _rawValue == other._rawValue;
        }

        public int CompareTo(Fixed64 other)
        {
            return _rawValue.CompareTo(other._rawValue);
        }

        public override string ToString()
        {
            return ((float)this).ToString();
        }

        public string ToString(IFormatProvider provider)
        {
            return ((float)this).ToString(provider);
        }
        public string ToString(string format)
        {
            return ((float)this).ToString(format);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return ((float)this).ToString(format, provider);
        }

        public static Fixed64 FromRaw(long rawValue)
        {
            return new Fixed64(rawValue);
        }
        #endregion
    }
}