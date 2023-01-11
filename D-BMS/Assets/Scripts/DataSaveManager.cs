using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int koolCount;
    public int coolCount;
    public int goodCount;
    public int missCount;
    public int failCount;
    public int maxCombo;
    public int rankIndex;
    public double score;
    public double accuracy;
    public List<float> scoreBarList;

    public void InitData()
    {
        koolCount = 0;
        coolCount = 0;
        goodCount = 0;
        missCount = 0;
        failCount = 0;
        maxCombo = 0;
        rankIndex = 11;
        score = 0;
        accuracy = 0;
        scoreBarList.Clear();
    }

    public void SetData(BMSResult result)
    {
        koolCount = result.koolCount;
        coolCount = result.coolCount;
        goodCount = result.goodCount;
        missCount = result.missCount;
        failCount = result.failCount;
        maxCombo = result.maxCombo;
        rankIndex = result.rankIndex;
        score = result.score;
        accuracy = result.accuracy;
        scoreBarList.Clear();
        int count = result.scoreBarArray.Length;
        for (int i = 0; i < count; i++)
        {
            scoreBarList.Add(result.scoreBarArray[i]);
        }
    }

    public SaveData()
    {
        koolCount = 0;
        coolCount = 0;
        goodCount = 0;
        missCount = 0;
        failCount = 0;
        maxCombo = 0;
        rankIndex = 0;
        score = 0;
        accuracy = 0;
        scoreBarList = new List<float>();
    }
}

public static class DataSaveManager
{
    private static string dataSavePath;

    public static void SaveResultData(BMSResult result, string fileName)
    {
        if (string.IsNullOrEmpty(dataSavePath))
        {
#if UNITY_EDITOR
            dataSavePath = $@"{Directory.GetParent((Directory.GetParent(Directory.GetParent(Application.dataPath).ToString())).ToString())}\DataSave\";
#else
            dataSavePath = $@"{Directory.GetParent(Application.dataPath).ToString()}\DataSave\";
#endif
        }

        if (!Directory.Exists(dataSavePath))
        {
            Directory.CreateDirectory(dataSavePath);
        }

        SongSelectUIManager.songRecordData.SetData(result);

        string saveJson = JsonUtility.ToJson(SongSelectUIManager.songRecordData);
        string saveFilePath = dataSavePath + fileName + ".json";

        File.WriteAllText(saveFilePath, saveJson);
    }

    public static void LoadResultData(string fileName)
    {
        if (string.IsNullOrEmpty(dataSavePath))
        {
#if UNITY_EDITOR
            dataSavePath = $@"{Directory.GetParent((Directory.GetParent(Directory.GetParent(Application.dataPath).ToString())).ToString())}\DataSave\";
#else
            dataSavePath = $@"{Directory.GetParent(Application.dataPath).ToString()}\DataSave\";
#endif
        }

        string saveFilePath = dataSavePath + fileName + ".json";

        if (File.Exists(saveFilePath))
        {
            SongSelectUIManager.songRecordData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
        }
        else
        {
            SongSelectUIManager.songRecordData.InitData();
        }
    }
}