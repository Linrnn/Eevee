using Eevee.Collection;
using Eevee.Define;
using Eevee.Diagnosis;
using Eevee.Pool;
using System;
using System.Collections.Generic;

namespace Eevee.Event
{
    /// <summary>
    /// 事件模块，事件的生效域，不同的域之间的事件不会相互影响
    /// </summary>
    public sealed class EventModule
    {
        #region 类型
        private readonly struct Wrapper
        {
            internal readonly int EventId;
            internal readonly IEventContext Context;

            internal Wrapper(int eventId, IEventContext context)
            {
                EventId = eventId;
                Context = context;
            }
            internal bool BeContain(List<Wrapper> wrappers)
            {
                foreach (var other in wrappers)
                    if (IsEqual(in other))
                        return true;

                return false;
            }
            private bool IsEqual(in Wrapper other)
            {
                return EventId == other.EventId && Context == other.Context;
            }
        }
        #endregion

        #region 字段
        private readonly Dictionary<int, RefArray<Delegate>> _listeners = new(128); // 使用List而不是使用Set，因为listener需要有序性
        private readonly List<Wrapper> _waitWrappers = new(32); // 等待执行的事件
        #endregion

        #region 生命周期，需要外部调用
        /// <summary>
        /// 处理“this.Enqueue()”产生的队列
        /// </summary>
        public void Update()
        {
            int waitWrapperCount = _waitWrappers.Count;
            if (waitWrapperCount == 0)
                return;

            var invokeWrappers = ArrayExt.SharedRent<Wrapper>(waitWrapperCount);
            _waitWrappers.CopyTo(invokeWrappers); // 中转一层“invokeWrappers”，可以防止“Dispatch”的过程中，修改“_waitWrappers”
            _waitWrappers.Clear(); // “Dispatch”前移除，“Dispatch”过程中可能会修改“_waitWrappers”

            for (int i = 0; i < waitWrapperCount; ++i)
                Invokes(invokeWrappers[i].EventId, invokeWrappers[i].Context, true);
            invokeWrappers.SharedReturn();
        }
        /// <summary>
        /// 生命周期，需要外部调用
        /// </summary>
        public void Disable()
        {
            if (_listeners.Count > 0)
                LogRelay.Warn($"[Event] Listeners.Count is {_listeners.Count}, need clear!");

            foreach (var pair in _listeners)
                RefArray.Return(pair.Value);

            _listeners.Clear();
            _waitWrappers.Clear();
        }
        #endregion

        #region 处理监听
        /// <summary>
        /// 添加监听
        /// </summary>
        internal void Register(int eventId, Delegate listener)
        {
            if (!_listeners.TryGetValue(eventId, out var listeners))
                _listeners.Add(eventId, listeners = new RefArray<Delegate>(null));

            if (listeners.Contains(listener))
                return;

            RefArray.Add(ref listeners, listener);
            _listeners[eventId] = listeners;
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        internal void UnRegister(int eventId, Delegate listener)
        {
            if (!_listeners.TryGetValue(eventId, out var listeners))
            {
                return;
            }

            RefArray.Remove(ref listeners, listener);
            if (listeners.Count > 0)
            {
                _listeners[eventId] = listeners;
                return;
            }

            RefArray.Return(ref listeners);
            _listeners.Remove(eventId);
        }
        #endregion

        #region 派发/入队列
        /// <summary>
        /// 实时派发事件
        /// </summary>
        public void Dispatch<TContext>(int eventId, TContext context, bool recycle = false) where TContext : IEventContext
        {
            Invokes(eventId, context, recycle);
        }
        /// <summary>
        /// 实时派发事件
        /// </summary>
        public void Dispatch(int eventId)
        {
            Invokes<IEventContext>(eventId, null, false);
        }

        /// <summary>
        /// 延迟派发事件<br/>
        /// “context”不建议是“struct”，因为会触发“Box”
        /// </summary>
        public void Enqueue(int eventId, IEventContext context = null, bool allowRepeat = true)
        {
            var wrapper = new Wrapper(eventId, context);
            if (allowRepeat || !wrapper.BeContain(_waitWrappers))
                _waitWrappers.Add(wrapper);
            else
                LogRelay.Warn($"[Event] EventId:{eventId} is repeat");
        }
        #endregion

        #region 执行
        private void Invokes<TContext>(int eventId, TContext context, bool recycle) where TContext : IEventContext
        {
            if (!_listeners.TryGetValue(eventId, out var listeners))
                return;

            var newListeners = ArrayExt.SharedRent<Delegate>(listeners.Count);
            Array.Copy(listeners.Items, 0, newListeners, 0, listeners.Count);
            var span = newListeners.AsReadOnlySpan(0, listeners.Count);

            foreach (var listener in span)
            {
                if (Macro.HasTryCatch)
                {
                    try
                    {
                        bool success = Invoke(listener, context);
                        if (!success)
                            LogRelay.Error($"[Event] EventId:{eventId}, context isn't {typeof(TContext).FullName}");
                    }
                    catch (Exception exception)
                    {
                        LogRelay.Fail($"[Event] Invokes fail, EventId:{eventId}\n{exception}");
                    }
                }
                else
                {
                    bool success = Invoke(listener, context);
                    if (!success)
                        LogRelay.Error($"[Event] EventId:{eventId}, context isn't {typeof(TContext).FullName}");
                }
            }

            newListeners.SharedReturn();
            if (recycle)
                (context as IRecyclable)?.Recycle();
        }
        private bool Invoke<TContext>(Delegate listener, TContext context) where TContext : IEventContext
        {
            switch (listener)
            {
                case Action<TContext> action1:
                    action1(context);
                    return true;

                case Action<IEventContext> action2:
                    action2(context);
                    return true;

                case Action action3:
                    action3();
                    return true;

                default: return false;
            }
        }
        #endregion
    }
}