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
    private TextMeshProUGUI totalnotesText;
    [SerializeField]
    private TextMeshProUGUI koolText;
    [SerializeField]
    private TextMeshProUGUI coolText;
    [SerializeField]
    private TextMeshProUGUI goodText;
    [SerializeField]
    private TextMeshProUGUI missText;
    [SerializeField]
    private TextMeshProUGUI failText;
    [SerializeField]
    private TextMeshProUGUI accuracyText;
    [SerializeField]
    private TextMeshProUGUI maxComboText;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI koolDiffText;
    [SerializeField]
    private TextMeshProUGUI coolDiffText;
    [SerializeField]
    private TextMeshProUGUI goodDiffText;
    [SerializeField]
    private TextMeshProUGUI missDiffText;
    [SerializeField]
    private TextMeshProUGUI failDiffText;
    [SerializeField]
    private TextMeshProUGUI accuracyDiffText;
    [SerializeField]
    private TextMeshProUGUI maxComboDiffText;
    [SerializeField]
    private Image koolChangeImage;
    [SerializeField]
    private Image coolChangeImage;
    [SerializeField]
    private Image goodChangeImage;
    [SerializeField]
    private Image missChangeImage;
    [SerializeField]
    private Image failChangeImage;
    [SerializeField]
    private Image accuracyChangeImage;
    [SerializeField]
    private Image maxComboChangeImage;
    [SerializeField]
    private Sprite changeImage;
    [SerializeField]
    private GameObject newRecordImage;
    [SerializeField]
    private TextMeshProUGUI noteSpeedText;
    [SerializeField]
    private TextMeshProUGUI randomEffectorText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private Image rankImage;
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
    private Transform dotParent;
    [SerializeField]
    private GameObject dot;
    [SerializeField]
    private TextMeshProUGUI averageInputTimingText;
    [SerializeField]
    private RawImage banner;
    [SerializeField]
    private Texture noBannerTexture;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI artistText;
    [SerializeField]
    private TextMeshProUGUI bpmText;
    [SerializeField]
    private Animator fadeAnimator;
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

        if (BMSGameManager.isClear && SongSelectUIManager.songRecordData.score < bmsResult.score)
        {
            DataSaveManager.SaveResultData(bmsResult, header.fileName);
            newRecordImage.SetActive(true);
        }

        StartCoroutine(CoFadeOut());
    }

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeAnimator.SetTrigger("FadeOut");
    }

    private IEnumerator CoLoadSelectScene()
    {
        fadeAnimator.SetTrigger("FadeIn");

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
        totalnotesText.text = bmsResult.noteCount.ToString();
        koolText.text = bmsResult.koolCount.ToString();
        coolText.text = bmsResult.coolCount.ToString();
        goodText.text = bmsResult.goodCount.ToString();
        missText.text = bmsResult.missCount.ToString();
        failText.text = bmsResult.failCount.ToString();
        accuracyText.text = ((float)bmsResult.accuracy).ToString("P");
        maxComboText.text = bmsResult.maxCombo.ToString();
        scoreText.text = ((int)((float)bmsResult.score)).ToString();
        noteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(PlayerPrefs.GetInt("RandomEffector"));
        levelText.text = header.level.ToString();

        DrawDiffTextAndImage(bmsResult.koolCount - SongSelectUIManager.songRecordData.koolCount, koolDiffText, koolChangeImage);
        DrawDiffTextAndImage(bmsResult.coolCount - SongSelectUIManager.songRecordData.coolCount, coolDiffText, coolChangeImage);
        DrawDiffTextAndImage(bmsResult.goodCount - SongSelectUIManager.songRecordData.goodCount, goodDiffText, goodChangeImage);
        DrawDiffTextAndImage(bmsResult.missCount - SongSelectUIManager.songRecordData.missCount, missDiffText, missChangeImage, -1);
        DrawDiffTextAndImage(bmsResult.failCount - SongSelectUIManager.songRecordData.failCount, failDiffText, failChangeImage, -1);
        DrawDiffTextAndImage((float)(bmsResult.accuracy - SongSelectUIManager.songRecordData.accuracy), accuracyDiffText, accuracyChangeImage, 1, true);
        DrawDiffTextAndImage(bmsResult.maxCombo - SongSelectUIManager.songRecordData.maxCombo, maxComboDiffText, maxComboChangeImage);

        rankImage.sprite = rankImageArray[bmsResult.rankIndex];

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
            case 0: randomEffectorText.text = "NONE"; break;
            case 1: randomEffectorText.text = "RANDOM"; break;
            case 2: randomEffectorText.text = "MIRROR"; break;
            case 3: randomEffectorText.text = "F-RANDOM"; break;
            case 4: randomEffectorText.text = "MF-RANDOM"; break;
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
        averageInputTimingText.text = $"{average} MS";
    }

    private IEnumerator DrawSongInfo()
    {
        if (string.IsNullOrEmpty(header.bannerPath)) { banner.texture = noBannerTexture; }
        else
        {
            string imagePath = $@"file:\\{header.musicFolderPath}{header.bannerPath}";

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

            banner.texture = (tex ?? noBannerTexture);
        }

        if (header.title.Length > 30) { titleText.fontSize = 15; }
        else if (header.title.Length <= 30 && header.title.Length >= 15) { titleText.fontSize = 25; }
        else { titleText.fontSize = 50; }
        titleText.text = header.title;
        if (header.artist.Length >= 30) { artistText.fontSize = 15; }
        else { artistText.fontSize = 25; }
        artistText.text = header.artist;
        if (header.minBPM == header.maxBPM) { bpmText.text = "BPM: " + header.bpm.ToString(); }
        else { bpmText.text = "BPM: " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
    }
}
