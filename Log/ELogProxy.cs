namespace Eevee.Log
{
    /// <summary>
    /// Log代理
    /// </summary>
    public struct ELogProxy
    {
        private static IELogger _impl;

        internal static IELogger Impl => _impl ??=
#if UNITY_STANDALONE
            new EUnityLog();
#else
            new EConsoleLog();
#endif

        /// <summary>
        /// 注入log实例
        /// </summary>
        public static void Inject(IELogger impl) => _impl = impl;

        /// <summary>
        /// 清空log实例
        /// </summary>
        public static void UnInject() => _impl = null;
    }
}