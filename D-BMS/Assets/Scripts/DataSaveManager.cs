using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

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

    public ResultData(int rank)
    {
        koolCount = 0;
        coolCount = 0;
        goodCount = 0;
        missCount = 0;
        failCount = 0;
        earlyCount = 0;
        lateCount = 0;
        maxCombo = 0;
        rankIndex = rank;
        score = 0;
        accuracy = 0;
    }
}

public class ScoreGraphData
{
    public List<float> scoreGraphList;

    public ScoreGraphData(int count)
    {
        scoreGraphList = new List<float>(count);
        for (int i = 0; i < count; i++)
        {
            scoreGraphList.Add(0.0f);
        }
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

    public ConfigData()
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
}

public class SongClearLamp
{
    public Dictionary<string, int> clearLampDict;

    public SongClearLamp()
    {
        clearLampDict = new Dictionary<string, int>(BMSFileSystem.headers.Length);
    }
}

public class FavoriteSong
{
    public HashSet<string> favoriteSongSet;

    public FavoriteSong()
    {
        favoriteSongSet = new HashSet<string>();
    }
}

public class ReplayData
{
    public List<AbstractNote>[] noteList;
    public List<ReplayNoteData>[] replayNoteList;
    public string replayTitle;
    public int randomEffector;
    public string date;
    public double score;
}

public static class DataSaveManager
{
    public static void SaveData<T>(string saveFolder, string jsonFileName, T saveObject)
    {
        string parentPath = $@"{Directory.GetParent(Application.dataPath)}\{saveFolder}\";

        if (!Directory.Exists(parentPath))
        {
            Directory.CreateDirectory(parentPath);
        }

        File.WriteAllText(parentPath + jsonFileName, JsonConvert.SerializeObject(saveObject));
    }

    public static T LoadData<T>(string saveFolder, string jsonFileName)
    {
        string parentPath = $@"{Directory.GetParent(Application.dataPath)}\{saveFolder}\";

        return File.Exists(parentPath + jsonFileName) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(parentPath + jsonFileName)) : default(T);
    }
}