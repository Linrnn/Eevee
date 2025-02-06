using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 三角函数，sin
    /// </summary>
    internal readonly struct Sine
    {
        #region 输出32位小数精度表
        private static void Sin0To90()
        {
            for (int i = 0; i <= 90; ++i)
            {
                double rad = i * Math.PI / 180D;
                double sin = Math.Sin(rad);
                double scale = sin * (1L << Const.FractionalBits);
                long round = (long)Math.Round(scale);
                string hex = Convert.ToString(round, 16).PadLeft(9, '0').ToUpper();

                Console.Write("0x{0}L, ", hex);
                if ((i + 1) % 7 == 0)
                    Console.WriteLine();
            }
        }
        private static void Sin0_01To0_99()
        {
            for (int i = 1; i <= 99; ++i)
            {
                double rad = i * Math.PI * 0.01D / 180D;
                double sin = Math.Sin(rad);
                double scale = sin * (1L << Const.FractionalBits);
                long round = (long)Math.Round(scale);
                string hex = Convert.ToString(round, 16).PadLeft(7, '0').ToUpper();

                Console.Write("0x{0}L, ", hex);
                if (i % 9 == 0)
                    Console.WriteLine();
            }
        }
        #endregion

        #region 32位小数精度
        private static readonly long[] _table1 = // 0°到90°
        {
            0x000000000L, 0x00477C2CBL, 0x008EF2C65L, 0x00D65E3A4L, 0x011DB8F6DL, 0x0164FD6B9L, 0x01AC2609BL,
            0x01F32D44CL, 0x023A0D92DL, 0x0280C16CFL, 0x02C7434FCL, 0x030D8DBBBL, 0x03539B359L, 0x03996646EL,
            0x03DEE97E6L, 0x04241F706L, 0x046902B75L, 0x04AD8DF3EL, 0x04F1BBCDDL, 0x053586F40L, 0x0578EA1D2L,
            0x05BBE007FL, 0x05FE637BCL, 0x06406F48AL, 0x0681FE484L, 0x06C30B5DDL, 0x07039176BL, 0x07438B8ADL,
            0x0782F49D1L, 0x07C1C7BB8L, 0x080000000L, 0x083D98908L, 0x087A8C9F5L, 0x08B6D76BCL, 0x08F274421L,
            0x092D5E7C4L, 0x096791824L, 0x09A108CA3L, 0x09D9BFD8EL, 0x0A11B2422L, 0x0A48DBA92L, 0x0A7F37C0AL,
            0x0AB4C24B7L, 0x0AE9771CCL, 0x0B1D52187L, 0x0B504F334L, 0x0B826A735L, 0x0BB39FF06L, 0x0BE3EBD42L,
            0x0C134A5A5L, 0x0C41B7D16L, 0x0C6F309A9L, 0x0C9BB12A0L, 0x0CC736075L, 0x0CF1BBCDDL, 0x0D1B3F2C9L,
            0x0D43BCE6DL, 0x0D6B31D45L, 0x0D919AE16L, 0x0DB6F50F4L, 0x0DDB3D743L, 0x0DFE713BFL, 0x0E208DA7BL,
            0x0E41900EAL, 0x0E6175DDBL, 0x0E803C981L, 0x0E9DE1D78L, 0x0EBA634C1L, 0x0ED5BEBCDL, 0x0EEFF2078L,
            0x0F08FB213L, 0x0F20D8160L, 0x0F378709AL, 0x0F4D06374L, 0x0F6153F1BL, 0x0F746EA3AL, 0x0F8654CFCL,
            0x0F970510AL, 0x0FA67E194L, 0x0FB4BEB4AL, 0x0FC1C5C64L, 0x0FCD924A1L, 0x0FD82354AL, 0x0FE17812DL,
            0x0FE98FCA8L, 0x0FF069DA1L, 0x0FF605B8CL, 0x0FFA62F69L, 0x0FFD813C6L, 0x0FFF604C0L, 0x100000000L,
        };
        private static readonly long[] _table2 = // 0.01°到0.99°
        {
            0x00B702DL, 0x016E05AL, 0x0225088L, 0x02DC0B5L, 0x03930E2L, 0x044A10EL, 0x050113BL, 0x05B8168L, 0x066F194L,
            0x07261C0L, 0x07DD1ECL, 0x0894218L, 0x094B243L, 0x0A0226EL, 0x0AB9299L, 0x0B702C4L, 0x0C272EEL, 0x0CDE318L,
            0x0D95341L, 0x0E4C36AL, 0x0F03392L, 0x0FBA3BAL, 0x10713E1L, 0x1128408L, 0x11DF42FL, 0x1296454L, 0x134D47AL,
            0x140449EL, 0x14BB4C2L, 0x15724E5L, 0x1629508L, 0x16E052AL, 0x179754BL, 0x184E56BL, 0x190558BL, 0x19BC5AAL,
            0x1A735C8L, 0x1B2A5E5L, 0x1BE1601L, 0x1C9861DL, 0x1D4F637L, 0x1E06651L, 0x1EBD669L, 0x1F74681L, 0x202B697L,
            0x20E26ADL, 0x21996C2L, 0x22506D5L, 0x23076E7L, 0x23BE6F9L, 0x2475709L, 0x252C718L, 0x25E3725L, 0x269A732L,
            0x275173DL, 0x2808747L, 0x28BF750L, 0x2976757L, 0x2A2D75EL, 0x2AE4762L, 0x2B9B766L, 0x2C52768L, 0x2D09768L,
            0x2DC0768L, 0x2E77765L, 0x2F2E761L, 0x2FE575CL, 0x309C755L, 0x315374DL, 0x320A743L, 0x32C1738L, 0x337872AL,
            0x342F71CL, 0x34E670BL, 0x359D6F9L, 0x36546E5L, 0x370B6CFL, 0x37C26B8L, 0x387969FL, 0x3930684L, 0x39E7667L,
            0x3A9E649L, 0x3B55628L, 0x3C0C606L, 0x3CC35E1L, 0x3D7A5BBL, 0x3E31593L, 0x3EE8569L, 0x3F9F53CL, 0x405650EL,
            0x410D4DEL, 0x41C44ABL, 0x427B477L, 0x4332440L, 0x43E9408L, 0x44A03CDL, 0x4557390L, 0x460E350L, 0x46C530FL,
        };
        #endregion

        /// <summary>
        /// _table2精度的倒数
        /// </summary>
        internal static short Table2Scale = 100;

        internal static long CountInteger(int value) => _table1[value];
        internal static long CountFractional(int value) => _table2[value - 1];
    }
}