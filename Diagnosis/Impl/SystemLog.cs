using System;

namespace Eevee.Diagnosis
{
    /// <summary>
    /// 使用 Console.WriteLine 实现 ILog
    /// </summary>
    public sealed class SystemLog : ILog
    {
        public void Trace(string message) => Console.WriteLine(message);
        public void Log(string message) => Console.WriteLine(message);
        public void Info(string message) => Console.WriteLine(message);
        public void Warn(string message) => Console.WriteLine(message);
        public void Error(string message) => Console.WriteLine(message);
        public void Error(Exception exception) => Console.WriteLine(exception);
        public void Fail(string message) => Console.WriteLine(message);
        public void Fail(Exception exception) => Console.WriteLine(exception);
    }
}