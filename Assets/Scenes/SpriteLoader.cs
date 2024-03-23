using UnityEngine;
using UnityEngine.Networking;
using ybwork.Async;

public static class SpriteLoader
{
    private static async YueTask<Texture2D> LoadTextrueFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        UnityWebRequest request = UnityWebRequest.Get(url);
        await request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(request.downloadHandler.data);
            return tex;
        }
        else
        {
            Debug.Log("下载图片失败 : " + url);
            return null;
        }
    }

    public static async YueTask<Sprite> LoadSpriteFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        Texture2D tex = await LoadTextrueFromUrl(url);
        if (tex == null)
            return null;

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }
}
