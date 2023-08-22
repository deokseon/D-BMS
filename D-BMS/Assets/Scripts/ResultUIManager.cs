using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine;
using B83.Image.BMP;

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

    private BMPLoader loader;

    private BMSHeader header;
    private BMSResult bmsResult;

    public void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        loader = new BMPLoader();

        header = BMSGameManager.header;
        bmsResult = BMSGameManager.bmsResult;

        DrawStatisticsResult();
        DrawJudgeGraph();
        StartCoroutine(DrawSongInfo());

        if (BMSGameManager.isClear && SongSelectUIManager.resultData.score < bmsResult.score)
        {
            DataSaveManager.SaveResultData(bmsResult, header.fileName);
            newRecordImage.SetActive(true);
        }

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
        if (Input.anyKeyDown)
        {
            if (fadeImage.IsActive()) { return; }
            StartCoroutine(CoLoadSelectScene());
        }
    }

    private void DrawStatisticsResult()
    {
        TextMeshProUGUI titleText = GameObject.Find("Title_Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI subtitleText = GameObject.Find("Subtitle_Text").GetComponent<TextMeshProUGUI>();
        titleText.text = header.title;
        titleText.fontSize = 28;
        subtitleText.rectTransform.localPosition = new Vector3(-445.0f + titleText.preferredWidth, 13.0f, 0.0f);
        subtitleText.text = header.subTitle;
        subtitleText.fontSize = 17;
        while (titleText.preferredWidth + subtitleText.preferredWidth > 835.0f)
        {
            titleText.fontSize -= 0.1f;
            subtitleText.rectTransform.localPosition = new Vector3(-445.0f + titleText.preferredWidth, 13.0f, 0.0f);
            subtitleText.fontSize -= 0.1f;
        }

        TextMeshProUGUI artistText = GameObject.Find("Artist_Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI bpmText = GameObject.Find("BPM_Text").GetComponent<TextMeshProUGUI>();
        artistText.text = header.artist;
        artistText.fontSize = 15;
        bpmText.rectTransform.localPosition = new Vector3(-420.0f + artistText.preferredWidth, -20.0f, 0.0f);
        if (header.maxBPM == header.minBPM) { bpmText.text = "BPM " + header.bpm.ToString(); }
        else { bpmText.text = "BPM " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
        bpmText.fontSize = 15;
        while (artistText.preferredWidth + bpmText.preferredWidth > 835.0f)
        {
            artistText.fontSize -= 0.1f;
            bpmText.rectTransform.localPosition = new Vector3(-420.0f + artistText.preferredWidth, -20.0f, 0.0f);
            bpmText.fontSize -= 0.1f;
        }
        GameObject.Find("Level_Text").GetComponent<TextMeshProUGUI>().text = header.level.ToString();

        GameObject.Find("TotalNote").GetComponent<TextMeshProUGUI>().text = bmsResult.noteCount.ToString();
        GameObject.Find("Kool").GetComponent<TextMeshProUGUI>().text = bmsResult.koolCount.ToString();
        GameObject.Find("Cool").GetComponent<TextMeshProUGUI>().text = bmsResult.coolCount.ToString();
        GameObject.Find("Good").GetComponent<TextMeshProUGUI>().text = bmsResult.goodCount.ToString();
        GameObject.Find("Miss").GetComponent<TextMeshProUGUI>().text = bmsResult.missCount.ToString();
        GameObject.Find("Fail").GetComponent<TextMeshProUGUI>().text = bmsResult.failCount.ToString();
        GameObject.Find("Accuracy").GetComponent<TextMeshProUGUI>().text = ((float)bmsResult.accuracy).ToString("P");
        GameObject.Find("MaxCombo").GetComponent<TextMeshProUGUI>().text = bmsResult.maxCombo.ToString();
        GameObject.Find("Score").GetComponent<TextMeshProUGUI>().text = ((int)((float)bmsResult.score)).ToString();
        GameObject.Find("NoteSpeed").GetComponent<TextMeshProUGUI>().text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(PlayerPrefs.GetInt("RandomEffector"));
        GameObject.Find("Fader").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat("FadeIn") == 0.0f ? "NONE" : $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";

        DrawDiffTextAndImage(bmsResult.koolCount - SongSelectUIManager.resultData.koolCount, GameObject.Find("KoolDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("KoolChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(bmsResult.coolCount - SongSelectUIManager.resultData.coolCount, GameObject.Find("CoolDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("CoolChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(bmsResult.goodCount - SongSelectUIManager.resultData.goodCount, GameObject.Find("GoodDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("GoodChangeImage").GetComponent<Image>());
        DrawDiffTextAndImage(bmsResult.missCount - SongSelectUIManager.resultData.missCount, GameObject.Find("MissDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("MissChangeImage").GetComponent<Image>(), -1);
        DrawDiffTextAndImage(bmsResult.failCount - SongSelectUIManager.resultData.failCount, GameObject.Find("FailDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("FailChangeImage").GetComponent<Image>(), -1);
        DrawDiffTextAndImage((float)(bmsResult.accuracy - SongSelectUIManager.resultData.accuracy), GameObject.Find("AccuracyDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("AccuracyChangeImage").GetComponent<Image>(), 1, true);
        DrawDiffTextAndImage(bmsResult.maxCombo - SongSelectUIManager.resultData.maxCombo, GameObject.Find("MaxComboDiff").GetComponent<TextMeshProUGUI>(), GameObject.Find("MaxComboChangeImage").GetComponent<Image>());

        GameObject.Find("Rank").GetComponent<Image>().sprite = rankImageArray[bmsResult.rankIndex];

        if (!BMSGameManager.isClear) 
        { 
            playLamp.SetActive(true);
        }
        else if (bmsResult.missCount > 0 || bmsResult.failCount > 0) 
        { 
            clearLamp.SetActive(true);
        }
        else if (bmsResult.goodCount > 0)
        { 
            nomissLamp.SetActive(true);
        }
        else 
        { 
            allcoolLamp.SetActive(true);
        }
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
        int len = bmsResult.judgeList.Length;
        double divideNoteCount = 1.0d / bmsResult.noteCount;
        double total = 0;
        int totalCount = 0;
        for (int i = 1; i < len; i++)
        {
            double y = bmsResult.judgeList[i];
            if (Utility.Dabs(y) > 115) { continue; }
            total += y; totalCount++;
            double x = (i * divideNoteCount * 600) - 300;

            GameObject tempDot = Instantiate(dot, dotParent);
            tempDot.transform.localPosition = new Vector3((float)x, (float)y * 2, 0.0f);
        }
        int average = (totalCount == 0 ? 0 : (int)(total / totalCount));
        GameObject.Find("AverageInputTiming").GetComponent<TextMeshProUGUI>().text = $"{average} MS";
    }

    private IEnumerator DrawSongInfo()
    {
        RawImage stageImage = GameObject.Find("StageImage").GetComponent<RawImage>();
        if (string.IsNullOrEmpty(header.stageFilePath)) 
        {
            stageImage.texture = noBannerTexture;
            stageImage.color = new Color32(0, 0, 0, 230);
        }
        else
        {
            string imagePath = $@"file:\\{header.musicFolderPath}{header.stageFilePath}";

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
