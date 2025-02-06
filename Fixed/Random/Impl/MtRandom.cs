namespace Eevee.Fixed
{
    /// <summary>
    /// 根据“梅森旋转算法”实现的随机数<br/>
    /// 参考链接：http://www.codeproject.com/Articles/164087/Random-Number-Generation
    /// </summary>
    public sealed class MtRandom : EasyRandom
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MatrixA = 0x9908B0DF;
        private const uint UpperMask = 1U << 31;
        private const uint LowerMask = int.MaxValue;
        private static readonly uint[] _mag01 = { 0, MatrixA };

        private readonly uint[] _mt = new uint[N];
        private int _mti = N + 1;

        public MtRandom(int seed) => Initialize((uint)seed);
        protected override int GetInt(int minInclusive, int maxExclusive)
        {
            int range = RangeInt32();
            int diff = maxExclusive - minInclusive;
            return minInclusive + range % diff;
        }

        private int RangeInt32() => (int)(RangeUInt32() >> 1);
        private uint RangeUInt32()
        {
            if (_mti >= N)
            {
                if (_mti == N + 1)
                {
                    Initialize(5489);
                }

                int i;

                for (i = 0; i < N - M; ++i)
                {
                    uint x = _mt[i] & UpperMask | _mt[i + 1] & LowerMask;
                    _mt[i] = _mt[i + M] ^ x >> 1 ^ _mag01[x & 1];
                }

                for (; i < N - 1; ++i)
                {
                    uint x = _mt[i] & UpperMask | _mt[i + 1] & LowerMask;
                    _mt[i] = _mt[i + (M - N)] ^ x >> 1 ^ _mag01[x & 1];
                }

                uint y = _mt[N - 1] & UpperMask | _mt[0] & LowerMask;
                _mt[N - 1] = _mt[M - 1] ^ y >> 1 ^ _mag01[y & 1];
                _mti = 0;
            }

            uint a = _mt[_mti++];
            uint b = a ^ a >> 11;
            uint c = b ^ b << 7 & 0x9D2C5680;
            uint d = c ^ c << 15 & 0xEfC60000;
            uint e = d ^ d >> 18;
            return e;
        }
        private void Initialize(uint seed)
        {
            _mt[0] = seed;
            for (_mti = 1; _mti < N; ++_mti)
            {
                uint prev = _mt[_mti - 1];
                _mt[_mti] = (uint)(1812433253 * (prev ^ prev >> 30) + _mti);
            }
        }
    }
}