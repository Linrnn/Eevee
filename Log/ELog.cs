using Eevee.Define;
using System;
using System.Diagnostics;

namespace Eevee.Log
{
    public struct ELog
    {
        [Conditional(Macro.Editor)]
        public static void Trace(string message) => ELogProxy.Impl.Trace(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug)]
        public static void Debug(string message) => ELogProxy.Impl.Debug(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Info(string message) => ELogProxy.Impl.Info(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug)]
        public static void Warn(string message) => ELogProxy.Impl.Warn(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Error(string message) => ELogProxy.Impl.Error(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Error(Exception exception) => ELogProxy.Impl.Error(exception);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Fail(string message) => ELogProxy.Impl.Fail(message);

        [Conditional(Macro.Editor), Conditional(Macro.Debug), Conditional(Macro.Release)]
        public static void Fail(Exception exception) => ELogProxy.Impl.Fail(exception);
    }
}