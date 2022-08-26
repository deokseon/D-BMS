using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using B83.Image.BMP;

public class GameUIManager : MonoBehaviour
{
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
    private Canvas endCanvas;

    private int maxCombo;
    private int earlyCount;
    private int lateCount;
    private double divide20000 = 1.0d / 20000.0d;
    private double divide6250 = 1.0d / 6250.0d;
    private List<KeyValuePair<int, double>> tempJudgeList;

    BMPLoader loader;

    private void Awake()
    {
        maxCombo = 0;
        earlyCount = 0;
        lateCount = 0;

        hpBar.value = 1.0f;

        maxComboText.text = maxCombo.ToString("D5");

        loader = new BMPLoader();

        endCanvas.enabled = false;

        bgImageTable = new Dictionary<string, string>();
        bgSprites = new Dictionary<string, Texture2D>();
        tempJudgeList = new List<KeyValuePair<int, double>>(1000);
    }

    public void LoadImages()
    {
        foreach (KeyValuePair<string, string> p in bgImageTable)
        {
            string path = BMSGameManager.header.musicFolderPath + p.Value;

            Texture2D texture2D = null;
            texture2D = (p.Value.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase) ?
                         loader.LoadBMP(BMSGameManager.header.bmpPath + p.Value).ToTexture2D() :
                         Resources.Load<Texture2D>(BMSGameManager.header.musicFolderPath + p.Value.Substring(0, path.Length - 4)));

            bgSprites.Add(p.Key, texture2D);
        }
    }

    public void DeleteImages()
    {
        bgSprites = null;
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
        BMSGameManager.bmsResult.maxCombo = maxCombo;

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

    public void SaveJudgeList(BMSResult res)
    {
        int len = tempJudgeList.Count;
        for (int i = 0; i < len; i++)
        {
            res.judgeList.Add(tempJudgeList[i]);
        }
    }
}
