using UnityEngine;
using UnityEngine.UI;
using ybwork.Async;

public class MyTaskTest : MonoBehaviour
{
    [SerializeField] Image _image;

    private void Start()
    {
        Test();
    }

    private async void Test()
    {
        Log("0");
        await YueTask.CompletedTask;
        Log("1");
        await YueTask.Delay(1);
        Log("2");
        YueTask<int> yueTask = Test1();
        yueTask.Cancel();
        int v1 = await yueTask;
        Log(v1);
        bool v2 = await Test2();
        Log(v2);
    }

    private async YueTask<int> Test1()
    {
        string url = "https://img-home.csdnimg.cn/images/20201124032511.png";
        Sprite sprite = await SpriteLoader.LoadSpriteFromUrl(url);
        _image.sprite = sprite;
        _image.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
        return 2;
    }

    private YueTask<bool> Test2()
    {
        YueTask<bool> task = new YueTask<bool>();
        task.SetValue(true);
        return task;
    }

    private void Log(object obj)
    {
        Debug.Log(Time.frameCount + ":" + obj);
    }
}
