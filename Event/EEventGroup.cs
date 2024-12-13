using Eevee.Log;
using System;
using System.Collections.Generic;

namespace Eevee.Event
{
    /// <summary>
    /// 事件组，可以包含多个事件
    /// </summary>
    public sealed class EEventGroup
    {
        private readonly struct Wrapper
        {
            internal readonly EEventModule Module;
            internal readonly int EventId;
            internal readonly Delegate Listener;

            public Wrapper(EEventModule module, int eventId, Delegate listener)
            {
                Module = module;
                EventId = eventId;
                Listener = listener;
            }
        }

        private readonly Dictionary<ulong, Wrapper> _listeners = new(32);

        /// <summary>
        /// 添加监听<br/>
        /// 可能无法收到延迟派发的事件<br/>
        /// 如果需要接收延迟事件，可以将TContext修改为IEEventContext
        /// </summary>
        public void AddListener<TContext>(EEventModule module, int eventId, Action<TContext> listener) where TContext : IEEventContext
        {
            Register(module, eventId, listener);
        }
        /// <summary>
        /// 添加监听<br/>
        /// 可以收到延迟派发的事件<br/>
        /// context 不建议是 struct，因为会触发 Box
        /// </summary>
        public void AddListener(EEventModule module, int eventId, Action<IEEventContext> listener)
        {
            Register(module, eventId, listener);
        }
        /// <summary>
        /// 添加监听<br/>
        /// 可以收到延迟派发的事件
        /// </summary>
        public void AddListener(EEventModule module, int eventId, Action listener)
        {
            Register(module, eventId, listener);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener<TContext>(EEventModule module, int eventId, Action<TContext> listener) where TContext : IEEventContext
        {
            UnRegister(module, eventId, listener);
        }
        /// <summary>
        /// 移除监听<br/>
        /// context 不建议是 struct，因为会触发 Box
        /// </summary>
        public void RemoveListener(EEventModule module, int eventId, Action<IEEventContext> listener)
        {
            UnRegister(module, eventId, listener);
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(EEventModule module, int eventId, Action listener)
        {
            UnRegister(module, eventId, listener);
        }

        /// <summary>
        /// 清除监听
        /// </summary>
        public void RemoveAllListener()
        {
            UnAllRegister();
        }

        private ulong GetKey(EEventModule module, Delegate listener)
        {
            int hashCode1 = module.GetHashCode();
            int hashCode2 = listener.GetHashCode();
            ulong key = ((ulong)(uint)hashCode1 << 32) | (uint)hashCode2; // (ulong)-1 = 18446744073709551615，(uint)-1 = 4294967295，结果不一致
            return key;
        }

        private void Register(EEventModule module, int eventId, Delegate listener)
        {
            ulong key = GetKey(module, listener);

            if (_listeners.ContainsKey(key))
            {
                ELog.Error($"[Event] listener is exist, EventId:{eventId}");
            }
            else
            {
                _listeners.Add(key, new Wrapper(module, eventId, listener));
                module.Register(eventId, listener);
            }
        }
        private void UnRegister(EEventModule module, int eventId, Delegate listener)
        {
            ulong key = GetKey(module, listener);

            if (_listeners.Remove(key))
                module.UnRegister(eventId, listener);
            else
                ELog.Warn($"[Event] listener isn't exist, EventId:{eventId}");
        }

        private void UnAllRegister()
        {
            foreach (var pair in _listeners)
            {
                var wrapper = pair.Value;
                wrapper.Module.UnRegister(wrapper.EventId, wrapper.Listener);
            }

            _listeners.Clear();
        }
    }
}