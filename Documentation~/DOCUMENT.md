# TaskQueue  

Thread 目录下 TaskQueue.cs 和 Loom.cs 两个文件

使用 LimitedConcurrencyLevelTaskScheduler 调度任务，控制任务并发数，以达到任务队列效果  

使用 Loom 提供的子线程访问主线程方法

### Property

- DefaultConcurrentCount  

  默认并发数 32  

- DefaultSerialQueueTag  

  默认全局串行队列 tag  

- DefaultConcurrentQueueTag  

  默认全局并行队列 tag

- DefaultSerailQueue  

  默认串行队列

- DefaultConcurrentQueue  

  默认并发队列，默认并发数32   

### Method

- TaskQueue(int concurrentCount)

  构造方法，依据需求传入并发数，默认为1即串行队列  

- TaskQueue CreateSerialQueue()  

  静态方法，快捷创建串行队列，即并发数1

- TaskQueue CreateConcurrentQueue()  

  静态方法，快捷创建并发队列，默认并发数32  

- TaskQueue CreateGlobalQueue(string tag, int concurrentCount)  

  静态方法，创建全局队列，tag 队列标记，concurrentCount 并发数，默认 1  

- TaskQueue CreateGlobalSerialQueue(string tag)  

  静态方法，创建全局串行队列，tag 队列标记  

- TaskQueue CreateGlobalConcurrentQueue(string tag)  

  静态方法，创建全局并发队列，tag 队列标记

- Task RunAsync(Action action, float delay)  

  异步延时执行，不阻塞当前调用线程，delay 秒  

- Task RunAsync(Action action)  

  异步执行，不阻塞当前调用线程  

- Task RunSync(Action action)  

  同步执行，阻塞当前调用线程  

- RunSyncOnMainThread(Action action)    

  静态方法，于当前线程同步执行主线程任务  

- RunAsyncOnMainThread(Action action, float delay)  

  静态延时方法，于当前线程异步执行主线程任务（主线程使用，任务会在另一时机调用）

- RunAsyncOnMainThread(Action action)  

  静态方法，于当前线程异步执行主线程任务（主线程使用，任务会在另一时机调用）  

  

## LimitedConcurrencyLevelTaskScheduler

TaskScheduler 为抽象类，想自定义任务调度需继承该类，并复写部分内部调度方法

LimitedConcurrencyLevelTaskScheduler为[微软官方文档](https://docs.microsoft.com/zh-cn/dotnet/api/system.threading.tasks.taskscheduler?redirectedfrom=MSDN&view=netframework-4.8) 提供的示例代码，用于调度任务，控制并发数    

```c#
//创建并发数32的调度器
LimitedConcurrencyLevelTaskScheduler scheduler = new LimitedConcurrencyLevelTaskScheduler(32);
//方法1
TaskFactory factory = new TaskFactory(scheduler);
factory.StartNew(()=>{
  //任务
});
Task task = new Task(()=>{
  //任务
});
task.Start(scheduler);
```



## Loom

网上流传比较广的子线程访问主线程解决方案，通过在场景中挂载mono对象，于该mono对象update方法执行任务队列

* maxThreads  

  静态变量，最大线程数

* Initialize()  

  静态初始化方法，自动调用，也可主动调用，在场景中创建一个不销毁的Loom对象  

* QueueOnMainThread(Action<object> taction, object tparam)  

  静态方法，向主线程执行队列中添加任务

* QueueOnMainThread(Action<object> taction, object tparam, float time)  

  静态方法，向主线程添加延迟执行的任务  

* Thread RunAsync(Action a)  

  静态方法，创建新线程异步执行任务 