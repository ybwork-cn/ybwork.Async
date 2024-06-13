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
        int v = await Test1();
        Log(v);
    }

    private async YueTask<int> Test1()
    {
        string url = "https://img-home.csdnimg.cn/images/20201124032511.png";
        Sprite sprite = await SpriteLoader.LoadSpriteFromUrl(url);
        _image.sprite = sprite;
        _image.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
        return 2;
    }

    private void Log(object obj)
    {
        Debug.Log(Time.frameCount + ":" + obj);
    }
}
