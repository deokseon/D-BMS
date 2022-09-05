using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using B83.Image.BMP;

public class GameUIManager : MonoBehaviour
{
    public bool isPrepared { get; set; } = false;
    public RawImage bga;
    public Dictionary<string, string> bgImageTable { get; set; }
    public Dictionary<string, Texture2D> bgSprites { get; set; }

    [SerializeField]
    private Slider hpBar;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI currentComboText;
    [SerializeField]
    private Animator comboTextAnimator;
    [SerializeField]
    private TextMeshProUGUI comboText;
    [SerializeField]
    private Animator comboAnimator;
    [SerializeField]
    private TextMeshProUGUI maxComboText;
    [SerializeField]
    private TextMeshProUGUI statisticsText;
    [SerializeField]
    private TextMeshProUGUI accuracyText;

    [SerializeField]
    private Animator[] noteBomb;
    [SerializeField]
    private Animator[] noteBombCenter;

    [SerializeField]
    private Animator judgeAnimator;

    [SerializeField]
    private Sprite[] earlyLateImageArray;
    [SerializeField]
    private Image[] earlyLateImage;
    [SerializeField]
    private Animator[] earlyLateAnimator;

    private int currentIdx;
    private float[] yPos;
    [SerializeField]
    private Sprite[] rank;
    [SerializeField]
    private Image rankImage;
    private Transform rankImageTransform;
    [SerializeField]
    private RectTransform scoreStick;
    [SerializeField]
    private RectTransform maxScoreStick;

    [SerializeField]
    private TextMeshProUGUI earlyCountText;
    [SerializeField]
    private TextMeshProUGUI lateCountText;
    [SerializeField]
    private TextMeshProUGUI koolCountText;
    [SerializeField]
    private TextMeshProUGUI coolCountText;
    [SerializeField]
    private TextMeshProUGUI goodCountText;

    [SerializeField]
    private TextMeshProUGUI bpmText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI noteSpeedText;
    [SerializeField]
    private TextMeshProUGUI randomEffectorText;

    [SerializeField]
    private GameObject loadingPanel;
    [SerializeField]
    private RawImage stageImage;
    [SerializeField]
    private Texture noStageImage;
    [SerializeField]
    private TextMeshProUGUI loadingTitleText;
    [SerializeField]
    private TextMeshProUGUI loadingNoteSpeedText;
    [SerializeField]
    private TextMeshProUGUI loadingRandomEffetorText;
    [SerializeField]
    private TextMeshProUGUI sliderValueText;
    [SerializeField]
    private Slider loadingSlider;

    [SerializeField]
    private Canvas endCanvas;

    public int maxCombo { get; set; }
    private int earlyCount;
    private int lateCount;
    private double divide20000 = 1.0d / 20000.0d;
    private double divide6250 = 1.0d / 6250.0d;
    private List<KeyValuePair<int, double>> tempJudgeList;

    private BMPLoader loader;
    private void Awake()
    {
        maxCombo = 0;
        earlyCount = 0;
        lateCount = 0;

        hpBar.value = 1.0f;

        maxComboText.text = maxCombo.ToString("D5");

        currentIdx = -1;
        rankImageTransform = rankImage.transform;
        yPos = new float[10] { 19.5f, 71.5f, 151.5f, 231.5f, 271.5f, 311.5f, 351.5f, 371.5f, 391.5f, 423.5f };

        endCanvas.enabled = false;

        loader = new BMPLoader();

        bgImageTable = new Dictionary<string, string>();
        bgSprites = new Dictionary<string, Texture2D>();
        tempJudgeList = new List<KeyValuePair<int, double>>(1000);
    }

    public void LoadImages()
    {
        StartCoroutine(CLoadImages());
    }

    private IEnumerator CLoadImages()
    {
        foreach (KeyValuePair<string, string> p in bgImageTable)
        {
            string path = @"file:\\" + BMSGameManager.header.musicFolderPath + p.Value;

            Texture2D texture2D = null;
            if (path.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
            {
                UnityWebRequest uwr = UnityWebRequest.Get(path);
                yield return uwr.SendWebRequest();

                texture2D = loader.LoadBMP(uwr.downloadHandler.data).ToTexture2D();
            }
            else if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
            {
                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
                yield return uwr.SendWebRequest();

                texture2D = (uwr.downloadHandler as DownloadHandlerTexture).texture;
            }

            bgSprites.Add(p.Key, texture2D);
            BMSGameManager.currentLoading++;
        }
        isPrepared = true;
    }

    public void ChangeBGA(string key)
    {
        if (bgSprites.ContainsKey(key))
        {
            bga.texture = bgSprites[key];
        }
    }

    public void TextUpdate(int combo, JudgeType judge, int index)
    {
        if (combo != 0)
        {
            currentComboText.text = combo.ToString();
            comboTextAnimator.SetTrigger("ComboText");
            comboAnimator.SetTrigger("Combo");
        }
        else
        {
            currentComboText.enabled = false;
            comboText.enabled = false;
        }

        if (combo > maxCombo)
        {
            maxCombo = combo;
            maxComboText.text = maxCombo.ToString("D5");
        }

        if (judge == JudgeType.KOOL) { judgeAnimator.SetTrigger("KoolJudge"); }
        else if (judge == JudgeType.COOL) { judgeAnimator.SetTrigger("CoolJudge"); }
        else if (judge == JudgeType.GOOD) { judgeAnimator.SetTrigger("GoodJudge"); }
        else if (judge == JudgeType.MISS) { judgeAnimator.SetTrigger("MissJudge"); }
        else  { judgeAnimator.SetTrigger("FailJudge"); }

        if (judge >= JudgeType.COOL) 
        {
            noteBombCenter[index].SetTrigger("NoteBombCenter");
            noteBomb[index].SetTrigger("NoteBomb");
        }
    }

    public void UpdateScore(BMSResult res, float hpChange, double accuracy, double score, double maxScore)
    {
        StartCoroutine(ChangeHPBarValue(hpChange));

        accuracyText.text = accuracy.ToString("P");

        statisticsText.text =
            $"{res.koolCount.ToString("D4")}\n" +
            $"{res.coolCount.ToString("D4")}\n" +
            $"{res.goodCount.ToString("D4")}\n" +
            $"{res.missCount.ToString("D4")}\n" +
            $"{res.failCount.ToString("D4")}";

        scoreText.text = ((int)score).ToString("D7");

        double under60 = (score > 600000.0d ? 600000.0d : score) * divide20000;
        double up60 = (score > 600000.0d ? score - 600000.0d : 0) * divide6250;
        scoreStick.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)(under60 + up60));

        under60 = (maxScore > 600000.0d ? 600000.0d : maxScore) * divide20000;
        up60 = (maxScore > 600000.0d ? maxScore - 600000.0d : 0) * divide6250;
        maxScoreStick.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)(under60 + up60));

        int idx = -1;
        if (score >= 1090000) { idx = 9; }
        else if (score >= 1050000 && score < 1090000) { idx = 8; }
        else if (score >= 1025000 && score < 1050000) { idx = 7; }
        else if (score >= 1000000 && score < 1025000) { idx = 6; }
        else if (score >= 950000 && score < 1000000) { idx = 5; }
        else if (score >= 900000 && score < 950000) { idx = 4; }
        else if (score >= 850000 && score < 900000) { idx = 3; }
        else if (score >= 750000 && score < 850000) { idx = 2; }
        else if (score >= 650000 && score < 750000) { idx = 1; }
        else if (score >= 550000 && score < 650000) { idx = 0; }
        if (idx != -1 && currentIdx != idx)
        {
            rankImage.sprite = rank[idx];
            rankImageTransform.localPosition = new Vector3(-390.0f, yPos[idx], 0.0f);
            currentIdx = idx;
        }
    }

    private IEnumerator ChangeHPBarValue(float hpChange)
    {
        for (int i = 0; i < 60; i++)
        {
            hpBar.value += hpChange;
            yield return null;
        }
    }

    public void UpdateFSText(double diff, int idx, int currentCount)
    {
        tempJudgeList.Add(new KeyValuePair<int, double>(currentCount, diff));
        if (Utility.Dabs(diff) <= 22.0d) { return; }

        int earlylate = 0;
        if (diff < 0)
        {
            earlylate = 0;
            earlyCount++;
        }
        else
        {
            earlylate = 1;
            lateCount++;
        }
        int index = (idx < 2) ? 0 : 1;

        earlyLateImage[index].sprite = earlyLateImageArray[earlylate];
        earlyLateAnimator[index].SetTrigger("EarlyLate");
    }

    public void UpdateSongEndText(int koolCount, int coolCount, int goodCount)
    {
        earlyCountText.text = earlyCount.ToString();
        lateCountText.text = lateCount.ToString();
        koolCountText.text = koolCount.ToString();
        coolCountText.text = coolCount.ToString();
        goodCountText.text = goodCount.ToString();

        endCanvas.enabled = true;
    }

    public void UpdateInfoText()
    {
        levelText.text = BMSGameManager.header.level.ToString();
        noteSpeedText.text = BMSGameManager.userSpeed.ToString("0.0");
        randomEffectorText.text = BMSGameManager.randomEffector.ToString();
    }

    public void UpdateBPMText(double bpm)
    {
        bpmText.text = bpm.ToString();
    }

    public void SetLoading()
    {
        stageImage.texture = (BMSGameManager.stageTexture.name.CompareTo("NoStageImage") == 0) ? 
                                noStageImage : BMSGameManager.stageTexture;
        loadingTitleText.text = BMSGameManager.header.title;
        loadingNoteSpeedText.text = BMSGameManager.userSpeed.ToString("0.0");
        loadingRandomEffetorText.text = BMSGameManager.randomEffector.ToString();
    }

    public void SetLoadingSlider(float loadingValue)
    {
        loadingSlider.value = loadingValue;
        sliderValueText.text = ((int)(loadingValue * 100.0f)).ToString() + "%";
    }

    public void CloseLoading()
    {
        loadingPanel.SetActive(false);
    }

    public void SaveJudgeList(BMSResult res)
    {
        int len = tempJudgeList.Count;
        for (int i = 0; i < len; i++)
        {
            res.judgeList.Add(tempJudgeList[i]);
        }
    }
}
