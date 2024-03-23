using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ybwork.Async;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        float t = Time.time;
        var task1 = YueTask.Delay(2);
        var task2 = YueTask.Delay(4);
        var task3 = YueTask.Delay(6);
        var task4 = YueTask.Delay(8);
        var task5 = YueTask.Delay(10);
        await YueTask.WaitAll(task1, task2, task3, task4, task5);
        Debug.Log(Time.time - t);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
