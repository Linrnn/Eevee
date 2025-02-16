namespace Eevee.Fixed
{
    internal readonly struct Const
    {
        #region 字节数
        /// <summary>
        /// 总字节数，这个值不允许修改！！！
        /// </summary>
        internal const int FullBits = 64;
        /// <summary>
        /// 小数位数，必须是偶数，且必须≤32<br/>
        /// 修改后，检查字段引用，确定是否需要修改引用处
        /// </summary>
        internal const int FractionalBits = 32;
        /// <summary>
        /// 原本的小数位数，这个值不允许修改！！！
        /// </summary>
        private const int OriginFractionalBits = 32;
        /// <summary>
        /// 需要偏移的小数位数<br/>
        /// OffsetFractionalPlaces必须>0
        /// </summary>
        private const int OffsetBits = OriginFractionalBits - FractionalBits;
        #endregion

        #region 极值
        /// <summary>
        /// 可表示的最小值（非Fixed64最小值）
        /// </summary>e
        internal const long MinPeak = long.MinValue;
        /// <summary>
        /// 可表示的最大值（非Fixed64最大值）
        /// </summary>
        internal const long MaxPeak = long.MaxValue;

        /// <summary>
        /// 负无穷大
        /// </summary>
        internal const long NegativeInfinity = MinPeak + 1;
        /// <summary>
        /// 无穷大
        /// </summary>
        internal const long Infinity = MaxPeak - 1;

        /// <summary>
        /// 最小值（Fixed64最小值）
        /// </summary>
        internal const long MinValue = MinPeak + 2;
        /// <summary>
        /// 最大值（Fixed64最大值）
        /// </summary>
        internal const long MaxValue = MaxPeak - 2;
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
        internal const long IntegerPart = ~FractionalPart;
        /// <summary>
        /// 小数部分
        /// </summary>
        internal const long FractionalPart = (1L << FractionalBits) - 1L;
        #endregion

        #region 圆周率
        /// <summary>
        /// π/6
        /// </summary>
        internal const long Rad30 = 0x860A91C1 >> OffsetBits;
        /// <summary>
        /// π/4
        /// </summary>
        internal const long Rad45 = 0xC90FDAA2 >> OffsetBits;
        /// <summary>
        /// π/3
        /// </summary>
        internal const long Rad60 = 0x10C152382 >> OffsetBits;
        /// <summary>
        /// π/2
        /// </summary>
        internal const long Rad90 = 0x1921FB544 >> OffsetBits;
        /// <summary>
        /// 2/3*π
        /// </summary>
        internal const long Rad120 = 0x2182A4705 >> OffsetBits;
        /// <summary>
        /// 3/4*π
        /// </summary>
        internal const long Rad135 = 0x25B2F8FE6 >> OffsetBits;
        /// <summary>
        /// 5/6*π
        /// </summary>
        internal const long Rad150 = 0x29E34D8C7 >> OffsetBits;
        /// <summary>
        /// π
        /// </summary>
        internal const long Rad180 = 0x3243F6A88 >> OffsetBits;
        /// <summary>
        /// 7/6*π
        /// </summary>
        internal const long Rad210 = 0x3AA49FC49 >> OffsetBits;
        /// <summary>
        /// 5/4*π
        /// </summary>
        internal const long Rad225 = 0x3ED4F452A >> OffsetBits;
        /// <summary>
        /// 4/3*π
        /// </summary>
        internal const long Rad240 = 0x430548E0B >> OffsetBits;
        /// <summary>
        /// 3/2*π
        /// </summary>
        internal const long Rad270 = 0x4B65F1FCC >> OffsetBits;
        /// <summary>
        /// 5/3*π
        /// </summary>
        internal const long Rad300 = 0x53C69B18E >> OffsetBits;
        /// <summary>
        /// 7/4*π
        /// </summary>
        internal const long Rad315 = 0x57F6EFA6E >> OffsetBits;
        /// <summary>
        /// 11/6*π
        /// </summary>
        internal const long Rad330 = 0x5C274434F >> OffsetBits;
        /// <summary>
        /// 2*π
        /// </summary>
        internal const long Rad360 = 0x6487ED511 >> OffsetBits;

        /// <summary>
        /// π/6
        /// </summary>
        internal const long Deg30 = 30L << FractionalBits;
        /// <summary>
        /// π/4
        /// </summary>
        internal const long Deg45 = 45L << FractionalBits;
        /// <summary>
        /// π/3
        /// </summary>
        internal const long Deg60 = 60L << FractionalBits;
        /// <summary>
        /// π/2
        /// </summary>
        internal const long Deg90 = 90L << FractionalBits;
        /// <summary>
        /// 2/3*π
        /// </summary>
        internal const long Deg120 = 120L << FractionalBits;
        /// <summary>
        /// 3/4*π
        /// </summary>
        internal const long Deg135 = 135L << FractionalBits;
        /// <summary>
        /// 5/6*π
        /// </summary>
        internal const long Deg150 = 150L << FractionalBits;
        /// <summary>
        /// π
        /// </summary>
        internal const long Deg180 = 180L << FractionalBits;
        /// <summary>
        /// 7/6*π
        /// </summary>
        internal const long Deg210 = 210L << FractionalBits;
        /// <summary>
        /// 5/4*π
        /// </summary>
        internal const long Deg225 = 225L << FractionalBits;
        /// <summary>
        /// 4/3*π
        /// </summary>
        internal const long Deg240 = 240L << FractionalBits;
        /// <summary>
        /// 3/2*π
        /// </summary>
        internal const long Deg270 = 270L << FractionalBits;
        /// <summary>
        /// 5/3*π
        /// </summary>
        internal const long Deg300 = 300L << FractionalBits;
        /// <summary>
        /// 7/4*π
        /// </summary>
        internal const long Deg315 = 315L << FractionalBits;
        /// <summary>
        /// 11/6*π
        /// </summary>
        internal const long Deg330 = 330L << FractionalBits;
        /// <summary>
        /// 2*π
        /// </summary>
        internal const long Deg360 = 360L << FractionalBits;

        /// <summary>
        /// π*π
        /// </summary>
        internal const long PiOver2 = 0x1921FB544 >> OffsetBits;
        #endregion

        #region 对数
        /// <summary>
        /// 2的自然对数
        /// </summary>
        internal const long LogE2 = 0xB17217F7 >> OffsetBits;
        /// <summary>
        /// Number高位位数<br/>
        /// Log2(Number(高位)) <= Log2Max
        /// </summary>
        internal const long Log2Max = 0x1F00000000 >> OffsetBits;
        /// <summary>
        /// Number低位位数<br/>
        /// Log2(Number(低位)) >= Log2Max
        /// </summary>
        internal const long Log2Min = -0x2000000000 >> OffsetBits;
        #endregion

        /// <summary>
        /// 显示查找表Size
        /// </summary>
        internal const int TableSize = (int)(PiOver2 >> 15);
    }
}