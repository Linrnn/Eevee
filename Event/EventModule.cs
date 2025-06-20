using Eevee.Collection;
using Eevee.Define;
using Eevee.Diagnosis;
using Eevee.Pool;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Event
{
    /// <summary>
    /// 事件模块，事件的生效域，不同的域之间的事件不会相互影响
    /// </summary>
    public sealed class EventModule
    {
        #region 类型
        private readonly struct Wrapper : IEquatable<Wrapper>
        {
            internal readonly int EventId;
            internal readonly IEventContext Context;

            internal Wrapper(int eventId, IEventContext context)
            {
                EventId = eventId;
                Context = context;
            }

            public static bool operator ==(in Wrapper lhs, in Wrapper rhs) => lhs.EventId == rhs.EventId && lhs.Context == rhs.Context;
            public static bool operator !=(in Wrapper lhs, in Wrapper rhs) => lhs.EventId != rhs.EventId || lhs.Context != rhs.Context;

            public bool Equals(Wrapper other) => this == other;
            public override bool Equals(object obj) => obj is Wrapper other && this == other;
            public override int GetHashCode() => EventId ^ Context.GetHashCode();
        }
        #endregion

        #region 字段/构造函数
        private readonly Dictionary<int, RefArray<Delegate>> _listeners = new(128); // 使用List而不是使用Set，因为listener需要有序性
        private readonly List<Wrapper> _waitWrappers = new(32); // 等待执行的事件
        private ArrayPool<Delegate> _arrayPool;
        private int _validStart; // “_waitWrappers”从“_validStart”开始是有效的

        public EventModule(ArrayPool<Delegate> arrayPool)
        {
            Assert.NotNull<ArgumentNullException, AssertArgs>(arrayPool, nameof(arrayPool), "is null!");

            _arrayPool = arrayPool;
        }
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

            _validStart = waitWrapperCount; // “Dispatch”过程中可能会修改“_waitWrappers”，先记录有效的启示索引
            for (int i = 0; i < waitWrapperCount; ++i)
                if (_waitWrappers[i] is { } invokeWrapper)
                    Invokes(invokeWrapper.EventId, invokeWrapper.Context, true);
            _validStart = 0;
        }
        /// <summary>
        /// 生命周期，需要外部调用
        /// </summary>
        public void Disable()
        {
            if (_listeners.Count > 0)
                LogRelay.Warn($"[Event] Listeners.Count is {_listeners.Count}, need clear!");

            foreach (var pair in _listeners)
                RefArray.Return(pair.Value, _arrayPool);

            _listeners.Clear();
            _waitWrappers.Clear();
            _arrayPool = null;
            _validStart = 0;
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

            RefArray.Add(ref listeners, listener, _arrayPool);
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

            RefArray.Return(ref listeners, _arrayPool);
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
            if (allowRepeat || _waitWrappers.IndexOf(wrapper) < _validStart)
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

            int count = listeners.Count;
            int size = Unsafe.SizeOf<Delegate>();
            int index = 0;
            var newListeners = new StackAllocSpan<Delegate>(size, stackalloc byte[count * size]);
            for (int i = 0; i < count; ++i)
                newListeners.Set(ref index, listeners.Items[i]); // “Invoke”可能会修改listeners，先提前拷贝

            for (int i = 0; i < index;)
            {
                var listener = newListeners.Get(ref i);
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