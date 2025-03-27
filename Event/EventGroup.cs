using Eevee.Diagnosis;
using System;
using System.Collections.Generic;

namespace Eevee.Event
{
    /// <summary>
    /// 事件组，可以包含多个事件
    /// </summary>
    public sealed class EventGroup
    {
        #region Type
        private readonly struct Wrapper
        {
            internal readonly EventModule Module;
            internal readonly int EventId;
            internal readonly Delegate Listener;

            internal Wrapper(EventModule module, int eventId, Delegate listener)
            {
                Module = module;
                EventId = eventId;
                Listener = listener;
            }
        }
        #endregion

        // todo Eevee _listeners 未接入 EPool
        // todo Eevee _listeners 未接入 FixedDictionary
        private readonly Dictionary<ulong, Wrapper> _listeners = new(32);

        #region Add
        /// <summary>
        /// 添加监听<br/>
        /// 可能无法收到延迟派发的事件<br/>
        /// 如果需要接收延迟事件，可以将“TContext”修改为“IEEventContext”
        /// </summary>
        public void AddListener<TContext>(EventModule module, int eventId, Action<TContext> listener) where TContext : IEventContext
        {
            Register(module, eventId, listener);
        }
        /// <summary>
        /// 添加监听<br/>
        /// 可以收到延迟派发的事件<br/>
        /// “context”不建议是“struct”，因为会触发“Box”
        /// </summary>
        public void AddListener(EventModule module, int eventId, Action<IEventContext> listener)
        {
            Register(module, eventId, listener);
        }
        /// <summary>
        /// 添加监听<br/>
        /// 可以收到延迟派发的事件
        /// </summary>
        public void AddListener(EventModule module, int eventId, Action listener)
        {
            Register(module, eventId, listener);
        }

        /// <summary>
        /// 添加监听，执行一次后移除事件<br/>
        /// 可能无法收到延迟派发的事件<br/>
        /// 如果需要接收延迟事件，可以将“TContext”修改为“IEEventContext”
        /// </summary>
        public void AddOnceListener<TContext>(EventModule module, int eventId, Action<TContext> listener) where TContext : IEventContext
        {
            Action<TContext> remove = null;
            remove = _ =>
            {
                RemoveListener(module, eventId, listener);
                RemoveListener(module, eventId, remove);
            };

            AddListener(module, eventId, listener);
            AddListener(module, eventId, remove);
        }
        /// <summary>
        /// 添加监听，执行一次后移除事件<br/>
        /// 可以收到延迟派发的事件<br/>
        /// “context”不建议是“struct”，因为会触发“Box”
        /// </summary>
        public void AddOnceListener(EventModule module, int eventId, Action<IEventContext> listener)
        {
            Action<IEventContext> remove = null;
            remove = _ =>
            {
                RemoveListener(module, eventId, listener);
                RemoveListener(module, eventId, remove);
            };

            AddListener(module, eventId, listener);
            AddListener(module, eventId, remove);
        }
        /// <summary>
        /// 添加监听，执行一次后移除事件<br/>
        /// 可以收到延迟派发的事件
        /// </summary>
        public void AddOnceListener(EventModule module, int eventId, Action listener)
        {
            Action remove = null;
            remove = () =>
            {
                RemoveListener(module, eventId, listener);
                RemoveListener(module, eventId, remove);
            };

            AddListener(module, eventId, listener);
            AddListener(module, eventId, remove);
        }
        #endregion

        #region Remove
        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener<TContext>(EventModule module, int eventId, Action<TContext> listener) where TContext : IEventContext
        {
            UnRegister(module, eventId, listener);
        }
        /// <summary>
        /// 移除监听<br/>
        /// “context”不建议是“struct”，因为会触发“Box”
        /// </summary>
        public void RemoveListener(EventModule module, int eventId, Action<IEventContext> listener)
        {
            UnRegister(module, eventId, listener);
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(EventModule module, int eventId, Action listener)
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
        #endregion

        #region Private
        private ulong GetKey(EventModule module, Delegate listener)
        {
            int hashCode1 = module.GetHashCode();
            int hashCode2 = listener.GetHashCode();
            ulong key = (ulong)(uint)hashCode1 << 32 | (uint)hashCode2; // (ulong)-1 = 18446744073709551615，(uint)-1 = 4294967295，结果不一致
            return key;
        }

        private void Register(EventModule module, int eventId, Delegate listener)
        {
            ulong key = GetKey(module, listener);
            _listeners.Add(key, new Wrapper(module, eventId, listener));
            module.Register(eventId, listener);
        }
        private void UnRegister(EventModule module, int eventId, Delegate listener)
        {
            ulong key = GetKey(module, listener);

            if (_listeners.Remove(key))
                module.UnRegister(eventId, listener);
            else
                LogRelay.Warn($"[Event] listener isn't exist, EventId:{eventId}");
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
        #endregion
    }
}