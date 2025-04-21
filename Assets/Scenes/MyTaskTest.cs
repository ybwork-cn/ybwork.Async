using UnityEngine;
using UnityEngine.UI;
using ybwork.Async;

public class MyTaskTest : MonoBehaviour
{
    [SerializeField] Image _image;

    private void Start()
    {
        YueTask.Run(TestAsync);
    }

    private async YueTask TestAsync()
    {
        Log("0");
        await YueTask.CompletedTask;
        Log("1");
        YueTask delay = YueTask.Delay(1);
        delay.Cancel();
        delay.Then(() =>
        {
            Log("2");
        });
        Log("3");
        await delay;
        YueTask<int> yueTask = Test1Async();
        //yueTask.Cancel();
        int v1 = await yueTask;
        Log(v1);
        bool v2 = await Test2Async();
        Log(v2);
    }

    private async YueTask<int> Test1Async()
    {
        string url = "https://img-home.csdnimg.cn/images/20201124032511.png";
        Sprite sprite = await SpriteLoader.LoadSpriteFromUrlAsync(url);
        _image.sprite = sprite;
        _image.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
        return 2;
    }

    private YueTask<bool> Test2Async()
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
