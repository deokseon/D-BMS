using B83.Image.BMP;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class TextureDownloadManager : MonoBehaviour
{

    private readonly string[] videoExtension = { ".mp4", ".avi", ".wmv", ".mpeg", ".mpg" };
    public async UniTask<Texture2D> GetTexture(string path)
    {
        BMPLoader loader = new BMPLoader();
        var token = this.GetCancellationTokenOnDestroy();

        Texture2D texture2D = null;
        try
        {
            if (File.Exists(path + ".bmp"))
            {
                var uwr = await UnityWebRequest.Get(@"file:\\" + path + ".bmp").SendWebRequest().WithCancellation(cancellationToken: token);
                var uwrData = uwr.downloadHandler.data;
                var bmpImage = await UniTask.RunOnThreadPool(() => loader.LoadBMP(uwrData));
                texture2D = bmpImage.ToTexture2D();
            }
            else if (File.Exists(path + ".jpg"))
            {
                var uwr = await UnityWebRequestTexture.GetTexture(@"file:\\" + path + ".jpg").SendWebRequest().WithCancellation(cancellationToken: token);
                texture2D = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            }
            else if (File.Exists(path + ".png"))
            {
                var uwr = await UnityWebRequestTexture.GetTexture(@"file:\\" + path + ".png").SendWebRequest().WithCancellation(cancellationToken: token);
                texture2D = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            }
        }
        catch (System.Exception)
        {
            texture2D = null;
        }

        return texture2D;
    }

    public async UniTask PrepareVideo(string path, string video, string screen)
    {
        VideoPlayer videoPlayer = GameObject.Find(video).GetComponent<VideoPlayer>();
        int i;
        for (i = 0; i < videoExtension.Length; i++)
        {
            if (File.Exists(path + videoExtension[i]))
            {
                path = path + videoExtension[i];
                break;
            }
        }

        if (i == videoExtension.Length) return;

        videoPlayer.url = $"file://{path}";

        videoPlayer.Prepare();

        await UniTask.WaitUntil(() => videoPlayer.isPrepared);

        GameObject.Find(screen).GetComponent<RawImage>().texture = videoPlayer.texture;

        videoPlayer.Play();
    }
}
