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
    private TextMeshProUGUI frontScoreText;
    [SerializeField]
    private TextMeshProUGUI backScoreText;
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
    private TextMeshProUGUI frontAccuracyText;
    [SerializeField]
    private TextMeshProUGUI backAccuracyText;

    [SerializeField]
    private Animator[] noteBomb;

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
    private readonly RectTransform.Axis vertical = RectTransform.Axis.Vertical;

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
    private GameObject endInfo;
    [SerializeField]
    private Animator fadeinAnimator;
    [SerializeField]
    private GameObject fadeinObject;

    [HideInInspector]
    public int maxCombo;
    private int earlyCount;
    private int lateCount;

    private BMPLoader loader;

    private readonly int hashComboText = Animator.StringToHash("ComboText");
    private readonly int hashCombo = Animator.StringToHash("Combo");
    private readonly int hashKoolJudge = Animator.StringToHash("KoolJudge");
    private readonly int hashCoolJudge = Animator.StringToHash("CoolJudge");
    private readonly int hashGoodJudge = Animator.StringToHash("GoodJudge");
    private readonly int hashMissJudge = Animator.StringToHash("MissJudge");
    private readonly int hashFailJudge = Animator.StringToHash("FailJudge");
    private readonly int hashBombEffect = Animator.StringToHash("BombEffect");
    private readonly int hashEarlyLate = Animator.StringToHash("EarlyLate");

    private const double divide20000 = 1.0d / 20000.0d;
    private const double divide6250 = 1.0d / 6250.0d;
    private string[] str0000to9999Table;
    private string[] str0to999Table;
    private string[] str00to100Table;
    private string[] str000to110Table;

    private void Awake()
    {
        maxCombo = 0;
        earlyCount = 0;
        lateCount = 0;

        hpBar.value = 1.0f;

        maxComboText.text = maxCombo.ToString("D4");

        currentIdx = -1;
        rankImageTransform = rankImage.transform;
        yPos = new float[10] { 19.5f, 71.5f, 151.5f, 231.5f, 271.5f, 311.5f, 351.5f, 371.5f, 391.5f, 423.5f };

        endInfo.SetActive(false);

        loader = new BMPLoader();

        bgImageTable = new Dictionary<string, string>();
        bgSprites = new Dictionary<string, Texture2D>();

        scoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
        maxScoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
    }

    public void MakeStringTable()
    {
        str0000to9999Table = new string[10000];
        for (int i = 0; i < 10000;i++) { str0000to9999Table[i] = i.ToString("D4"); }

        str0to999Table = new string[1000];
        for (int i = 0; i < 1000; i++) { str0to999Table[i] = i.ToString(); }

        str00to100Table = new string[101];
        for (int i = 0; i < 100; i++) { str00to100Table[i] = i.ToString("D2"); }
        str00to100Table[100] = "100";

        str000to110Table = new string[111];
        for (int i = 0; i < 111; i++) { str000to110Table[i] = i.ToString("D3"); }
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

    public void TextUpdate(BMSResult res, int combo, JudgeType judge, int index)
    {
        if (combo != 0)
        {
            currentComboText.text = (combo < 1000 ? str0to999Table[combo] : str0000to9999Table[combo]);
            comboTextAnimator.SetTrigger(hashComboText);
            comboAnimator.SetTrigger(hashCombo);
        }
        else
        {
            currentComboText.enabled = false;
            comboText.enabled = false;
        }

        if (combo > maxCombo)
        {
            maxCombo = combo;
            maxComboText.text = str0000to9999Table[maxCombo];
        }

        switch (judge)
        {
            case JudgeType.KOOL:
                koolText.text = str0000to9999Table[res.koolCount];
                judgeAnimator.SetTrigger(hashKoolJudge); 
                noteBomb[index].SetTrigger(hashBombEffect); 
                break;
            case JudgeType.COOL:
                coolText.text = str0000to9999Table[res.coolCount];
                judgeAnimator.SetTrigger(hashCoolJudge); 
                noteBomb[index].SetTrigger(hashBombEffect); 
                break;
            case JudgeType.GOOD:
                goodText.text = str0000to9999Table[res.goodCount];
                judgeAnimator.SetTrigger(hashGoodJudge); 
                break;
            case JudgeType.MISS:
                missText.text = str0000to9999Table[res.missCount];
                judgeAnimator.SetTrigger(hashMissJudge); 
                break;
            default:
                failText.text = str0000to9999Table[res.failCount];
                judgeAnimator.SetTrigger(hashFailJudge); 
                break;
        }
    }

    public void UpdateScore(float hp, double accuracy, double score, double maxScore)
    {
        hpBar.value = hp;

        int frontAC = (int)(accuracy);
        int backAC = (int)((accuracy - frontAC) * 100.0d);
        frontAccuracyText.text = str00to100Table[frontAC];
        backAccuracyText.text = str00to100Table[backAC];

        int frontSC = (int)(score * 0.0001d);
        int backSC = (int)(score - (frontSC * 10000));
        frontScoreText.text = str000to110Table[frontSC];
        backScoreText.text = str0000to9999Table[backSC];

        double under60 = (score > 600000.0d ? 600000.0d : score) * divide20000;
        double up60 = (score > 600000.0d ? score - 600000.0d : 0.0d) * divide6250;
        scoreStick.SetSizeWithCurrentAnchors(vertical, (float)(under60 + up60));

        under60 = (maxScore > 600000.0d ? 600000.0d : maxScore) * divide20000;
        up60 = (maxScore > 600000.0d ? maxScore - 600000.0d : 0.0d) * divide6250;
        maxScoreStick.SetSizeWithCurrentAnchors(vertical, (float)(under60 + up60));

        int idx;
        switch (score)
        {
            case double n when (n >= 550000.0d && n < 650000.0d): idx = 0; break;
            case double n when (n >= 650000.0d && n < 750000.0d): idx = 1; break;
            case double n when (n >= 750000.0d && n < 850000.0d): idx = 2; break;
            case double n when (n >= 850000.0d && n < 900000.0d): idx = 3; break;
            case double n when (n >= 900000.0d && n < 950000.0d): idx = 4; break;
            case double n when (n >= 950000.0d && n < 1000000.0d): idx = 5; break;
            case double n when (n >= 1000000.0d && n < 1025000.0d): idx = 6; break;
            case double n when (n >= 1025000.0d && n < 1050000.0d): idx = 7; break;
            case double n when (n >= 1050000.0d && n < 1090000.0d): idx = 8; break;
            case double n when (n >= 1090000.0d): idx = 9; break;
            default: idx = -1; break;
        }
        if (idx != -1 && currentIdx != idx)
        {
            rankImage.sprite = rank[idx];
            rankImageTransform.localPosition = new Vector3(-390.0f, yPos[idx], 0.0f);
            currentIdx = idx;
        }
    }

    public void UpdateFSText(double diff, int idx)
    {
        if ((diff > 0 ? diff : -diff) <= 22.0d) { return; }

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
        earlyLateAnimator[index].SetTrigger(hashEarlyLate);
    }

    public void UpdateSongEndText(int koolCount, int coolCount, int goodCount)
    {
        earlyCountText.text = earlyCount.ToString();
        lateCountText.text = lateCount.ToString();
        koolCountText.text = koolCount.ToString();
        coolCountText.text = coolCount.ToString();
        goodCountText.text = goodCount.ToString();

        endInfo.SetActive(true);
    }
    public void FadeIn()
    {
        fadeinObject.SetActive(true);
        fadeinAnimator.SetTrigger("FadeIn");
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
}
