namespace Eevee.Fixed
{
    /// <summary>
    /// 三角函数<br/>
    /// 参考链接：https://zh.wikipedia.org/wiki/%E4%B8%89%E8%A7%92%E5%87%BD%E6%95%B0<br/>
    /// 参考链接：https://zh.wikipedia.org/wiki/%E5%8F%8D%E4%B8%89%E8%A7%92%E5%87%BD%E6%95%B0<br/>
    /// 参考链接：https://www.cnblogs.com/Carrawayang/p/14545043.html
    /// </summary>
    internal readonly struct Trigonometric
    {
        #region 正弦的泰勒展开
        private static readonly long[] _sinDen =
        {
            1, // 1! = 1
            -6, // 3! = 6
            120, // 5! = 120
            -5040, // 7! = 5040
            362880, // 9! = 362880
            -39916800, // 11! = 39916800
            6227020800, // 13! = 6227020800
            -1307674368000, // 15! = 1307674368000
            355687428096000, // 17! = 355687428096000
            -121645100408832000, // 19! = 121645100408832000
        };

        internal static Fixed64 Sine(Fixed64 rad, int times = 5)
        {
            var sum = rad;
            var pow = rad;
            var sqr = rad.Sqr();

            for (int i = 1; i < times; ++i)
            {
                pow *= sqr;
                sum += pow / _sinDen[i];
            }

            return sum;
        }
        #endregion

        #region 余弦的泰勒展开
        private static readonly long[] _cosDen =
        {
            1, // 0! = 1
            -2, // 2! = 2
            24, // 4! = 24
            -720, // 6! = 720
            40320, // 8! = 40320
            -3628800, // 10! = 3628800
            479001600, // 12! = 479001600
            -87178291200, // 14! = 87178291200
            20922789888000, // 16! = 20922789888000
            -6402373705728000, // 18! = 6402373705728000
            2432902008176640000, // 20! = 2432902008176640000
        };

        internal static Fixed64 Cosine(Fixed64 rad, int times = 5)
        {
            var sum = Fixed64.One;
            var pow = Fixed64.One;
            var sqr = rad.Sqr();

            for (int i = 1; i < times; ++i)
            {
                pow *= sqr;
                sum += pow / _cosDen[i];
            }

            return sum;
        }
        #endregion

        #region 余切的泰勒展开
        private static readonly long[] _cotNum =
        {
            1,
            1,
            1,
            2,
            1,
            2,
            1382,
            4,
            3617,
            87734,
            174611,
        };
        private static readonly long[] _cotDen =
        {
            1,
            3,
            45,
            945,
            4725,
            93555,
            638512875,
            18243225,
            325641566250,
            38979295480125,
            1531329465290625,
        };

        internal static Fixed64 Cotangent(Fixed64 rad, int times = 7)
        {
            var reciprocal = rad.Reciprocal();
            var sum = reciprocal;
            var pow = reciprocal;
            var sqr = rad.Sqr();

            for (int i = 1; i < times; ++i)
            {
                pow *= sqr;
                sum -= pow * _cotNum[i] / _cotDen[i];
            }

            return sum;
        }
        #endregion

        #region 反正弦的泰勒展开
        private static readonly long[] _asinNum =
        {
            1, // 1 / 1 = 1
            1, // 1 / 1 = 1
            3, // 3 / 1 = 3
            5, // 15 / 3 = 5
            35, // 105 / 3 = 35
            63, // 945 / 15 = 63
            231, // 10395 / 45 = 231
            143, // 135135 / 945 = 143
            6435, // 2027025 / 315 = 6435
            12155, // 34459425 / 2835 = 12155
            46189, // 654729075 / 14175 = 46189
            88179, // 13749310575 / 155925 = 88179
            676039, // 316234143225 / 467775 = 676039
            1300075, // 7905853580625 / 6081075 = 1300075
            5014575, // 213458046676875 / 42567525 = 5014575
            9694845, // 6190283353629375 / 638512875 = 9694845
        };
        private static readonly long[] _asinDen =
        {
            1, // 1 / 1 = 1
            6, // 6 / 1 = 6
            40, // 40 / 1 = 40
            112, // 336 / 3 = 112
            1152, // 3456 / 3 = 1152
            2816, // 42240 / 15 = 2816
            13312, // 599040 / 45 = 13312
            10240, // 9676800 / 945 = 10240
            557056, // 175472640 / 315 = 557056
            1245184, // 3530096640 / 2835 = 1245184
            5505024, // 78033715200 / 14175 = 5505024
            12058624, // 1880240947200 / 155925 = 12058624
            104857600, // 49049763840000 / 467775 = 104857600
            226492416, // 1377317368627200 / 6081075 = 226492416
            973078528, // 41421544567603200 / 42567525 = 973078528
            2080374784, // 1328346084409344000 / 638512875 = 2080374784
        };

        internal static Fixed64 ArcSine(Fixed64 value, int times = 8)
        {
            var sum = value;
            var pow = value;
            var sqr = value.Sqr();

            for (int i = 1; i < times; ++i)
            {
                pow *= sqr;
                sum += pow * _asinNum[i] / _asinDen[i];
            }

            return sum;
        }
        #endregion
    }
}