using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 依靠数组实现的随机数<br/>
    /// 数组先置空，因为数组过大导致ide卡死（恼
    /// </summary>
    public sealed class ArrayRandom : EasyRandom
    {
        private static readonly int[] _pool = Array.Empty<int>();

        private long _index;
        private readonly uint _offset;

        public ArrayRandom()
        {
            _index = 0;
            _offset = 1;
        }
        public ArrayRandom(int seed)
        {
            _index = seed;
            _offset = (uint)Math.Abs((long)seed);
        }

        protected override int GetInt(int minInclusive, int maxExclusive)
        {
            int number = GetAndOffset();
            long value = SquashNumber(number, minInclusive, maxExclusive);
            return (int)value;
        }

        private int GetAndOffset()
        {
            int index = GetIndex();
            int value = _pool[index];
            _index = index + _offset;
            return value;
        }
        private int GetIndex()
        {
            long dividend = _index; // 被除数
            int divisor = _pool.Length; // 除数

            if (dividend >= 0)
                return (int)(dividend % divisor);

            int remainder = (int)(-dividend % divisor); // 余数
            return remainder == 0 ? 0 : divisor - remainder;
        }
        private long SquashNumber(long number, long inputMin, long inputMax, long limitMin = int.MinValue, long limitMax = int.MaxValue) // 压缩数字
        {
            var slope = inputMax - inputMin;
            var suffix = inputMin * limitMax - inputMax * limitMin;
            var numerator = slope * number + suffix;
            var denominator = limitMax - limitMin;
            return numerator / denominator;
        }
    }
}