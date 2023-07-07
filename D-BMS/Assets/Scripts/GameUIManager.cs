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
    private TextMeshProUGUI frontScoreText;
    [SerializeField]
    private TextMeshProUGUI backScoreText;
    [SerializeField]
    private TextMeshProUGUI currentComboText;
    [SerializeField]
    private GameObject currentComboObject;
    [SerializeField]
    private Animator comboTextAnimator;
    [SerializeField]
    private GameObject comboText;
    [SerializeField]
    private Animator comboAnimator;
    [SerializeField]
    private TextMeshProUGUI maxComboText;

    [SerializeField]
    private SpriteRenderer leftPanel;
    [SerializeField]
    private SpriteRenderer rightPanel;
    [SerializeField]
    private SpriteRenderer panelBackground;
    [SerializeField]
    private SpriteRenderer hpBarBackground;
    [SerializeField]
    private Transform hpBarMask;
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
    private Animator[] earlyLateAnimator;
    [SerializeField]
    private Animator judgeEffectAnimator;

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
    private TextMeshProUGUI loadingArtistText;
    [SerializeField]
    private TextMeshProUGUI loadingNoteSpeedText;
    [SerializeField]
    private TextMeshProUGUI loadingRandomEffetorText;
    [SerializeField]
    private TextMeshProUGUI loadingFaderText;
    [SerializeField]
    private TextMeshProUGUI sliderValueText;
    [SerializeField]
    private Slider loadingSlider;
    [SerializeField]
    private Image countdownCircle;
    [SerializeField]
    private TextMeshProUGUI countdownText;
    [SerializeField]
    private TextMeshProUGUI pausePanelNoteSpeedText;

    [SerializeField]
    private Animator fadeinAnimator;
    [SerializeField]
    private GameObject fadeinObject;

    [SerializeField]
    private Sprite[] judgeLineSprites;

    [SerializeField]
    private SpriteMask panelFade;

    [SerializeField]
    private BMSGameManager bmsGameManager;

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

    [HideInInspector] public string[] str0000to9999Table;
    private string[] str0to999Table;
    [HideInInspector] public string[] str00to100Table;
    private string[] str000to110Table;

    private Coroutine judgeAdjTextCoroutine;

    private void Awake()
    {
        SetGamePanel();
        SetJudgeLine();
        SetNoteBombPosition();
        SetKeyFeedback();

        loader = new BMPLoader();
        wait1sec = new WaitForSecondsRealtime(1.0f);
        wait10ms = new WaitForSecondsRealtime(0.01f);

        bgImageTable = new Dictionary<string, string>(500);
        bgSprites = new Dictionary<string, Texture2D>(500);
    }

    private void SetGamePanel()
    {
        float cameraSize = Camera.main.orthographicSize;
        leftPanel.transform.localPosition = new Vector3((-1.25f * 0.3f) - 7.63f, 2.5f, 0.0f);
        leftPanel.transform.localScale = new Vector3(1.0f, (cameraSize * 2.0f) / leftPanel.sprite.bounds.size.y, 1.0f);
        rightPanel.transform.localPosition = new Vector3((1.25f * 0.3f) - 4.592f, 2.5f, 0.0f);
        rightPanel.transform.localScale = new Vector3(1.0f, (cameraSize * 2.0f) / rightPanel.sprite.bounds.size.y, 1.0f);

        panelBackground.transform.localScale = new Vector3(3.8f / panelBackground.sprite.bounds.size.x,
                                                           (cameraSize + 2.74f) / panelBackground.sprite.bounds.size.y, 1.0f);

        hpBarBackground.transform.localPosition = new Vector3(rightPanel.transform.localPosition.x + rightPanel.sprite.bounds.size.x + hpBarBackground.sprite.bounds.size.x * 0.5f,
                                                              -0.24f + hpBarBackground.sprite.bounds.size.y * 0.5f, 0.0f);

        float fadeInSize = PlayerPrefs.GetFloat("FadeIn");
        if (fadeInSize == 0.0f)
        {
            panelFade.gameObject.SetActive(false);
        }
        else 
        {
            panelFade.transform.localPosition = new Vector3(-6.111f, 2.5f + cameraSize, 0.0f);
            panelFade.transform.localScale = new Vector3(3.8f / panelFade.sprite.bounds.size.x, 8.0f * fadeInSize, 1.0f);
        }

        float keyboardWidth = 0.76f / keyboard[0].sprite.bounds.size.x;
        float keyboardHeight = Mathf.Abs(2.74f - cameraSize) / keyboard[0].sprite.bounds.size.y;
        for (int i = 0; i < 5; i++) { keyboard[i].transform.localScale = new Vector3(keyboardWidth, keyboardHeight, 1.0f); }
    }

    private void SetJudgeLine()
    {
        int index = PlayerPrefs.GetInt("NoteSkin");
        float judgeLineYPosition = PlayerPrefs.GetInt("JudgeLine") == 0 ? 0.0f : -0.24f;
        for (int i = 1; i < 6; i++)
        {
            GameObject tempObject = GameObject.Find($"JudgeLine{i}");
            tempObject.GetComponent<SpriteRenderer>().sprite = judgeLineSprites[index];
            tempObject.transform.localPosition = new Vector3(tempObject.transform.localPosition.x, judgeLineYPosition, tempObject.transform.localPosition.z);
        }
    }

    private void SetNoteBombPosition()
    {
        float yPos = GameObject.Find("JudgeLine1").GetComponent<SpriteRenderer>().sprite.bounds.size.y * 
                     GameObject.Find("JudgeLine1").transform.localScale.y * 0.5f + (PlayerPrefs.GetInt("JudgeLine") == 0 ? 0.0f : -0.24f);
        for (int i = 1; i < 6; i++)
        {
            GameObject tempObject = GameObject.Find($"NoteBomb{i}");
            tempObject.transform.localPosition = new Vector3(tempObject.transform.localPosition.x, yPos, tempObject.transform.localPosition.z);
        }
    }

    private void SetKeyFeedback()
    {
        Color oddColor = new Color(PlayerPrefs.GetFloat("OddKeyFeedbackColorR"), PlayerPrefs.GetFloat("OddKeyFeedbackColorG"),
                                   PlayerPrefs.GetFloat("OddKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        Color evenColor = new Color(PlayerPrefs.GetFloat("EvenKeyFeedbackColorR"), PlayerPrefs.GetFloat("EvenKeyFeedbackColorG"),
                                    PlayerPrefs.GetFloat("EvenKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        for (int i = 0; i < 5; i++)
        {
            SpriteRenderer tempSpriteRenderer = keyFeedback[i].GetComponent<SpriteRenderer>();
            tempSpriteRenderer.color = i % 2 == 0 ? oddColor : evenColor;
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

    public void NoteBombActive(int index)
    {
        noteBombEffect[index].SetTrigger(hashNoteBombEffect);
    }

    public void GameUIUpdate(int combo, JudgeType judge)
    {
        if (combo != 0)
        {
            if (!currentComboObject.activeSelf)
            {
                currentComboObject.SetActive(true);
                comboText.SetActive(true);
            }
            currentComboText.text = (combo < 1000 ? str0to999Table[combo] : str0000to9999Table[combo]);
            comboTextAnimator.SetTrigger(hashComboText);
            comboAnimator.SetTrigger(hashCombo);
        }
        else
        {
            currentComboObject.SetActive(false);
            comboText.SetActive(false);
        }

        maxComboText.text = str0000to9999Table[BMSGameManager.bmsResult.maxCombo];

        hpBarMask.localScale = new Vector3(1.0f, bmsGameManager.gauge.hp, 1.0f);

        float score = (float)(bmsGameManager.currentScore);
        int frontSC = (int)(score * 0.0001d);
        int backSC = (int)(score - (frontSC * 10000));
        frontScoreText.text = str000to110Table[frontSC];
        backScoreText.text = str0000to9999Table[backSC];

        switch (judge)
        {
            case JudgeType.KOOL:
                judgeEffectAnimator.SetTrigger(hashJudgeEffectKOOL);
                break;
            case JudgeType.COOL:
                judgeEffectAnimator.SetTrigger(hashJudgeEffectCOOL);
                break;
            case JudgeType.GOOD:
                judgeEffectAnimator.SetTrigger(hashJudgeEffectGOOD);
                break;
            case JudgeType.MISS:
                judgeEffectAnimator.SetTrigger(hashJudgeEffectMISS);
                break;
            case JudgeType.FAIL:
                judgeEffectAnimator.SetTrigger(hashJudgeEffectFAIL);
                break;
        }
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

    public void CoUpdateJudgeAdjText()
    {
        if (judgeAdjTextCoroutine != null)
        {
            StopCoroutine(judgeAdjTextCoroutine);
            judgeAdjTextCoroutine = null;
        }
        judgeAdjTextCoroutine = StartCoroutine(UpdateJudgeAdjValueText());
    }

    private IEnumerator UpdateJudgeAdjValueText()
    {
        int value = PlayerPrefs.GetInt("DisplayDelayCorrection");
        judgeAdjValueText.text = "JudgeAdjustValue : " + (value > 0 ? "+" : "") + value.ToString() + "ms";
        judgeAdjValueText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.0f);
        judgeAdjValueText.gameObject.SetActive(false);
        judgeAdjTextCoroutine = null;
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
        //StartCoroutine(StageImageFade());
        loadingTitleText.text = BMSGameManager.header.title;
        loadingArtistText.text = BMSGameManager.header.artist;
        loadingNoteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(loadingRandomEffetorText, PlayerPrefs.GetInt("RandomEffector"));
        loadingFaderText.text = PlayerPrefs.GetFloat("FadeIn") == 0.0f ? "NONE" : $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";
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
        if (string.IsNullOrEmpty(path))
        {
            rawImage.texture = noImage;
            rawImage.color = new Color32(0, 0, 0, 230);
            yield break; 
        }

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

        if (tex == null)
        {
            rawImage.texture = noImage;
            rawImage.color = new Color32(0, 0, 0, 230);
        }
        else
        {
            rawImage.texture = tex;
            rawImage.color = new Color32(255, 255, 255, 255);
        }
    }

    public void SetLoadingSlider(float loadingValue)
    {
        loadingSlider.value = loadingValue;
        sliderValueText.text = ((int)(loadingValue * 100.0f)).ToString() + "%";
    }

    public void CloseLoading()
    {
        hpBarMask.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        loadingPanel.SetActive(false);
    }

    public void SetCountdown(float amount, int second)
    {
        countdownCircle.fillAmount = amount;
        countdownText.text = str0to999Table[second == 3 ? second : second + 1];
    }

    public void SetActiveCountdown(bool isActive)
    {
        countdownCircle.gameObject.SetActive(isActive);
        countdownText.gameObject.SetActive(isActive);
    }

    public void SetPausePanelNoteSpeedText()
    {
        pausePanelNoteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
    }

    public void AnimationPause(bool isPause)
    {
        float animationSpeed = isPause ? 0.0f : 1.0f;
        comboTextAnimator.speed = animationSpeed;
        comboAnimator.speed = animationSpeed;
        for (int i = noteBombEffect.Length - 1; i >= 0; i--)
        {
            noteBombEffect[i].speed = animationSpeed;
        }
        for (int i = earlyLateAnimator.Length - 1; i >= 0; i--)
        {
            earlyLateAnimator[i].speed = animationSpeed;
        }
        judgeEffectAnimator.speed = animationSpeed;
    }
}
