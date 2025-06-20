using Eevee.Diagnosis;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 质数
    /// </summary>
    public readonly struct Prime
    {
        private const int HashPrime = 101;
        private static readonly int[] _primes =
        {
            0000003, 0000007, 0000011, 0000017, 0000023, 0000029,
            0000037, 0000047, 0000059, 0000071, 0000089, 0000107,
            0000131, 0000163, 0000197, 0000239, 0000293, 0000353,
            0000431, 0000521, 0000631, 0000761, 0000919, 0001103,
            0001327, 0001597, 0001931, 0002333, 0002801, 0003371,
            0004049, 0004861, 0005839, 0007013, 0008419, 0010103,
            0012143, 0014591, 0017519, 0021023, 0025229, 0030293,
            0036353, 0043627, 0052361, 0062851, 0075431, 0090523,
            0108631, 0130363, 0156437, 0187751, 0225307, 0270371,
            0324449, 0389357, 0467237, 0560689, 0672827, 0807403,
            0968897, 1162687, 1395263, 1674319, 2009191, 2411033,
            2893249, 3471899, 4166287, 4999559, 5999471, 7199369,
        };

        /// <summary>
        /// 判断一个数是否为质数
        /// </summary>
        public static bool NumberIs(int value)
        {
            if ((value & 1) == 0)
                return value == 2;

            int limit = (int)SquareRoot.Count(value);
            for (int divisor = 3; divisor <= limit; divisor += 2)
                if (value % divisor == 0)
                    return false;
            return true;
        }
        /// <summary>
        /// 获取≥min的最小质数
        /// </summary>
        public static int GetNumber(int value)
        {
            Assert.GreaterEqual<ArgumentException, AssertArgs<int>, int>(value, 0, nameof(value), "获取质数传入参数错误：{0}<0", new AssertArgs<int>(value));
            foreach (int prime in _primes)
                if (prime >= value)
                    return prime;
            for (int i = value | 1; i < int.MaxValue; i += 2)
                if (NumberIs(i) && (i - 1) % HashPrime != 0)
                    return i;
            return value;
        }
    }
}