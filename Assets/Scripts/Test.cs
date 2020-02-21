using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour
{
    private Stopwatch sw;
    // Start is called before the first frame update
    void Start()
    { 
        sw = Stopwatch.StartNew();
        Debug.Log(sw.Elapsed + " Start");
        
        // Invoke(nameof(TestFunc),5);
        // InvokeRepeating(nameof(TestFunc),5,1);
        // Timer tm = new Timer();
        // System.Timers.Timer timer = new System.Timers.Timer(2000);
        // timer.Elapsed += (sender, args) => TestFunc();  
        // timer.AutoReset = true;
        // timer.Enabled = true;


        // AsyncTask();

        var t = Task.Run(delegate
        {
            Debug.Log(sw.Elapsed + " Task Start " + Thread.CurrentThread.ManagedThreadId );
            Thread.Sleep(3000);
            Debug.Log(sw.Elapsed + " Task End " + Thread.CurrentThread.ManagedThreadId );
        });
        
        t.ContinueWith(delegate(Task task)
        {
            
        });



        t.GetAwaiter().OnCompleted(() => { Debug.Log("Complete"); });

        // t.GetAwaiter().GetResult();
        
        
        // var t = Task.Run(async delegate
        // {
        //     Debug.Log(sw.Elapsed + " TaskResult Start " + Thread.CurrentThread.ManagedThreadId );
        //     await Task.Delay(3000);
        //     Debug.Log(sw.Elapsed + " TaskResult End " + Thread.CurrentThread.ManagedThreadId );
        //     return 99;
        // });
        //
        // t.Wait();
        // Debug.Log(sw.Elapsed + " Wait "  + Thread.CurrentThread.ManagedThreadId + " "+ t.Result);
        
        
        
        Debug.Log(sw.Elapsed + " End");
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
}
