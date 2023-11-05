using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine;
using B83.Image.BMP;
using System.IO;
using UnityEngine.Video;

public class ResultUIManager : MonoBehaviour
{
    [SerializeField]
    private Sprite changeImage;
    [SerializeField]
    private GameObject newRecordImage;
    [SerializeField]
    private Sprite[] rankImageArray;
    [SerializeField]
    private GameObject playLamp;
    [SerializeField]
    private GameObject clearLamp;
    [SerializeField]
    private GameObject nomissLamp;
    [SerializeField]
    private GameObject allcoolLamp;
    [SerializeField]
    private GameObject dot;
    [SerializeField]
    private Texture noBannerTexture;
    [SerializeField]
    private Image fadeImage;

    private SongInfoObject songInfoObject;

    private BMPLoader loader;

    public void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        loader = new BMPLoader();

        songInfoObject = FindObjectOfType<SongInfoObject>();

        DrawStatisticsResult();
        DrawJudgeGraph();
        StartCoroutine(DrawSongInfo());

        if (BMSGameManager.isClear && SongSelectUIManager.resultData.score < BMSGameManager.bmsResult.resultData.score)
        {
            DataSaveManager.SaveData("DataSave", BMSGameManager.header.fileName + ".json", BMSGameManager.bmsResult.resultData);
            DataSaveManager.SaveData("DataSave", BMSGameManager.header.fileName + "_SG.json", BMSGameManager.bmsResult.scoreGraphData);
            newRecordImage.SetActive(true);
        }

        SetBackground();
    }

    private void SetBackground()
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\result-bg";
        if (File.Exists(filePath + ".jpg"))
        {
            StartCoroutine(LoadBG(filePath + ".jpg"));
        }
        else if (File.Exists(filePath + ".png"))
        {
            StartCoroutine(LoadBG(filePath + ".png"));
        }
        else if (File.Exists(filePath + ".mp4"))
        {
            StartCoroutine(PrepareVideo(filePath + ".mp4"));
        }
        else if (File.Exists(filePath + ".avi"))
        {
            StartCoroutine(PrepareVideo(filePath + ".avi"));
        }
        else if (File.Exists(filePath + ".wmv"))
        {
            StartCoroutine(PrepareVideo(filePath + ".wmv"));
        }
        else if (File.Exists(filePath + ".mpeg"))
        {
            StartCoroutine(PrepareVideo(filePath + ".mpeg"));
        }
        else if (File.Exists(filePath + ".mpg"))
        {
            StartCoroutine(PrepareVideo(filePath + ".mpg"));
        }
    }

    private IEnumerator LoadBG(string path)
    {
        string imagePath = $@"file:\\{path}";

        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath);
        yield return uwr.SendWebRequest();

        GameObject.Find("Screen").GetComponent<RawImage>().texture = (uwr.downloadHandler as DownloadHandlerTexture).texture;

        StartCoroutine(CoFadeOut());
    }


    private IEnumerator PrepareVideo(string path)
    {
        VideoPlayer videoPlayer = GameObject.Find("VideoPlayer").GetComponent<VideoPlayer>();
        videoPlayer.url = $"file://{path}";

        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);

        GameObject.Find("Screen").GetComponent<RawImage>().texture = videoPlayer.texture;

        videoPlayer.Play();

        StartCoroutine(CoFadeOut());
    }

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeImage.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private IEnumerator CoLoadSelectScene()
    {
        fadeImage.GetComponent<Animator>().SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (fadeImage.IsActive()) { return; }
            StartCoroutine(CoLoadSelectScene());
        }
    }

    private void DrawStatisticsResult()
    {
        songInfoObject.SetSongInfo(BMSGameManager.header);
        songInfoObject.SetFavoriteToggle(BMSGameManager.header);

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
        SetRandomEffectorText(PlayerPrefs.GetInt("RandomEffector"));
        GameObject.Find("Fader").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat("FadeIn") == 0.0f ? "NONE" : $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";

        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.koolCount - SongSelectUIManager.resultData.koolCount, GameObject.Find("KoolDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("KoolChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.coolCount - SongSelectUIManager.resultData.coolCount, GameObject.Find("CoolDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("CoolChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.goodCount - SongSelectUIManager.resultData.goodCount, GameObject.Find("GoodDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("GoodChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.missCount - SongSelectUIManager.resultData.missCount, GameObject.Find("MissDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("MissChangeImage").GetComponent<Image>(), -1);
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.failCount - SongSelectUIManager.resultData.failCount, GameObject.Find("FailDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("FailChangeImage").GetComponent<Image>(), -1);
        DrawDiffTextAndImage((float)(BMSGameManager.bmsResult.resultData.accuracy - SongSelectUIManager.resultData.accuracy), GameObject.Find("AccuracyDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("AccuracyChangeImage").GetComponent<Image>(), 1, true);
        DrawDiffTextAndImage(BMSGameManager.bmsResult.resultData.maxCombo - SongSelectUIManager.resultData.maxCombo, GameObject.Find("MaxComboDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("MaxComboChangeImage").GetComponent<Image>());

        GameObject.Find("Rank").GetComponent<Image>().sprite = rankImageArray[BMSGameManager.bmsResult.resultData.rankIndex];

        int clearLampIndex;
        if (!BMSGameManager.isClear) 
        { 
            playLamp.SetActive(true);
            clearLampIndex = 0;
        }
        else if (BMSGameManager.bmsResult.resultData.missCount > 0 || BMSGameManager.bmsResult.resultData.failCount > 0) 
        { 
            clearLamp.SetActive(true);
            clearLampIndex = 1;
        }
        else if (BMSGameManager.bmsResult.resultData.goodCount > 0)
        { 
            nomissLamp.SetActive(true);
            clearLampIndex = 2;
        }
        else 
        { 
            allcoolLamp.SetActive(true);
            clearLampIndex = 3;
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
            if (y > 22.0d || y < -22.0d)
            {
                tempDot.GetComponent<Image>().color = y > 0.0d ? Color.red : new Color(0.0f, 155.0f / 255.0f, 1.0f);
            }
            tempDot.transform.localPosition = new Vector3((float)x, (float)y * 2, 0.0f);
        }
        int average = (totalCount == 0 ? 0 : (int)(total / totalCount));
        GameObject.Find("AverageInputTiming").GetComponent<TextMeshProUGUI>().text = $"{average} MS";
    }

    private IEnumerator DrawSongInfo()
    {
        RawImage stageImage = GameObject.Find("StageImage").GetComponent<RawImage>();
        if (string.IsNullOrEmpty(BMSGameManager.header.stageFilePath)) 
        {
            stageImage.texture = noBannerTexture;
            stageImage.color = new Color32(0, 0, 0, 230);
        }
        else
        {
            string imagePath = $@"file:\\{BMSGameManager.header.musicFolderPath}{BMSGameManager.header.stageFilePath}";

            Texture tex = null;
            if (imagePath.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
            {
                UnityWebRequest uwr = UnityWebRequest.Get(imagePath);
                yield return uwr.SendWebRequest();

                tex = loader.LoadBMP(uwr.downloadHandler.data).ToTexture2D();
            }
            else if (imagePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                     imagePath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
            {
                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath);
                yield return uwr.SendWebRequest();

                tex = (uwr.downloadHandler as DownloadHandlerTexture).texture;
            }

            if (tex == null)
            {
                stageImage.texture = noBannerTexture;
                stageImage.color = new Color32(0, 0, 0, 230);
            }
            else
            {
                stageImage.texture = tex;
                stageImage.color = new Color32(255, 255, 255, 255);
            }
        }
    }
}
