#if UNITY_STANDALONE
using System;
using UnityEngine;

namespace Eevee.Log
{
    /// <summary>
    /// 使用 UnityEngine.Debug 实现 ILog
    /// </summary>
    public sealed class UnityLog : ILog
    {
        public void Trace(string message) => Debug.Log(message);
        public void Log(string message) => Debug.Log(message);
        public void Info(string message) => Debug.Log(message);
        public void Warn(string message) => Debug.LogWarning(message);
        public void Error(string message) => Debug.LogError(message);
        public void Error(Exception exception) => Debug.LogException(exception);
        public void Fail(string message) => Debug.LogError(message);
        public void Fail(Exception exception) => Debug.LogException(exception);
    }
}
#endif