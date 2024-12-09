#if UNITY_STANDALONE
using System;
using UDebug = UnityEngine.Debug;

namespace Eevee.Log
{
    internal sealed class EUnityLog : IELogger
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