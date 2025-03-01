namespace Eevee.Fixed
{
    public readonly struct Const
    {
        #region 字节数
        /// <summary>
        /// 总字节数，这个值不允许修改！！！
        /// </summary>
        public const int FullBits = 64;
        /// <summary>
        /// 小数位数，必须是偶数，且必须≤32<br/>
        /// 修改后，检查字段引用，确定是否需要修改引用处
        /// </summary>
        public const int FractionalBits = 32;
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
        /// 最小值（Fixed64最小值）
        /// </summary>
        public const long MinValue = MinPeak + 2L;
        /// <summary>
        /// 最大值（Fixed64最大值）
        /// </summary>
        public const long MaxValue = MaxPeak - 2L;

        /// <summary>
        /// 无穷小
        /// </summary>
        public const long Infinitesimal = MinPeak + 1L;
        /// <summary>
        /// 无穷大
        /// </summary>
        public const long Infinity = MaxPeak - 1L;

        /// <summary>
        /// 可表示的最小值（非Fixed64最小值）
        /// </summary>e
        internal const long MinPeak = long.MinValue;
        /// <summary>
        /// 可表示的最大值（非Fixed64最大值）
        /// </summary>
        internal const long MaxPeak = long.MaxValue;
        #endregion

        #region 数字
        /// <summary>
        /// 0
        /// </summary>
        public const long Zero = 0L;
        /// <summary>
        /// 0.5
        /// </summary>
        public const long Half = One >> 1;
        /// <summary>
        /// 1
        /// </summary>
        public const long One = 1L << FractionalBits;
        /// <summary>
        /// 极小值
        /// </summary>
        public const long Epsilon = One / 1000L;

        /// <summary>
        /// 整数部分
        /// </summary>
        public const long IntegerPart = ~(One - 1L);
        /// <summary>
        /// 小数部分
        /// </summary>
        public const long FractionalPart = One - 1L;
        #endregion

        #region 圆周率
        /// <summary>
        /// π/6
        /// </summary>
        public const long Deg30 = 30L << FractionalBits;
        /// <summary>
        /// π/4
        /// </summary>
        public const long Deg45 = 45L << FractionalBits;
        /// <summary>
        /// π/3
        /// </summary>
        public const long Deg60 = 60L << FractionalBits;
        /// <summary>
        /// π/2
        /// </summary>
        public const long Deg90 = 90L << FractionalBits;
        /// <summary>
        /// 2/3*π
        /// </summary>
        public const long Deg120 = 120L << FractionalBits;
        /// <summary>
        /// 3/4*π
        /// </summary>
        public const long Deg135 = 135L << FractionalBits;
        /// <summary>
        /// 5/6*π
        /// </summary>
        public const long Deg150 = 150L << FractionalBits;
        /// <summary>
        /// π
        /// </summary>
        public const long Deg180 = 180L << FractionalBits;
        /// <summary>
        /// 7/6*π
        /// </summary>
        public const long Deg210 = 210L << FractionalBits;
        /// <summary>
        /// 5/4*π
        /// </summary>
        public const long Deg225 = 225L << FractionalBits;
        /// <summary>
        /// 4/3*π
        /// </summary>
        public const long Deg240 = 240L << FractionalBits;
        /// <summary>
        /// 3/2*π
        /// </summary>
        public const long Deg270 = 270L << FractionalBits;
        /// <summary>
        /// 5/3*π
        /// </summary>
        public const long Deg300 = 300L << FractionalBits;
        /// <summary>
        /// 7/4*π
        /// </summary>
        public const long Deg315 = 315L << FractionalBits;
        /// <summary>
        /// 11/6*π
        /// </summary>
        public const long Deg330 = 330L << FractionalBits;
        /// <summary>
        /// 2*π
        /// </summary>
        public const long Deg360 = 360L << FractionalBits;

        /// <summary>
        /// π/6
        /// </summary>
        public const long Rad30 = 0x860A91C1 >> OffsetBits;
        /// <summary>
        /// π/4
        /// </summary>
        public const long Rad45 = 0xC90FDAA2 >> OffsetBits;
        /// <summary>
        /// π/3
        /// </summary>
        public const long Rad60 = 0x10C152382 >> OffsetBits;
        /// <summary>
        /// π/2
        /// </summary>
        public const long Rad90 = 0x1921FB544 >> OffsetBits;
        /// <summary>
        /// 2/3*π
        /// </summary>
        public const long Rad120 = 0x2182A4705 >> OffsetBits;
        /// <summary>
        /// 3/4*π
        /// </summary>
        public const long Rad135 = 0x25B2F8FE6 >> OffsetBits;
        /// <summary>
        /// 5/6*π
        /// </summary>
        public const long Rad150 = 0x29E34D8C7 >> OffsetBits;
        /// <summary>
        /// π
        /// </summary>
        public const long Rad180 = 0x3243F6A88 >> OffsetBits;
        /// <summary>
        /// 7/6*π
        /// </summary>
        public const long Rad210 = 0x3AA49FC49 >> OffsetBits;
        /// <summary>
        /// 5/4*π
        /// </summary>
        public const long Rad225 = 0x3ED4F452A >> OffsetBits;
        /// <summary>
        /// 4/3*π
        /// </summary>
        public const long Rad240 = 0x430548E0B >> OffsetBits;
        /// <summary>
        /// 3/2*π
        /// </summary>
        public const long Rad270 = 0x4B65F1FCC >> OffsetBits;
        /// <summary>
        /// 5/3*π
        /// </summary>
        public const long Rad300 = 0x53C69B18E >> OffsetBits;
        /// <summary>
        /// 7/4*π
        /// </summary>
        public const long Rad315 = 0x57F6EFA6E >> OffsetBits;
        /// <summary>
        /// 11/6*π
        /// </summary>
        public const long Rad330 = 0x5C274434F >> OffsetBits;
        /// <summary>
        /// 2*π
        /// </summary>
        public const long Rad360 = 0x6487ED511 >> OffsetBits;

        /// <summary>
        /// π/180
        /// </summary>
        public const long Deg2Rad = 0x477D1A8 >> OffsetBits;
        /// <summary>
        /// 180/π
        /// </summary>
        public const long Rad2Deg = 0x394BB834C7 >> OffsetBits;
        #endregion

        #region 对数
        /// <summary>
        /// 2的自然对数（即e为底，2的对数）
        /// </summary>
        public const long Ln2 = 0xB17217F7 >> OffsetBits;
        /// <summary>
        /// 10为底，2的对数
        /// </summary>
        public const long Lg2 = 0x4D104D42 >> OffsetBits;

        /// <summary>
        /// 低位位数<br/>
        /// Log2（低位）≥ Log2Min
        /// </summary>
        public const long Log2Min = -0x2000000000 >> OffsetBits;
        /// <summary>
        /// 高位位数<br/>
        /// Log2（高位）≤ Log2Max
        /// </summary>
        public const long Log2Max = 0x1F00000000 >> OffsetBits;
        #endregion
    }
}