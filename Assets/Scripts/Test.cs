using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GRTools;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour
{
    private Stopwatch sw;
    // Start is called before the first frame update
    void Start()
    { 
        sw = Stopwatch.StartNew();
        Debug.Log(sw.Elapsed + " Main Start On Thread: " + Thread.CurrentThread.ManagedThreadId );
        TestTaskQueue();
        Debug.Log(sw.Elapsed + " Main End On Thread: " + Thread.CurrentThread.ManagedThreadId );
    }

    async void AsyncTask()
    {
        Debug.Log(sw.Elapsed + " AsyncTask Start " + Thread.CurrentThread.ManagedThreadId );
        await Task.Delay(3000);


        Debug.Log(sw.Elapsed + " AsyncTask 1 Done " + Thread.CurrentThread.ManagedThreadId );
        await Task.Run(delegate
        {
            Debug.Log(sw.Elapsed + " AsyncTask 2 Start " + Thread.CurrentThread.ManagedThreadId );
            Thread.Sleep(3000);
            Debug.Log(sw.Elapsed + " AsyncTask 2 End " + Thread.CurrentThread.ManagedThreadId );
        });//
        Debug.Log(sw.Elapsed + " AsyncTask 2 Done " + Thread.CurrentThread.ManagedThreadId );
        await Task.Delay(3000);
        Debug.Log(sw.Elapsed + " AsyncTask End" + Thread.CurrentThread.ManagedThreadId);
        
    }

    void TestFunc()
    {
        Debug.Log(sw.Elapsed + " TaskFunc " + Thread.CurrentThread.ManagedThreadId);
    }

    void TestTaskQueue()
    {
        Debug.Log(sw.Elapsed + " Add Task On Thread: " + Thread.CurrentThread.ManagedThreadId );
        TaskQueue.DefaultConcurrentQueue.RunAsync(() =>
        {
            Debug.Log(sw.Elapsed + " Before Call RunSyncOnMainThread On Thread: " + Thread.CurrentThread.ManagedThreadId );
            TaskQueue.RunSyncOnMainThread(() =>
            {
                Debug.Log(sw.Elapsed + " RunSyncOnMainThread On Thread: " + Thread.CurrentThread.ManagedThreadId );
            });
            Debug.Log(sw.Elapsed + " After Call RunSyncOnMainThread On Thread: " + Thread.CurrentThread.ManagedThreadId );
        });
        // Debug.Log(sw.Elapsed + " Add Task 2 On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // TaskQueue.DefaultSerailQueue.RunAsync(() =>
        // {
        //     Debug.Log(sw.Elapsed + " Execute Task 2 After 6s  On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // },6);
        // Debug.Log(sw.Elapsed + " Add Task 3 On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // TaskQueue.DefaultSerailQueue.RunAsync(() =>
        // {
        //     Debug.Log(sw.Elapsed + " Execute Task 3 After 9s  On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // },9);
        // Debug.Log(sw.Elapsed + " Add Task 4 On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // TaskQueue.DefaultSerailQueue.RunAsync(() =>
        // {
        //     Debug.Log(sw.Elapsed + " Execute Task 4 After 12s  On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // },12);
        // Debug.Log(sw.Elapsed + " Add Task 5 On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // TaskQueue.DefaultSerailQueue.RunAsync(() =>
        // {
        //     Debug.Log(sw.Elapsed + " Execute Task 5 After 15s  On Thread: " + Thread.CurrentThread.ManagedThreadId );
        // },15);
    }
}
