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
    private GameObject comboTitle;
    [SerializeField]
    private Transform comboParentTransform;
    [SerializeField]
    private Sprite[] comboNumberArray;
    [SerializeField]
    private SpriteRenderer[] comboDigitArray;
    [SerializeField]
    private Animator comboTitleAnimator;
    [SerializeField]
    private Animator comboTitleBounceAnimator;
    [SerializeField]
    private Animator comboBounceAnimator;
    [SerializeField]
    private Animator[] comboAnimatorArray;
    private float[] comboPositionX;

    public Sprite[] defaultNumberArray;
    [SerializeField]
    private SpriteRenderer[] scoreDigitArray;
    [SerializeField]
    private SpriteRenderer[] maxcomboDigitArray;

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
    private SpriteRenderer[] noteBombArray;
    [SerializeField]
    private Sprite[] noteBombSpriteArray;
    private int[] noteBombAnimationIndex;
    private int noteBombSpriteArrayLength;
    private WaitUntil[] noteBombWaitUntilArray;
    private WaitForSeconds[] noteBombWaitSecondsArray;

    [SerializeField]
    private Animator judgeEffectAnimator;
    [SerializeField]
    private SpriteRenderer judgeSpriteRenderer;
    [SerializeField]
    private Sprite[] koolSprite;
    [SerializeField]
    private Sprite[] coolSprite;
    [SerializeField]
    private Sprite[] goodSprite;
    [SerializeField]
    private Sprite[] missSprite;
    [SerializeField]
    private Sprite[] failSprite;
    private Sprite[,] judgeSpriteArray;
    private int currentJudge;
    private int judgeEffectIndex; 
    private WaitUntil judgeEffectWaitUntil;
    private WaitForSeconds judgeEffectWaitSeconds;

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
    private GameObject countdownObject;
    [SerializeField]
    private SpriteRenderer countdownBar;
    [SerializeField]
    private SpriteRenderer countdownTimeText;
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

    private readonly int hashComboTitle = Animator.StringToHash("ComboTitle"); 
    private readonly int hashComboTitleBounce = Animator.StringToHash("ComboTitleBounce");
    private readonly int hashComboBounce = Animator.StringToHash("ComboBounce");
    private readonly int hashCombo = Animator.StringToHash("Combo");
    private readonly int hashJudgeEffect = Animator.StringToHash("JudgeEffect");

    private Coroutine judgeAdjTextCoroutine;

    private void Awake()
    {
        loader = new BMPLoader();
        wait1sec = new WaitForSecondsRealtime(1.0f);
        wait10ms = new WaitForSecondsRealtime(0.01f);

        noteBombSpriteArrayLength = noteBombSpriteArray.Length;
        noteBombAnimationIndex = new int[5];
        noteBombWaitUntilArray = new WaitUntil[5];
        noteBombWaitSecondsArray = new WaitForSeconds[5];
        for (int i = 0; i < 5; i++)
        {
            NoteBombAnimationSet(i);
        }

        judgeSpriteArray = new Sprite[5, 15];
        for (int i = 0; i < 15; i++)
        {
            judgeSpriteArray[4, i] = koolSprite[i % koolSprite.Length];
            judgeSpriteArray[3, i] = coolSprite[i % coolSprite.Length];
            judgeSpriteArray[2, i] = goodSprite[i % goodSprite.Length];
            judgeSpriteArray[1, i] = missSprite[i % missSprite.Length];
            judgeSpriteArray[0, i] = failSprite[i % failSprite.Length];
        }
        currentJudge = -1;
        judgeEffectIndex = 15;
        judgeEffectWaitUntil = new WaitUntil(() => judgeEffectIndex == 0);
        judgeEffectWaitSeconds = new WaitForSeconds(1.0f / 30.0f);
        StartCoroutine(JudgeEffect());

        bgImageTable = new Dictionary<string, string>(500);
        bgSprites = new Dictionary<string, Texture2D>(500);

        PopulateAtlasInfo();
    }

    void PopulateAtlasInfo()
    {
        Rect sprite = countdownBar.sprite.textureRect;

        Vector4 spriteData = new Vector4(
            sprite.x / countdownBar.sprite.texture.width,
            sprite.y / countdownBar.sprite.texture.height,
            sprite.width / countdownBar.sprite.texture.width,
            sprite.height / countdownBar.sprite.texture.height
            );

        countdownBar.material.SetVector("_SpriteData", spriteData);
    }

    private void NoteBombAnimationSet(int line)
    {
        noteBombAnimationIndex[line] = noteBombSpriteArrayLength;
        noteBombWaitUntilArray[line] = new WaitUntil(() => noteBombAnimationIndex[line] == 0);
        noteBombWaitSecondsArray[line] = new WaitForSeconds(1.0f / 60.0f);
        StartCoroutine(NoteBombEffect(line));
    }

    public void SetGamePanel()
    {
        float cameraSize = Camera.main.orthographicSize;
        float noteWidth = ObjectPool.poolInstance.GetNoteWidth();
        leftPanel.transform.localPosition = new Vector3(bmsGameManager.xPosition[0] - noteWidth * 0.5f, 2.5f, 0.0f);
        leftPanel.transform.localScale = new Vector3(1.0f, (cameraSize * 2.0f) / leftPanel.sprite.bounds.size.y, 1.0f);
        rightPanel.transform.localPosition = new Vector3(bmsGameManager.xPosition[4] + noteWidth * 0.5f, 2.5f, 0.0f);
        rightPanel.transform.localScale = new Vector3(1.0f, (cameraSize * 2.0f) / rightPanel.sprite.bounds.size.y, 1.0f);

        panelBackground.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], -0.24f, 0.0f);
        panelBackground.transform.localScale = new Vector3(noteWidth * 5.0f / panelBackground.sprite.bounds.size.x,
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
            panelFade.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], 2.5f + cameraSize, 0.0f);
            panelFade.transform.localScale = new Vector3(noteWidth * 5.0f / panelFade.sprite.bounds.size.x, 8.0f * fadeInSize, 1.0f);
        }

        judgeSpriteRenderer.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], 1.4f, 0.0f);

        float keyboardWidth = noteWidth / keyboard[0].sprite.bounds.size.x;
        float keyboardHeight = Mathf.Abs(2.74f - cameraSize) / keyboard[0].sprite.bounds.size.y;
        for (int i = 0; i < 5; i++) 
        {
            keyboard[i].transform.localPosition = new Vector3(bmsGameManager.xPosition[i], -0.24f, 0.0f);
            keyboard[i].transform.localScale = new Vector3(keyboardWidth, keyboardHeight, 1.0f);
        }

        countdownObject.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], 2.2f, 0.0f);
    }

    public void SetJudgeLine()
    {
        int index = PlayerPrefs.GetInt("NoteSkin");
        float judgeLineYPosition = PlayerPrefs.GetInt("JudgeLine") == 0 ? 0.0f : -0.24f;
        for (int i = 1; i < 6; i++)
        {
            GameObject tempObject = GameObject.Find($"JudgeLine{i}");
            tempObject.GetComponent<SpriteRenderer>().sprite = judgeLineSprites[index];
            tempObject.transform.localPosition = new Vector3(bmsGameManager.xPosition[i - 1], judgeLineYPosition, 0.0f);
        }
    }

    public void SetNoteBombPosition()
    {
        float yPos = GameObject.Find("JudgeLine1").GetComponent<SpriteRenderer>().sprite.bounds.size.y * 
                     GameObject.Find("JudgeLine1").transform.localScale.y * 0.5f + (PlayerPrefs.GetInt("JudgeLine") == 0 ? 0.0f : -0.24f);
        for (int i = 1; i < 6; i++)
        {
            GameObject tempObject = GameObject.Find($"NoteBomb{i}");
            tempObject.transform.localPosition = new Vector3(bmsGameManager.xPosition[i - 1], yPos, 0.0f);
        }
    }

    public void SetKeyFeedback()
    {
        Color oddColor = new Color(PlayerPrefs.GetFloat("OddKeyFeedbackColorR"), PlayerPrefs.GetFloat("OddKeyFeedbackColorG"),
                                   PlayerPrefs.GetFloat("OddKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        Color evenColor = new Color(PlayerPrefs.GetFloat("EvenKeyFeedbackColorR"), PlayerPrefs.GetFloat("EvenKeyFeedbackColorG"),
                                    PlayerPrefs.GetFloat("EvenKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        float xScale = ObjectPool.poolInstance.GetNoteWidth() / keyFeedback[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float yScale = (Camera.main.orthographicSize + 2.74f) / keyFeedback[0].GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        for (int i = 0; i < 5; i++)
        {
            SpriteRenderer tempSpriteRenderer = keyFeedback[i].GetComponent<SpriteRenderer>();
            tempSpriteRenderer.color = i % 2 == 0 ? oddColor : evenColor;
            keyFeedback[i].transform.localPosition = new Vector3(bmsGameManager.xPosition[i], -0.24f, 0.0f);
            keyFeedback[i].transform.localScale = new Vector3(xScale, yScale, 1.0f);
        }
    }

    public void SetCombo()
    {
        comboParentTransform.localPosition = new Vector3(bmsGameManager.xPosition[2], 5.15f, 0.0f);
        float comboNumberSize = comboNumberArray[0].bounds.size.x * comboDigitArray[0].transform.localScale.x;
        comboPositionX = new float[5];
        for (int i = 0; i < 5; i++)
        {
            comboDigitArray[i].transform.localPosition = new Vector3((2 - i) * comboNumberSize, 0.0f, 0.0f);
            comboPositionX[i] = bmsGameManager.xPosition[2] - ((4 - i) * comboNumberSize * 0.5f);
        }
        comboTitle.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], 5.84f, 0.0f);
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
        noteBombAnimationIndex[index] = 0;
    }

    public void GameUIUpdate(int combo, JudgeType judge)
    {
        if (combo != 0)
        {
            if (!comboTitle.activeSelf)
            {
                comboTitle.SetActive(true);
            }
            int digitCount = 0;
            while (combo > 0)
            {
                int tempValue = (int)(combo * 0.1f);
                int remainder = combo - (tempValue * 10);
                comboDigitArray[digitCount].sprite = comboNumberArray[remainder];
                comboAnimatorArray[digitCount++].SetTrigger(hashCombo);
                combo = tempValue;
            }
            comboTitleAnimator.SetTrigger(hashComboTitle);
            comboTitleBounceAnimator.SetTrigger(hashComboTitleBounce);
            comboBounceAnimator.SetTrigger(hashComboBounce);
            comboParentTransform.localPosition = new Vector3(comboPositionX[digitCount - 1], 5.15f, 0.0f);
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                comboDigitArray[i].sprite = null;
            }
            comboTitle.SetActive(false);
        }

        hpBarMask.localScale = new Vector3(1.0f, bmsGameManager.gauge.hp, 1.0f);

        int maxcombo = BMSGameManager.bmsResult.maxCombo;
        for (int i = 0; i < 5; i++)
        {
            int tempValue = (int)(maxcombo * 0.1f);
            int remainder = maxcombo - (tempValue * 10);
            maxcomboDigitArray[i].sprite = defaultNumberArray[remainder];
            maxcombo = tempValue;
        }

        int score = (int)(float)bmsGameManager.currentScore;
        for (int i = 0; i < 7; i++)
        {
            int tempValue = (int)(score * 0.1f);
            int remainder = score - (tempValue * 10);
            scoreDigitArray[i].sprite = defaultNumberArray[remainder];
            score = tempValue;
        }

        if (judge != JudgeType.IGNORE)
        {
            currentJudge = (int)judge - 1;
            judgeEffectIndex = 0;
            judgeEffectAnimator.SetTrigger(hashJudgeEffect);
        }
    }

    private IEnumerator NoteBombEffect(int line)
    {
        while (true)
        {
            yield return noteBombWaitUntilArray[line];
            while (noteBombAnimationIndex[line] < noteBombSpriteArrayLength)
            {
                noteBombArray[line].sprite = noteBombSpriteArray[noteBombAnimationIndex[line]++];
                yield return noteBombWaitSecondsArray[line];
            }
            noteBombArray[line].sprite = null;
        }
    }

    private IEnumerator JudgeEffect()
    {
        while (true)
        {
            yield return judgeEffectWaitUntil;
            while (judgeEffectIndex < 15)
            {
                judgeSpriteRenderer.sprite = judgeSpriteArray[currentJudge, judgeEffectIndex++];
                yield return judgeEffectWaitSeconds;
            }
            judgeSpriteRenderer.sprite = null;
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
        countdownBar.material.SetInt("_Arc2", (int)((1.0f - amount) * 360.0f));
        countdownTimeText.sprite = comboNumberArray[second == 3 ? second : second + 1];
    }

    public void SetActiveCountdown(bool isActive)
    {
        countdownObject.SetActive(isActive);
    }

    public void SetPausePanelNoteSpeedText()
    {
        pausePanelNoteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
    }

    public void AnimationPause(bool isPause)
    {
        Time.timeScale = isPause ? 0.0f : 1.0f;
    }
}
