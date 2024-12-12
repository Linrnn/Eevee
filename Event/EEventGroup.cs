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
        private bool _active;
        private EEventModule _module;
        private readonly Dictionary<Delegate, int> _listeners = new(32);

        /// <summary>
        /// 生命周期，需要外部调用
        /// </summary>
        public void Enable(EEventModule module)
        {
            if (module == null)
            {
                ELog.Error("[Event] module is null");
            }
            else if (_module != null)
            {
                ELog.Error("[Event] _module is exist");
            }
            else
            {
                _module = module;
                _active = true;
            }
        }
        /// <summary>
        /// 生命周期，需要外部调用
        /// </summary>
        public void Disable()
        {
            if (!CheckActive())
                return;

            UnAllRegister();
            _module = null;
            _active = false;
        }

        /// <summary>
        /// 添加监听<br/>
        /// 可能无法收到延迟派发的事件<br/>
        /// 如果需要接收延迟事件，可以将TContext修改为IEEventContext
        /// </summary>
        public void AddListener<TContext>(int eventId, Action<TContext> listener) where TContext : IEEventContext
        {
            Register(eventId, listener);
        }
        /// <summary>
        /// 添加监听<br/>
        /// 可以收到延迟派发的事件<br/>
        /// context 不建议是 struct，因为会触发 InBox
        /// </summary>
        public void AddListener(int eventId, Action<IEEventContext> listener)
        {
            Register(eventId, listener);
        }
        /// <summary>
        /// 添加监听<br/>
        /// 可以收到延迟派发的事件
        /// </summary>
        public void AddListener(int eventId, Action listener)
        {
            Register(eventId, listener);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener<TContext>(int eventId, Action<TContext> listener) where TContext : IEEventContext
        {
            UnRegister(eventId, listener);
        }
        /// <summary>
        /// 移除监听<br/>
        /// context 不建议是 struct，因为会触发 InBox
        /// </summary>
        public void RemoveListener(int eventId, Action<IEEventContext> listener)
        {
            UnRegister(eventId, listener);
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        public void RemoveListener(int eventId, Action listener)
        {
            UnRegister(eventId, listener);
        }

        /// <summary>
        /// 清除监听
        /// </summary>
        public void RemoveAllListener()
        {
            if (!CheckActive())
                return;

            UnAllRegister();
        }

        private void Register(int eventId, Delegate listener)
        {
            if (!CheckActive())
                return;

            if (_listeners.TryAdd(listener, eventId))
                _module.Register(eventId, listener);
            else
                ELog.Warn($"[Event] listener is exist, EventId:{eventId}");
        }
        private void UnRegister(int eventId, Delegate listener)
        {
            if (!CheckActive())
                return;

            if (_listeners.Remove(listener))
                _module.UnRegister(eventId, listener);
            else
                ELog.Warn($"[Event] listener isn't exist, EventId:{eventId}");
        }
        private void UnAllRegister()
        {
            foreach (var pair in _listeners)
                _module.UnRegister(pair.Value, pair.Key);
            _listeners.Clear();
        }

        private bool CheckActive()
        {
            if (_active)
                return true;

            ELog.Error("[Event] _active is false, please call \"EEventGroup.Enable()\"");
            return false;
        }
    }
}