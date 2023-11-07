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
        StartCoroutine(LoadRankImage());
    }

    private IEnumerator LoadRankImage()
    {
        if (rankImageArray != null) { yield break; }
        rankImageArray = new Texture2D[12];
        for (int i = 0; i < 12; i++)
        {
            string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Rank\{GetRankImageName(i)}";
            if (File.Exists(filePath + ".jpg"))
            {
                filePath += ".jpg";
            }
            else if (File.Exists(filePath + ".png"))
            {
                filePath += ".png";
            }
            else
            {
                continue;
            }
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath);
            yield return uwr.SendWebRequest();

            rankImageArray[i] = (uwr.downloadHandler as DownloadHandlerTexture).texture;
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
