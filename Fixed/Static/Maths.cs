using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 包含常见的数学操作
    /// </summary>
    public readonly struct Maths
    {
        #region 字段
        public static readonly Fixed64 Ln2 = new(Const.Ln2);
        public static readonly Fixed64 Lg2 = new(Const.Lg2);
        public static readonly Fixed64 Epsilon = Fixed64.One / 1000L;
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

        #region 基础方法
        /// <summary>
        /// value大于0，返回1<br/>
        /// value等于0，返回0<br/>
        /// value小于0，返回-1
        /// </summary>
        public static int Sign(Fixed64 value) => value.Sign();
        /// <summary>
        /// 绝对值
        /// </summary>
        public static Fixed64 Abs(Fixed64 value) => value.Abs();

        /// <summary>
        /// 向上取整
        /// </summary>
        public static Fixed64 Ceiling(Fixed64 value) => value.Ceiling();
        /// <summary>
        /// 向下取整
        /// </summary>
        public static Fixed64 Floor(Fixed64 value) => value.Floor();
        /// <summary>
        /// 四舍五入到最接近的整数值<br/>
        /// </summary>
        public static Fixed64 Round(Fixed64 value) => value.Round();

        /// <summary>
        /// 开平方
        /// </summary>
        public static Fixed64 Sqrt(Fixed64 fixed64) => fixed64.Sqrt();
        #endregion

        #region 三角函数/反三角函数
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
            var value = ClampAngle(rad, Rad180);
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
            var value = ClampAngle(deg, Deg180);
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
            var value = ClampAngle(rad, Rad180);
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
            var value = ClampAngle(deg, Deg180);
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
        /// 计算反正弦，返回弧度
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
                    return value.RawValue < 0L ? -Rad90 - rad : Rad90 - rad;
            }
        }
        /// <summary>
        /// 计算反正弦，返回角度
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
                    return value.RawValue < 0L ? -Deg90 - deg : Deg90 - deg;
            }
        }

        /// <summary>
        /// 计算反余弦，返回弧度
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
                    return value.RawValue < 0L ? Rad180 + rad : rad;
            }
        }
        /// <summary>
        /// 计算反余弦，返回角度
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
                    return value.RawValue < 0L ? Deg180 + deg : deg;
            }
        }

        /// <summary>
        /// 计算反正切，返回弧度
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
        /// 计算反正切，返回角度
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
        /// 计算反正切，返回弧度
        /// </summary>
        public static Fixed64 Atan2(Fixed64 y, Fixed64 x) => x.RawValue switch
        {
            < 0L => y.RawValue switch
            {
                > 0L => Atan(y / x) + Rad180,
                < 0L => Atan(y / x) - Rad180,
                _ => Rad180,
            },
            0L => y.RawValue switch
            {
                < 0L => -Rad90,
                > 0L => Rad90,
                0L => Fixed64.Zero,
            },
            _ => Atan(y / x),
        };
        /// <summary>
        /// 计算反正切，返回角度
        /// </summary>
        public static Fixed64 Atan2Deg(Fixed64 y, Fixed64 x) => x.RawValue switch
        {
            < 0L => y.RawValue switch
            {
                > 0L => AtanDeg(y / x) + Deg180,
                < 0L => AtanDeg(y / x) - Deg180,
                _ => Deg180,
            },
            0L => y.RawValue switch
            {
                < 0L => -Deg90,
                > 0L => Deg90,
                0L => Fixed64.Zero,
            },
            _ => AtanDeg(y / x),
        };

        /// <summary>
        /// 计算反余切，返回弧度
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
        /// 计算反余切，返回角度
        /// </summary>
        public static Fixed64 AcotDeg(Fixed64 value) => Deg90 - AtanDeg(value);
        #endregion

        #region 工具方法
        /// <summary>
        /// 将弧度限制在0~2π之间
        /// </summary>
        public static Fixed64 ClampRad(Fixed64 rad) => ClampAngle(rad, Rad360);
        /// <summary>
        /// 将角度限制在0~360°之间
        /// </summary>
        public static Fixed64 ClampDeg(Fixed64 deg) => ClampAngle(deg, Deg360);

        private static Fixed64 ClampAngle(Fixed64 rad, Fixed64 mod)
        {
            var value = rad % mod;
            return value.RawValue < 0L ? value + mod : value;
        }
        #endregion
        #endregion

        #region 幂/指数/对数，参考链接：https: //zh.wikipedia.org/wiki/%E6%8C%87%E6%95%B0%E5%87%BD%E6%95%B0
        /// <summary>
        /// 输入数值，返回2次幂
        /// </summary>
        public static Fixed64 Pow2(Fixed64 exp)
        {
            if (exp.RawValue == 0L)
            {
                return Fixed64.One;
            }

            bool neg = exp.RawValue < 0L;
            var abs = neg ? -exp : exp;
            switch (abs.RawValue)
            {
                case >= Const.Log2Max: return neg ? Fixed64.MaxValue.Reciprocal() : Fixed64.MaxValue;
                case <= Const.Log2Min: return neg ? Fixed64.MaxValue : Fixed64.Zero;
            }

            var sum = Fixed64.One;
            for (Fixed64 fractional = abs.FractionalPart(), term = Fixed64.One, divisor = Fixed64.One; term.RawValue != 0L; ++divisor)
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
            if (e.RawValue == 0L)
                return Fixed64.One;

            return b.RawValue switch
            {
                Const.Zero => e.RawValue < 0L ? Fixed64.Infinity : Fixed64.Zero,
                Const.One => Fixed64.One,
                _ => Pow2(e * Log2(b)),
            };
        }

        /// <summary>
        /// 返回以2为底的对数
        /// </summary>
        public static Fixed64 Log2(Fixed64 a)
        {
            if (a.RawValue <= 0L)
            {
                throw new ArgumentOutOfRangeException($"x：{a}≤0，无法计算对数");
            }

            long xrv = a.RawValue;
            long y = 0L;
            while (xrv < Const.One)
            {
                xrv <<= 1;
                y -= Const.One;
            }
            while (xrv >= Const.One << 1)
            {
                xrv >>= 1;
                y += Const.One;
            }

            var z = new Fixed64(xrv);
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
        public static bool IsPowerOf2(Fixed64 a) => IsPowerOf2(a.RawValue);
        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(long a) => a > 0L && (a & a - 1L) == 0L;
        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(ulong a) => a != 0UL && (a & a - 1UL) == 0UL;
        #endregion

        #region Func
        /// <summary>
        /// 3x3矩阵所有值取绝对值
        /// </summary>
        public static void Absolute(ref Matrix3X3 matrix, out Matrix3X3 result)
        {
            result.M11 = matrix.M11.Abs();
            result.M12 = matrix.M12.Abs();
            result.M13 = matrix.M13.Abs();
            result.M21 = matrix.M21.Abs();
            result.M22 = matrix.M22.Abs();
            result.M23 = matrix.M23.Abs();
            result.M31 = matrix.M31.Abs();
            result.M32 = matrix.M32.Abs();
            result.M33 = matrix.M33.Abs();
        }

        /// <summary>
        /// 质心
        /// </summary>
        /// <param name="value1">值1</param>
        /// <param name="value2">值2</param>
        /// <param name="value3">值3</param>
        /// <param name="amount1">值2加权因子</param>
        /// <param name="amount2">值3加权因子</param>
        public static Fixed64 Barycentric(Fixed64 value1, Fixed64 value2, Fixed64 value3, Fixed64 amount1, Fixed64 amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        /// <summary>
        /// 区间值更正（如果出区间，值取最近）
        /// </summary>
        public static Fixed64 Clamp(Fixed64 value, Fixed64 min, Fixed64 max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        /// <summary>
        /// 区间[FixFloat.Zero, FixFloat,One]值更正（如果出区间，值取最近）
        /// </summary>
        public static Fixed64 Clamp01(Fixed64 value) => value.RawValue switch
        {
            < Const.Zero => Fixed64.Zero,
            > Const.One => Fixed64.One,
            _ => value,
        };

        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        /// <param name="value1">控制点1</param>
        /// <param name="value2">控制点2</param>
        /// <param name="value3">控制点3</param>
        /// <param name="value4">控制点4</param>
        /// <param name="amount">加权因子</param>
        public static Fixed64 CatmullRom(Fixed64 value1, Fixed64 value2, Fixed64 value3, Fixed64 value4, Fixed64 amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using FPs not to lose precission
            var amountSquared = amount * amount;
            var amountCubed = amountSquared * amount;
            return 0.5 * (2.0 * value2 + (value3 - value1) * amount + (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared + (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed);
        }

        /// <summary>
        /// 最小角度差（弧度）
        /// </summary>
        public static Fixed64 DeltaAngle(Fixed64 current, Fixed64 target)
        {
            var num = Repeat(target - current, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }

            return num;
        }

        /// <summary>
        /// 两点间距
        /// </summary>
        public static Fixed64 Distance(Fixed64 value1, Fixed64 value2) => value1 > value2 ? value1 - value2 : value2 - value1;

        /// <summary>
        /// Hermite插值
        /// </summary>
        /// <param name="value1">位置1</param>
        /// <param name="tangent1">切线1</param>
        /// <param name="value2">位置2</param>
        /// <param name="tangent2">切线2</param>
        /// <param name="amount">加权因子</param>
        public static Fixed64 Hermite(Fixed64 value1, Fixed64 tangent1, Fixed64 value2, Fixed64 tangent2, Fixed64 amount)
        {
            // 全部转换为FixFloat为了不丢失精度
            // 否则，对于大量的参数:amount，结果是NaN，而不是无穷大
            var v1 = value1;
            var v2 = value2;
            var t1 = tangent1;
            var t2 = tangent2;
            var s = amount;
            var sCubed = s * s * s;
            var sSquared = s * s;

            if (amount == Fixed64.Zero)
                return value1;

            if (amount == Fixed64.One)
                return value2;

            return (2 * v1 - 2 * v2 + t2 + t1) * sCubed + (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared + t1 * s + v1;
        }

        /// <summary>
        /// 计算线性参数amount，该线性参数amount在[value1，value2]范围内产生插值。
        /// </summary>
        /// <param name="value1">开始值</param>
        /// <param name="value2">结束值</param>
        /// <param name="amount">插值</param>
        /// <returns>amount介于开始和结束之间的值的百分比</returns>
        public static Fixed64 InverseLerp(Fixed64 value1, Fixed64 value2, Fixed64 amount)
        {
            if (value1 != value2)
                return Clamp01((amount - value1) / (value2 - value1));
            return Fixed64.Zero;
        }

        /// <summary>
        /// 线性插值
        /// [value1, value2]通过参数amount进行插值
        /// </summary>
        /// <param name="value1">开始值</param>
        /// <param name="value2">结束值</param>
        /// <param name="amount">开始值与结束值之间的参数</param>
        /// <returns>
        /// amount = 0, return value1<br/>
        /// amount = 1, return value2<br/>
        /// amount = 0.5, return the midpoint of value1 and value2
        /// </returns>
        public static Fixed64 Lerp(Fixed64 value1, Fixed64 value2, Fixed64 amount)
        {
            return value1 + (value2 - value1) * Clamp01(amount);
        }

        /// <summary>
        /// 最大值比较
        /// </summary>
        public static Fixed64 Max(Fixed64 val1, Fixed64 val2)
        {
            return val1 > val2 ? val1 : val2;
        }

        /// <summary>
        /// 最小值比较
        /// </summary>
        public static Fixed64 Min(Fixed64 val1, Fixed64 val2)
        {
            return val1 < val2 ? val1 : val2;
        }

        /// <summary>
        /// 最大值比较
        /// </summary>
        public static Fixed64 Max(Fixed64 val1, Fixed64 val2, Fixed64 val3)
        {
            var max12 = val1 > val2 ? val1 : val2;
            return max12 > val3 ? max12 : val3;
        }

        /// <summary>
        /// 移动到目标
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="maxDelta">最大改变值（负值将远离目标）</param>
        public static Fixed64 MoveTowards(Fixed64 current, Fixed64 target, Fixed64 maxDelta) => (target - current).Abs() <= maxDelta ? target : current + (target - current).Sign() * maxDelta;

        /// <summary>
        /// 与MoveTowards相同，相对于角度
        /// </summary>
        public static Fixed64 MoveTowardsAngle(Fixed64 current, Fixed64 target, float maxDelta)
        {
            target = current + DeltaAngle(current, target);
            return MoveTowards(current, target, maxDelta);
        }

        /// <summary>
        /// 循环值（类似于取模操作）
        /// </summary>
        public static Fixed64 Repeat(Fixed64 t, Fixed64 length) => (t - (t / length).Floor() * length);

        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        /// <param name="value1">开始值</param>
        /// <param name="value2">结束值</param>
        /// <param name="amount">加权因子</param>
        public static Fixed64 SmoothStep(Fixed64 value1, Fixed64 value2, Fixed64 amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            Fixed64 result = Clamp(amount, 0f, 1f);
            result = Hermite(value1, 0f, value2, 0f, result);
            return result;
        }

        /// <summary>
        /// 平滑阻尼
        /// </summary>
        /// <param name="current">当前位置</param>
        /// <param name="target">目标位置</param>
        /// <param name="currentVelocity">当前速度</param>
        /// <param name="smoothTime">到达目标时间</param>
        /// <param name="maxSpeed">允许的最大速度</param>
        /// <param name="deltaTime">自上次调用此函数以来的时间</param>
        public static Fixed64 SmoothDamp(Fixed64 current, Fixed64 target, ref Fixed64 currentVelocity, Fixed64 smoothTime, Fixed64 maxSpeed, Fixed64 deltaTime)
        {
            var num = 2f / smoothTime;
            var num2 = num * deltaTime;
            var num3 = (Fixed64.One + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2).Reciprocal();
            var num4 = current - target;
            var num5 = target;
            var max = maxSpeed * smoothTime;
            num4 = Clamp(num4, -max, max);
            target = current - num4;
            var num7 = (currentVelocity + num * num4) * deltaTime;
            currentVelocity = (currentVelocity - num * num7) * num3;
            Fixed64 num8 = target + (num4 + num7) * num3;
            if (num5 - current > Fixed64.Zero == num8 > num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }

            return num8;
        }
        #endregion
    }
}