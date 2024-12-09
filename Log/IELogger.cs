using System;

namespace Eevee.Log
{
    public interface IELogger
    {
        /// <summary>
        /// 生效范围：UNITY_EDITOR<br/>
        /// 默认实现：UnityEngine.Debug.Impl()
        /// </summary>
        /// <param name="message">输出内容</param>
        void Trace(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG<br/>
        /// 默认实现：UnityEngine.Debug.Impl()
        /// </summary>
        /// <param name="message">输出内容</param>
        void Debug(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Debug.Impl()
        /// </summary>
        /// <param name="message">输出内容</param>
        void Info(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG<br/>
        /// 默认实现：UnityEngine.Debug.LogWarning()
        /// </summary>
        /// <param name="message">输出内容</param>
        void Warn(string message);

        /// <summary>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Debug.LogError()
        /// </summary>
        /// <param name="message">输出内容</param>
        void Error(string message);

        /// <summary>
        /// 不影响游戏流程的报错<br/>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Debug.LogError()
        /// </summary>
        /// <param name="exception">异常</param>
        void Error(Exception exception);

        /// <summary>
        /// 影响游戏流程的报错<br/>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Debug.LogException()
        /// </summary>
        /// <param name="message">输出内容</param>
        void Fail(string message);

        /// <summary>
        /// 影响游戏流程的异常<br/>
        /// 生效范围：UNITY_EDITOR，DEBUG，RELEASE<br/>
        /// 默认实现：UnityEngine.Debug.LogException()
        /// </summary>
        /// <param name="exception">异常</param>
        void Fail(Exception exception);
    }
}