using Eevee.Diagnosis;
using Eevee.Pool;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Eevee.Utils
{
    /// <summary>
    /// 线程工具
    /// </summary>
    public readonly struct ThreadUtils
    {
        #region 类型
        public sealed class ReuseTaskHandle<T> : IObjectRelease, IObjectDestroy
        {
            #region 字段
            // 参考链接：https://www.cnblogs.com/code1992/p/14359004.html
            private readonly AutoResetEvent _waitEvent = new(false);
            private readonly AutoResetEvent _freeEvent = new(false);
            private Task _task;
            private volatile bool _active = true;
            private volatile Action<T, int> _action;
            private volatile IReadOnlyList<T> _states;
            private volatile int _start = -1;
            private volatile int _end = -1;
            #endregion

            #region 方法
            internal void Start(Action<T, int> action, IReadOnlyList<T> states, int start, int end) // 主线程执行
            {
                _task ??= Task.Factory.StartNew(Run);
                _action = action;
                _states = states;
                _start = start;
                _end = end;
                _freeEvent.Set();
            }
            internal void Wait(int timeout) // 主线程执行
            {
                if (!_waitEvent.WaitOne(timeout))
                    LogRelay.Error($"[Task] timeout:{timeout}, task exist");
            }

            private void Run() // 子线程执行
            {
                while (_active)
                {
                    _freeEvent.WaitOne();

                    try
                    {
                        Invoke();
                    }
                    catch (Exception exception)
                    {
                        LogRelay.Error(exception.ToString());
                    }
                    finally
                    {
                        Dispose(false);
                        _waitEvent.Set();
                    }
                }
            }
            private void Invoke() // 子线程执行
            {
                var action = _action;
                var states = _states;
                for (int end = _end, i = _start; i < end; ++i)
                    action(states[i], i);
            }
            private void Dispose(bool destroy) // 可能主线程执行，也可能子线程执行
            {
                if (destroy)
                {
                    //_task.Dispose(); // Task不需要Dispose
                    _waitEvent.Close();
                    _freeEvent.Close();
                    _task = null;
                    _active = false;
                }

                _action = null;
                _states = null;
                _start = -1;
                _end = -1;
            }
            #endregion

            #region 继承
            public void OnRelease() => Dispose(false);
            public void OnDestroy() => Dispose(true);
            #endregion
        }

        public sealed class TaskHandle<T> : IObjectRelease, IObjectDestroy
        {
            #region 字段
            private Task _task;
            private Action _run; // 不要置空，减少GC
            private volatile Action<T, int> _action;
            private volatile IReadOnlyList<T> _states;
            private volatile int _start = -1;
            private volatile int _end = -1;
            #endregion

            #region 方法
            internal void Start(Action<T, int> action, IReadOnlyList<T> states, int start, int end) // 主线程执行
            {
                if (_task != null)
                    throw new Exception("[Task] can't alloc");

                _run ??= Run;
                _action = action;
                _states = states;
                _start = start;
                _end = end;
                _task = Task.Run(_run); // 最后赋值
            }
            internal void Wait(int timeout) // 主线程执行
            {
                var task = _task; // 多线程下，先缓存_task
                if (task == null)
                    return;

                if (!task.Wait(timeout))
                    LogRelay.Error($"[Task] timeout:{timeout}, task exist");

                if (_task != null)
                    LogRelay.Warn("[Task] task isn't null");
            }

            private void Run() // 子线程执行
            {
                try
                {
                    Invoke();
                }
                catch (Exception exception)
                {
                    LogRelay.Error(exception.ToString());
                }
                finally
                {
                    Dispose(false);
                }
            }
            private void Invoke() // 子线程执行
            {
                var action = _action;
                var states = _states;
                for (int end = _end, i = _start; i < end; ++i)
                    action(states[i], i);
            }
            private void Dispose(bool destroy) // 可能主线程执行，也可能子线程执行
            {
                _task = null;
                _action = null;
                _states = null;
                _start = -1;
                _end = -1;
                if (destroy)
                    _run = null;
            }
            #endregion

            #region 继承
            public void OnRelease() => Dispose(false);
            public void OnDestroy() => Dispose(true);
            #endregion
        }

        public sealed class ParallelHandle<T> : IObjectDestroy
        {
            #region 委托
            internal Action<T, int> Action;

            private Action<T, ParallelLoopState, long> _taskAction;
            internal Action<T, ParallelLoopState, long> TaskAction => _taskAction ??= (arg, _, index) => Action(arg, (int)index);
            #endregion

            #region 继承
            public void OnRelease() => Action = null;
            public void OnDestroy() => _taskAction = null;
            #endregion
        }
        #endregion

        /// <summary>
        /// 计算每个“Thread”执行多少“State”
        /// </summary>
        /// <param name="totalStateCount">State的总个数</param>
        /// <param name="leastStateCount">至少X个State，启用1个线程</param>
        /// <param name="mostThreadCount">最大线程数</param>
        /// <param name="stateCount">每个线程计算多少State</param>
        /// <param name="threadCount">最终的线程数</param>
        private static void Count(int totalStateCount, int leastStateCount, int mostThreadCount, out int stateCount, out int threadCount)
        {
            float needThreadScale = totalStateCount / (float)leastStateCount;
            int needThreadNum = (int)MathF.Ceiling(needThreadScale);
            int allocThreadNum = Math.Clamp(needThreadNum, 1, mostThreadCount);
            int allocCount = totalStateCount / allocThreadNum;

            stateCount = allocCount * allocThreadNum < totalStateCount ? allocCount + 1 : allocCount;
            threadCount = allocThreadNum;
        }

        /// minStateCount，默认值：20
        /// maxThreadCount，默认值：4
        /// timeout，默认值：100
        public static void Start<T>(Action<T, int> action, IReadOnlyList<T> states, int leastStateCount, int mostThreadCount, int timeout, bool enable, CollectionPool<List<ReuseTaskHandle<T>>> handlesPool, ObjectInterPool<ReuseTaskHandle<T>> handlePool) where T : class
        {
            int stateCount = states.Count;
            if (stateCount == 0)
                return;

            Count(stateCount, leastStateCount, mostThreadCount, out int countCount, out int threadCount);
            if (enable && threadCount > 1) // 分配了一个线程，最后放在主线程执行，不开启子线程
            {
                var handles = handlesPool.Alloc();
                int end0 = 0;

                for (int end = 0, ti = 0; ti < threadCount; ++ti)
                {
                    int start = end;
                    end = Math.Min(start + countCount, stateCount);

                    if (ti == 0)
                    {
                        end0 = end;
                    }
                    else
                    {
                        var handle = handlePool.Alloc();
                        handle.Start(action, states, start, end);
                        handles.Add(handle);
                    }
                }

                for (int i = 0; i < end0; ++i)
                    action(states[i], i);

                foreach (var handle in handles)
                    handle.Wait(timeout);

                foreach (var handle in handles)
                    handlePool.Release(handle);

                handlesPool.Release(handles);
            }
            else
            {
                for (int count = states.Count, i = 0; i < count; ++i) // “IReadOnlyList”迭代器存在GC
                    action(states[i], i);
            }
        }
        public static void Start<T>(Action<T, int> action, IReadOnlyList<T> states, int leastStateCount, int mostThreadCount, int timeout, bool enable, CollectionPool<List<TaskHandle<T>>> handlesPool, ObjectInterPool<TaskHandle<T>> handlePool) where T : class
        {
            int stateCount = states.Count;
            if (stateCount == 0)
                return;

            Count(stateCount, leastStateCount, mostThreadCount, out int countCount, out int threadCount);
            if (enable && threadCount > 1) // 分配了一个线程，最后放在主线程执行，不开启子线程
            {
                var handles = handlesPool.Alloc();
                int end0 = 0;

                for (int end = 0, ti = 0; ti < threadCount; ++ti)
                {
                    int start = end;
                    end = Math.Min(start + countCount, stateCount);

                    if (ti == 0)
                    {
                        end0 = end;
                    }
                    else
                    {
                        var handle = handlePool.Alloc();
                        handle.Start(action, states, start, end);
                        handles.Add(handle);
                    }
                }

                for (int i = 0; i < end0; ++i)
                    action(states[i], i);

                foreach (var handle in handles)
                    handle.Wait(timeout);

                foreach (var handle in handles)
                    handlePool.Release(handle);

                handlesPool.Release(handles);
            }
            else
            {
                for (int count = states.Count, i = 0; i < count; ++i) // “IReadOnlyList”迭代器存在GC
                    action(states[i], i);
            }
        }
        public static void ForEach<T>(Action<T, int> action, IEnumerable<T> states, ObjectInterPool<ParallelHandle<T>> handlePool)
        {
            var handle = handlePool.Alloc();
            handle.Action = action;
            Parallel.ForEach(states, handle.TaskAction);
            handlePool.Release(handle);
        }
    }
}