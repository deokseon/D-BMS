using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine;
using B83.Image.BMP;
using System.IO;
using UnityEngine.Video;
using Cysharp.Threading.Tasks;
using System;

public class ResultUIManager : MonoBehaviour
{
    private Texture stageImageTexture = null;
    private Texture bgImageTexture = null;

    [SerializeField]
    private Sprite changeImage;
    [SerializeField]
    private GameObject dot;
    [SerializeField]
    private Image fadeImage;

    private int replaySaveIndex = 0;
    [SerializeField]
    private GameObject replayPanel;
    [SerializeField]
    private GameObject[] replaySaveProcessPanels;
    [SerializeField]
    private TextMeshProUGUI[] replayTextList;
    [SerializeField]
    private TMP_InputField replayTitleInputField;

    private SongInfoObject songInfoObject;

    public void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        songInfoObject = FindObjectOfType<SongInfoObject>();

        DrawStatisticsResult();
        DrawJudgeGraph();
        _ = DrawSongInfo();

        if (!BMSGameManager.isReplay)
        {
            BMSGameManager.replayData.replayTitle = null;
            ReplayAutoSave();
            if (BMSGameManager.isClear && SongSelectUIManager.resultData.score < BMSGameManager.bmsResult.resultData.score)
            {
                DataSaveManager.SaveData("DataSave", BMSGameManager.header.fileName + ".json", BMSGameManager.bmsResult.resultData);
                DataSaveManager.SaveData("DataSave", BMSGameManager.header.fileName + "_SG.json", BMSGameManager.bmsResult.scoreGraphData);
                DataSaveManager.SaveData("Replay", BMSGameManager.header.fileName + "_RP2.json", BMSGameManager.replayData);
            }
            else
            {
                GameObject.Find("NewRecord").SetActive(false);
                if (!BMSGameManager.isClear)
                {
                    DataSaveManager.SaveData("Replay", BMSGameManager.header.fileName + "_RP3.json", BMSGameManager.replayData);
                }
            }
        }
        else
        {
            GameObject.Find("NewRecord").SetActive(false);
        }

        _ = SetBackground();
    }

    private void ReplayAutoSave()
    {
        ReplayData autoSaveData1 = DataSaveManager.LoadData<ReplayData>("Replay", $"{BMSGameManager.header.fileName}_RP{0}.json");
        ReplayData autoSaveData2 = DataSaveManager.LoadData<ReplayData>("Replay", $"{BMSGameManager.header.fileName}_RP{1}.json");
        if (autoSaveData1 == null)
        {
            DataSaveManager.SaveData("Replay", BMSGameManager.header.fileName + "_RP0.json", BMSGameManager.replayData);
        }
        else if (autoSaveData2 == null)
        {
            DataSaveManager.SaveData("Replay", BMSGameManager.header.fileName + "_RP1.json", BMSGameManager.replayData);
        }
        else
        {
            if (DateTime.Compare(DateTime.ParseExact(autoSaveData1.date, "yyyy-MM-dd HH:mm:ss", null), DateTime.ParseExact(autoSaveData2.date, "yyyy-MM-dd HH:mm:ss", null)) == -1)
            {
                DataSaveManager.SaveData("Replay", BMSGameManager.header.fileName + "_RP0.json", BMSGameManager.replayData);
            }
            else
            {
                DataSaveManager.SaveData("Replay", BMSGameManager.header.fileName + "_RP1.json", BMSGameManager.replayData);
            }
        }
    }

    private async UniTask SetBackground()
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\result-bg";

        bgImageTexture = await FindObjectOfType<TextureDownloadManager>().GetTexture(filePath);
        if (bgImageTexture != null)
        {
            GameObject.Find("Screen").GetComponent<RawImage>().texture = bgImageTexture;
        }
        else
        {
            await FindObjectOfType<TextureDownloadManager>().PrepareVideo(filePath, "VideoPlayer", "Screen");
        }
        _ = FadeOut();
    }

    private async UniTask FadeOut()
    {
        await UniTask.Delay(500);
        fadeImage.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private async UniTask LoadScene(int scene)
    {
        fadeImage.GetComponent<Animator>().SetTrigger("FadeIn");

        await UniTask.Delay(1000);

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    void Update()
    {
        if (fadeImage.IsActive()) { return; }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (replayPanel.activeSelf)
                replayPanel.SetActive(false);
            else
                _ = LoadScene(1);
        }
        if (replayPanel.activeSelf) { return; }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            Restart();
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            replayPanel.SetActive(true);
            SetReplayProcess(0);
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            FavoriteToggleClick();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SetStageImageRotateSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _ = LoadScene(1);
        }
    }

    private void Restart()
    {
        if (!BMSGameManager.isReplay)
        {
            BMSGameManager.replayData = null;
        }
        BMSFileSystem.selectedHeader = BMSGameManager.header;
        _ = LoadScene(2);
    }

    private void FavoriteToggleClick()
    {
        GameObject.Find("Favorite_Toggle").GetComponent<Toggle>().isOn = !GameObject.Find("Favorite_Toggle").GetComponent<Toggle>().isOn;
    }

    private void SetStageImageRotateSpeed()
    {
        GameObject.Find("StageImage").GetComponent<ImageRotate>().SetRotateSpeed();
    }

    public void ReplaySaveExitButton()
    {
        replayPanel.SetActive(false);
    }

    private void SetSelectSlotPanel()
    {
        for (int i = 0; i < replayTextList.Length; i++)
        {
            ReplayData replayData = DataSaveManager.LoadData<ReplayData>("Replay", $"{BMSGameManager.header.fileName}_RP{i + 4}.json");
            replayTextList[i].text = $"[{i + 1}]" + (replayData == null ? " - EMPTY" : $" {replayData.replayTitle} - {(int)(float)replayData.score} : {replayData.date}");
        }
    }

    private void SetSaveCheckPanel()
    {
        GameObject.Find("SlotNameText").GetComponent<TextMeshProUGUI>().text = replayTextList[replaySaveIndex - 4].text;
    }

    private void SetEnterReplayTitlePanel()
    {
        replayTitleInputField.text = null;
        GameObject.Find("EnterTitleNoticeText").GetComponent<TextMeshProUGUI>().text = $"[{replaySaveIndex - 3}] Enter the title of the replay.";
    }

    public void SetReplayProcess(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            replaySaveProcessPanels[i].SetActive(i == index ? true : false);
        }

        switch (index)
        {
            case 0: SetSelectSlotPanel(); break;
            case 1: SetSaveCheckPanel(); break;
            case 2: SetEnterReplayTitlePanel(); break;
        }
    }

    public void SelectReplaySlot(int index)
    {
        replaySaveIndex = index;
        SetReplayProcess(1);
    }

    public void SaveReplay()
    {
        BMSGameManager.replayData.replayTitle = replayTitleInputField.text;
        DataSaveManager.SaveData("Replay", $"{BMSGameManager.header.fileName}_RP{replaySaveIndex}.json", BMSGameManager.replayData);
        replayPanel.SetActive(false);
    }

    private void DrawStatisticsResult()
    {
        songInfoObject.SetSongInfo(BMSGameManager.header);
        songInfoObject.SetFavoriteToggle(BMSGameManager.header);
        songInfoObject.FavoriteToggleAddListener(BMSGameManager.header);

        GameObject.Find("TotalNote").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.noteCount.ToString();
        GameObject.Find("Kool").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.koolCount.ToString();
        GameObject.Find("Cool").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.coolCount.ToString();
        GameObject.Find("Good").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.goodCount.ToString();
        GameObject.Find("Miss").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.missCount.ToString();
        GameObject.Find("Fail").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.failCount.ToString();
        GameObject.Find("Early").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.earlyCount.ToString();
        GameObject.Find("Late").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.lateCount.ToString();
        GameObject.Find("Accuracy").GetComponent<TextMeshProUGUI>().text = ((float)BMSGameManager.bmsResult.resultData.accuracy).ToString("P");
        GameObject.Find("MaxCombo").GetComponent<TextMeshProUGUI>().text = BMSGameManager.bmsResult.resultData.maxCombo.ToString();
        GameObject.Find("Score").GetComponent<TextMeshProUGUI>().text = ((int)((float)BMSGameManager.bmsResult.resultData.score)).ToString();
        GameObject.Find("NoteSpeed").GetComponent<TextMeshProUGUI>().text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(BMSGameManager.isReplay ? BMSGameManager.replayData.randomEffector : PlayerPrefs.GetInt("RandomEffector"));
        GameObject.Find("Fader").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat("FadeIn") == 0.0f ? "NONE" : $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";

        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.koolCount - SongSelectUIManager.resultData.koolCount, GameObject.Find("KoolDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("KoolChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.coolCount - SongSelectUIManager.resultData.coolCount, GameObject.Find("CoolDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("CoolChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.goodCount - SongSelectUIManager.resultData.goodCount, GameObject.Find("GoodDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("GoodChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.missCount - SongSelectUIManager.resultData.missCount, GameObject.Find("MissDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("MissChangeImage").GetComponent<Image>(), -1);
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.failCount - SongSelectUIManager.resultData.failCount, GameObject.Find("FailDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("FailChangeImage").GetComponent<Image>(), -1);
        DrawDiffTextAndImage((float)(BMSGameManager.bmsResult.resultData.accuracy - SongSelectUIManager.resultData.accuracy), GameObject.Find("AccuracyDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("AccuracyChangeImage").GetComponent<Image>(), 1, true);
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.maxCombo - SongSelectUIManager.resultData.maxCombo, GameObject.Find("MaxComboDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("MaxComboChangeImage").GetComponent<Image>());

        GameObject.Find("Rank").GetComponent<RawImage>().texture = RankImageManager.rankImageArray[BMSGameManager.bmsResult.resultData.rankIndex];

        int clearLampIndex;
        if (!BMSGameManager.isClear) 
        { 
            clearLampIndex = 0;
        }
        else if (BMSGameManager.bmsResult.resultData.missCount > 0 || BMSGameManager.bmsResult.resultData.failCount > 0) 
        { 
            clearLampIndex = 1;
        }
        else if (BMSGameManager.bmsResult.resultData.goodCount > 0)
        { 
            clearLampIndex = 2;
        }
        else 
        { 
            clearLampIndex = 3;
        }

        GameObject[] lampArray = { GameObject.Find("PlayLamp_on"), GameObject.Find("ClearLamp_on"), GameObject.Find("NoMissLamp_on"), GameObject.Find("AllCoolLamp_on") };
        for (int i = 0; i < 4; i++)
        {
            lampArray[i].SetActive(i == clearLampIndex);
        }

        int priClearLampIndex;
        if (BMSFileSystem.songClearLamp.clearLampDict.TryGetValue(BMSGameManager.header.fileName, out priClearLampIndex))
        {
            if (priClearLampIndex < clearLampIndex)
            {
                BMSFileSystem.songClearLamp.clearLampDict[BMSGameManager.header.fileName] = clearLampIndex;
            }
        }
        else
        {
            BMSFileSystem.songClearLamp.clearLampDict.Add(BMSGameManager.header.fileName, clearLampIndex);
        }
        DataSaveManager.SaveData("DataSave", "ClearLamp.json", BMSFileSystem.songClearLamp);
        songInfoObject.SetClearLamp(BMSGameManager.header);
    }

    private void SetRandomEffectorText(int index)
    {
        switch (index)
        {
            case 0: GameObject.Find("Random").GetComponent<TextMeshProUGUI>().text = "NONE"; break;
            case 1: GameObject.Find("Random").GetComponent<TextMeshProUGUI>().text = "RANDOM"; break;
            case 2: GameObject.Find("Random").GetComponent<TextMeshProUGUI>().text = "MIRROR"; break;
            case 3: GameObject.Find("Random").GetComponent<TextMeshProUGUI>().text = "F-RANDOM"; break;
            case 4: GameObject.Find("Random").GetComponent<TextMeshProUGUI>().text = "MF-RANDOM"; break;
        }
    }

    private void DrawDiffTextAndImage(float diff, TextMeshProUGUI diffText, Image image, int isReverse = 1, bool isPercentage = false)
    {
        string diffString = isPercentage ? diff.ToString("P") : diff.ToString();
        if (diff > 0) { diffString = "+" + diffString; }
        diffText.text = diffString;

        if (diff != 0)
        {
            image.sprite = changeImage;
            image.color = diff * isReverse > 0 ? Color.yellow : Color.red;
            image.transform.rotation = diff > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180);
        }
    }

    private void DrawJudgeGraph()
    {
        int threshold = PlayerPrefs.GetInt("EarlyLateThreshold");
        Transform dotParent = GameObject.Find("JudgeGraph_Panel").transform;
        int len = BMSGameManager.bmsResult.judgeList.Length;
        double divideNoteCount = 1.0d / BMSGameManager.bmsResult.noteCount;
        double total = 0;
        int totalCount = 0;
        for (int i = 1; i < len; i++)
        {
            double y = BMSGameManager.bmsResult.judgeList[i];
            if (Utility.Dabs(y) > 110.0d) { continue; }
            total += y; totalCount++;
            double x = (i * divideNoteCount * 600) - 300;

            GameObject tempDot = Instantiate(dot, dotParent);
            if (y > threshold || y < -threshold)
            {
                tempDot.GetComponent<Image>().color = y > 0.0d ? Color.red : new Color(0.0f, 155.0f / 255.0f, 1.0f);
            }
            tempDot.transform.localPosition = new Vector3((float)x, (float)y * 2, 0.0f);
        }
        int average = (totalCount == 0 ? 0 : (int)(total / totalCount));
        GameObject.Find("AverageInputTiming").GetComponent<TextMeshProUGUI>().text = $"{average} MS";
        GameObject.Find("Late_Line").transform.localPosition = new Vector3(0.0f, threshold * 2.0f, 0.0f);
        GameObject.Find("Early_Line").transform.localPosition = new Vector3(0.0f, -threshold * 2.0f, 0.0f);
    }

    private async UniTask DrawSongInfo()
    {
        stageImageTexture = await FindObjectOfType<TextureDownloadManager>().GetTexture($"{BMSGameManager.header.musicFolderPath}{BMSGameManager.header.stageFilePath}");
        if (stageImageTexture != null)
        {
            GameObject.Find("StageImage").GetComponent<RawImage>().texture = stageImageTexture;
        }
    }

    private void OnDestroy()
    {
        if (stageImageTexture != null)
        {
            Destroy(stageImageTexture);
        }
        if (bgImageTexture != null)
        {
            Destroy(bgImageTexture);
        }
    }
}
