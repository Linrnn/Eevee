#if UNITY_STANDALONE
using UnityEngine;

namespace Eevee.Fixed
{
    /// <summary>
    /// 使用 UnityEngine.Random 实现 IRandom<br/>
    /// UnityEngine.Random.InitState() 是静态方法，逻辑层不推荐使用 UnityRandom
    /// </summary>
    public sealed class UnityRandom : EasyRandom
    {
        public UnityRandom(int seed) => Random.InitState(seed);

        protected override int GetInt(int minInclusive, int maxExclusive) => Random.Range(minInclusive, maxExclusive);
    }
}
#endif