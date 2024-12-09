namespace Eevee.Log
{
    /// <summary>
    /// Log代理
    /// </summary>
    public struct ELogProxy
    {
        private static IELogger _impl;

#if UNITY_STANDALONE
        internal static IELogger Impl => _impl ??= new EUnityLog();
#else
        internal static IELogger Impl => _impl ??= new EConsoleLog();
#endif

        /// <summary>
        /// 注入log实现
        /// </summary>
        /// <param name="impl">实现类实例</param>
        public static void Inject(IELogger impl) => _impl = impl;
    }
}