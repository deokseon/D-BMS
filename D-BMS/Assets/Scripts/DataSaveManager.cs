using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResultData
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
    }

    public ResultData()
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
    }
}

public class ScoreGraphData
{
    public List<float> scoreGraphList;

    public void InitData()
    {
        scoreGraphList.Clear();
    }

    public void SetData(BMSResult result)
    {
        scoreGraphList.Clear();
        int count = result.scoreGraphArray.Length;
        for (int i = 0; i < count; i++)
        {
            scoreGraphList.Add(result.scoreGraphArray[i]);
        }
    }

    public ScoreGraphData()
    {
        scoreGraphList = new List<float>(4000);
    }
}

public static class DataSaveManager
{
    private static string dataSavePath;

    public static void SaveResultData(BMSResult result, string fileName)
    {
        if (string.IsNullOrEmpty(dataSavePath))
        {
            dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\DataSave\";
        }

        if (!Directory.Exists(dataSavePath))
        {
            Directory.CreateDirectory(dataSavePath);
        }

        SongSelectUIManager.resultData.SetData(result);
        SongSelectUIManager.scoreGraphData.SetData(result);

        string resultDataJson = JsonUtility.ToJson(SongSelectUIManager.resultData);
        string resultDataPath = dataSavePath + fileName + ".json";

        File.WriteAllText(resultDataPath, resultDataJson);

        string scoreGraphDataJson = JsonUtility.ToJson(SongSelectUIManager.scoreGraphData);
        string scoreGraphDataPath = dataSavePath + fileName + "_SG.json";

        File.WriteAllText(scoreGraphDataPath, scoreGraphDataJson);
    }

    public static void LoadResultData(string fileName)
    {
        if (string.IsNullOrEmpty(dataSavePath))
        {
            dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\DataSave\";
        }

        string saveFilePath = dataSavePath + fileName + ".json";

        if (File.Exists(saveFilePath))
        {
            SongSelectUIManager.resultData = JsonUtility.FromJson<ResultData>(File.ReadAllText(saveFilePath));
        }
        else
        {
            SongSelectUIManager.resultData.InitData();
        }
    }

    public static void LoadScoreGraphData(string fileName)
    {
        if (string.IsNullOrEmpty(dataSavePath))
        {
            dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\DataSave\";
        }

        string saveFilePath = dataSavePath + fileName + "_SG.json";

        if (File.Exists(saveFilePath))
        {
            SongSelectUIManager.scoreGraphData = JsonUtility.FromJson<ScoreGraphData>(File.ReadAllText(saveFilePath));
        }
        else
        {
            SongSelectUIManager.scoreGraphData.InitData();
        }
    }
}