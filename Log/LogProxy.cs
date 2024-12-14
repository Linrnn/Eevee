namespace Eevee.Log
{
    /// <summary>
    /// Log代理
    /// </summary>
    public readonly struct LogProxy
    {
        private static ILog _impl;

        internal static ILog Impl => _impl ??=
#if UNITY_STANDALONE
            new UnityLog();
#else
            new SystemLog();
#endif

        /// <summary>
        /// 注入log实例
        /// </summary>
        public static void Inject(ILog impl) => _impl = impl;

        /// <summary>
        /// 清空log实例
        /// </summary>
        public static void UnInject() => _impl = null;
    }
}