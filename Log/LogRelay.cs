using Eevee.Define;
using System;
using System.Diagnostics;

namespace Eevee.Log
{
    /// <summary>
    /// Log转发
    /// </summary>
    public readonly struct LogRelay
    {
        [Conditional(Macro.Editor)]
        public static void Trace(string message) => LogProxy.Impl.Trace(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug)]
        public static void Log(string message) => LogProxy.Impl.Log(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Info(string message) => LogProxy.Impl.Info(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug)]
        public static void Warn(string message) => LogProxy.Impl.Warn(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Error(string message) => LogProxy.Impl.Error(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Error(Exception exception) => LogProxy.Impl.Error(exception);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Fail(string message) => LogProxy.Impl.Fail(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Fail(Exception exception) => LogProxy.Impl.Fail(exception);
    }
}