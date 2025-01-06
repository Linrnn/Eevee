namespace Eevee.Fixed
{
    internal readonly struct Const
    {
        #region 极值
        /// <summary>
        /// 可表示的最大值（非FiniteNumber最大值）
        /// </summary>
        internal const long MaxValue = long.MaxValue;

        /// <summary>
        /// 可表示的最小值（非FiniteNumber最小值）
        /// </summary>
        internal const long MinValue = long.MinValue;
        #endregion

        #region 字节数
        /// <summary>
        /// 总字节数
        /// </summary>
        internal const int NumBits = 64;

        /// <summary>
        /// 原本的小数位数
        /// </summary>
        private const int OriginFractionalPlaces = 32;

        /// <summary>
        /// 小数位数
        /// </summary>
        internal const int FractionalPlaces = 32;

        /// <summary>
        /// 需要偏移的小数位数<br/>
        /// OffsetFractionalPlaces必须>0
        /// </summary>
        internal const int OffsetFractionalPlaces = OriginFractionalPlaces - FractionalPlaces;
        #endregion

        #region 单位
        /// <summary>
        /// 单位化
        /// </summary>
        internal const long One = 1L << FractionalPlaces;

        /// <summary>
        /// 十倍单位
        /// </summary>
        internal const long Ten = 10L << FractionalPlaces;

        /// <summary>
        /// 半个单位
        /// </summary>
        internal const long Half = 1L << FractionalPlaces - 1;
        #endregion

        #region 圆周率
        /// <summary>
        /// π
        /// </summary>
        internal const long Pi = 0x3243F6A88 >> OffsetFractionalPlaces;

        /// <summary>
        /// π*2
        /// </summary>
        internal const long PiTimes2 = 0x6487ED511 >> OffsetFractionalPlaces;

        /// <summary>
        /// π^2
        /// </summary>
        internal const long PiOver2 = 0x1921FB544 >> OffsetFractionalPlaces;
        #endregion

        #region 对数
        /// <summary>
        /// 2的自然对数
        /// </summary>
        internal const long LogE2 = 0xB17217F7 >> OffsetFractionalPlaces;

        /// <summary>
        /// Number高位位数<br/>
        /// Log2(Number(高位)) <= Log2Max
        /// </summary>
        internal const long Log2Max = 0x1F00000000 >> OffsetFractionalPlaces;

        /// <summary>
        /// Number低位位数<br/>
        /// Log2(Number(低位)) >= Log2Max
        /// </summary>
        internal const long Log2Min = -0x2000000000 >> OffsetFractionalPlaces;
        #endregion

        /// <summary>
        /// 显示查找表Size
        /// </summary>
        internal const int TableSize = (int)(PiOver2 >> 15);
    }
}