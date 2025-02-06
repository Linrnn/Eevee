namespace Eevee.Fixed
{
    internal readonly struct Const
    {
        #region 字节数
        /// <summary>
        /// 总字节数
        /// </summary>
        internal const int FullBits = 64;
        /// <summary>
        /// 原本的小数位数，这个值不允许修改！！！
        /// </summary>
        private const int OriginFractionalBits = 32;
        /// <summary>
        /// 小数位数，必须是偶数
        /// </summary>
        internal const int FractionalBits = 32;
        /// <summary>
        /// 需要偏移的小数位数<br/>
        /// OffsetFractionalPlaces必须>0
        /// </summary>
        internal const int OffsetFractionalBits = OriginFractionalBits - FractionalBits;
        #endregion

        #region 极值
        /// <summary>
        /// 可表示的最大值（非FiniteNumber最大值）
        /// </summary>
        internal const long MaxPeak = long.MaxValue;
        /// <summary>
        /// 可表示的最小值（非FiniteNumber最小值）
        /// </summary>e
        internal const long MinPeak = long.MinValue;
        #endregion

        #region 数字
        /// <summary>
        /// 0.5
        /// </summary>
        internal const long Half = 1L << FractionalBits - 1;
        /// <summary>
        /// 1
        /// </summary>
        internal const long One = 1L << FractionalBits;
        /// <summary>
        /// 10
        /// </summary>
        internal const long Ten = 10L << FractionalBits;

        /// <summary>
        /// 整数部分
        /// </summary>
        public const ulong IntegerPart = ~(ulong)FractionalPart;
        /// <summary>
        /// 小数部分
        /// </summary>
        public const long FractionalPart = (1L << FractionalBits) - 1;
        #endregion

        #region 角度
        /// <summary>
        /// 90°
        /// </summary>
        internal const short RightAngle = 90;
        /// <summary>
        /// 180°
        /// </summary>
        internal const short FlatAngle = 180;
        /// <summary>
        /// 360°
        /// </summary>
        internal const short FullAngle = 360;
        #endregion

        #region 圆周率
        /// <summary>
        /// π
        /// </summary>
        internal const long Pi = 0x3243F6A88 >> OffsetFractionalBits;
        /// <summary>
        /// π*2
        /// </summary>
        internal const long PiTimes2 = 0x6487ED511 >> OffsetFractionalBits;
        /// <summary>
        /// π^2
        /// </summary>
        internal const long PiOver2 = 0x1921FB544 >> OffsetFractionalBits;
        #endregion

        #region 对数
        /// <summary>
        /// 2的自然对数
        /// </summary>
        internal const long LogE2 = 0xB17217F7 >> OffsetFractionalBits;
        /// <summary>
        /// Number高位位数<br/>
        /// Log2(Number(高位)) <= Log2Max
        /// </summary>
        internal const long Log2Max = 0x1F00000000 >> OffsetFractionalBits;
        /// <summary>
        /// Number低位位数<br/>
        /// Log2(Number(低位)) >= Log2Max
        /// </summary>
        internal const long Log2Min = -0x2000000000 >> OffsetFractionalBits;
        #endregion

        /// <summary>
        /// 显示查找表Size
        /// </summary>
        internal const int TableSize = (int)(PiOver2 >> 15);
    }
}