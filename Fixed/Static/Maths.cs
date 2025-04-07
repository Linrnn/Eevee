using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 数学相关
    /// </summary>
    public readonly struct Maths
    {
        #region 字段
        public static readonly Fixed64 Ln2 = new(Const.Ln2);
        public static readonly Fixed64 Lg2 = new(Const.Lg2);
        public static readonly Fixed64 Pi = new(Const.Rad180);
        public static readonly Fixed64 Deg2Rad = new(Const.Deg2Rad);
        public static readonly Fixed64 Rad2Deg = new(Const.Rad2Deg);

        public static readonly Fixed64 Rad30 = new(Const.Rad30);
        public static readonly Fixed64 Rad45 = new(Const.Rad45);
        public static readonly Fixed64 Rad60 = new(Const.Rad60);
        public static readonly Fixed64 Rad90 = new(Const.Rad90);
        public static readonly Fixed64 Rad120 = new(Const.Rad120);
        public static readonly Fixed64 Rad135 = new(Const.Rad135);
        public static readonly Fixed64 Rad180 = new(Const.Rad180);
        public static readonly Fixed64 Rad360 = new(Const.Rad360);

        public static readonly Fixed64 Deg30 = new(Const.Deg30);
        public static readonly Fixed64 Deg45 = new(Const.Deg45);
        public static readonly Fixed64 Deg60 = new(Const.Deg60);
        public static readonly Fixed64 Deg90 = new(Const.Deg90);
        public static readonly Fixed64 Deg120 = new(Const.Deg120);
        public static readonly Fixed64 Deg180 = new(Const.Deg180);
        public static readonly Fixed64 Deg360 = new(Const.Deg360);
        #endregion

        #region 三角函数
        /// <summary>
        /// 输入弧度，计算正弦
        /// </summary>
        public static Fixed64 Sin(Fixed64 rad)
        {
            var value = ClampRad(rad);
            return value.RawValue switch
            {
                Const.Zero => Fixed64.Zero,
                Const.Rad30 => Fixed64.Half,
                < Const.Rad90 => Trigonometric.Sine(value),
                Const.Rad90 => Fixed64.One,
                Const.Rad150 => Fixed64.Half,
                < Const.Rad180 => Trigonometric.Sine(Rad180 - value),
                Const.Rad180 => Fixed64.Zero,
                Const.Rad210 => -Fixed64.Half,
                < Const.Rad270 => -Trigonometric.Sine(value - Rad180),
                Const.Rad270 => -Fixed64.One,
                Const.Rad330 => -Fixed64.Half,
                _ => -Trigonometric.Sine(Rad360 - value),
            };
        }
        /// <summary>
        /// 输入角度，计算正弦
        /// </summary>
        public static Fixed64 SinDeg(Fixed64 deg)
        {
            var value = ClampDeg(deg);
            return value.RawValue switch
            {
                Const.Zero => Fixed64.Zero,
                Const.Deg30 => Fixed64.Half,
                < Const.Deg90 => Trigonometric.Sine(value * Deg2Rad),
                Const.Deg90 => Fixed64.One,
                Const.Deg150 => Fixed64.Half,
                < Const.Deg180 => Trigonometric.Sine((Deg180 - value) * Deg2Rad),
                Const.Deg180 => Fixed64.Zero,
                Const.Deg210 => -Fixed64.Half,
                < Const.Deg270 => -Trigonometric.Sine((value - Deg180) * Deg2Rad),
                Const.Deg270 => -Fixed64.One,
                Const.Deg330 => -Fixed64.Half,
                _ => -Trigonometric.Sine((Deg360 - value) * Deg2Rad),
            };
        }

        /// <summary>
        /// 输入弧度，计算余弦
        /// </summary>
        public static Fixed64 Cos(Fixed64 rad)
        {
            var value = ClampRad(rad);
            return value.RawValue switch
            {
                Const.Zero => Fixed64.One,
                Const.Rad60 => Fixed64.Half,
                < Const.Rad90 => Trigonometric.Cosine(value),
                Const.Rad90 => Fixed64.Zero,
                Const.Rad120 => -Fixed64.Half,
                < Const.Rad180 => -Trigonometric.Cosine(Rad180 - value),
                Const.Rad180 => -Fixed64.One,
                Const.Rad240 => -Fixed64.Half,
                < Const.Rad270 => -Trigonometric.Cosine(value - Rad180),
                Const.Rad270 => Fixed64.Zero,
                Const.Rad300 => Fixed64.Half,
                _ => Trigonometric.Cosine(Rad360 - value),
            };
        }
        /// <summary>
        /// 输入角度，计算余弦
        /// </summary>
        public static Fixed64 CosDeg(Fixed64 deg) => SinDeg(deg + Deg90);

        /// <summary>
        /// 输入弧度，计算正切
        /// </summary>
        public static Fixed64 Tan(Fixed64 rad)
        {
            var value = ClampFull(rad, Rad180);
            return value.RawValue switch
            {
                Const.Zero => Fixed64.Zero,
                Const.Rad45 => Fixed64.One,
                < Const.Rad90 => Trigonometric.Cotangent(Rad90 - value),
                Const.Rad90 => throw new DivideByZeroException("[Fixed] Tan无法计算90°，因为是无穷大"),
                Const.Rad135 => -Fixed64.One,
                _ => -Trigonometric.Cotangent(value - Rad90),
            };
        }
        /// <summary>
        /// 输入角度，计算正切
        /// </summary>
        public static Fixed64 TanDeg(Fixed64 deg)
        {
            var value = ClampFull(deg, Deg180);
            return value.RawValue switch
            {
                Const.Zero => Fixed64.Zero,
                Const.Deg45 => Fixed64.One,
                < Const.Deg90 => Trigonometric.Cotangent((Deg90 - value) * Deg2Rad),
                Const.Deg90 => throw new DivideByZeroException("[Fixed] Tan无法计算90°，因为是无穷大"),
                Const.Deg135 => -Fixed64.One,
                _ => -Trigonometric.Cotangent((value - Deg90) * Deg2Rad),
            };
        }

        /// <summary>
        /// 输入弧度，计算余切
        /// </summary>
        public static Fixed64 Cot(Fixed64 rad)
        {
            var value = ClampFull(rad, Rad180);
            return value.RawValue switch
            {
                Const.Zero => throw new DivideByZeroException("[Fixed] Cot无法计算0°，因为是无穷大"),
                Const.Rad45 => Fixed64.One,
                < Const.Rad90 => Trigonometric.Cotangent(value),
                Const.Rad90 => Fixed64.Zero,
                Const.Rad135 => -Fixed64.One,
                _ => -Trigonometric.Cotangent(Rad180 - value),
            };
        }
        /// <summary>
        /// 输入角度，计算余切
        /// </summary>
        public static Fixed64 CotDeg(Fixed64 deg)
        {
            var value = ClampFull(deg, Deg180);
            return value.RawValue switch
            {
                Const.Zero => throw new DivideByZeroException("[Fixed] Cot无法计算0°，因为是无穷大"),
                Const.Deg45 => Fixed64.One,
                < Const.Deg90 => Trigonometric.Cotangent(value * Deg2Rad),
                Const.Deg90 => Fixed64.Zero,
                Const.Deg135 => -Fixed64.One,
                _ => -Trigonometric.Cotangent((Deg180 - value) * Deg2Rad),
            };
        }
        #endregion

        #region 反三角函数
        /// <summary>
        /// 计算反正弦，返回弧度<br/>
        /// 值域：[-π/2, π/2]
        /// </summary>
        public static Fixed64 Asin(Fixed64 value)
        {
            switch (value.RawValue)
            {
                case < -Const.One: return Fixed64.NaN;
                case -Const.One: return -Rad90;
                case -Const.Half: return -Rad30;
                case Const.Zero: return Fixed64.Zero;
                case Const.Half: return Rad30;
                case Const.One: return Rad90;
                case > Const.One: return Fixed64.NaN;
                default:
                    var rad = Atan((Fixed64.One - value.Sqr()).Sqrt() / value);
                    return value.RawValue < 0 ? -Rad90 - rad : Rad90 - rad;
            }
        }
        /// <summary>
        /// 计算反正弦，返回角度<br/>
        /// 值域：[-90°, 90°]
        /// </summary>
        public static Fixed64 AsinDeg(Fixed64 value)
        {
            switch (value.RawValue)
            {
                case < -Const.One: return Fixed64.NaN;
                case -Const.One: return -Deg90;
                case -Const.Half: return -Deg30;
                case Const.Zero: return Fixed64.Zero;
                case Const.Half: return Deg30;
                case Const.One: return Deg90;
                case > Const.One: return Fixed64.NaN;
                default:
                    var deg = AtanDeg((Fixed64.One - value.Sqr()).Sqrt() / value);
                    return value.RawValue < 0 ? -Deg90 - deg : Deg90 - deg;
            }
        }

        /// <summary>
        /// 计算反余弦，返回弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 Acos(Fixed64 value)
        {
            switch (value.RawValue)
            {
                case < -Const.One: return Fixed64.NaN;
                case -Const.One: return Rad180;
                case -Const.Half: return Rad120;
                case Const.Zero: return Rad90;
                case Const.Half: return Rad60;
                case Const.One: return Fixed64.Zero;
                case > Const.One: return Fixed64.NaN;
                default:
                    var rad = Atan((Fixed64.One - value.Sqr()).Sqrt() / value);
                    return value.RawValue < 0 ? Rad180 + rad : rad;
            }
        }
        /// <summary>
        /// 计算反余弦，返回角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 AcosDeg(Fixed64 value)
        {
            switch (value.RawValue)
            {
                case < -Const.One: return Fixed64.NaN;
                case -Const.One: return Deg180;
                case -Const.Half: return Deg120;
                case Const.Zero: return Deg90;
                case Const.Half: return Deg60;
                case Const.One: return Fixed64.Zero;
                case > Const.One: return Fixed64.NaN;
                default:
                    var deg = AtanDeg((Fixed64.One - value.Sqr()).Sqrt() / value);
                    return value.RawValue < 0 ? Deg180 + deg : deg;
            }
        }

        /// <summary>
        /// 计算反正切，返回弧度<br/>
        /// 值域：(-π/2, π/2)
        /// </summary>
        public static Fixed64 Atan(Fixed64 value) => value.RawValue switch
        {
            -Const.One => -Rad45,
            < -Const.One => Trigonometric.Arctangent0To45(-value.Reciprocal()) - Rad90,
            < Const.Zero => -Trigonometric.Arctangent0To45(-value),
            Const.Zero => Fixed64.Zero,
            < Const.One => Trigonometric.Arctangent0To45(value),
            Const.One => Rad45,
            > Const.One => Rad90 - Trigonometric.Arctangent0To45(value.Reciprocal()),
        };
        /// <summary>
        /// 计算反正切，返回角度<br/>
        /// 值域：(-90°, 90°)
        /// </summary>
        public static Fixed64 AtanDeg(Fixed64 value) => value.RawValue switch
        {
            -Const.One => -Deg45,
            < -Const.One => Trigonometric.Arctangent0To45(-value.Reciprocal()) * Rad2Deg - Deg90,
            < Const.Zero => -Trigonometric.Arctangent0To45(-value) * Rad2Deg,
            Const.Zero => Fixed64.Zero,
            < Const.One => Trigonometric.Arctangent0To45(value) * Rad2Deg,
            Const.One => Deg45,
            > Const.One => Deg90 - Trigonometric.Arctangent0To45(value.Reciprocal()) * Rad2Deg,
        };
        /// <summary>
        /// 计算反正切，返回弧度<br/>
        /// 值域：(-π, π]
        /// </summary>
        public static Fixed64 Atan2(Fixed64 y, Fixed64 x) => x.RawValue switch
        {
            < 0 => y.RawValue switch
            {
                > 0 => Atan(y / x) + Rad180,
                < 0 => Atan(y / x) - Rad180,
                _ => Rad180,
            },
            0 => y.RawValue switch
            {
                < 0 => -Rad90,
                > 0 => Rad90,
                0 => Fixed64.Zero,
            },
            _ => Atan(y / x),
        };
        /// <summary>
        /// 计算反正切，返回角度<br/>
        /// 值域：(-180°, 180°]
        /// </summary>
        public static Fixed64 Atan2Deg(Fixed64 y, Fixed64 x) => x.RawValue switch
        {
            < 0 => y.RawValue switch
            {
                > 0 => AtanDeg(y / x) + Deg180,
                < 0 => AtanDeg(y / x) - Deg180,
                _ => Deg180,
            },
            0 => y.RawValue switch
            {
                < 0 => -Deg90,
                > 0 => Deg90,
                0 => Fixed64.Zero,
            },
            _ => AtanDeg(y / x),
        };

        /// <summary>
        /// 计算反余切，返回弧度<br/>
        /// 值域：(0, π)
        /// </summary>
        public static Fixed64 Acot(Fixed64 value) => value.RawValue switch
        {
            -Const.One => Rad135,
            < -Const.One => Rad180 - Trigonometric.Arctangent0To45(-value.Reciprocal()),
            < Const.Zero => Rad90 + Trigonometric.Arctangent0To45(-value),
            Const.Zero => Rad90,
            < Const.One => Rad90 - Trigonometric.Arctangent0To45(value),
            Const.One => Rad45,
            > Const.One => Trigonometric.Arctangent0To45(value.Reciprocal()),
        };
        /// <summary>
        /// 计算反余切，返回角度<br/>
        /// 值域：(0°, 180°)
        /// </summary>
        public static Fixed64 AcotDeg(Fixed64 value) => Deg90 - AtanDeg(value);
        #endregion

        #region 弧度角/角度角
        /// <summary>
        /// 返回弧度，值域：[0, 2π]
        /// </summary>
        public static Fixed64 ClampRad(Fixed64 rad) => ClampFull(rad, Rad360);
        /// <summary>
        /// 返回角度，值域：[0°, 360°]
        /// </summary>
        public static Fixed64 ClampDeg(Fixed64 deg) => ClampFull(deg, Deg360);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 ClampFull(Fixed64 angle, Fixed64 mod)
        {
            var value = angle % mod;
            return value.RawValue < 0 ? value + mod : value;
        }

        /// <summary>
        /// 返回弧度，值域：[-π, π]
        /// </summary>
        public static Fixed64 DeltaAngleRad(Fixed64 lhs, Fixed64 rhs) => ClampHalf(rhs - lhs, Rad360);
        /// <summary>
        /// 返回角度，值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 DeltaAngle(Fixed64 lhs, Fixed64 rhs) => ClampHalf(rhs - lhs, Deg360);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 ClampHalf(Fixed64 angle, Fixed64 mod)
        {
            var value = angle % mod;
            var half = mod >> 1;
            if (value < -half)
                return value + mod;
            if (value > half)
                return value - mod;
            return value;
        }
        #endregion

        #region 幂/指数/对数
        /// <summary>
        /// 输入数值，返回2次幂
        /// </summary>
        public static Fixed64 Pow2(Fixed64 exp)
        {
            if (exp.RawValue == 0)
            {
                return Fixed64.One;
            }

            bool neg = exp.RawValue < 0;
            var abs = neg ? -exp : exp;
            switch (abs.RawValue)
            {
                case >= Const.Log2Max: return neg ? Fixed64.MaxValue.Reciprocal() : Fixed64.MaxValue;
                case <= Const.Log2Min: return neg ? Fixed64.MaxValue : Fixed64.Zero;
            }

            var sum = Fixed64.One;
            for (Fixed64 fractional = abs.FractionalPart(), term = Fixed64.One, divisor = Fixed64.One; term.RawValue != 0; ++divisor)
            {
                term *= Ln2 * fractional / divisor;
                sum += term;
            }

            int integer = (int)abs.Floor();
            return neg ? sum.Reciprocal() >> integer : sum << integer;
        }
        /// <summary>
        /// 幂运算/指数运算
        /// </summary>
        public static Fixed64 Pow(Fixed64 b, Fixed64 e)
        {
            if (e.RawValue == 0)
                return Fixed64.One;

            return b.RawValue switch
            {
                Const.Zero => e.RawValue < 0 ? Fixed64.Infinity : Fixed64.Zero,
                Const.One => Fixed64.One,
                _ => Pow2(e * Log2(b)),
            };
        }
        /// <summary>
        /// 幂运算/指数运算
        /// </summary>
        public static Fixed64 Pow(Fixed64 b, int a)
        {
            if (b.RawValue == 0)
                return Fixed64.Zero;

            if (b.Abs().RawValue > Const.One)
                return a > 0 ? PrivatePow(b, a) : PrivatePow(b, -a).Reciprocal();

            return a > 0 ? PrivatePow(b.Reciprocal(), a).Reciprocal() : PrivatePow(b.RawValue, -a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 PrivatePow(Fixed64 b, int a)
        {
            if (b.RawValue == Const.One || a == 0)
                return Fixed64.One;

            if (a == 1)
                return b;

            var pow = PrivatePow(b, a >> 1);
            return (a & 1) == 0 ? pow.Sqr() : b * pow.Sqr();
        }

        /// <summary>
        /// 返回以2为底的对数
        /// </summary>
        public static int Log2(int a)
        {
            Assert.Greater<ArgumentOutOfRangeException, AssertArgs<int>, int>(a, 0, nameof(a), "a：{0}≤0，无法计算对数", new AssertArgs<int>(a));
            return Log2((uint)a);
        }
        /// <summary>
        /// 返回以2为底的对数
        /// </summary>
        public static int Log2(uint a)
        {
            Assert.Greater<ArgumentOutOfRangeException, AssertArgs<uint>, uint>(a, 0, nameof(a), "a：{0}=0，无法计算对数", new AssertArgs<uint>(a));
            uint num = a;
            int log = 0;
            if ((num & 0xFFFF0000) != 0) { num >>= 16; log |= 16; }
            if ((num & 0xFF00) != 0) { num >>= 8; log |= 8; }
            if ((num & 0xF0) != 0) { num >>= 4; log |= 4; }
            if ((num & 0xC) != 0) { num >>= 2; log |= 2; }
            if ((num & 0x2) != 0) { log |= 1; }
            return log;
        }
        /// <summary>
        /// 返回以2为底的对数
        /// </summary>
        public static int Log2(long a)
        {
            Assert.Greater<ArgumentOutOfRangeException, AssertArgs<long>, long>(a, 0, nameof(a), "a：{0}≤0，无法计算对数", new AssertArgs<long>(a));
            return Log2((ulong)a);
        }
        /// <summary>
        /// 返回以2为底的对数
        /// </summary>
        public static int Log2(ulong a)
        {
            Assert.Greater<ArgumentOutOfRangeException, AssertArgs<ulong>, ulong>(a, 0, nameof(a), "a：{0}=0，无法计算对数", new AssertArgs<ulong>(a));
            ulong num = a;
            int log = 0;
            if ((num & 0xFFFFFFFF00000000) != 0) { num >>= 32; log |= 32; }
            if ((num & 0xFFFF0000) != 0) { num >>= 16; log |= 16; }
            if ((num & 0xFF00) != 0) { num >>= 8; log |= 8; }
            if ((num & 0xF0) != 0) { num >>= 4; log |= 4; }
            if ((num & 0xC) != 0) { num >>= 2; log |= 2; }
            if ((num & 0x2) != 0) { log |= 1; }
            return log;
        }
        /// <summary>
        /// 返回以2为底的对数
        /// </summary>
        public static Fixed64 Log2(Fixed64 a)
        {
            Assert.Greater<ArgumentOutOfRangeException, AssertArgs<Fixed64>, Fixed64>(a, 0, nameof(a), "x：{0}≤0，无法计算对数", new AssertArgs<Fixed64>(a));
            long arv = a.RawValue;
            long y = 0;
            while (arv < Const.One)
            {
                arv <<= 1;
                y -= Const.One;
            }
            while (arv >= Const.One << 1)
            {
                arv >>= 1;
                y += Const.One;
            }

            var z = new Fixed64(arv);
            long b = 1L << Const.FractionalBits - 1;
            for (int i = 0; i < Const.FractionalBits; ++i)
            {
                z = z.Sqr();
                if (z.RawValue >= Const.One << 1)
                {
                    z >>= 1;
                    y += b;
                }

                b >>= 1;
            }

            return new Fixed64(y);
        }
        /// <summary>
        /// 返回自然对数（即以e为底的对数）
        /// </summary>
        public static Fixed64 Ln(Fixed64 a) => Log2(a) * Ln2;
        /// <summary>
        /// 返回以10为底的对数
        /// </summary>
        public static Fixed64 Lg(Fixed64 a) => Log2(a) * Lg2;

        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(int a) => a > 0 && (a & a - 1) == 0;
        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(long a) => a > 0 && (a & a - 1) == 0;
        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(ulong a) => a != 0 && (a & a - 1) == 0;
        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(Fixed64 a) => IsPowerOf2(a.RawValue);
        #endregion
    }
}