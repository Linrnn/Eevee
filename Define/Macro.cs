namespace Eevee.Define
{
    /// <summary>
    /// 宏定义
    /// </summary>
    public readonly struct Macro
    {
        public const string Debug = "DEBUG";
        public const string Release = "RELEASE";

        public const string Editor = "UNITY_EDITOR";
        public const string Standalone = "UNITY_STANDALONE";

        public const string TryCatch = "EEVEE_TRY_CATCH";

        public static bool HasTryCatch =>
#if EEVEE_TRY_CATCH
            true;
#else
            false;
#endif
    }
}