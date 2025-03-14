﻿using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 数学库
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
        /// 值域：(-π/2, π/2)
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
        /// 值域：(-90°, 90°)
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
            return value.RawValue < 0 ? value + mod : value;
        }
        #endregion
        #endregion

        #region 幂/指数/对数，参考链接：https: //zh.wikipedia.org/wiki/%E6%8C%87%E6%95%B0%E5%87%BD%E6%95%B0
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
        /// 返回以2为底的对数
        /// </summary>
        public static Fixed64 Log2(Fixed64 a)
        {
            if (a.RawValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a), $"x：{a}≤0，无法计算对数");
            }

            long xrv = a.RawValue;
            long y = 0;
            while (xrv < Const.One)
            {
                xrv <<= 1;
                --y;
            }
            while (xrv >= Const.One << 1)
            {
                xrv >>= 1;
                ++y;
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
        public static bool IsPowerOf2(long a) => a > 0 && (a & a - 1) == 0;
        /// <summary>
        /// 是否是2的次幂
        /// </summary>
        public static bool IsPowerOf2(ulong a) => a != 0 && (a & a - 1) == 0;
        #endregion

        #region Fixed64/Vector2D/Vector3D/Vector4D/Quaternion/Rectangle 关联操作
        #region 基础插值
        /// <summary>
        /// 计算线性参数amount在[lsh，rsh]范围内产生插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 InverseLerp(Fixed64 from, Fixed64 to, Fixed64 value) => from == to ? Fixed64.Zero : ((value - from) / (to - from)).Clamp01();
        public static Vector2D PointToNormalized(in Rectangle rect, in Vector2D point) => new()
        {
            X = InverseLerp(rect.X, rect.XMax, point.X),
            Y = InverseLerp(rect.Y, rect.YMax, point.Y),
        };

        /// <summary>
        /// 线性插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Lerp(Fixed64 from, Fixed64 to, Fixed64 percent) => LerpUnClamp(from, to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector2D Lerp(in Vector2D from, in Vector2D to, Fixed64 percent) => LerpUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector3D Lerp(in Vector3D from, in Vector3D to, Fixed64 percent) => LerpUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector4D Lerp(in Vector4D from, in Vector4D to, Fixed64 percent) => LerpUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector2D NormalizedToPoint(in Rectangle rect, in Vector2D normalized) => new()
        {
            X = Lerp(rect.X, rect.XMax, normalized.X),
            Y = Lerp(rect.Y, rect.YMax, normalized.Y),
        };
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Quaternion Lerp(in Quaternion from, in Quaternion to, Fixed64 percent) => LerpUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 球面线性插值
        /// </summary>
        public static Quaternion SLerp(in Quaternion from, in Quaternion to, Fixed64 percent) => SLerpUnClamp(in from, in to, percent.Clamp01());

        /// <summary>
        /// 线性插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 LerpUnClamp(Fixed64 from, Fixed64 to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector2D LerpUnClamp(in Vector2D from, in Vector2D to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector3D LerpUnClamp(in Vector3D from, in Vector3D to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector4D LerpUnClamp(in Vector4D from, in Vector4D to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Quaternion LerpUnClamp(in Quaternion from, in Quaternion to, Fixed64 percent) => (from + (to - from) * percent).Normalized();
        /// <summary>
        /// 球面线性插值
        /// </summary>
        public static Quaternion SLerpUnClamp(in Quaternion from, in Quaternion to, Fixed64 percent)
        {
            var dot = Quaternion.Dot(in from, in to);
            var abs = dot < Fixed64.Zero ? -to : to;
            var rad = Acos(dot.Abs());
            var quaternion = Sin((Fixed64.One - percent) * rad) * from + Sin(percent * rad) * abs;
            return quaternion / Sin(rad);
        }

        /// <summary>
        /// 移动到目标，类似LerpUnClamp
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 MoveTowards(Fixed64 from, Fixed64 to, Fixed64 maxDelta)
        {
            var delta = to - from;
            return delta.Abs() <= maxDelta ? to : from + delta.Sign() * maxDelta;
        }
        /// <summary>
        /// 移动到目标，类似LerpUnClamp
        /// </summary>
        public static Vector2D MoveTowards(in Vector2D from, in Vector2D to, Fixed64 maxDelta)
        {
            var delta = to - from;
            var sqrMagnitude = delta.SqrMagnitude();
            if (sqrMagnitude.RawValue == 0 || maxDelta.RawValue >= 0 && sqrMagnitude <= maxDelta.Sqr())
                return to;

            return from + maxDelta / sqrMagnitude.Sqrt() * delta;
        }
        /// <summary>
        /// 移动到目标，类似LerpUnClamp
        /// </summary>
        public static Vector3D MoveTowards(in Vector3D from, in Vector3D to, Fixed64 maxDelta)
        {
            var delta = to - from;
            var sqrMagnitude = delta.SqrMagnitude();
            if (sqrMagnitude.RawValue == 0 || maxDelta.RawValue >= 0 && sqrMagnitude <= maxDelta.Sqr())
                return to;

            return from + maxDelta / sqrMagnitude.Sqrt() * delta;
        }
        /// <summary>
        /// 移动到目标，类似LerpUnClamp
        /// </summary>
        public static Vector4D MoveTowards(in Vector4D from, in Vector4D to, Fixed64 maxDelta)
        {
            var delta = to - from;
            var sqrMagnitude = delta.SqrMagnitude();
            if (sqrMagnitude.RawValue == 0 || maxDelta.RawValue >= 0 && sqrMagnitude <= maxDelta.Sqr())
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
        /// <summary>
        /// 输入角度角，将from向to旋转
        /// </summary>
        public static Quaternion RotateTowards(in Quaternion from, in Quaternion to, Fixed64 maxDelta)
        {
            var dot = Quaternion.Dot(in from, in to);
            var abs = dot < Fixed64.Zero ? -to : to;
            var rad = Acos(dot.Abs());
            var theta = rad << 1;

            maxDelta *= Deg2Rad;
            if (maxDelta >= theta)
                return abs;

            maxDelta /= theta;
            var quaternion = Sin((Fixed64.One - maxDelta) * rad) * from + Sin(maxDelta * rad) * abs;
            return quaternion * Sin(rad).Reciprocal();
        }
        #endregion

        #region 复杂插值
        /// <summary>
        /// Catmull-Rom插值<br/>
        /// 参考链接：http://www.mvps.org/directx/articles/catmull/
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 LerpCatmullRom(Fixed64 v0, Fixed64 v1, Fixed64 v2, Fixed64 v3, Fixed64 a)
        {
            var sqr = a.Sqr();
            var cube = sqr * a;
            return (v1 << 1) + (v2 - v0) * a + ((v0 << 1) - v1 * 5 + (v2 << 2) - v3) * sqr + ((v1 - v2) * 3 - v0 + v3) * cube >> 1;
        }
        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        public static Vector2D LerpCatmullRom(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3, Fixed64 a) => new()
        {
            X = LerpCatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = LerpCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
        };
        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        public static Vector3D LerpCatmullRom(in Vector3D p0, in Vector3D p1, in Vector3D p2, in Vector3D p3, Fixed64 a) => new()
        {
            X = LerpCatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = LerpCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
            Z = LerpCatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, a),
        };
        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        public static Vector4D LerpCatmullRom(in Vector4D p0, in Vector4D p1, in Vector4D p2, in Vector4D p3, Fixed64 a) => new()
        {
            X = LerpCatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = LerpCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
            Z = LerpCatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, a),
            W = LerpCatmullRom(p0.W, p1.W, p2.W, p3.W, a),
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

            var sqr = a.Sqr();
            var cube = sqr * a;
            return ((v0 - v1 << 1) + t0 + t1) * cube + ((v1 - v0) * 3 - (t0 << 1) - t1) * sqr + t0 * a + v0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Vector2D LerpHermite(in Vector2D p0, in Vector2D t0, in Vector2D p1, in Vector2D t1, Fixed64 a) => new()
        {
            X = LerpHermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = LerpHermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
        };
        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Vector3D LerpHermite(in Vector3D p0, in Vector3D t0, in Vector3D p1, in Vector3D t1, Fixed64 a) => new()
        {
            X = LerpHermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = LerpHermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
            Z = LerpHermite(p0.Z, t0.Z, p1.Z, t1.Z, a),
        };
        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Vector4D LerpHermite(in Vector4D p0, in Vector4D t0, in Vector4D p1, in Vector4D t1, Fixed64 a) => new()
        {
            X = LerpHermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = LerpHermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
            Z = LerpHermite(p0.Z, t0.Z, p1.Z, t1.Z, a),
            W = LerpHermite(p0.W, t0.W, p1.W, t1.W, a),
        };

        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 LerpSmoothStep(Fixed64 v0, Fixed64 v1, Fixed64 a) => LerpHermite(v0, Fixed64.Zero, v1, Fixed64.Zero, a.Clamp01());
        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        public static Vector2D LerpSmoothStep(in Vector2D p0, in Vector2D p1, Fixed64 a) => new()
        {
            X = LerpSmoothStep(p0.X, p1.X, a),
            Y = LerpSmoothStep(p0.Y, p1.Y, a),
        };
        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        public static Vector3D LerpSmoothStep(in Vector3D p0, in Vector3D p1, Fixed64 a) => new()
        {
            X = LerpSmoothStep(p0.X, p1.X, a),
            Y = LerpSmoothStep(p0.Y, p1.Y, a),
            Z = LerpSmoothStep(p0.Z, p1.Z, a),
        };
        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        public static Vector4D LerpSmoothStep(in Vector4D p0, in Vector4D p1, Fixed64 a) => new()
        {
            X = LerpSmoothStep(p0.X, p1.X, a),
            Y = LerpSmoothStep(p0.Y, p1.Y, a),
            Z = LerpSmoothStep(p0.Z, p1.Z, a),
            W = LerpSmoothStep(p0.W, p1.W, a),
        };
        #endregion

        #region 夹角计算
        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Vector2D lhs, in Vector2D rhs) => Acos(Vector2D.Dot(lhs.Normalized(), rhs.Normalized()));
        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Vector3D lhs, in Vector3D rhs) => Acos(Vector3D.Dot(lhs.Normalized(), rhs.Normalized()));
        /// <summary>
        /// 返回两个旋转之间的夹角，返回无符号弧度<br/>
        /// 值域：[0, π]
        /// </summary>
        public static Fixed64 AngleRad(in Quaternion lhs, in Quaternion rhs)
        {
            var rad = Rad0To2Pi(in lhs, in rhs);
            return rad > Rad180 ? Rad360 - rad : rad;
        }

        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 Angle(in Vector2D lhs, in Vector2D rhs) => AcosDeg(Vector2D.Dot(lhs.Normalized(), rhs.Normalized()));
        /// <summary>
        /// 返回两个向量之间的夹角，返回无符号角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 Angle(in Vector3D lhs, in Vector3D rhs) => AcosDeg(Vector3D.Dot(lhs.Normalized(), rhs.Normalized()));
        /// <summary>
        /// 返回两个旋转之间的夹角，返回无符号角度<br/>
        /// 值域：[0°, 180°]
        /// </summary>
        public static Fixed64 Angle(in Quaternion lhs, in Quaternion rhs)
        {
            var deg = Deg0To360(in lhs, in rhs);
            return deg > Deg180 ? Deg360 - deg : deg;
        }

        /// <summary>
        /// 返回两个向量之间的夹角，返回弧度<br/>
        /// 值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Vector2D lhs, in Vector2D rhs) => AngleRad(lhs, rhs) * Vector2D.Cross(in lhs, in rhs).Sign();
        /// <summary>
        /// 返回两个向量之间的夹角，返回弧度<br/>
        /// 值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Vector3D lhs, in Vector3D rhs, in Vector3D axis)
        {
            var lshNorm = lhs.Normalized();
            var rshNorm = rhs.Normalized();
            var rad = Acos(Vector3D.Dot(in lshNorm, in rshNorm));
            var cross = Vector3D.Cross(in lshNorm, in rshNorm);
            var dot = Vector3D.Dot(in axis, in cross);
            return rad * dot.Sign();
        }
        /// <summary>
        /// 返回两个旋转之间的角度，返回弧度<br/>
        /// 值域：[-π, π]
        /// </summary>
        public static Fixed64 SignedAngleRad(in Quaternion lhs, in Quaternion rhs)
        {
            var rad = Rad0To2Pi(in lhs, in rhs);
            return rad > Rad180 ? rad - Rad360 : rad;
        }

        /// <summary>
        /// 返回两个向量之间的夹角，返回角度<br/>
        /// 值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngle(in Vector2D lhs, in Vector2D rhs) => Angle(lhs, rhs) * Vector2D.Cross(in lhs, in rhs).Sign();
        /// <summary>
        /// 返回两个向量之间的夹角，返回角度<br/>
        /// 值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngle(in Vector3D lhs, in Vector3D rhs, in Vector3D axis)
        {
            var lshNorm = lhs.Normalized();
            var rshNorm = rhs.Normalized();
            var deg = AcosDeg(Vector3D.Dot(in lshNorm, in rshNorm));
            var cross = Vector3D.Cross(in lshNorm, in rshNorm);
            var dot = Vector3D.Dot(in axis, in cross);
            return deg * dot.Sign();
        }
        /// <summary>
        /// 返回两个旋转之间的夹角，返回角度<br/>
        /// 值域：[-180°, 180°]
        /// </summary>
        public static Fixed64 SignedAngle(in Quaternion lhs, in Quaternion rhs)
        {
            var deg = Deg0To360(in lhs, in rhs);
            return deg > Deg180 ? deg - Deg360 : deg;
        }

        private static Fixed64 Rad0To2Pi(in Quaternion lhs, in Quaternion rhs) => Acos(Cos(in lhs, in rhs)) >> 1;
        private static Fixed64 Deg0To360(in Quaternion lhs, in Quaternion rhs) => AcosDeg(Cos(in lhs, in rhs)) >> 1;
        private static Fixed64 Cos(in Quaternion lhs, in Quaternion rhs)
        {
            var inverse = lhs.Inverse();
            return inverse.W * rhs.W - inverse.X * rhs.X - inverse.Y * rhs.Y - inverse.Z * rhs.Z;
        }
        #endregion

        /// <summary>
        /// 重心/质心
        /// </summary>
        public static Fixed64 Barycentric(Fixed64 v1, Fixed64 v2, Fixed64 v3, Fixed64 a1, Fixed64 a2) => v1 + (v2 - v1) * a1 + (v3 - v1) * a2;
        /// <summary>
        /// 重心/质心
        /// </summary>
        public static Vector2D Barycentric(in Vector2D p1, in Vector2D p2, in Vector2D p3, Fixed64 a1, Fixed64 a2) => new()
        {
            X = Barycentric(p1.X, p2.X, p3.X, a1, a2),
            Y = Barycentric(p1.Y, p2.Y, p3.Y, a1, a2),
        };
        #endregion

        #region Vector3D/Quaternion/Matrix3X3/Matrix4X4 相互转换
        /// <summary>
        /// 返回欧拉角的角度角
        /// </summary>
        public static Vector3D EulerAngles(in Quaternion quaternion)
        {
            var ySqr = quaternion.Y.Sqr();
            var t0 = Fixed64.One - (ySqr + quaternion.Z.Sqr() << 1);
            var t1 = quaternion.X * quaternion.Y - quaternion.W * quaternion.Z << 1;
            var t2 = -quaternion.X * quaternion.Z - quaternion.W * quaternion.Y << 1;
            var t3 = quaternion.Y * quaternion.Z - quaternion.W * quaternion.X << 1;
            var t4 = Fixed64.One - (quaternion.X.Sqr() + ySqr << 1);

            return new Vector3D
            {
                X = -Atan2Deg(t3, t4),
                Y = -AsinDeg(t2.Clamp(-Fixed64.One, Fixed64.One)),
                Z = -Atan2Deg(t1, t0),
            };
        }
        /// <summary>
        /// 返回欧拉角的角度角
        /// </summary>
        public static Vector3D EulerAngles(in Matrix3X3 matrix) => new()
        {
            X = -Atan2Deg(matrix.M21, matrix.M22),
            Y = -Atan2Deg(-matrix.M20, (matrix.M21.Sqr() + matrix.M22.Sqr()).Sqrt()),
            Z = -Atan2Deg(matrix.M10, matrix.M00),
        };

        #region 返回Quaternion
        /// <summary>
        /// 返回一个四元数，它围绕z轴旋转z度、围绕x轴旋转x度、围绕y轴旋转y度
        /// </summary>
        public static Quaternion Euler(in Vector3D eulerAngles) => CreateQuaternionDeg(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
        /// <summary>
        /// 返回一个四元数，它围绕z轴旋转z度、围绕x轴旋转x度、围绕y轴旋转y度
        /// </summary>
        public static Quaternion Euler(Fixed64 x, Fixed64 y, Fixed64 z) => CreateQuaternionDeg(y, x, z);

        /// <summary>
        /// 输入弧度，创建一个围绕“axis”旋转“rad”度的四元数
        /// </summary>
        public static Quaternion AngleAxisQuaternion(in Vector3D axis, Fixed64 rad)
        {
            var half = rad >> 1;
            var xyz = axis.Normalized() * Sin(half);
            return new Quaternion(in xyz, Cos(half));
        }
        /// <summary>
        /// 输入角度，创建一个围绕“axis”旋转“deg”度的四元数
        /// </summary>
        public static Quaternion AngleAxisQuaternionDeg(in Vector3D axis, Fixed64 deg)
        {
            var half = deg >> 1;
            var xyz = axis.Normalized() * SinDeg(half);
            return new Quaternion(in xyz, CosDeg(half));
        }

        /// <summary>
        /// 输入弧度，从旋转矩阵创建四元数
        /// </summary>
        public static Quaternion CreateQuaternion(Fixed64 yaw, Fixed64 pitch, Fixed64 roll)
        {
            var rh = roll >> 1;
            var rs = Sin(rh);
            var rc = Cos(rh);
            var ph = pitch >> 1;
            var ps = Sin(ph);
            var pc = Cos(ph);
            var yh = yaw >> 1;
            var ys = Sin(yh);
            var yc = Cos(yh);
            return new Quaternion
            {
                X = yc * ps * rc + ys * pc * rs,
                Y = ys * pc * rc - yc * ps * rs,
                Z = yc * pc * rs - ys * ps * rc,
                W = yc * pc * rc + ys * ps * rs,
            };
        }
        /// <summary>
        /// 输入角度，从旋转矩阵创建四元数
        /// </summary>
        public static Quaternion CreateQuaternionDeg(Fixed64 yaw, Fixed64 pitch, Fixed64 roll)
        {
            var rh = roll >> 1;
            var rs = SinDeg(rh);
            var rc = CosDeg(rh);
            var ph = pitch >> 1;
            var ps = SinDeg(ph);
            var pc = CosDeg(ph);
            var yh = yaw >> 1;
            var ys = SinDeg(yh);
            var yc = CosDeg(yh);
            return new Quaternion
            {
                X = yc * ps * rc + ys * pc * rs,
                Y = ys * pc * rc - yc * ps * rs,
                Z = yc * pc * rs - ys * ps * rc,
                W = yc * pc * rc + ys * ps * rs,
            };
        }
        #endregion

        #region 返回Matrix3X3
        /// <summary>
        /// 输入弧度，创建一个围绕“axis”旋转“rad”度的四元数3*3矩阵
        /// </summary>
        public static Matrix3X3 AngleAxisMatrix3X3(in Vector3D axis, Fixed64 rad)
        {
            var sin = Sin(rad);
            var cos = Cos(rad);
            var xx = axis.X.Sqr();
            var yy = axis.Y.Sqr();
            var zz = axis.Z.Sqr();
            var xy = axis.X * axis.Y;
            var xz = axis.X * axis.Z;
            var yz = axis.Y * axis.Z;
            return new Matrix3X3
            {
                M00 = xx + cos * (Fixed64.One - xx),
                M01 = xy - cos * xy + sin * axis.Z,
                M02 = xz - cos * xz - sin * axis.Y,
                M10 = xy - cos * xy - sin * axis.Z,
                M11 = yy + cos * (Fixed64.One - yy),
                M12 = yz - cos * yz + sin * axis.X,
                M20 = xz - cos * xz + sin * axis.Y,
                M21 = yz - cos * yz - sin * axis.X,
                M22 = zz + cos * (Fixed64.One - zz),
            };
        }
        /// <summary>
        /// 输入角度，创建一个围绕“axis”旋转“deg”度的四元数3*3矩阵
        /// </summary>
        public static Matrix3X3 AngleAxisMatrix3X3Deg(in Vector3D axis, Fixed64 deg)
        {
            var sin = SinDeg(deg);
            var cos = CosDeg(deg);
            var xx = axis.X.Sqr();
            var yy = axis.Y.Sqr();
            var zz = axis.Z.Sqr();
            var xy = axis.X * axis.Y;
            var xz = axis.X * axis.Z;
            var yz = axis.Y * axis.Z;
            return new Matrix3X3
            {
                M00 = xx + cos * (Fixed64.One - xx),
                M01 = xy - cos * xy + sin * axis.Z,
                M02 = xz - cos * xz - sin * axis.Y,
                M10 = xy - cos * xy - sin * axis.Z,
                M11 = yy + cos * (Fixed64.One - yy),
                M12 = yz - cos * yz + sin * axis.X,
                M20 = xz - cos * xz + sin * axis.Y,
                M21 = yz - cos * yz - sin * axis.X,
                M22 = zz + cos * (Fixed64.One - zz),
            };
        }

        /// <summary>
        /// 输入弧度，从旋转矩阵创建3*3矩阵
        /// </summary>
        public static Matrix3X3 CreateMatrix(Fixed64 yaw, Fixed64 pitch, Fixed64 roll) => Matrix3X3.Rotate(CreateQuaternion(yaw, pitch, roll));

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationMatrix3X3X(Fixed64 rad) => RotateXMatrix3X3(Cos(rad), Sin(rad));
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationMatrix3X3Y(Fixed64 rad) => RotateYMatrix3X3(Cos(rad), Sin(rad));
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationMatrix3X3Z(Fixed64 rad) => RotateZMatrix3X3(Cos(rad), Sin(rad));

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationMatrix3X3XDeg(Fixed64 deg) => RotateXMatrix3X3(CosDeg(deg), SinDeg(deg));
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationMatrix3X3YDeg(Fixed64 deg) => RotateYMatrix3X3(CosDeg(deg), SinDeg(deg));
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationMatrix3X3ZDeg(Fixed64 deg) => RotateZMatrix3X3(CosDeg(deg), SinDeg(deg));

        private static Matrix3X3 RotateXMatrix3X3(Fixed64 cos, Fixed64 sin) => new()
        {
            M00 = Fixed64.One,
            M01 = Fixed64.Zero,
            M02 = Fixed64.Zero,
            M10 = Fixed64.Zero,
            M11 = cos,
            M12 = sin,
            M20 = Fixed64.Zero,
            M21 = -sin,
            M22 = cos,
        };
        private static Matrix3X3 RotateYMatrix3X3(Fixed64 cos, Fixed64 sin) => new()
        {
            M00 = cos,
            M01 = Fixed64.Zero,
            M02 = -sin,
            M10 = Fixed64.Zero,
            M11 = Fixed64.One,
            M12 = Fixed64.Zero,
            M20 = sin,
            M21 = Fixed64.Zero,
            M22 = cos,
        };
        private static Matrix3X3 RotateZMatrix3X3(Fixed64 cos, Fixed64 sin) => new()
        {
            M00 = cos,
            M01 = sin,
            M02 = Fixed64.Zero,
            M10 = -sin,
            M11 = cos,
            M12 = Fixed64.Zero,
            M20 = Fixed64.Zero,
            M21 = Fixed64.Zero,
            M22 = Fixed64.One,
        };
        #endregion

        #region 返回Matrix4X4
        /// <summary>
        /// 输入弧度，创建一个围绕“axis”旋转“rad”度的四元数4*4矩阵
        /// </summary>
        public static Matrix4X4 AxisAngleMatrix4X4(in Vector3D axis, Fixed64 rad) => AxisAngleMatrix4X4(in axis, Sin(rad), Cos(rad));
        /// <summary>
        /// 输入角度，创建一个围绕“axis”旋转“deg”度的四元数4*4矩阵
        /// </summary>
        public static Matrix4X4 AxisAngleMatrix4X4Deg(in Vector3D axis, Fixed64 deg) => AxisAngleMatrix4X4(in axis, SinDeg(deg), CosDeg(deg));

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMatrix4X4(Fixed64 rad) => RotateXMatrix4X4(Cos(rad), Sin(rad), Fixed64.Zero, Fixed64.Zero);
        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMatrix4X4(Fixed64 rad, in Vector3D position)
        {
            var cos = Cos(rad);
            var sin = Sin(rad);
            var one = Fixed64.One - cos;
            var m31 = position.Y * one + position.Z * sin;
            var m32 = position.Z * one - position.Y * sin;
            return RotateXMatrix4X4(cos, sin, m31, m32);
        }
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMatrix4X4(Fixed64 rad) => RotateYMatrix4X4(Cos(rad), Sin(rad), Fixed64.Zero, Fixed64.Zero);
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMatrix4X4(Fixed64 rad, in Vector3D position)
        {
            var cos = Cos(rad);
            var sin = Sin(rad);
            var one = Fixed64.One - cos;
            var m30 = position.X * one - position.Z * sin;
            var m32 = position.X * one + position.X * sin;
            return RotateYMatrix4X4(cos, sin, m30, m32);
        }
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMatrix4X4(Fixed64 rad) => RotateZMatrix4X4(Cos(rad), Sin(rad), Fixed64.Zero, Fixed64.Zero);
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMatrix4X4(Fixed64 rad, in Vector3D position)
        {
            var cos = Cos(rad);
            var sin = Sin(rad);
            var one = Fixed64.One - cos;
            var m30 = position.X * one + position.Y * sin;
            var m31 = position.Y * one - position.X * sin;
            return RotateZMatrix4X4(cos, sin, m30, m31);
        }

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMatrix4X4Deg(Fixed64 deg) => RotateXMatrix4X4(CosDeg(deg), SinDeg(deg), Fixed64.Zero, Fixed64.Zero);
        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMatrix4X4Deg(Fixed64 deg, in Vector3D position)
        {
            var cos = CosDeg(deg);
            var sin = SinDeg(deg);
            var one = Fixed64.One - cos;
            var m31 = position.Y * one + position.Z * sin;
            var m32 = position.Z * one - position.Y * sin;
            return RotateXMatrix4X4(cos, sin, m31, m32);
        }
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMatrix4X4Deg(Fixed64 deg) => RotateYMatrix4X4(CosDeg(deg), SinDeg(deg), Fixed64.Zero, Fixed64.Zero);
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMatrix4X4Deg(Fixed64 deg, in Vector3D position)
        {
            var cos = CosDeg(deg);
            var sin = SinDeg(deg);
            var one = Fixed64.One - cos;
            var m30 = position.X * one - position.Z * sin;
            var m32 = position.X * one + position.X * sin;
            return RotateYMatrix4X4(cos, sin, m30, m32);
        }
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMatrix4X4Deg(Fixed64 deg) => RotateZMatrix4X4(CosDeg(deg), SinDeg(deg), Fixed64.Zero, Fixed64.Zero);
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMatrix4X4Deg(Fixed64 deg, in Vector3D position)
        {
            var cos = CosDeg(deg);
            var sin = SinDeg(deg);
            var one = Fixed64.One - cos;
            var m30 = position.X * one + position.Y * sin;
            var m31 = position.Y * one - position.X * sin;
            return RotateZMatrix4X4(cos, sin, m30, m31);
        }

        private static Matrix4X4 AxisAngleMatrix4X4(in Vector3D axis, Fixed64 sin, Fixed64 cos)
        {
            var xx = axis.X.Sqr();
            var yy = axis.Y.Sqr();
            var zz = axis.Z.Sqr();
            var xy = axis.X * axis.Y;
            var xz = axis.X * axis.Z;
            var yz = axis.Y * axis.Z;
            return new Matrix4X4
            {
                M00 = xx + cos * (Fixed64.One - xx),
                M01 = xy - cos * xy + sin * axis.Z,
                M02 = xz - cos * xz - sin * axis.Y,
                M03 = Fixed64.Zero,
                M10 = xy - cos * xy - sin * axis.Z,
                M11 = yy + cos * (Fixed64.One - yy),
                M12 = yz - cos * yz + sin * axis.X,
                M13 = Fixed64.Zero,
                M20 = xz - cos * xz + sin * axis.Y,
                M21 = yz - cos * yz - sin * axis.X,
                M22 = zz + cos * (Fixed64.One - zz),
                M23 = Fixed64.Zero,
                M30 = Fixed64.Zero,
                M31 = Fixed64.Zero,
                M32 = Fixed64.Zero,
                M33 = Fixed64.One,
            };
        }
        private static Matrix4X4 RotateXMatrix4X4(Fixed64 cos, Fixed64 sin, Fixed64 m31, Fixed64 m32) => new()
        {
            M00 = Fixed64.One,
            M01 = Fixed64.Zero,
            M02 = Fixed64.Zero,
            M03 = Fixed64.Zero,
            M10 = Fixed64.Zero,
            M11 = cos,
            M12 = sin,
            M13 = Fixed64.Zero,
            M20 = Fixed64.Zero,
            M21 = -sin,
            M22 = cos,
            M23 = Fixed64.Zero,
            M30 = Fixed64.Zero,
            M31 = m31,
            M32 = m32,
            M33 = Fixed64.One,
        };
        private static Matrix4X4 RotateYMatrix4X4(Fixed64 cos, Fixed64 sin, Fixed64 m30, Fixed64 m32) => new()
        {
            M00 = cos,
            M01 = Fixed64.Zero,
            M02 = -sin,
            M03 = Fixed64.Zero,
            M10 = Fixed64.Zero,
            M11 = Fixed64.One,
            M12 = Fixed64.Zero,
            M13 = Fixed64.Zero,
            M20 = sin,
            M21 = Fixed64.Zero,
            M22 = cos,
            M23 = Fixed64.Zero,
            M30 = m30,
            M31 = Fixed64.Zero,
            M32 = m32,
            M33 = Fixed64.One,
        };
        private static Matrix4X4 RotateZMatrix4X4(Fixed64 cos, Fixed64 sin, Fixed64 m30, Fixed64 m31) => new()
        {
            M00 = cos,
            M01 = sin,
            M02 = Fixed64.Zero,
            M03 = Fixed64.Zero,
            M10 = -sin,
            M11 = cos,
            M12 = Fixed64.Zero,
            M13 = Fixed64.Zero,
            M20 = Fixed64.Zero,
            M21 = Fixed64.Zero,
            M22 = Fixed64.One,
            M23 = Fixed64.Zero,
            M30 = m30,
            M31 = m31,
            M32 = Fixed64.Zero,
            M33 = Fixed64.One,
        };
        #endregion
        #endregion
    }
}