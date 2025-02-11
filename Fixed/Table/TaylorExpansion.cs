namespace Eevee.Fixed
{
    /// <summary>
    /// 泰勒展开<br/>
    /// 参考链接：https://www.cnblogs.com/Carrawayang/p/14545043.html
    /// 参考链接：https://zh.wikipedia.org/wiki/%E4%B8%89%E8%A7%92%E5%87%BD%E6%95%B0
    /// </summary>
    internal readonly struct TaylorExpansion
    {
        private const byte Times = 5;

        #region Sin的泰勒展开
        private static readonly long[] _sinFactorials =
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

        internal static Fixed64 Sine(Fixed64 value, byte times = Times)
        {
            var sum = value;
            var pow = value;
            var sqr = value * value;

            for (byte i = 1; i < times; ++i)
            {
                pow *= sqr;
                sum += pow / _sinFactorials[i];
            }

            return sum;
        }
        #endregion

        #region Cos的泰勒展开
        private static readonly long[] _cosFactorials =
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

        internal static Fixed64 Cosine(Fixed64 value, byte times = Times)
        {
            var sum = Fixed64.One;
            var pow = Fixed64.One;
            var sqr = value * value;

            for (byte i = 1; i < times; ++i)
            {
                pow *= sqr;
                sum += pow / _cosFactorials[i];
            }

            return sum;
        }
        #endregion
    }
}