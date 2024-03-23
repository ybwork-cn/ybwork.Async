using UnityEngine;
using UnityEngine.UI;
using ybwork.Async;

public class MyTaskTest : MonoBehaviour
{
    [SerializeField] Image Image;

    private void Start()
    {
        Test();
    }

    private async void Test()
    {
        YueTask task1 = YueTask.Delay(3);
        task1.Then(() =>
        {
            Log("Task1");
        });

        YueTask<int> task2 = Test1();
        task2.Then((value) =>
        {
            Log("Task2:" + value);
        });

        await YueTask.WaitAll(task1, task2);
        Log("Test");
        task1.Then(() =>
        {
            Log("Task1--");
        });
        task2.Then((value) =>
        {
            Log("Task2--:" + value);
        });
    }

    private async YueTask<int> Test1()
    {
        string url = "https://img-home.csdnimg.cn/images/20201124032511.png";
        Sprite sprite = await SpriteLoader.LoadSpriteFromUrl(url);
        Image.sprite = sprite;
        Image.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
        return 2;
    }

    private void Log(object obj)
    {
        Debug.Log(Time.frameCount + ":" + obj);
    }
}
