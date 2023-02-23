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
    private Dictionary<string, Texture2D> bgSprites;

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
    private GameObject[] keyFeedback;
    [SerializeField]
    private SpriteRenderer[] keyboard;
    [SerializeField]
    private Sprite[] keyInitImage;
    [SerializeField]
    private Sprite[] keyPressedImage;

    [SerializeField]
    private Animator[] noteBombEffect;

    [SerializeField]
    private Animator judgeEffectAnimator;

    [SerializeField]
    private Sprite[] earlyLateImageArray;
    [SerializeField]
    private SpriteRenderer[] earlyLateSprite;
    [SerializeField]
    private Animator[] earlyLateEffectAnimator;

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
    private TextMeshProUGUI judgeAdjValueText;

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
    [HideInInspector]
    public int earlyCount;
    [HideInInspector]
    public int lateCount;

    private BMPLoader loader;
    private WaitForSecondsRealtime wait1sec;
    private WaitForSecondsRealtime wait10ms;

    private readonly int hashComboText = Animator.StringToHash("ComboText");
    private readonly int hashCombo = Animator.StringToHash("Combo");
    private readonly int hashJudgeEffectKOOL = Animator.StringToHash("JudgeEffectKOOL");
    private readonly int hashJudgeEffectCOOL = Animator.StringToHash("JudgeEffectCOOL");
    private readonly int hashJudgeEffectGOOD = Animator.StringToHash("JudgeEffectGOOD");
    private readonly int hashJudgeEffectMISS = Animator.StringToHash("JudgeEffectMISS");
    private readonly int hashJudgeEffectFAIL = Animator.StringToHash("JudgeEffectFAIL");
    private readonly int hashNoteBombEffect = Animator.StringToHash("NoteBomb");
    private readonly int hashEarlyLateEffect = Animator.StringToHash("EarlyLateEffect");

    private const double divide20000 = 1.0d / 20000.0d;
    private const double height60 = 600000.0d * divide20000;
    private const double divide6250 = 1.0d / 6250.0d;
    private string[] str0000to9999Table;
    private string[] str0to999Table;
    private string[] str00to100Table;
    private string[] str000to110Table;

    private void Awake()
    {
        SetNoteBombPosition();
        SetKeyFeedback();

        maxCombo = 0;
        earlyCount = 0;
        lateCount = 0;

        hpBar.value = 1.0f;

        maxComboText.text = maxCombo.ToString("D4");

        currentIdx = -1;
        rankImageTransform = rankImage.transform;
        yPos = new float[11] { -90.0f, 19.5f, 71.5f, 151.5f, 231.5f, 271.5f, 311.5f, 351.5f, 371.5f, 391.5f, 423.5f };

        endInfo.SetActive(false);

        loader = new BMPLoader();
        wait1sec = new WaitForSecondsRealtime(1.0f);
        wait10ms = new WaitForSecondsRealtime(0.01f);

        bgImageTable = new Dictionary<string, string>();
        bgSprites = new Dictionary<string, Texture2D>();

        scoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
        maxScoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
    }

    private void SetNoteBombPosition()
    {
        float yPos = GameObject.Find("JudgeLine1").GetComponent<SpriteRenderer>().sprite.bounds.size.y * GameObject.Find("JudgeLine1").transform.localScale.y * 0.5f;
        for (int i = 1; i < 6; i++)
        {
            GameObject tempObject = GameObject.Find($"NoteBomb{i}");
            tempObject.transform.localPosition = new Vector3(tempObject.transform.localPosition.x, yPos, tempObject.transform.localPosition.z);
        }
    }

    private void SetKeyFeedback()
    {
        for (int i = 0; i < 5; i++)
        {
            SpriteRenderer tempSpriteRenderer = keyFeedback[i].GetComponent<SpriteRenderer>();
            tempSpriteRenderer.color = 
                new Color(tempSpriteRenderer.color.r, tempSpriteRenderer.color.g, tempSpriteRenderer.color.b, PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        }
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

    public void KeyInputImageSetActive(bool active, int index)
    {
        keyFeedback[index].SetActive(active);
        keyboard[index].sprite = active ? keyPressedImage[index] : keyInitImage[index];
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

        if (combo >= maxCombo)
        {
            maxCombo = combo;
            maxComboText.text = str0000to9999Table[maxCombo];
        }

        switch (judge)
        {
            case JudgeType.KOOL:
                koolText.text = str0000to9999Table[res.koolCount];
                judgeEffectAnimator.SetTrigger(hashJudgeEffectKOOL);
                noteBombEffect[index].SetTrigger(hashNoteBombEffect);
                break;
            case JudgeType.COOL:
                coolText.text = str0000to9999Table[res.coolCount];
                judgeEffectAnimator.SetTrigger(hashJudgeEffectCOOL);
                noteBombEffect[index].SetTrigger(hashNoteBombEffect);
                break;
            case JudgeType.GOOD:
                goodText.text = str0000to9999Table[res.goodCount];
                judgeEffectAnimator.SetTrigger(hashJudgeEffectGOOD);
                break;
            case JudgeType.MISS:
                missText.text = str0000to9999Table[res.missCount];
                judgeEffectAnimator.SetTrigger(hashJudgeEffectMISS);
                break;
            case JudgeType.FAIL:
                failText.text = str0000to9999Table[res.failCount];
                judgeEffectAnimator.SetTrigger(hashJudgeEffectFAIL);
                break;
            case JudgeType.IGNORE:
                koolText.text = str0000to9999Table[res.koolCount];
                coolText.text = str0000to9999Table[res.coolCount];
                goodText.text = str0000to9999Table[res.goodCount];
                missText.text = str0000to9999Table[res.missCount];
                failText.text = str0000to9999Table[res.failCount];
                break;
        }
    }

    public void UpdateScore(BMSResult res, int currentCount, float hp, float accuracy, float score, float maxScore)
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

        double under60 = height60;
        double up60 = 0.0d;
        if (score <= 600000.0d) { under60 = score * divide20000; }
        else { up60 = (score - 600000.0d) * divide6250; }
        float scoreStickHeight = (float)(under60 + up60);
        scoreStick.SetSizeWithCurrentAnchors(vertical, scoreStickHeight);
        maxScoreStick.SetSizeWithCurrentAnchors(vertical, maxScore);

        res.scoreBarArray[currentCount] = scoreStickHeight;

        switch (score)
        {
            case float n when (n >= 0.0f && n < 550000.0f): res.rankIndex = 0; break;
            case float n when (n >= 550000.0f && n < 650000.0f): res.rankIndex = 1; break;
            case float n when (n >= 650000.0f && n < 750000.0f): res.rankIndex = 2; break;
            case float n when (n >= 750000.0f && n < 850000.0f): res.rankIndex = 3; break;
            case float n when (n >= 850000.0f && n < 900000.0f): res.rankIndex = 4; break;
            case float n when (n >= 900000.0f && n < 950000.0f): res.rankIndex = 5; break;
            case float n when (n >= 950000.0f && n < 1000000.0f): res.rankIndex = 6; break;
            case float n when (n >= 1000000.0f && n < 1025000.0f): res.rankIndex = 7; break;
            case float n when (n >= 1025000.0f && n < 1050000.0f): res.rankIndex = 8; break;
            case float n when (n >= 1050000.0f && n < 1090000.0f): res.rankIndex = 9; break;
            case float n when (n >= 1090000.0f): res.rankIndex = 10; break;
        }
        if (currentIdx != res.rankIndex)
        {
            rankImage.sprite = rank[res.rankIndex];
            rankImageTransform.localPosition = new Vector3(-244.0f, yPos[res.rankIndex], 0.0f);
            currentIdx = res.rankIndex;
        }
    }

    public void UpdateFSText(double diff, int idx)
    {
        int earlylate;
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

        earlyLateSprite[idx].sprite = earlyLateImageArray[earlylate];
        earlyLateEffectAnimator[idx].SetTrigger(hashEarlyLateEffect);
    }

    public void UpdateSongEndText(int koolCount, int coolCount, int goodCount, bool isActive)
    {
        earlyCountText.text = earlyCount.ToString();
        lateCountText.text = lateCount.ToString();
        koolCountText.text = koolCount.ToString();
        coolCountText.text = coolCount.ToString();
        goodCountText.text = goodCount.ToString();

        endInfo.SetActive(isActive);
    }
    public void FadeIn()
    {
        fadeinObject.SetActive(true);
        fadeinAnimator.SetTrigger("FadeIn");
    }
    public IEnumerator FadeOut()
    {
        fadeinAnimator.SetTrigger("FadeOut");
        yield return wait1sec;
        fadeinObject.SetActive(false);
    }

    public IEnumerator UpdateJudgeAdjValueText()
    {
        int value = PlayerPrefs.GetInt("DisplayDelayCorrection");
        judgeAdjValueText.text = "JudgeAdjustValue : " + (value > 0 ? "+" : "") + value.ToString() + "ms";
        judgeAdjValueText.gameObject.SetActive(true);
        yield return wait1sec;
        judgeAdjValueText.gameObject.SetActive(false);
    }

    public void UpdateInfoText()
    {
        levelText.text = BMSGameManager.header.level.ToString();
        SetRandomEffectorText(randomEffectorText, PlayerPrefs.GetInt("RandomEffector"));
    }

    private void SetRandomEffectorText(TextMeshProUGUI randomText, int index)
    {
        switch (index)
        {
            case 0: randomText.text = "NONE"; break;
            case 1: randomText.text = "RANDOM"; break;
            case 2: randomText.text = "MIRROR"; break;
            case 3: randomText.text = "F-RANDOM"; break;
            case 4: randomText.text = "MF-RANDOM"; break;
        }
    }

    public void UpdateSpeedText()
    {
        noteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
    }

    public void UpdateBPMText(double bpm)
    {
        bpmText.text = bpm.ToString();
    }

    public void SetLoading()
    {
        StartCoroutine(LoadRawImage(stageImage, BMSGameManager.header.musicFolderPath, BMSGameManager.header.stageFilePath, noStageImage));
        StartCoroutine(StageImageFade());
        loadingTitleText.text = BMSGameManager.header.title;
        loadingNoteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(loadingRandomEffetorText, PlayerPrefs.GetInt("RandomEffector"));
    }

    private IEnumerator StageImageFade()
    {
        float fadeValue = 0.1f;
        while (fadeValue < 0.9f)
        {
            fadeValue += 0.015f;
            yield return wait10ms;
            stageImage.color = new Color(1, 1, 1, fadeValue);
        }
    }

    public IEnumerator LoadRawImage(RawImage rawImage, string musicFolderPath, string path, Texture noImage)
    {
        if (string.IsNullOrEmpty(path)) { rawImage.texture = noImage; yield break; }

        string imagePath = $@"file:\\{musicFolderPath}{path}";

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

        rawImage.texture = (tex ?? noImage);
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
