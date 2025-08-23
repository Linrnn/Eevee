namespace Eevee.Diagnosis
{
    /// <summary>
    /// 断言参数约束
    /// </summary>
    internal interface IDiagnosisArgs
    {
        string Build(string format);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct DiagnosisArgs : IDiagnosisArgs
    {
        public string Build(string format) => format;
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct DiagnosisArgs<TArg0> : IDiagnosisArgs
    {
        internal readonly TArg0 Arg0;

        internal DiagnosisArgs(TArg0 arg0) => Arg0 = arg0;
        public string Build(string format) => string.Format(format, Arg0);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct DiagnosisArgs<TArg0, TArg1> : IDiagnosisArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;

        internal DiagnosisArgs(TArg0 arg0, TArg1 arg1)
        {
            Arg0 = arg0;
            Arg1 = arg1;
        }
        public string Build(string format) => string.Format(format, Arg0, Arg1);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct DiagnosisArgs<TArg0, TArg1, TArg2> : IDiagnosisArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;
        internal readonly TArg2 Arg2;

        internal DiagnosisArgs(TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
        }
        public string Build(string format) => string.Format(format, Arg0, Arg1, Arg2);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct DiagnosisArgs<TArg0, TArg1, TArg2, TArg3> : IDiagnosisArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;
        internal readonly TArg2 Arg2;
        internal readonly TArg3 Arg3;

        internal DiagnosisArgs(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }
        public string Build(string format) => string.Format(format, Arg0, Arg1, Arg2, Arg3);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct DiagnosisArgs<TArg0, TArg1, TArg2, TArg3, TArg4> : IDiagnosisArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;
        internal readonly TArg2 Arg2;
        internal readonly TArg3 Arg3;
        internal readonly TArg4 Arg4;

        internal DiagnosisArgs(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }
        public string Build(string format) => string.Format(format, Arg0, Arg1, Arg2, Arg3, Arg4);
    }
}