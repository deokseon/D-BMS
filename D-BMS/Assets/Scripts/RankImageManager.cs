using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class RankImageManager : MonoBehaviour
{
    public static Texture2D[] rankImageArray;

    void Awake()
    {
        if (rankImageArray == null)
        {
            _ = LoadRankImage();
        }
    }

    private async UniTask LoadRankImage()
    {
        rankImageArray = new Texture2D[12];
        for (int i = 0; i < 12; i++)
        {
            rankImageArray[i] = await FindObjectOfType<TextureDownloadManager>().GetTexture($@"{Directory.GetParent(Application.dataPath)}\Skin\Rank\{GetRankImageName(i)}");
            Texture2D textureMipmaps = new Texture2D(rankImageArray[i].width, rankImageArray[i].height, rankImageArray[i].format, true);
            textureMipmaps.SetPixels32(rankImageArray[i].GetPixels32(0), 0);
            textureMipmaps.filterMode = FilterMode.Trilinear;
            textureMipmaps.Apply(true);
            rankImageArray[i] = textureMipmaps;
        }
    }

    private string GetRankImageName(int index)
    {
        string name = "";
        switch (index)
        {
            case 0: name = "F_0"; break;
            case 1: name = "E_0"; break;
            case 2: name = "D_0"; break;
            case 3: name = "C_0"; break;
            case 4: name = "B_0"; break;
            case 5: name = "A_0"; break;
            case 6: name = "A_1"; break;
            case 7: name = "S_0"; break;
            case 8: name = "S_1"; break;
            case 9: name = "S_2"; break;
            case 10: name = "S_3"; break;
            case 11: name = "NoRank"; break;
        }
        return name;
    }
}
