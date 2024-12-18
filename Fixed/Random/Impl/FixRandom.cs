namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性随机数
    /// </summary>
    public sealed class FixRandom : EasyRandom
    {
        protected override int Get(int minInclusive, int maxExclusive)
        {
            // todo Eevee 未实现 FixRandom
            throw new System.NotImplementedException();
        }
    }
}