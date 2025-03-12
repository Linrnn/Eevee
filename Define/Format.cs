using System;
using System.Globalization;

namespace Eevee.Define
{
    /// <summary>
    /// 格式化
    /// </summary>
    public readonly struct Format
    {
        /// <summary>
        /// 小数显示的格式
        /// </summary>
        public const string Fractional = "0.#######";

        /// <summary>
        /// 当前项目使用的格式
        /// </summary>
        public static IFormatProvider Use => NumberFormatInfo.CurrentInfo;

        /// <summary>
        /// 当前文化格式
        /// </summary>
        public static IFormatProvider CurrentCulture => NumberFormatInfo.CurrentInfo;
        /// <summary>
        /// 固定文化格式
        /// </summary>
        public static IFormatProvider InvariantCulture => CultureInfo.InvariantCulture.NumberFormat;
    }
}