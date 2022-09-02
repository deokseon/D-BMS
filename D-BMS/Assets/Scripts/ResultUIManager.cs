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

    private BMPLoader loader;

    private BMSHeader header;
    private BMSResult bmsResult;

    public void Awake()
    {
        loader = new BMPLoader();

        header = BMSGameManager.header;
        bmsResult = BMSGameManager.bmsResult;

        DrawStatisticsResult();
        DrawJudgeGraph();
        StartCoroutine(DrawSongInfo());
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
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
        accuracyText.text = bmsResult.accuracy.ToString("P");
        maxComboText.text = bmsResult.maxCombo.ToString();
        scoreText.text = ((int)bmsResult.score).ToString();
        noteSpeedText.text = BMSGameManager.userSpeed.ToString();
        randomEffectorText.text = BMSGameManager.randomEffector.ToString();
        levelText.text = header.level.ToString();

        int idx = 0;
        int score = ((int)bmsResult.score);
        if (score >= 1090000) { idx = 0; }  // S+++
        else if (score >= 1050000 && score < 1090000) { idx = 1; }  // S++
        else if (score >= 1025000 && score < 1050000) { idx = 2; }  // S+
        else if (score >= 1000000 && score < 1025000) { idx = 3; }  // S
        else if (score >= 950000 && score < 1000000) { idx = 4; }   // A+
        else if (score >= 900000 && score < 950000) { idx = 5; }    // A
        else if (score >= 850000 && score < 900000) { idx = 6; }    // B
        else if (score >= 750000 && score < 850000) { idx = 7; }    // C
        else if (score >= 650000 && score < 750000) { idx = 8; }    // D
        else if (score >= 550000 && score < 650000) { idx = 9; }    // E
        else { idx = 10; }  // F
        rankImage.sprite = rankImageArray[idx];

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

    private void DrawJudgeGraph()
    {
        int len = bmsResult.judgeList.Count;
        double divideNoteCount = 1.0d / bmsResult.noteCount;
        double total = 0;
        for (int i = 0; i < len; i++)
        {
            double y = bmsResult.judgeList[i].Value;
            total += y;
            if (Utility.Dabs(y) > 115) { continue; }
            double x = (bmsResult.judgeList[i].Key * divideNoteCount * 600) - 300;

            GameObject tempDot = Instantiate(dot, dotParent);
            tempDot.transform.localPosition = new Vector3((float)x, (float)y * 2, 0.0f);
        }
        averageInputTimingText.text = $"{(int)(total * divideNoteCount)} MS";
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

        titleText.text = header.title;
        artistText.text = header.artist;
        if (header.minBPM == header.maxBPM) { bpmText.text = "BPM: " + header.bpm.ToString(); }
        else { bpmText.text = "BPM: " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
    }
}
