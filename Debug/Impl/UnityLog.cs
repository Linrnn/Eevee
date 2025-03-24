#if UNITY_STANDALONE
using System;
using UnityEngine;

namespace Eevee.Debug
{
    /// <summary>
    /// 使用 UnityEngine.Debug 实现 ILog
    /// </summary>
    public sealed class UnityLog : ILog
    {
        public void Trace(string message) => UnityEngine.Debug.Log(message);
        public void Log(string message) => UnityEngine.Debug.Log(message);
        public void Info(string message) => UnityEngine.Debug.Log(message);
        public void Warn(string message) => UnityEngine.Debug.LogWarning(message);
        public void Error(string message) => UnityEngine.Debug.LogError(message);
        public void Error(Exception exception) => UnityEngine.Debug.LogException(exception);
        public void Fail(string message) => UnityEngine.Debug.LogError(message);
        public void Fail(Exception exception) => UnityEngine.Debug.LogException(exception);
    }
}
#endif