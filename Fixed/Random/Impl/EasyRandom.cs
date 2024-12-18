using System.Numerics;

namespace Eevee.Fixed
{
    /// <summary>
    /// 便捷实现，子类需要实现 EasyRandom.Get()
    /// </summary>
    public abstract class EasyRandom : IRandom
    {
        protected abstract int Get(int minInclusive, int maxExclusive);

        public virtual sbyte GetSbyte(sbyte min, sbyte max)
        {
            int value = Get(min, max);
            return (sbyte)value;
        }
        public virtual byte GetByte(byte min, byte max)
        {
            int value = Get(min, max);
            return (byte)value;
        }

        public virtual short GetInt16(short min, short max)
        {
            int value = Get(min, max);
            return (short)value;
        }
        public virtual ushort GetUInt16(ushort min, ushort max)
        {
            int value = Get(min, max);
            return (ushort)value;
        }

        public virtual int GetInt32(int min, int max)
        {
            int value = Get(min, max);
            return value;
        }
        public virtual uint GetUInt32(uint min, uint max)
        {
            int left = (int)(min + int.MinValue);
            int right = (int)(max + int.MinValue);
            int value = Get(left, right);
            return (uint)(value - (long)int.MinValue);
        }

        public long GetInt64(long min, long max)
        {
            int left = Get(int.MinValue, int.MaxValue);
            uint right = GetUInt32(uint.MinValue, uint.MaxValue);
            long number = ((long)left << 32) + right;
            var value = SquashNumber(min, max, long.MinValue, long.MaxValue, number);
            return (long)value;
        }
        public ulong GetUInt64(ulong min, ulong max)
        {
            uint left = GetUInt32(uint.MinValue, uint.MaxValue);
            uint right = GetUInt32(uint.MinValue, uint.MaxValue);
            ulong number = ((ulong)left << 32) + right;
            var value = SquashNumber(min, max, ulong.MinValue, ulong.MaxValue, number);
            return (ulong)value;
        }

        /// <summary>
        /// 高GC实现，慎重调用
        /// </summary>
        private BigInteger SquashNumber(BigInteger inputMin, BigInteger inputMax, BigInteger limitMin, BigInteger limitMax, BigInteger number)
        {
            var slope = inputMax - inputMin;
            var suffix = inputMin * limitMax - inputMax * limitMin;
            var numerator = slope * number + suffix;
            var denominator = limitMax - limitMin;
            return numerator / denominator;
        }
    }
}