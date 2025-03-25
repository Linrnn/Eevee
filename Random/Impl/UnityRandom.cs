#if UNITY_5_3_OR_NEWER
using URandom = UnityEngine.Random;

namespace Eevee.Random
{
    /// <summary>
    /// 使用 UnityEngine.Random 实现 IRandom<br/>
    /// UnityEngine.Random.InitState() 是静态方法，逻辑层不推荐使用 UnityRandom
    /// </summary>
    public sealed class UnityRandom : EasyRandom
    {
        public UnityRandom(int seed) => URandom.InitState(seed);

        protected override int GetInt(int minInclusive, int maxExclusive) => URandom.Range(minInclusive, maxExclusive);
    }
}
#endif