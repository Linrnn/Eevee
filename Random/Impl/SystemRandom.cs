using SRandom = System.Random;

namespace Eevee.Random
{
    /// <summary>
    /// 使用“System.Random”实现“IRandom”<br/>
    /// “double”参与了“System.Random.Next()”的运算，所以逻辑层不推荐使用“SystemRandom”
    /// </summary>
    public sealed class SystemRandom : EasyRandom
    {
        private readonly SRandom _random;

        public SystemRandom(int seed) => _random = new SRandom(seed);

        protected override int GetInt(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}