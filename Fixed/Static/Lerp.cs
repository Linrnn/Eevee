using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 插值相关
    /// </summary>
    public readonly struct Lerp
    {
        #region 基础插值
        /// <summary>
        /// 线性插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Linear(Fixed64 from, Fixed64 to, Fixed64 percent) => LinearUnClamp(from, to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector2D Linear(in Vector2D from, in Vector2D to, Fixed64 percent) => LinearUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector3D Linear(in Vector3D from, in Vector3D to, Fixed64 percent) => LinearUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector4D Linear(in Vector4D from, in Vector4D to, Fixed64 percent) => LinearUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Quaternion Linear(in Quaternion from, in Quaternion to, Fixed64 percent) => LinearUnClamp(in from, in to, percent.Clamp01());
        /// <summary>
        /// 球面线性插值
        /// </summary>
        public static Quaternion SphereLinear(in Quaternion from, in Quaternion to, Fixed64 percent) => SphereLinearUnClamp(in from, in to, percent.Clamp01());

        /// <summary>
        /// 线性插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 LinearUnClamp(Fixed64 from, Fixed64 to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector2D LinearUnClamp(in Vector2D from, in Vector2D to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector3D LinearUnClamp(in Vector3D from, in Vector3D to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Vector4D LinearUnClamp(in Vector4D from, in Vector4D to, Fixed64 percent) => from + (to - from) * percent;
        /// <summary>
        /// 线性插值
        /// </summary>
        public static Quaternion LinearUnClamp(in Quaternion from, in Quaternion to, Fixed64 percent) => (from + (to - from) * percent).Normalized();
        /// <summary>
        /// 球面线性插值
        /// </summary>
        public static Quaternion SphereLinearUnClamp(in Quaternion from, in Quaternion to, Fixed64 percent)
        {
            var dot = Quaternion.Dot(in from, in to);
            var abs = dot < Fixed64.Zero ? -to : to;
            var rad = Maths.Acos(dot.Abs());
            var quaternion = Maths.Sin((Fixed64.One - percent) * rad) * from + Maths.Sin(percent * rad) * abs;
            return quaternion / Maths.Sin(rad);
        }

        /// <summary>
        /// 计算线性参数amount在[lsh，rsh]范围内产生插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 LinearInverse(Fixed64 from, Fixed64 to, Fixed64 value) => LinearInverseUnClamp(from, to, value).Clamp01();
        /// <summary>
        /// 计算线性参数amount在[lsh，rsh]范围内产生插值，不进行范围检查
        /// </summary>
        public static Fixed64 LinearInverseUnClamp(Fixed64 from, Fixed64 to, Fixed64 value) => from == to ? Fixed64.Zero : (value - from) / (to - from);
        /// <summary>
        /// 根据给定的标准化坐标，返回矩形内的点<br/>
        /// 等价于线程插值
        /// </summary>
        public static Vector2D NormalizedToPoint(in Rectangle rect, in Vector2D normalized) => new()
        {
            X = Linear(rect.X, rect.XMax, normalized.X),
            Y = Linear(rect.Y, rect.YMax, normalized.Y),
        };
        /// <summary>
        /// 返回与点对应的标准化坐标
        /// </summary>
        public static Vector2D PointToNormalized(in Rectangle rect, in Vector2D point) => new()
        {
            X = LinearInverse(rect.X, rect.XMax, point.X),
            Y = LinearInverse(rect.Y, rect.YMax, point.Y),
        };

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
        public static Fixed64 MoveTowardsAngleRad(Fixed64 from, Fixed64 to, Fixed64 maxDelta) => MoveTowards(from, from + Maths.DeltaAngleRad(from, to), maxDelta);
        /// <summary>
        /// 与MoveTowards相同，返回角度
        /// </summary>
        public static Fixed64 MoveTowardsAngle(Fixed64 from, Fixed64 to, Fixed64 maxDelta) => MoveTowards(from, from + Maths.DeltaAngle(from, to), maxDelta);
        /// <summary>
        /// 输入角度角，将from向to旋转
        /// </summary>
        public static Quaternion RotateTowards(in Quaternion from, in Quaternion to, Fixed64 maxDelta)
        {
            var num = Geometry.Angle(in from, in to);
            if (num.RawValue == 0)
                return to;
            return SphereLinearUnClamp(in from, in to, Fixed64.Min(Fixed64.One, maxDelta / num));
        }
        #endregion

        #region 复杂插值
        /// <summary>
        /// Catmull-Rom插值<br/>
        /// 参考链接：http://www.mvps.org/directx/articles/catmull/
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 CatmullRom(Fixed64 v0, Fixed64 v1, Fixed64 v2, Fixed64 v3, Fixed64 a)
        {
            var sqr = a.Sqr();
            var cube = sqr * a;
            return (v1 << 1) + (v2 - v0) * a + ((v0 << 1) - v1 * 5 + (v2 << 2) - v3) * sqr + ((v1 - v2) * 3 - v0 + v3) * cube >> 1;
        }
        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        public static Vector2D CatmullRom(in Vector2D p0, in Vector2D p1, in Vector2D p2, in Vector2D p3, Fixed64 a) => new()
        {
            X = CatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = CatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
        };
        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        public static Vector3D CatmullRom(in Vector3D p0, in Vector3D p1, in Vector3D p2, in Vector3D p3, Fixed64 a) => new()
        {
            X = CatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = CatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
            Z = CatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, a),
        };
        /// <summary>
        /// Catmull-Rom插值
        /// </summary>
        public static Vector4D CatmullRom(in Vector4D p0, in Vector4D p1, in Vector4D p2, in Vector4D p3, Fixed64 a) => new()
        {
            X = CatmullRom(p0.X, p1.X, p2.X, p3.X, a),
            Y = CatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, a),
            Z = CatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, a),
            W = CatmullRom(p0.W, p1.W, p2.W, p3.W, a),
        };

        /// <summary>
        /// Hermite插值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 Hermite(Fixed64 v0, Fixed64 t0, Fixed64 v1, Fixed64 t1, Fixed64 a)
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
        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Vector2D Hermite(in Vector2D p0, in Vector2D t0, in Vector2D p1, in Vector2D t1, Fixed64 a) => new()
        {
            X = Hermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = Hermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
        };
        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Vector3D Hermite(in Vector3D p0, in Vector3D t0, in Vector3D p1, in Vector3D t1, Fixed64 a) => new()
        {
            X = Hermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = Hermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
            Z = Hermite(p0.Z, t0.Z, p1.Z, t1.Z, a),
        };
        /// <summary>
        /// Hermite插值
        /// </summary>
        public static Vector4D Hermite(in Vector4D p0, in Vector4D t0, in Vector4D p1, in Vector4D t1, Fixed64 a) => new()
        {
            X = Hermite(p0.X, t0.X, p1.X, t1.X, a),
            Y = Hermite(p0.Y, t0.Y, p1.Y, t1.Y, a),
            Z = Hermite(p0.Z, t0.Z, p1.Z, t1.Z, a),
            W = Hermite(p0.W, t0.W, p1.W, t1.W, a),
        };

        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed64 SmoothStep(Fixed64 v0, Fixed64 v1, Fixed64 a) => Hermite(v0, Fixed64.Zero, v1, Fixed64.Zero, a.Clamp01());
        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        public static Vector2D SmoothStep(in Vector2D p0, in Vector2D p1, Fixed64 a) => new()
        {
            X = SmoothStep(p0.X, p1.X, a),
            Y = SmoothStep(p0.Y, p1.Y, a),
        };
        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        public static Vector3D SmoothStep(in Vector3D p0, in Vector3D p1, Fixed64 a) => new()
        {
            X = SmoothStep(p0.X, p1.X, a),
            Y = SmoothStep(p0.Y, p1.Y, a),
            Z = SmoothStep(p0.Z, p1.Z, a),
        };
        /// <summary>
        /// 平滑插值（自然的动画，淡入淡出和其他过渡非常有用）
        /// </summary>
        public static Vector4D SmoothStep(in Vector4D p0, in Vector4D p1, Fixed64 a) => new()
        {
            X = SmoothStep(p0.X, p1.X, a),
            Y = SmoothStep(p0.Y, p1.Y, a),
            Z = SmoothStep(p0.Z, p1.Z, a),
            W = SmoothStep(p0.W, p1.W, a),
        };
        #endregion
    }
}