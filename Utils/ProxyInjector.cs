using System;

namespace Eevee.Utils
{
    public abstract class ProxyInjector<T> where T : class
    {
        internal static T Impl { get; private set; }

        /// <summary>
        /// 注入实例
        /// </summary>
        public static void Inject(T impl) => Impl = impl ?? throw new ArgumentNullException(nameof(impl));
        /// <summary>
        /// 清空实例
        /// </summary>
        public static void UnInject() => Impl = null;
    }
}