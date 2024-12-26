namespace Eevee.Fixed
{
    /// <summary>
    /// 便捷实现，子类需要实现 EasyRandom.GetInt()
    /// </summary>
    public abstract class EasyRandom : IRandom
    {
        public virtual sbyte GetSbyte(sbyte minInclusive, sbyte maxExclusive)
        {
            int value = GetInt(minInclusive, maxExclusive);
            return (sbyte)value;
        }
        public virtual byte GetByte(byte minInclusive, byte maxExclusive)
        {
            int value = GetInt(minInclusive, maxExclusive);
            return (byte)value;
        }

        public virtual short GetInt16(short minInclusive, short maxExclusive)
        {
            int value = GetInt(minInclusive, maxExclusive);
            return (short)value;
        }
        public virtual ushort GetUInt16(ushort minInclusive, ushort maxExclusive)
        {
            int value = GetInt(minInclusive, maxExclusive);
            return (ushort)value;
        }

        public virtual int GetInt32(int minInclusive, int maxExclusive)
        {
            int value = GetInt(minInclusive, maxExclusive);
            return value;
        }
        public virtual uint GetUInt32(uint minInclusive, uint maxExclusive)
        {
            uint value = GetUInt(minInclusive, maxExclusive);
            return value;
        }

        public virtual long GetInt64(long minInclusive, long maxExclusive)
        {
            ulong min = L2Ul(minInclusive);
            ulong max = L2Ul(maxExclusive);
            ulong value = GetULong(min, max);
            return Ul2L(value);
        }
        public virtual ulong GetUInt64(ulong minInclusive, ulong maxExclusive)
        {
            ulong value = GetULong(minInclusive, maxExclusive);
            return value;
        }

        /// <summary>
        /// 随机获取Int32<br/>
        /// 随机数的核心接口
        /// </summary>
        /// <param name="minInclusive">包括最小值</param>
        /// <param name="maxExclusive">不包括最大值</param>
        protected abstract int GetInt(int minInclusive, int maxExclusive);

        private uint GetUInt(uint minInclusive, uint maxExclusive)
        {
            int min = (int)(minInclusive + int.MinValue);
            int max = (int)(maxExclusive + int.MinValue);
            int value = GetInt(min, max);
            return (uint)(value - (long)int.MinValue);
        }
        private ulong GetULong(ulong minInclusive, ulong maxExclusive) // 此实现随机数分布不均匀
        {
            uint minHigh = (uint)(minInclusive >> 32);
            uint maxHigh = (uint)(maxExclusive >> 32);

            if (minHigh == maxHigh)
            {
                uint minLow = (uint)(minInclusive & uint.MaxValue);
                uint maxLow = (uint)(maxExclusive & uint.MaxValue);
                ulong low = GetUInt(minLow, maxLow);
                return (ulong)minHigh << 32 | low;
            }

            uint realMaxHigh = maxHigh == uint.MaxValue ? maxHigh : maxHigh + 1;
            ulong high = GetUInt(minHigh, realMaxHigh);
            if (high == minHigh)
            {
                uint minLow = (uint)(minInclusive & uint.MaxValue);
                ulong low = GetUInt(minLow, uint.MaxValue);
                return high << 32 | low;
            }

            if (high == realMaxHigh)
            {
                uint maxLow = (uint)(maxExclusive & uint.MaxValue);
                ulong low = GetUInt(uint.MinValue, maxLow);
                return high << 32 | low;
            }
            else
            {
                ulong low = GetUInt(uint.MinValue, uint.MaxValue);
                return high << 32 | low;
            }
        }

        private ulong L2Ul(long num) => num >= 0L ? (ulong)num + long.MaxValue + 1 : (ulong)(num - long.MinValue);
        private long Ul2L(ulong num) => num > long.MaxValue ? (long)(num - long.MaxValue - 1) : (long)num + long.MinValue;
    }
}