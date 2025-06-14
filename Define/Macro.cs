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
        public const string Runtime = "UNITY_5_3_OR_NEWER";

        public const string Assert = "EEVEE_ASSERT";
        public const string TryCatch = "EEVEE_TRY_CATCH";
        public const string CheckRelease = "EEVEE_CHECK_RELEASE";

#if DEBUG || UNITY_EDITOR
        public static bool HasTryCatch =>
#else
        public const bool HasTryCatch =
#endif
#if EEVEE_TRY_CATCH
            true;
#else
            false;
#endif

#if DEBUG || UNITY_EDITOR
        public static bool HasCheckRelease =>
#else
        public const bool HasCheckRelease =
#endif
#if EEVEE_CHECK_RELEASE
            true;
#else
            false;
#endif
    }
}