namespace Eevee.Fixed
{
    /// <summary>
    /// Random代理
    /// </summary>
    public readonly struct RandomProxy
    {
        internal static IRandom Impl { get; private set; }

        /// <summary>
        /// 注入Random实例
        /// </summary>
        public static void Inject(IRandom impl) => Impl = impl;
        /// <summary>
        /// 清空Random实例
        /// </summary>
        public static void UnInject() => Impl = null;
    }
}