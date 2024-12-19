using System.Numerics;

namespace Eevee.Fixed
{
    /// <summary>
    /// 便捷实现，子类需要实现 EasyRandom.Get()
    /// </summary>
    public abstract class EasyRandom : IRandom
    {
        /// <summary>
        /// 随机获取Int32，随机数的核心接口
        /// </summary>
        /// <param name="minInclusive">包括最小值</param>
        /// <param name="maxExclusive">不包括最大值</param>
        /// <returns></returns>
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

        public virtual long GetInt64(long min, long max)
        {
            int left = Get(int.MinValue, int.MaxValue);
            uint right = GetUInt32(uint.MinValue, uint.MaxValue);
            long number = ((long)left << 32) + right;
            var value = SquashNumber(number, min, max, long.MinValue, long.MaxValue);
            return (long)value;
        }
        public virtual ulong GetUInt64(ulong min, ulong max)
        {
            uint left = GetUInt32(uint.MinValue, uint.MaxValue);
            uint right = GetUInt32(uint.MinValue, uint.MaxValue);
            ulong number = ((ulong)left << 32) + right;
            var value = SquashNumber(number, min, max, ulong.MinValue, ulong.MaxValue);
            return (ulong)value;
        }

        /// <summary>
        /// 压缩数字
        /// 高GC实现，慎重调用<br/>
        /// </summary>
        private BigInteger SquashNumber(BigInteger number, BigInteger inputMin, BigInteger inputMax, BigInteger limitMin, BigInteger limitMax)
        {
            var slope = inputMax - inputMin;
            var suffix = inputMin * limitMax - inputMax * limitMin;
            var numerator = slope * number + suffix;
            var denominator = limitMax - limitMin;
            return numerator / denominator;
        }
    }
}