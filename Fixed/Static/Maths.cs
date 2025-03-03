using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 常见的数学操作
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

        #region 基础方法
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
        /// 区间[0, 1]值更正（如果出区间，值取最近）
        /// </summary>
        public static Fixed64 Clamp01(Fixed64 value) => value.RawValue switch
        {
            < Const.Zero => Fixed64.Zero,
            > Const.One => Fixed64.One,
            _ => value,
        };
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
        /// 计算反正弦，返回弧度，值域：[-π/2, π/2]
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
        /// 计算反正弦，返回角度，值域：[-90°, 90°]
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
        /// 计算反余弦，返回弧度，值域：[0, π]
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
        /// 计算反余弦，返回角度，值域：[0°, 180°]
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
        /// 计算反正切，返回弧度，值域：(-π/2, π/2)
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
        /// 计算反正切，返回角度，值域：(-90°, 90°)
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
        /// 计算反正切，返回弧度，值域：(-π/2, π/2)
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
        /// 计算反正切，返回角度，值域：(-90°, 90°)
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
        /// 计算反余切，返回弧度，值域：(0, π)
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
        /// 计算反余切，返回角度，值域：(0°, 180°)
        /// </summary>
        public static Fixed64 AcotDeg(Fixed64 value) => Deg90 - AtanDeg(value);
        #endregion

        #region 工具方法
        /// <summary>
        /// 将弧度限制在0~2π之间
        /// </summary>
        public static Fixed64 ClampRad(Fixed64 rad) => ClampAngle(rad, Rad360);
        /// <summary>
        /// 将角度限制在-π~π之间
        /// </summary>
        public static Fixed64 DeltaAngleRad(Fixed64 lhs, Fixed64 rhs)
        {
            var deg = ClampAngle(rhs - lhs, Rad360);
            return deg.RawValue > Rad180 ? deg - Rad360 : deg;
        }

        /// <summary>
        /// 将角度限制在0~360°之间
        /// </summary>
        public static Fixed64 ClampDeg(Fixed64 deg) => ClampAngle(deg, Deg360);
        /// <summary>
        /// 将角度限制在-180~180°之间
        /// </summary>
        public static Fixed64 DeltaAngleDeg(Fixed64 lhs, Fixed64 rhs)
        {
            var deg = ClampAngle(rhs - lhs, Deg360);
            return deg > Deg180 ? deg - Deg360 : deg;
        }

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
                throw new ArgumentOutOfRangeException(nameof(a), $"x：{a}≤0，无法计算对数");
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

        #region Fixed64/Vector2D/Vector3D 关联操作
        #region 较小值/较大值
        /// <summary>
        /// 较小值
        /// </summary>
        public static Fixed64 Min(Fixed64 lsh, Fixed64 rsh) => lsh < rsh ? lsh : rsh;
        public static Fixed64 Min(Fixed64 lsh, Fixed64 msh, Fixed64 rsh)
        {
            var value = lsh < msh ? lsh : msh;
            return value < rsh ? value : rsh;
        }
        public static Vector2D Min(in Vector2D lsh, in Vector2D rsh) => new()
        {
            X = Min(lsh.X, rsh.X),
            Y = Min(lsh.Y, rsh.Y),
        };
        public static Vector3D Min(in Vector3D lsh, in Vector3D rsh) => new()
        {
            X = Min(lsh.X, rsh.X),
            Y = Min(lsh.Y, rsh.Y),
            Z = Min(lsh.Z, rsh.Z),
        };

        /// <summary>
        /// 较大值
        /// </summary>
        public static Fixed64 Max(Fixed64 lsh, Fixed64 rsh) => lsh > rsh ? lsh : rsh;
        public static Fixed64 Max(Fixed64 lsh, Fixed64 msh, Fixed64 rsh)
        {
            var value = lsh > msh ? lsh : msh;
            return value > rsh ? value : rsh;
        }
        public static Vector2D Max(in Vector2D lsh, in Vector2D rsh) => new()
        {
            X = Max(lsh.X, rsh.X),
            Y = Max(lsh.Y, rsh.Y),
        };
        public static Vector3D Max(in Vector3D lsh, in Vector3D rsh) => new()
        {
            X = Max(lsh.X, rsh.X),
            Y = Max(lsh.Y, rsh.Y),
            Z = Max(lsh.Z, rsh.Z),
        };
        #endregion

        #region 基础插值
        /// <summary>
        /// 计算线性参数amount在[lsh，rsh]范围内产生插值
        /// </summary>
        /// <param name="from">开始值</param>
        /// <param name="to">结束值</param>
        /// <param name="value">插值</param>
        /// <returns>amount介于开始和结束之间的值的百分比</returns>
        public static Fixed64 InverseLerp(Fixed64 from, Fixed64 to, Fixed64 value) => from == to ? Fixed64.Zero : Clamp01((value - from) / (to - from));

        /// <summary>
        /// 线性插值
        /// [lsh, rsh]通过参数amount进行插值
        /// </summary>
        /// <param name="from">开始值</param>
        /// <param name="to">结束值</param>
        /// <param name="percent">开始值与结束值之间的参数，0到1之间</param>
        public static Fixed64 Lerp(Fixed64 from, Fixed64 to, Fixed64 percent) => from + (to - from) * Clamp01(percent);
        public static Vector2D Lerp(in Vector2D from, in Vector2D to, Fixed64 percent) => from + (to - from) * Clamp01(percent);
        public static Vector3D Lerp(Vector3D from, Vector3D to, Fixed64 percent) => from + (to - from) * Clamp01(percent);

        /// <summary>
        /// 线性插值
        /// [lsh, rsh]通过参数amount进行插值
        /// </summary>
        /// <param name="from">开始值</param>
        /// <param name="to">结束值</param>
        /// <param name="percent">开始值与结束值之间的参数</param>
        public static Fixed64 LerpUnClamp(Fixed64 from, Fixed64 to, Fixed64 percent) => from + (to - from) * percent;
        public static Vector2D LerpUnClamp(in Vector2D from, in Vector2D to, Fixed64 percent) => from + (to - from) * percent;
        public static Vector3D LerpUnClamp(in Vector3D from, in Vector3D to, Fixed64 percent) => from + (to - from) * percent;

        /// <summary>
        /// 移动到目标，类似LerpUnClamp
        /// </summary>
        /// <param name="from">当前值</param>
        /// <param name="to">目标值</param>
        /// <param name="maxDelta">最大改变值（负值将远离目标）</param>
        public static Fixed64 MoveTowards(Fixed64 from, Fixed64 to, Fixed64 maxDelta)
        {
            var delta = to - from;
            return delta.Abs() <= maxDelta ? to : from + delta.Sign() * maxDelta;
        }
        public static Vector2D MoveTowards(in Vector2D from, in Vector2D to, Fixed64 maxDelta)
        {
            var delta = to - from;
            var sqrMagnitude = delta.SqrMagnitude();
            if (sqrMagnitude.RawValue == 0L || maxDelta.RawValue >= 0L && sqrMagnitude <= maxDelta.Sqr())
                return to;

            return from + maxDelta / sqrMagnitude.Sqrt() * delta;
        }
        public static Vector3D MoveTowards(in Vector3D from, in Vector3D to, Fixed64 maxDelta)
        {
            var delta = to - from;
            var sqrMagnitude = delta.SqrMagnitude();
            if (sqrMagnitude.RawValue == 0L || maxDelta.RawValue >= 0L && sqrMagnitude <= maxDelta.Sqr())
                return to;

            return from + maxDelta / sqrMagnitude.Sqrt() * delta;
        }

        /// <summary>
        /// 与MoveTowards相同，返回弧度
        /// </summary>
        public static Fixed64 MoveTowardsAngleRad(Fixed64 from, Fixed64 to, Fixed64 maxDelta)
        {
            var newTarget = from + DeltaAngleRad(from, to);
            return MoveTowards(from, newTarget, maxDelta);
        }
        /// <summary>
        /// 与MoveTowards相同，返回角度
        /// </summary>
        public static Fixed64 MoveTowardsAngleDeg(Fixed64 from, Fixed64 to, Fixed64 maxDelta)
        {
            var newTarget = from + DeltaAngleDeg(from, to);
            return MoveTowards(from, newTarget, maxDelta);
        }
        #endregion

        #region 复杂插值
        /// <summary>
        /// Catmull-Rom插值<br/>
        /// 参考链接：http://www.mvps.org/directx/articles/catmull/
        /// </summary>
        public static Fixed64 LerpCatmullRom(Fixed64 v0, Fixed64 v1, Fixed64 v2, Fixed64 v3, Fixed64 a)
        {
            var squared = a.Sqr();
            var cubed = squared * a;
            return (v1 << 1) + (v2 - v0) * a + ((v0 << 1) - v1 * 5L + (v2 << 2) - v3) * squared + ((v1 - v2) * 3L - v0 + v3) * cubed >> 1;
        }
        public static Vector2D LerpCatmullRom(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3, Fixed64 a) => new()
        {
            X = LerpCatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = LerpCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
        };
        public static Vector3D LerpCatmullRom(in Vector3D p0, in Vector3D p1, in Vector3D p2, in Vector3D p3, Fixed64 a) => new()
        {
            X = LerpCatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = LerpCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
            Z = LerpCatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, a),
        };

        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Fixed64 LerpHermite(Fixed64 v0, Fixed64 t0, Fixed64 v1, Fixed64 t1, Fixed64 a)
        {
            switch (a.RawValue)
            {
                case Const.Zero: return v0;
                case Const.One: return v1;
            }

            var squared = a.Sqr();
            var cubed = squared * a;
            return ((v0 - v1 << 1) + t0 + t1) * cubed + ((v1 - v0) * 3L - (t0 << 1) - t1) * squared + t0 * a + v0;
        }
        public static Vector2D LerpHermite(in Vector2D p0, in Vector2D t0, in Vector2D p1, in Vector2D t1, Fixed64 a) => new()
        {
            X = LerpHermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = LerpHermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
        };
        public static Vector3D LerpHermite(in Vector3D p0, in Vector3D t0, in Vector3D p1, in Vector3D t1, Fixed64 a) => new()
        {
            X = LerpHermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = LerpHermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
            Z = LerpHermite(p0.Z, t0.Z, p1.Z, t1.Z, a),
        };

        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        /// <param name="v0">开始值</param>
        /// <param name="v1">结束值</param>
        /// <param name="a">加权因子</param>
        public static Fixed64 LerpSmoothStep(Fixed64 v0, Fixed64 v1, Fixed64 a) => LerpHermite(v0, Fixed64.Zero, v1, Fixed64.Zero, Clamp01(a));
        public static Vector2D LerpSmoothStep(in Vector2D p0, in Vector2D p1, Fixed64 a) => new()
        {
            X = LerpSmoothStep(p0.X, p1.X, a),
            Y = LerpSmoothStep(p0.Y, p1.Y, a),
        };
        public static Vector3D LerpSmoothStep(in Vector3D p0, in Vector3D p1, Fixed64 a) => new()
        {
            X = LerpSmoothStep(p0.X, p1.X, a),
            Y = LerpSmoothStep(p0.Y, p1.Y, a),
            Z = LerpSmoothStep(p0.Z, p1.Z, a),
        };
        #endregion

        #region 夹角计算
        /// <summary>
        /// 返回两向量之间的夹角，返回弧度，值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Vector2D lhs, in Vector2D rhs) => Acos(Vector2D.Dot(lhs.Normalized(), rhs.Normalized()));
        public static Fixed64 AngleRad(in Vector3D lhs, in Vector3D rhs) => Acos(Vector3D.Dot(lhs.Normalized(), rhs.Normalized()));

        /// <summary>
        /// 返回两向量之间的夹角，返回无符号角度，值域：[0°, 180°]
        /// </summary>
        public static Fixed64 AngleDeg(in Vector2D lhs, in Vector2D rhs) => AcosDeg(Vector2D.Dot(lhs.Normalized(), rhs.Normalized()));
        public static Fixed64 AngleDeg(in Vector3D lhs, in Vector3D rhs) => AcosDeg(Vector3D.Dot(lhs.Normalized(), rhs.Normalized()));

        /// <summary>
        /// 返回两向量之间的夹角，返回弧度，值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Vector2D from, in Vector2D to) => AngleRad(from, to) * Vector2D.Cross(in from, in to).Sign();
        public static Fixed64 SignedAngleRad(in Vector3D from, in Vector3D to, in Vector3D axis)
        {
            var fromNorm = from.Normalized();
            var toNorm = to.Normalized();
            var acos = Acos(Vector3D.Dot(in fromNorm, in toNorm));
            var cross = Vector3D.Cross(in fromNorm, in toNorm);
            var dot = Vector3D.Dot(in axis, in cross);
            return acos * dot.Sign();
        }

        /// <summary>
        /// 返回两向量之间的夹角，返回弧度，值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngleDeg(in Vector2D from, in Vector2D to) => AngleDeg(from, to) * Vector2D.Cross(in from, in to).Sign();
        public static Fixed64 SignedAngleDeg(in Vector3D from, in Vector3D to, in Vector3D axis)
        {
            var fromNorm = from.Normalized();
            var toNorm = to.Normalized();
            var acos = AcosDeg(Vector3D.Dot(in fromNorm, in toNorm));
            var cross = Vector3D.Cross(in fromNorm, in toNorm);
            var dot = Vector3D.Dot(in axis, in cross);
            return acos * dot.Sign();
        }
        #endregion

        /// <summary>
        /// 质心
        /// </summary>
        public static Fixed64 Barycentric(Fixed64 v1, Fixed64 v2, Fixed64 v3, Fixed64 a1, Fixed64 a2) => v1 + (v2 - v1) * a1 + (v3 - v1) * a2;
        public static Vector2D Barycentric(in Vector2D p1, in Vector2D p2, in Vector2D p3, Fixed64 a1, Fixed64 a2) => new()
        {
            X = Barycentric(p1.X, p2.X, p3.X, a1, a2),
            Y = Barycentric(p1.Y, p2.Y, p3.Y, a1, a2),
        };
        #endregion
    }
}