using Eevee.Log;

namespace Eevee.Fixed
{
    /// <summary>
    /// 平方根
    /// </summary>
    internal readonly struct SquareRoot
    {
        // Math.Sqrt()，调用10000次，0.06s
        // Mathf.Sqrt()，调用10000次，0.22s
        // TableSqrt()，调用10000次，0.22s
        // NewtonSqrt()，调用10000次，0.69s
        // BitBinarySqrt()，调用10000次，1.18s
        // BinarySqrt()，调用10000次，1.43s

        #region 参考文档：https: //www.conerlius.cn/%E7%AE%97%E6%B3%95/2019/09/26/%E5%AE%9A%E7%82%B9%E6%95%B0%E5%BC%80%E6%A0%B9%E5%8F%B7%E7%9A%84%E6%80%A7%E8%83%BD%E9%97%AE%E9%A2%98.html
        private static readonly byte[] _table =
        {
            000, 016, 022, 027, 032, 035, 039, 042, 045, 048, 050, 053, 055, 057, 059, 061,
            064, 065, 067, 069, 071, 073, 075, 076, 078, 080, 081, 083, 084, 086, 087, 089,
            090, 091, 093, 094, 096, 097, 098, 099, 101, 102, 103, 104, 106, 107, 108, 109,
            110, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126,
            128, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142,
            143, 144, 144, 145, 146, 147, 148, 149, 150, 150, 151, 152, 153, 154, 155, 155,
            156, 157, 158, 159, 160, 160, 161, 162, 163, 163, 164, 165, 166, 167, 167, 168,
            169, 170, 170, 171, 172, 173, 173, 174, 175, 176, 176, 177, 178, 178, 179, 180,
            181, 181, 182, 183, 183, 184, 185, 185, 186, 187, 187, 188, 189, 189, 190, 191,
            192, 192, 193, 193, 194, 195, 195, 196, 197, 197, 198, 199, 199, 200, 201, 201,
            202, 203, 203, 204, 204, 205, 206, 206, 207, 208, 208, 209, 209, 210, 211, 211,
            212, 212, 213, 214, 214, 215, 215, 216, 217, 217, 218, 218, 219, 219, 220, 221,
            221, 222, 222, 223, 224, 224, 225, 225, 226, 226, 227, 227, 228, 229, 229, 230,
            230, 231, 231, 232, 232, 233, 234, 234, 235, 235, 236, 236, 237, 237, 238, 238,
            239, 240, 240, 241, 241, 242, 242, 243, 243, 244, 244, 245, 245, 246, 246, 247,
            247, 248, 248, 249, 249, 250, 250, 251, 251, 252, 252, 253, 253, 254, 254, 255,
        };
        #endregion

        internal static long Sqrt(long value)
        {
            if (value < 0L)
            {
                LogRelay.Error($"[Fixed] SquareRoot.Sqrt()，value：{value}是负数，无法开方");
                return 0;
            }

            if (value <= int.MaxValue)
            {
                return TableSqrt((int)value);
            }

            return NewtonSqrt(value);
        }
        private static long TableSqrt(int value) => value switch
        {
            >= 1 << 30 => From24(value, 24, 8),
            >= 1 << 28 => From24(value, 22, 7),
            >= 1 << 26 => From24(value, 20, 6),
            >= 1 << 24 => From24(value, 18, 5),
            >= 1 << 22 => From16(value, 16, 4),
            >= 1 << 20 => From16(value, 14, 3),
            >= 1 << 18 => From16(value, 12, 2),
            >= 1 << 16 => From16(value, 10, 1),
            >= 1 << 14 => From08(value, 8, 0, 1),
            >= 1 << 12 => From08(value, 6, 1, 1),
            >= 1 << 10 => From08(value, 4, 2, 1),
            >= 1 << 08 => From08(value, 2, 3, 1),
            _ => _table[value] >> 4,
        };
        private static long NewtonSqrt(long value) // 牛顿迭代法
        {
            long x0 = 1L;

            while (x0 * x0 != value)
            {
                long x1 = value / x0;
                long x2 = x0 + x1 >> 1;
                if (x0 == x2 || x1 == x2)
                    break;

                x0 = x2;
            }

            return x0;
        }
        private static long BitBinarySqrt(long value) // 二分法（先计算大致区间）
        {
            int midBit = Const.TotalBits >> 2;
            for (int startBit = 0, endBit = Const.TotalBits >> 1; startBit <= endBit;)
            {
                midBit = startBit + endBit >> 1;
                if (1L << (midBit << 1) > value)
                    endBit = midBit - 1;
                else
                    startBit = midBit + 1;
            }

            long start = 1L << midBit - 1;
            long end = 1L << midBit + 1;
            long mid = start + end >> 1;
            ulong ulValue = (ulong)value;
            while (start <= end && mid * mid != value)
            {
                mid = start + end >> 1;
                ulong ulMid = (ulong)mid;
                if (ulMid * ulMid > ulValue)
                    end = mid - 1;
                else
                    start = mid + 1;
            }

            return mid;
        }
        private static long BinarySqrt(long value) // 二分法
        {
            long start = 0;
            long end = 3037000499;
            long mid = 1518500250; // (start + end) / 2
            ulong ulValue = (ulong)value;
            while (start <= end && mid * mid != value)
            {
                mid = start + end >> 1;
                ulong ulMid = (ulong)mid;
                if (ulMid * ulMid > ulValue)
                    end = mid - 1;
                else
                    start = mid + 1;
            }

            return mid;
        }

        private static long From24(int x, byte i, byte o)
        {
            long x0 = _table[x >> i] << o;
            long x1 = x0 + 1 + x / x0 >> 1;
            long x2 = x1 + 1 + x / x1 >> 1;
            return x2 * x2 > x ? x2 - 1 : x2;
        }
        private static long From16(int x, byte i, byte o)
        {
            long x0 = _table[x >> i] << o;
            long x1 = x0 + 1 + x / x0 >> 1;
            return x1 * x1 > x ? x1 - 1 : x1;
        }
        private static long From08(int x, byte i, byte o, byte l)
        {
            long x0 = (_table[x >> i] >> o) + l;
            return x0 * x0 > x ? x0 - 1 : x0;
        }
    }
}