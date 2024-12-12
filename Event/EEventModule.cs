using Eevee.Log;
using System;
using System.Collections.Generic;

namespace Eevee.Event
{
    /// <summary>
    /// 事件模块，事件的生效域，不同的域之间的事件不会相互影响
    /// </summary>
    public sealed class EEventModule
    {
        private readonly struct Wrapper
        {
            internal readonly int EventId;
            internal readonly IEEventContext Context;

            internal Wrapper(int eventId, IEEventContext context)
            {
                EventId = eventId;
                Context = context;
            }
            internal bool IsEqual(in Wrapper other)
            {
                return EventId == other.EventId && Context == other.Context;
            }
        }

        private readonly Dictionary<int, List<Delegate>> _listeners = new(128); // 使用List而不是使用Set，因为需要保证listener的有序性
        private readonly List<Wrapper> _waitWrappers = new(32); // 等待执行的事件
        private readonly List<Wrapper> _invokeWrappers = new(32); // 执行中的事件

        /// <summary>
        /// 生命周期，需要外部调用<br/>
        /// 处理 Enqueue() 产生的队列
        /// </summary>
        public void Update()
        {
            if (_waitWrappers.Count == 0)
                return;

            _invokeWrappers.Clear();
            _invokeWrappers.AddRange(_waitWrappers); // 中转一层 _invokeWrappers，可以防止 Dispatch 的过程中，_waitWrappers 被添加
            _waitWrappers.Clear();

            foreach (var wrapper in _invokeWrappers)
                Invoke(wrapper.EventId, wrapper.Context);

            _invokeWrappers.Clear(); // Wrapper.Context 是引用类型，不Clear会导致 _invokeWrappers 一直持有 Context
        }
        /// <summary>
        /// 生命周期，需要外部调用
        /// </summary>
        public void Disable()
        {
            foreach (var pair in _listeners)
                pair.Value.Clear();

            _listeners.Clear();
            _waitWrappers.Clear();
            _invokeWrappers.Clear();
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        internal void Register(int eventId, Delegate listener)
        {
            if (!_listeners.TryGetValue(eventId, out var contexts))
                _listeners.Add(eventId, contexts = new List<Delegate>());

            if (!contexts.Contains(listener))
                contexts.Add(listener);
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        internal void UnRegister(int eventId, Delegate listener)
        {
            if (_listeners.TryGetValue(eventId, out var listeners))
                listeners.Remove(listener);
        }

        /// <summary>
        /// 实时派发事件
        /// </summary>
        public void Dispatch<TContext>(int eventId, TContext context = default) where TContext : IEEventContext
        {
            Invoke(eventId, context);
        }
        /// <summary>
        /// 实时派发事件
        /// </summary>
        public void Dispatch(int eventId)
        {
            Invoke<IEEventContext>(eventId, null);
        }

        /// <summary>
        /// 延迟派发事件<br/>
        /// context 不建议是 struct，因为会触发 InBox
        /// </summary>
        public void Enqueue(int eventId, IEEventContext context = null, bool allowRepeat = true)
        {
            var wrapper = new Wrapper(eventId, context);
            if (allowRepeat || !ExistWait(wrapper))
                _waitWrappers.Add(wrapper);
            else
                ELog.Warn($"[Event] EventId:{eventId} is repeat");
        }

        private void Invoke<TContext>(int eventId, TContext context) where TContext : IEEventContext
        {
            if (_listeners.TryGetValue(eventId, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    switch (listener)
                    {
                        case Action<TContext> action1: action1(context); break;
                        case Action<IEEventContext> action2: action2(context); break;
                        case Action action: action(); break;
                        default: ELog.Error($"[Event] EventId:{eventId}, context isn't {typeof(TContext).FullName}"); break;
                    }
                }
            }
        }
        private bool ExistWait(Wrapper wrapper)
        {
            foreach (var other in _waitWrappers)
                if (wrapper.IsEqual(in other))
                    return true;
            return false;
        }
    }
}