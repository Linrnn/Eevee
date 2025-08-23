#if UNITY_5_3_OR_NEWER
using System;
using UDebug = UnityEngine.Debug;

namespace Eevee.Diagnosis
{
    /// <summary>
    /// 使用 UnityEngine.Debug 实现 ILog
    /// </summary>
    public sealed class UnityLog : ILog
    {
        public void Trace(string message) => UDebug.Log(message);
        public void Debug(string message) => UDebug.Log(message);
        public void Info(string message) => UDebug.Log(message);
        public void Warn(string message) => UDebug.LogWarning(message);
        public void Error(string message) => UDebug.LogError(message);
        public void Error(Exception exception) => UDebug.LogException(exception);
        public void Fail(string message) => UDebug.LogError(message);
        public void Fail(Exception exception) => UDebug.LogException(exception);
    }
}
#endif