namespace Eevee.Fixed
{
    /// <summary>
    /// Random代理
    /// </summary>
    public readonly struct RandomProxy
    {
        private static IRandom _impl;

        internal static IRandom Impl => _impl ??= new MersenneTwisterRandom();

        /// <summary>
        /// 注入Random实例
        /// </summary>
        public static void Inject(IRandom impl) => _impl = impl;
        /// <summary>
        /// 清空Random实例
        /// </summary>
        public static void UnInject() => _impl = null;
    }
}