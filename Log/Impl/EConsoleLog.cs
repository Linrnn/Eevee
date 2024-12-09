using System;

namespace Eevee.Log
{
    internal sealed class EConsoleLog : IELogger
    {
        public void Trace(string message) => Console.WriteLine(message);
        public void Debug(string message) => Console.WriteLine(message);
        public void Info(string message) => Console.WriteLine(message);
        public void Warn(string message) => Console.WriteLine(message);
        public void Error(string message) => Console.WriteLine(message);
        public void Error(Exception exception) => Console.WriteLine(exception);
        public void Fail(string message) => Console.WriteLine(message);
        public void Fail(Exception exception) => Console.WriteLine(exception);
    }
}