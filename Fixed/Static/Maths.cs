using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 包含常见的数学操作
    /// </summary>
    public sealed class Maths
    {
        #region Static Variable
        /// <summary>
        /// Pi
        /// </summary>
        public static Fixed64 PI = new(Const.Pi);
        /// <summary>
        /// 无穷大
        /// </summary>
        public static Fixed64 Infinity = Fixed64.Infinity;
        /// <summary>
        /// 无穷小
        /// </summary>
        public static Fixed64 NegativeInfinity = Fixed64.NegativeInfinity;
        /// <summary>
        /// Pi ^ 2
        /// </summary>
        public static Fixed64 PIOver2 = new(Const.PiOver2);
        /// <summary>
        /// 2 * Pi
        /// </summary>
        public static Fixed64 PITime2 = new(Const.PiTimes2);
        /// <summary>
        /// 精度
        /// </summary>
        public static Fixed64 Epsilon = Fixed64.Epsilon;
        /// <summary>
        /// 单位弧度对应的角度
        /// </summary>
        public static Fixed64 Deg2Rad = PI / 180;
        /// <summary>
        /// 单位角度对应的弧度
        /// </summary>
        public static Fixed64 Rad2Deg = 180 / PI;
        #endregion

        #region Func
        /// <summary>
        /// 3x3矩阵所有值取绝对值
        /// </summary>
        public static void Absolute(ref Matrix3X3 matrix, out Matrix3X3 result)
        {
            result.M11 = Fixed64.Abs(matrix.M11);
            result.M12 = Fixed64.Abs(matrix.M12);
            result.M13 = Fixed64.Abs(matrix.M13);
            result.M21 = Fixed64.Abs(matrix.M21);
            result.M22 = Fixed64.Abs(matrix.M22);
            result.M23 = Fixed64.Abs(matrix.M23);
            result.M31 = Fixed64.Abs(matrix.M31);
            result.M32 = Fixed64.Abs(matrix.M32);
            result.M33 = Fixed64.Abs(matrix.M33);
        }

        /// <summary>
        /// 绝对值。
        /// Note: Abs(FixFloat.MinValue) == FixFloat.MaxValue.
        /// </summary>
        public static Fixed64 Abs(Fixed64 value)
        {
            return Fixed64.Abs(value);
        }

        /// <summary>
        /// 反正弦
        /// </summary>
        public static Fixed64 Asin(Fixed64 value)
        {
            return Fixed64.Asin(value);
        }

        /// <summary>
        /// 反余弦
        /// </summary>
        public static Fixed64 Acos(Fixed64 value)
        {
            return Fixed64.Acos(value);
        }

        /// <summary>
        /// 反正切
        /// </summary>
        public static Fixed64 Atan(Fixed64 value)
        {
            return Fixed64.Atan(value);
        }

        /// <summary>
        /// 方位角（y/x的反正切）
        /// </summary>
        public static Fixed64 Atan2(Fixed64 y, Fixed64 x)
        {
            return Fixed64.Atan2(y, x);
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
        /// 向上取整
        /// </summary>
        public static Fixed64 Ceiling(Fixed64 value)
        {
            return Fixed64.Ceiling(value);
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
        public static Fixed64 Clamp01(Fixed64 value)
        {
            if (value < Fixed64.Zero)
                return Fixed64.Zero;

            if (value > Fixed64.One)
                return Fixed64.One;

            return value;
        }

        /// <summary>
        /// 余弦
        /// </summary>
        public static Fixed64 Cos(Fixed64 value)
        {
            return Fixed64.Cos(value);
        }

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
        public static Fixed64 Distance(Fixed64 value1, Fixed64 value2)
        {
            return Fixed64.Abs(value1 - value2);
        }

        /// <summary>
        /// 向下取整
        /// </summary>
        public static Fixed64 Floor(Fixed64 value)
        {
            return Fixed64.Floor(value);
        }

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
        /// 判断高位是否是2的次幂
        /// </summary>
        public static bool IsPowerOfTwo(Fixed64 value)
        {
            if (Sign(value) < 1)
                return false;
            if (value.RawValue << Const.FractionalBits != 0)
                return false;
            var result = value.RawValue >> Const.FractionalBits;
            return (result & result - 1) == 0;
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
        /// 返回自然对数<br/>
        /// 提供至少7位小数的准确性
        /// </summary>
        public static Fixed64 Ln(Fixed64 x)
        {
            return Fixed64.FastMul(Log2(x), Fixed64.Ln2);
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
        public static Fixed64 MoveTowards(Fixed64 current, Fixed64 target, Fixed64 maxDelta)
        {
            if (Abs(target - current) <= maxDelta)
                return target;
            return (current + Sign(target - current) * maxDelta);
        }

        /// <summary>
        /// 与MoveTowards相同，相对于角度
        /// </summary>
        public static Fixed64 MoveTowardsAngle(Fixed64 current, Fixed64 target, float maxDelta)
        {
            target = current + DeltaAngle(current, target);
            return MoveTowards(current, target, maxDelta);
        }

        /// <summary>
        /// 幂运算
        /// 提供至少5位小数的准确性。
        /// </summary>
        public static Fixed64 Pow(Fixed64 b, Fixed64 exp)
        {
            if (b == Fixed64.One)
            {
                return Fixed64.One;
            }

            if (exp.RawValue == 0)
            {
                return Fixed64.One;
            }

            if (b.RawValue == 0)
            {
                if (exp.RawValue < 0)
                {
                    return Fixed64.Infinity;
                }

                return Fixed64.Zero;
            }

            var log2 = Log2(b);
            return Pow2(exp * log2);
        }

        /// <summary>
        /// 循环值（类似于取模操作）
        /// </summary>
        public static Fixed64 Repeat(Fixed64 t, Fixed64 length)
        {
            return (t - Floor(t / length) * length);
        }

        /// <summary>
        /// 四舍五入到最接近的整数值<br/>
        /// 如果一个数在偶数和奇数中间，则返回最近的偶数（如3.5 -> 4， 4.5 -> 4）
        /// </summary>
        public static Fixed64 Round(Fixed64 value)
        {
            return Fixed64.Round(value);
        }

        /// <summary>
        /// 开平方
        /// </summary>
        public static Fixed64 Sqrt(Fixed64 fixed64)
        {
            return Fixed64.Sqrt(fixed64);
        }

        /// <summary>
        /// 正弦
        /// </summary>
        public static Fixed64 Sin(Fixed64 value)
        {
            return Fixed64.Sin(value);
        }

        /// <summary>
        /// 返回一个数字，指示一个FixFloat数字的符号。
        /// 如果值为正，返回1;如果值为0，返回0;如果值为负，返回-1。
        /// </summary>
        public static int Sign(Fixed64 value)
        {
            return Fixed64.Sign(value);
        }

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
            var num3 = Fixed64.One / ((Fixed64.One + num2 + 0.48f * num2 * num2) + (0.235f * num2 * num2) * num2);
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

        /// <summary>
        /// 正切
        /// </summary>
        public static Fixed64 Tan(Fixed64 value)
        {
            return Fixed64.Tan(value);
        }
        #endregion

        #region Internal Func
        /// <summary>
        /// 2次幂<br/>
        /// 提供至少6位小数的准确性。
        /// </summary>
        internal static Fixed64 Pow2(Fixed64 x)
        {
            if (x.RawValue == 0)
            {
                return Fixed64.One;
            }

            // 利用exp(-x) = 1/exp(x)来避免负面的参数。
            bool neg = x.RawValue < 0;
            if (neg)
            {
                x = -x;
            }

            if (x == Fixed64.One)
            {
                return neg ? Fixed64.One / (Fixed64)2 : (Fixed64)2;
            }

            if (x >= Fixed64.Log2Max)
            {
                return neg ? Fixed64.One / Fixed64.MaxValue : Fixed64.MaxValue;
            }

            if (x <= Fixed64.Log2Min)
            {
                return neg ? Fixed64.MaxValue : Fixed64.Zero;
            }

            // The algorithm is based on the power series for exp(x):
            // http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
            // From term n, we get term n + 1 by multiplying with x / n.
            // When the sum term drops to zero, we can stop summing.

            int integerPart = (int)Floor(x);
            // Take fractional part of exponent
            x = Fixed64.FromRaw(x.RawValue & 0x00000000FFFFFFFF);

            var result = Fixed64.One;
            var term = Fixed64.One;
            int i = 1;
            while (term.RawValue != 0)
            {
                term = Fixed64.FastMul(Fixed64.FastMul(x, term), Fixed64.Ln2) / i;
                result += term;
                i++;
            }

            result = Fixed64.FromRaw(result.RawValue << integerPart);
            if (neg)
            {
                result = Fixed64.One / result;
            }

            return result;
        }

        /// <summary>
        /// 返回指定数值的以2为底的对数
        /// 提供至少9位小数的准确性
        /// </summary>
        internal static Fixed64 Log2(Fixed64 x)
        {
            if (x.RawValue <= 0)
            {
                throw new ArgumentOutOfRangeException("Math.Log2: Non-positive value passed to Ln", "x");
            }

            // This implementation is based on Clay. S. Turner's fast binary logarithm
            // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
            //     Processing Mag., pp. 124,140, Sep. 2010.)

            long b = 1U << (Const.FractionalBits - 1);
            long y = 0;

            long rawX = x.RawValue;
            while (rawX < Const.One)
            {
                rawX <<= 1;
                y -= Const.One;
            }

            while (rawX >= (Const.One << 1))
            {
                rawX >>= 1;
                y += Const.One;
            }

            var z = Fixed64.FromRaw(rawX);

            for (int i = 0; i < Const.FractionalBits; i++)
            {
                z = Fixed64.FastMul(z, z);
                if (z.RawValue >= (Const.One << 1))
                {
                    z = Fixed64.FromRaw(z.RawValue >> 1);
                    y += b;
                }

                b >>= 1;
            }

            return Fixed64.FromRaw(y);
        }
        #endregion
    }
}