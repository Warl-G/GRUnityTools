using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GRTools
{
    public class TaskQueue
    {
        private const int DefaultConcurrentCount = 32;
        private static object _lock = new object();
        private static TaskQueue _defaultSerial;
        private static TaskQueue _defaultConcurrent;

        private LimitedConcurrencyLevelTaskScheduler _scheduler;

        /// <summary>
        /// 默认串行队列
        /// </summary>
        public static TaskQueue DefaultSerailQueue
        {
            get
            {
                if (_defaultSerial == null)
                {
                    lock (_lock)
                    {
                        if (_defaultSerial == null)
                        {
                            _defaultSerial = new TaskQueue(1);
                        }
                    }
                }

                return _defaultSerial;
            }
        }

        /// <summary>
        /// 默认并发队列
        /// </summary>
        public static TaskQueue DefaultConcurrentQueue
        {
            get
            {
                if (_defaultConcurrent == null)
                {
                    lock (_lock)
                    {
                        if (_defaultConcurrent == null)
                        {
                            _defaultConcurrent = new TaskQueue(DefaultConcurrentCount);
                        }
                    }
                }

                return _defaultConcurrent;
            }
        }

        /// <summary>
        /// 创建并发队列
        /// </summary>
        /// <param name="concurrentCount">根据并发需求，为1即串行队列</param>
        public TaskQueue(int concurrentCount)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(concurrentCount);
            Loom.Initialize();
        }

        /// <summary>
        /// 创建串行队列
        /// </summary>
        /// <returns></returns>
        public static TaskQueue CreateSerialQueue()
        {
            return new TaskQueue(1);
        }

        /// <summary>
        /// 创建并发队列，默认并发数32
        /// </summary>
        /// <returns></returns>
        public static TaskQueue CreateConcurrentQueue()
        {
            return new TaskQueue(DefaultConcurrentCount);
        }

        /// <summary>
        /// 延迟异步执行方法
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public Task RunAsync(Action action, float delay)
        {
            Task t = Task.Run(async () =>
            {
                await Task.Delay((int) (delay * 1000));
                return RunAsync(action);
            });
            return t;
        }

        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task RunAsync(Action action)
        {
            Task t = new Task(action);
            t.Start(_scheduler);
            return t;
        }

        /// <summary>
        /// 同步执行方法
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task RunSync(Action action)
        {
            Task t = new Task(action);
            t.RunSynchronously(_scheduler);
            return t;
        }

        public static void RunSyncOnMainThread(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == 1)
            {
                action();
            }
            else
            {
                Semaphore sem = new Semaphore(0, 1);
                Loom.QueueOnMainThread((o =>
                {
                    action();
                    sem.Release();
                }), null);
                sem.WaitOne();
            }
        }

        public static void RunAsyncOnMainThread(Action action)
        {
            Loom.QueueOnMainThread((o => { action(); }), null);
        }

        public static void RunAsyncOnMainThread(Action action, float delay)
        {
            Loom.QueueOnMainThread((o => { action(); }), null, delay);
        }
    }

    /// <summary>
    /// 微软官方提供 负责Task并发调度
    /// </summary>
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic] private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed 
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler. 
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items. 
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism. 
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler. 
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough 
            // delegates currently queued or running to process tasks, schedule another. 
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler. 
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally
                {
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }

        // Attempts to execute the specified task on the current thread. 
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task. 
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler. 
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler. 
        public sealed override int MaximumConcurrencyLevel
        {
            get { return _maxDegreeOfParallelism; }
        }

        // Gets an enumerable of the tasks currently scheduled on this scheduler. 
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}