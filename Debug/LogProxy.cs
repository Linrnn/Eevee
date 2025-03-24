namespace Eevee.Debug
{
    /// <summary>
    /// Log代理
    /// </summary>
    public readonly struct LogProxy
    {
        internal static ILog Impl { get; private set; }

        /// <summary>
        /// 注入Log实例
        /// </summary>
        public static void Inject(ILog impl) => Impl = impl;
        /// <summary>
        /// 清空Log实例
        /// </summary>
        public static void UnInject() => Impl = null;
    }
}