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
    public int earlyCount;
    public int lateCount;
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
        earlyCount = 0;
        lateCount = 0;
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
        earlyCount = result.earlyCount;
        lateCount = result.lateCount;
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
        earlyCount = 0;
        lateCount = 0;
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

public class ConfigData
{
    public float comboPosition;
    public float judgePosition;
    public float earlyLatePosition;
    public float noteBombScale;
    public float scoreDigitPositionX;
    public float scoreDigitPositionY;
    public float scoreImagePositionX;
    public float scoreImagePositionY;
    public float maxcomboDigitPositionX;
    public float maxcomboDigitPositionY;
    public float maxcomboImagePositionX;
    public float maxcomboImagePositionY;
    public float panelPosition;
    public float judgeLinePosition;
    public float scoreGraphPositionOffsetX;
    public float scoreGraphPositionOffsetY;
    public float judgementTrackerPositionOffsetX;
    public float judgementTrackerPositionOffsetY;
    public float bgaPositionX;
    public float bgaPositionY;
    public float bgaWidth;
    public float bgaHeight;

    public void InitData()
    {
        comboPosition = 5.15f;
        judgePosition = 1.4f;
        earlyLatePosition = 2.17f;
        noteBombScale = 1.5f;
        scoreDigitPositionX = 1.19f;
        scoreDigitPositionY = -1.76f;
        scoreImagePositionX = -0.95f;
        scoreImagePositionY = -1.76f;
        maxcomboDigitPositionX = 1.19f;
        maxcomboDigitPositionY = -2.17f;
        maxcomboImagePositionX = -0.69f;
        maxcomboImagePositionY = -2.17f;
        panelPosition = -7.63f;
        judgeLinePosition = 0.0f;
        scoreGraphPositionOffsetX = 0.0f;
        scoreGraphPositionOffsetY = 0.0f;
        judgementTrackerPositionOffsetX = 0.0f;
        judgementTrackerPositionOffsetY = 0.0f;
        bgaPositionX = 520.0f;
        bgaPositionY = 100.0f;
        bgaWidth = 700.0f;
        bgaHeight = 700.0f;
    }

    public ConfigData()
    {
        comboPosition = 0.0f;
        judgePosition = 0.0f;
        earlyLatePosition = 0.0f;
        noteBombScale = 0.0f;
        scoreDigitPositionX = 0.0f;
        scoreDigitPositionY = 0.0f;
        scoreImagePositionX = 0.0f;
        scoreImagePositionY = 0.0f;
        maxcomboDigitPositionX = 0.0f;
        maxcomboDigitPositionY = 0.0f;
        maxcomboImagePositionX = 0.0f;
        maxcomboImagePositionY = 0.0f;
        panelPosition = 0.0f;
        judgeLinePosition = 0.0f;
        scoreGraphPositionOffsetX = 0.0f;
        scoreGraphPositionOffsetY = 0.0f;
        judgementTrackerPositionOffsetX = 0.0f;
        judgementTrackerPositionOffsetY = 0.0f;
        bgaPositionX = 0.0f;
        bgaPositionY = 0.0f;
        bgaWidth = 0.0f;
        bgaHeight = 0.0f;
    }
}

public static class DataSaveManager
{
    public static void SaveResultData(BMSResult result, string fileName)
    {
        string dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\DataSave\";

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
        string dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\DataSave\";

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
        string dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\DataSave\";

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

    public static void SaveConfigData()
    {
        string configPath = $@"{Directory.GetParent(Application.dataPath)}\Skin\";

        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
        }

        string configDataJson = JsonUtility.ToJson(GameUIManager.config);
        string configDataPath = configPath + "config.json";

        File.WriteAllText(configDataPath, configDataJson);
    }

    public static void LoadConfigData()
    {
        string dataSavePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\";

        string saveFilePath = dataSavePath + "config.json";

        if (File.Exists(saveFilePath))
        {
            GameUIManager.config = JsonUtility.FromJson<ConfigData>(File.ReadAllText(saveFilePath));
        }
        else
        {
            GameUIManager.config.InitData();
        }
    }
}