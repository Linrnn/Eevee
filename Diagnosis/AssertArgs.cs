namespace Eevee.Diagnosis
{
    /// <summary>
    /// 断言参数约束
    /// </summary>
    internal interface IAssertArgs
    {
        string BuildMessage(string format);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct AssertArgs : IAssertArgs
    {
        public string BuildMessage(string format) => format;
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct AssertArgs<TArg0> : IAssertArgs
    {
        internal readonly TArg0 Arg0;

        internal AssertArgs(TArg0 arg0) => Arg0 = arg0;
        public string BuildMessage(string format) => string.Format(format, Arg0);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct AssertArgs<TArg0, TArg1> : IAssertArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;

        internal AssertArgs(TArg0 arg0, TArg1 arg1)
        {
            Arg0 = arg0;
            Arg1 = arg1;
        }
        public string BuildMessage(string format) => string.Format(format, Arg0, Arg1);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct AssertArgs<TArg0, TArg1, TArg2> : IAssertArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;
        internal readonly TArg2 Arg2;

        internal AssertArgs(TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
        }
        public string BuildMessage(string format) => string.Format(format, Arg0, Arg1, Arg2);
    }

    /// <summary>
    /// 断言参数
    /// </summary>
    internal readonly struct AssertArgs<TArg0, TArg1, TArg2, TArg3> : IAssertArgs
    {
        internal readonly TArg0 Arg0;
        internal readonly TArg1 Arg1;
        internal readonly TArg2 Arg2;
        internal readonly TArg3 Arg3;

        internal AssertArgs(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }
        public string BuildMessage(string format) => string.Format(format, Arg0, Arg1, Arg2, Arg3);
    }
}