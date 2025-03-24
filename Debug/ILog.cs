using System;

namespace Eevee.Debug
{
    public interface ILog
    {
        /// <summary>
        /// 生效范围：UNITY_EDITOR<br/>
        /// 默认实现：UnityEngine.Log.Log()
        /// </summary>
        void Trace(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG<br/>
        /// 默认实现：UnityEngine.Log()
        /// </summary>
        void Log(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Log.Log()
        /// </summary>
        void Info(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG<br/>
        /// 默认实现：UnityEngine.Log.LogWarning()
        /// </summary>
        void Warn(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Log.LogError()
        /// </summary>
        void Error(string message);

        /// <summary>
        /// 不影响游戏流程的报错<br/>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Log.LogError()
        /// </summary>
        void Error(Exception exception);

        /// <summary>
        /// 影响游戏流程的报错<br/>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Log.LogException()
        /// </summary>
        void Fail(string message);

        /// <summary>
        /// 影响游戏流程的异常<br/>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Log.LogException()
        /// </summary>
        void Fail(Exception exception);
    }
}