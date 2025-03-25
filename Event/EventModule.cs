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
        // todo Eevee _listeners，_waitWrappers，_invokeWrappers 未接入 EPool
        // todo Eevee _listeners 未接入 FixedDictionary
        private readonly Dictionary<int, List<Delegate>> _listeners = new(128); // 使用List而不是使用Set，因为listener需要有序性
        private readonly List<Wrapper> _waitWrappers = new(32); // 等待执行的事件
        private readonly List<Wrapper> _invokeWrappers = new(32); // 执行中的事件
        #endregion

        #region 生命周期，需要外部调用
        /// <summary>
        /// 处理“this.Enqueue()”产生的队列
        /// </summary>
        public void Update()
        {
            if (_waitWrappers.Count == 0)
                return;

            _invokeWrappers.Update0GC(_waitWrappers); // 中转一层 _invokeWrappers，可以防止 Dispatch 的过程中，_waitWrappers 被添加
            _waitWrappers.Clear();

            foreach (var wrapper in _invokeWrappers)
                Invokes(wrapper.EventId, wrapper.Context, true);

            _invokeWrappers.Clear(); // Wrapper.Context 是引用类型，不Clear会导致 _invokeWrappers 一直持有 Context
        }
        /// <summary>
        /// 生命周期，需要外部调用
        /// </summary>
        public void Disable()
        {
            if (!_listeners.IsEmpty())
                LogRelay.Warn($"[Event] Listeners.Count is {_listeners.Count}, need clear!");

            foreach (var pair in _listeners)
                pair.Value.Clear();

            _listeners.Clear();
            _waitWrappers.Clear();
            _invokeWrappers.Clear();
        }
        #endregion

        #region 处理监听
        /// <summary>
        /// 添加监听
        /// </summary>
        internal void Register(int eventId, Delegate listener)
        {
            if (!_listeners.TryGetValue(eventId, out var listeners))
                _listeners.Add(eventId, listeners = new List<Delegate>());

            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }
        /// <summary>
        /// 移除监听
        /// </summary>
        internal void UnRegister(int eventId, Delegate listener)
        {
            if (_listeners.TryGetValue(eventId, out var listeners))
                listeners.Remove(listener);
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

            // todo Eevee 需要拷贝listeners
            foreach (var listener in listeners)
            {
                if (Macro.HasTryCatch)
                {
                    try
                    {
                        bool success = Invoke(listener, context);
                        if (!success)
                            LogRelay.Error($"[Event] EventId:{eventId}, context isn't {typeof(TContext).FullName}");
                        else if (recycle)
                            (context as IRecyclable)?.Recycle();
                    }
                    catch (Exception exception)
                    {
                        LogRelay.Fail($"[Event] Invokes fail, EventId:{eventId}\n{exception}");
                    }
                }
                else
                {
                    bool success = Invoke(listener, context);
                    Assert.IsTrue(success, "EventId:{0}, context isn't {1}", eventId, typeof(TContext), string.Empty);
                }
            }
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