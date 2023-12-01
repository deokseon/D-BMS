using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using B83.Image.BMP;
using DaVikingCode.AssetPacker;
using System.IO;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class GameUIManager : MonoBehaviour
{
    [HideInInspector] public int isPrepared { get; set; } = 0;
    public static bool isCreateReady = false;
    public RawImage bga;
    public RawImage layer;
    public Image bgaOpacity;
    [SerializeField]
    private Texture2D transparentTexture;
    [HideInInspector] public int bgSpriteArrayLength;
    public HashSet<int> layerImageSet { get; set; }
    public List<KeyValuePair<int, string>> bgImageList;
    private Texture2D[] bgSpriteArray;
    [HideInInspector] public int taskCount;

    private BMSDrawer bmsDrawer = null;
    private BMSGameManager bmsGameManager = null;

    [SerializeField]
    private GameObject comboTitle;
    [SerializeField]
    private Transform comboParentTransform;
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

    [HideInInspector]
    public Sprite[] defaultNumberArray;
    [SerializeField]
    private SpriteRenderer[] scoreDigitArray;
    [SerializeField]
    private SpriteRenderer[] maxcomboDigitArray;

    [SerializeField]
    private Transform hpBarMask;
    [SerializeField]
    private GameObject[] keyFeedback;
    [SerializeField]
    private SpriteRenderer[] keyboard;
    private Sprite[] keyInitImage;
    private Sprite[] keyPressedImage;

    private TimeSpan effectWaitSecond = TimeSpan.FromSeconds(1.0d / 60.0d);

    [SerializeField]
    private SpriteRenderer[] noteBombArray;
    private Sprite[][] noteBombSpriteArray;
    private int[] noteBombState;
    private int[] noteBombAnimationIndex;
    private int[] noteBombSpriteArrayLength;

    [SerializeField]
    private Animator judgeEffectAnimator;
    [SerializeField]
    private SpriteRenderer judgeSpriteRenderer;
    private Sprite[,] judgeSpriteArray;
    private int currentJudge;
    private int judgeEffectIndex; 

    [SerializeField]
    private GameObject infoPanel;
    [SerializeField]
    private TextMeshProUGUI noteSpeedText;
    [SerializeField]
    private TextMeshProUGUI bgaOpacityText;

    [SerializeField]
    private Texture noStageImage;
    [SerializeField]
    private GameObject countdownObject;

    public GameObject fadeObject;

    private BMPLoader loader;

    private readonly int hashComboTitle = Animator.StringToHash("ComboTitle"); 
    private readonly int hashComboTitleBounce = Animator.StringToHash("ComboTitleBounce");
    private readonly int hashComboBounce = Animator.StringToHash("ComboBounce");
    private readonly int hashCombo = Animator.StringToHash("Combo");
    private readonly int hashJudgeEffect = Animator.StringToHash("JudgeEffect");

    private Coroutine infoPanelCoroutine;

    [HideInInspector]
    public AssetPacker assetPacker;

    public static ConfigData config;

    private void Awake()
    {
        assetPacker = GetComponent<AssetPacker>();
        bmsGameManager = FindObjectOfType<BMSGameManager>();

        assetPacker.AddTexturesToPack(Directory.GetFiles($@"{Directory.GetParent(Application.dataPath)}\Skin\GameObject"));
        assetPacker.Process();
        assetPacker.OnProcessCompleted.AddListener(SpriteSetting);

        config = config ?? DataSaveManager.LoadData<ConfigData>("Skin", "config.json") ?? new ConfigData();

        if (bmsGameManager == null) 
        {
            isCreateReady = true;
            return;
        }

        bmsDrawer = FindObjectOfType<BMSDrawer>();

        loader = new BMPLoader();
        bgImageList = new List<KeyValuePair<int, string>>(500);
        layerImageSet = new HashSet<int>();
        taskCount = Mathf.Max((int)(SystemInfo.processorCount * 0.5f) - 2, 1);
    }

    private void SpriteSetting()
    {
        StartCoroutine(CoSpriteSetting());
    }

    private IEnumerator CoSpriteSetting()
    {
        ObjectPool.poolInstance.SetComponent();
        ObjectPool.poolInstance.SetVerticalLineSprite();
        ObjectPool.poolInstance.SetNoteSprite();
        ObjectPool.poolInstance.SetVerticalLine();

        SetBGAPanel();
        SetCombo();
        SetGamePanel();
        SetJudgeLine();
        SetNoteBomb();
        SetJudgeEffect();
        SetKeyFeedback();
        SetCountDown();
        SetInputBlock();
        SetScoreAndMaxcombo();

        if (bmsGameManager != null)
        {
            bmsGameManager.xPosition = new float[5];
            float noteWidth = ObjectPool.poolInstance.GetLineWidth();
            for (int i = 0; i < 5; i++)
            {
                bmsGameManager.xPosition[i] = config.panelPosition + noteWidth * i;
            }

            bmsGameManager.longNoteOffset = ObjectPool.poolInstance.GetOffset();
            bmsGameManager.longNoteLength = ObjectPool.poolInstance.GetLength();
            bmsDrawer.DrawNotes(bmsGameManager.xPosition);

            isPrepared++;
        }
        else
        {
            FindObjectOfType<EarlyLate>().ObjectSetting();
            GameUIUpdate(0, JudgeType.IGNORE, 1.0f, 789, 123456);
            yield return new WaitForSeconds(0.2f);
            CustomSetting();
            StartCoroutine(FadeOut());
        }
    }

    public void CustomSetting()
    {
        comboTitle.SetActive(true);
        comboParentTransform.localPosition = new Vector3(comboPositionX[2], config.comboPosition, 0.0f);
        comboDigitArray[0].sprite = comboNumberArray[3];
        comboDigitArray[1].sprite = comboNumberArray[6];
        comboDigitArray[2].sprite = comboNumberArray[9];
        judgeSpriteRenderer.sprite = judgeSpriteArray[4, 0];
        FindObjectOfType<EarlyLate>().CustomSetting();
        FindObjectOfType<CustomManager>().isPrepared = true;
    }

    public void SetBGAPanel()
    {
        bga.rectTransform.localPosition = new Vector3(config.bgaPositionX, config.bgaPositionY);
        bga.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, config.bgaHeight);
        bga.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, config.bgaWidth);

        layer.rectTransform.localPosition = new Vector3(config.bgaPositionX, config.bgaPositionY);
        layer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, config.bgaHeight);
        layer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, config.bgaWidth);

        bgaOpacity.rectTransform.localPosition = new Vector3(config.bgaPositionX, config.bgaPositionY);
        bgaOpacity.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, config.bgaHeight);
        bgaOpacity.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, config.bgaWidth);
    }

    public float GetXPosition(int index)
    {
        return ObjectPool.poolInstance.GetLineWidth() * index + config.panelPosition;
    }

    private void SetCountDown()
    {
        countdownObject.SetActive(true);
        Sprite countDownCircle = assetPacker.GetSprite("countdowncircle");
        SpriteRenderer countdownBar = GameObject.Find("CountDown_Bar").GetComponent<SpriteRenderer>();
        countdownBar.sprite = countDownCircle;
        Rect spriteRect = countDownCircle.textureRect;

        Vector4 spriteData = new Vector4(
            spriteRect.x / countDownCircle.texture.width,
            spriteRect.y / countDownCircle.texture.height,
            spriteRect.width / countDownCircle.texture.width,
            spriteRect.height / countDownCircle.texture.height
            );

        countdownBar.material.SetVector("_SpriteData", spriteData);
        countdownObject.SetActive(false);
    }

    private void SetInputBlock()
    {
        Sprite inputBlockSprite = assetPacker.GetSprite("inputblock");
        GameObject inputBlockLine = GameObject.Find("InputBlockLine");
        inputBlockLine.GetComponent<SpriteRenderer>().sprite = inputBlockSprite;
        inputBlockLine.transform.localScale = new Vector3(ObjectPool.poolInstance.GetLineWidth() * 5 / inputBlockSprite.bounds.size.x, 0.75f, 1.0f);
    }

    private void NoteBombAnimationSet(int line)
    {
        noteBombAnimationIndex[line] = noteBombSpriteArrayLength[0];
        _ = NoteBombEffect(line);
    }

    public void SetGamePanel()
    {
        float cameraSize = Camera.main.orthographicSize;
        float noteWidth = ObjectPool.poolInstance.GetLineWidth();

        Sprite panelLeftSprite = assetPacker.GetSprite("panel-left");
        GameObject leftPanel = GameObject.Find("Panel_Left");
        leftPanel.GetComponent<SpriteRenderer>().sprite = panelLeftSprite;
        leftPanel.transform.localPosition = new Vector3(GetXPosition(0) - noteWidth * 0.5f, 2.5f, 0.0f);
        leftPanel.transform.localScale = new Vector3(1.0f, (cameraSize * 2.0f) / panelLeftSprite.bounds.size.y, 1.0f);

        Sprite panelRightSprite = assetPacker.GetSprite("panel-right");
        GameObject rightPanel = GameObject.Find("Panel_Right");
        rightPanel.GetComponent<SpriteRenderer>().sprite = panelRightSprite;
        rightPanel.transform.localPosition = new Vector3(GetXPosition(4) + noteWidth * 0.5f, 2.5f, 0.0f);
        rightPanel.transform.localScale = new Vector3(1.0f, (cameraSize * 2.0f) / panelRightSprite.bounds.size.y, 1.0f);

        Sprite panelBGSprite = assetPacker.GetSprite("panel-bg");
        GameObject panelBackground = GameObject.Find("Panel_BG");
        panelBackground.GetComponent<SpriteRenderer>().sprite = panelBGSprite;
        panelBackground.transform.localPosition = new Vector3(GetXPosition(2), config.judgeLinePosition - 0.24f, 0.0f);
        panelBackground.transform.localScale = new Vector3(noteWidth * 5.0f / panelBGSprite.bounds.size.x, (cameraSize + 2.74f - config.judgeLinePosition) / panelBGSprite.bounds.size.y, 1.0f);

        Sprite hpBarBGSprite = assetPacker.GetSprite("hpbar-bg");
        GameObject hpBarBackground = GameObject.Find("HPbar_bg");
        hpBarBackground.GetComponent<SpriteRenderer>().sprite = hpBarBGSprite;
        hpBarBackground.transform.localPosition = new Vector3(GetXPosition(4) + noteWidth * 0.5f + panelRightSprite.bounds.size.x + hpBarBGSprite.bounds.size.x * 0.5f,
                                                      -0.24f + hpBarBGSprite.bounds.size.y * 0.5f, 0.0f);

        GameObject hpBar = GameObject.Find("HPbar");
        hpBar.GetComponent<SpriteRenderer>().sprite = assetPacker.GetSprite("hpbar");
        hpBarMask.GetComponent<SpriteMask>().sprite = assetPacker.GetSprite("hpbar");

        keyInitImage = new Sprite[5];
        keyPressedImage = new Sprite[5];
        float keyboardWidth = noteWidth / assetPacker.GetSprite("key1-init").bounds.size.x;
        float keyboardHeight = (config.judgeLinePosition + 2.26f) / assetPacker.GetSprite("key1-init").bounds.size.y;
        for (int i = 0; i < 5; i++) 
        {
            keyInitImage[i] = assetPacker.GetSprite($"key{(i % 2) + 1}-init");
            keyPressedImage[i] = assetPacker.GetSprite($"key{(i % 2) + 1}-pressed");
            keyboard[i].sprite = keyInitImage[i];
            keyboard[i].transform.localPosition = new Vector3(GetXPosition(i), config.judgeLinePosition - 0.24f, 0.0f);
            keyboard[i].transform.localScale = new Vector3(keyboardWidth, keyboardHeight, 1.0f);
        }

        countdownObject.transform.localPosition = new Vector3(GetXPosition(2), 2.2f, 0.0f);

        GameObject panelFade = GameObject.Find("Panel_Fade");
        if (panelFade == null) { return; }
        float fadeInSize = PlayerPrefs.GetFloat("FadeIn");
        if (fadeInSize == 0.0f)
        {
            panelFade.gameObject.SetActive(false);
        }
        else
        {
            panelFade.GetComponent<SpriteMask>().sprite = panelBGSprite;
            panelFade.transform.localPosition = new Vector3(GetXPosition(2), 2.5f + cameraSize, 0.0f);
            panelFade.transform.localScale = new Vector3(noteWidth * 5.0f / panelBGSprite.bounds.size.x, 4.0f / panelBGSprite.bounds.size.y * fadeInSize, 1.0f);
        }
    }

    public void SetJudgeLine()
    {
        float judgeLineYPosition = PlayerPrefs.GetInt("JudgeLine") == 0 ? config.judgeLinePosition : config.judgeLinePosition - 0.24f;
        Sprite judgelineSprite = assetPacker.GetSprite("judgeline");
        float judgelinesize = ObjectPool.poolInstance.GetLineWidth() / judgelineSprite.bounds.size.x;
        for (int i = 1; i < 6; i++)
        {
            GameObject tempObject = GameObject.Find($"JudgeLine{i}");
            tempObject.GetComponent<SpriteRenderer>().sprite = judgelineSprite;
            tempObject.transform.localPosition = new Vector3(GetXPosition(i - 1), judgeLineYPosition, 0.0f);
            tempObject.transform.localScale = new Vector3(judgelinesize, judgelinesize, 1.0f);
        }
    }

    private void SetJudgeEffect()
    {
        Sprite[] koolSprite = assetPacker.GetSprites("kool-");
        Sprite[] coolSprite = assetPacker.GetSprites("cool-");
        Sprite[] goodSprite = assetPacker.GetSprites("good-");
        Sprite[] missSprite = assetPacker.GetSprites("miss-");
        Sprite[] failSprite = assetPacker.GetSprites("fail-");
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
        _ = JudgeEffect();

        SetJudgePosition();
    }

    public void SetJudgePosition()
    {
        judgeSpriteRenderer.transform.localPosition = new Vector3(GetXPosition(2), config.judgePosition, 0.0f);
    }

    private void SetNoteBomb()
    {
        noteBombSpriteArray = new Sprite[2][] { assetPacker.GetSprites("notebombN-"), assetPacker.GetSprites("notebombL-") };
        noteBombSpriteArrayLength = new int[2] { noteBombSpriteArray[0].Length, noteBombSpriteArray[1].Length };

        noteBombState = new int[5] { 0, 0, 0, 0, 0 };
        noteBombAnimationIndex = new int[5];
        for (int i = 0; i < 5; i++)
        {
            NoteBombAnimationSet(i);
        }

        SetNoteBombScale();
        SetNoteBombPosition();
    }
    
    public void SetNoteBombScale()
    {
        for (int i = 1; i < 6; i++)
        {
            GameObject.Find($"NoteBomb{i}").transform.localScale = new Vector3(config.noteBombScale, config.noteBombScale, 1.0f);
        }
    }

    public void SetNoteBombPosition()
    {
        float yPos = GameObject.Find("JudgeLine1").GetComponent<SpriteRenderer>().sprite.bounds.size.y *
                     GameObject.Find("JudgeLine1").transform.localScale.y * 0.5f + (PlayerPrefs.GetInt("JudgeLine") == 0 ? config.judgeLinePosition : config.judgeLinePosition - 0.24f);
        for (int i = 1; i < 6; i++)
        {
            GameObject.Find($"NoteBomb{i}").transform.localPosition = new Vector3(GetXPosition(i - 1), yPos, 0.0f);
        }
    }

    public void SetKeyFeedback()
    {
        Sprite keyfeedbackSprite = assetPacker.GetSprite("keyfeedback");
        Color oddColor = new Color(PlayerPrefs.GetFloat("OddKeyFeedbackColorR"), PlayerPrefs.GetFloat("OddKeyFeedbackColorG"),
                                   PlayerPrefs.GetFloat("OddKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        Color evenColor = new Color(PlayerPrefs.GetFloat("EvenKeyFeedbackColorR"), PlayerPrefs.GetFloat("EvenKeyFeedbackColorG"),
                                    PlayerPrefs.GetFloat("EvenKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        float xScale = ObjectPool.poolInstance.GetLineWidth() / keyfeedbackSprite.bounds.size.x;
        float yScale = (7.74f - config.judgeLinePosition) / keyfeedbackSprite.bounds.size.y;
        for (int i = 0; i < 5; i++)
        {
            keyFeedback[i].GetComponent<SpriteRenderer>().color = i % 2 == 0 ? oddColor : evenColor;
            keyFeedback[i].GetComponent<SpriteRenderer>().sprite = keyfeedbackSprite;
            keyFeedback[i].transform.localPosition = new Vector3(GetXPosition(i), config.judgeLinePosition - 0.24f, 0.0f);
            keyFeedback[i].transform.localScale = new Vector3(xScale, yScale, 1.0f);
        }
    }

    public void SetCombo()
    {
        comboTitle.GetComponent<SpriteRenderer>().sprite = assetPacker.GetSprite("text-combo");
        comboNumberArray = assetPacker.GetSprites("combo-");

        comboParentTransform.localPosition = new Vector3(GetXPosition(2), config.comboPosition, 0.0f);
        float comboNumberSize = comboNumberArray[0].bounds.size.x * comboDigitArray[0].transform.localScale.x;
        comboPositionX = new float[5];
        for (int i = 0; i < 5; i++)
        {
            comboDigitArray[i].transform.localPosition = new Vector3((2 - i) * comboNumberSize, 0.0f, 0.0f);
            comboPositionX[i] = GetXPosition(2) - ((4 - i) * comboNumberSize * 0.5f);
        }

        float comboTitleYPos = 0.085f + config.comboPosition + comboNumberArray[0].bounds.size.y * comboDigitArray[0].transform.localScale.y * 0.5f +
                                comboTitle.GetComponent<SpriteRenderer>().sprite.bounds.size.y * comboTitle.transform.localScale.y * 0.5f;
        comboTitle.transform.localPosition = new Vector3(GetXPosition(2), comboTitleYPos, 0.0f);
    }

    public void SetScoreAndMaxcombo()
    {
        defaultNumberArray = assetPacker.GetSprites("default-");
        GameObject scoreTitle = GameObject.Find("Score_Title");
        scoreTitle.GetComponent<SpriteRenderer>().sprite = assetPacker.GetSprite("text-score");
        scoreTitle.transform.localPosition = new Vector3(config.scoreImagePositionX, config.scoreImagePositionY, 0.0f);

        GameObject maxcomboTitle = GameObject.Find("Maxcombo_Title");
        maxcomboTitle.GetComponent<SpriteRenderer>().sprite = assetPacker.GetSprite("text-maxcombo");
        maxcomboTitle.transform.localPosition = new Vector3(config.maxcomboImagePositionX, config.maxcomboImagePositionY, 0.0f);

        GameObject.Find("ScoreParent").transform.localPosition = new Vector3(GetXPosition(2), 0.0f, 0.0f);
        GameObject.Find("MaxcomboParent").transform.localPosition = new Vector3(GetXPosition(2), 0.0f, 0.0f);

        for (int i = 0; i < 7; i++)
        {
            scoreDigitArray[i].transform.localPosition = new Vector3(config.scoreDigitPositionX - i * defaultNumberArray[0].bounds.size.x * scoreDigitArray[i].transform.localScale.x, config.scoreDigitPositionY, 0.0f);
        }
        for (int i = 0; i < 5; i++)
        {
            maxcomboDigitArray[i].transform.localPosition = new Vector3(config.maxcomboDigitPositionX - i * defaultNumberArray[0].bounds.size.x * maxcomboDigitArray[i].transform.localScale.x, config.maxcomboDigitPositionY, 0.0f);
        }
    }

    public void LoadImages()
    {
        bgSpriteArray = new Texture2D[bgSpriteArrayLength + 1];
        for (int i = 0; i < taskCount; i++)
        {
            _ = LoadImageTask(i);
        }
    }

    private async UniTask LoadImageTask(int value)
    {
        var token = this.GetCancellationTokenOnDestroy();
        int start = (int)(value / (double)taskCount * bgImageList.Count);
        int end = (int)((value + 1) / (double)taskCount * bgImageList.Count);
        for (int i = start; i < end; i++)
        {
            string path = BMSGameManager.header.musicFolderPath + bgImageList[i].Value.Substring(0, bgImageList[i].Value.Length - 4);

            Texture2D texture2D = null;
            if (File.Exists(path + ".bmp"))
            {
                var uwr = await UnityWebRequest.Get(@"file:\\" + path + ".bmp").SendWebRequest().WithCancellation(cancellationToken: token);
                var uwrData = uwr.downloadHandler.data;
                var bmpImage = await UniTask.RunOnThreadPool(() => loader.LoadBMP(uwrData));
                texture2D = bmpImage.ToTexture2D();
            }
            else if (File.Exists(path + ".jpg"))
            {
                var uwr = await UnityWebRequestTexture.GetTexture(@"file:\\" + path + ".jpg").SendWebRequest().WithCancellation(cancellationToken: token);
                texture2D = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            }
            else if (File.Exists(path + ".png"))
            {
                var uwr = await UnityWebRequestTexture.GetTexture(@"file:\\" + path + ".png").SendWebRequest().WithCancellation(cancellationToken: token);
                texture2D = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            }

            if (texture2D != null && layerImageSet.Contains(bgImageList[i].Key))
            {
                var colors = texture2D.GetPixels32();

                for (int k = colors.Length - 1; k >= 0; k--)
                {
                    if (colors[k].a != 0 && colors[k].r + colors[k].g + colors[k].b == 0) colors[k].a = 0;
                }

                texture2D.SetPixels32(colors);
                texture2D.Apply();
            }
            bgSpriteArray[bgImageList[i].Key] = texture2D;
            lock (bmsGameManager.threadLock)
            {
                BMSGameManager.currentLoading++;
            }
        }
        isPrepared++;
    }

    public void SetNullBGArray()
    {
        for (int i = 0; i < bgSpriteArray.Length; i++)
        {
            if (bgSpriteArray[i] == null) bgSpriteArray[i] = transparentTexture;
        }
    }

    public void ChangeBGA(int key)
    {
        bga.texture = bgSpriteArray[key];
    }

    public void ChangeLayer(int key)
    {
        layer.texture = bgSpriteArray[key];
    }

    public void InitBGALayer()
    {
        bga.texture = transparentTexture;
        layer.texture = transparentTexture;
    }

    public void KeyInputImageSetActive(bool active, int index)
    {
        keyFeedback[index].SetActive(active);
        keyboard[index].sprite = active ? keyPressedImage[index] : keyInitImage[index];
    }

    public void NoteBombActive(int index, int state)
    {
        noteBombAnimationIndex[index] = 0;
        noteBombState[index] = state;
    }

    public void GameUIUpdate(int combo, JudgeType judge, float hp, int maxcombo, int score)
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
            comboParentTransform.localPosition = new Vector3(comboPositionX[digitCount - 1], config.comboPosition, 0.0f);
            while (digitCount < 5)
            {
                comboDigitArray[digitCount++].sprite = null;
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                comboDigitArray[i].sprite = null;
            }
            comboTitle.SetActive(false);
        }

        hpBarMask.localScale = new Vector3(1.0f, hp, 1.0f);

        for (int i = 0; i < 5; i++)
        {
            int tempValue = (int)(maxcombo * 0.1f);
            int remainder = maxcombo - (tempValue * 10);
            maxcomboDigitArray[i].sprite = defaultNumberArray[remainder];
            maxcombo = tempValue;
        }

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

    private async UniTask NoteBombEffect(int line)
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            while (noteBombAnimationIndex[line] < noteBombSpriteArrayLength[noteBombState[line]])
            {
                noteBombArray[line].sprite = noteBombSpriteArray[noteBombState[line]][noteBombAnimationIndex[line]++];
                await UniTask.Delay(effectWaitSecond, cancellationToken: token);
            }
            noteBombArray[line].sprite = null;
            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask JudgeEffect()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            while (judgeEffectIndex < 15)
            {
                judgeSpriteRenderer.sprite = judgeSpriteArray[currentJudge, judgeEffectIndex++];
                await UniTask.Delay(effectWaitSecond, cancellationToken: token);
            }
            judgeSpriteRenderer.sprite = null;
            await UniTask.Yield(cancellationToken: token);
        }
    }

    public void FadeIn()
    {
        fadeObject.SetActive(true);
        fadeObject.GetComponent<Animator>().SetTrigger("FadeIn");
    }
    public IEnumerator FadeOut()
    {
        fadeObject.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(1.0f);
        fadeObject.SetActive(false);
    }

    public void CoUpdateInfoPanel(bool isStart)
    {
        if (infoPanelCoroutine != null)
        {
            StopCoroutine(infoPanelCoroutine);
            infoPanelCoroutine = null;
        }
        if (isStart)
        {
            infoPanelCoroutine = StartCoroutine(UpdateInfoPanel());
        }
    }

    public void SetInfoPanel(bool isActive)
    {
        infoPanel.SetActive(isActive);
    }

    public void SetNoteSpeedText()
    {
        noteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
    }

    public void SetBGAOpacityText()
    {
        bgaOpacityText.text = (PlayerPrefs.GetInt("BGAOpacity") * 10) + "%";
    }

    private IEnumerator UpdateInfoPanel()
    {
        SetNoteSpeedText();
        SetBGAOpacityText();
        SetInfoPanel(true);
        yield return new WaitForSecondsRealtime(1.0f);
        SetInfoPanel(false);
        infoPanelCoroutine = null;
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
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\loading-bg";
        if (File.Exists(filePath + ".jpg"))
        {
            StartCoroutine(LoadRawImage(GameObject.Find("Loading_Panel").GetComponent<RawImage>(), "", filePath + ".jpg", noStageImage));
        }
        else if (File.Exists(filePath + ".png"))
        {
            StartCoroutine(LoadRawImage(GameObject.Find("Loading_Panel").GetComponent<RawImage>(), "", filePath + ".png", noStageImage));
        }
        StartCoroutine(LoadRawImage(GameObject.Find("Loading_StageImage").GetComponent<RawImage>(), BMSGameManager.header.musicFolderPath, BMSGameManager.header.stageFilePath, noStageImage));
        //StartCoroutine(StageImageFade());
        TextMeshProUGUI titleText = GameObject.Find("Loading_Title").GetComponent<TextMeshProUGUI>();
        titleText.text = BMSGameManager.header.title;
        titleText.fontSize = 35;
        while (titleText.preferredWidth > 775.0f)
        {
            titleText.fontSize -= 0.1f;
        }
        GameObject.Find("Loading_Artist").GetComponent<TextMeshProUGUI>().text = BMSGameManager.header.artist;
        GameObject.Find("Loading_NoteSpeed").GetComponent<TextMeshProUGUI>().text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(GameObject.Find("Loading_Random").GetComponent<TextMeshProUGUI>(), PlayerPrefs.GetInt("RandomEffector"));
        GameObject.Find("Loading_Fader").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat("FadeIn") == 0.0f ? "NONE" : $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";
    }

    /*private IEnumerator StageImageFade()
    {
        float fadeValue = 0.1f;
        while (fadeValue < 0.9f)
        {
            fadeValue += 0.015f;
            yield return new WaitForSecondsRealtime(0.01f);
            stageImage.color = new Color(1, 1, 1, fadeValue);
        }
    }*/

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
        GameObject.Find("LoadingBar").GetComponent<Slider>().value = loadingValue;
        GameObject.Find("LoadingValue").GetComponent<TextMeshProUGUI>().text = ((int)(loadingValue * 100.0f)).ToString() + "%";
    }

    public void CloseLoading()
    {
        hpBarMask.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        GameObject.Find("Loading_Panel").SetActive(false);
    }

    public void SetCountdown(float amount, int second, GameObject countdownBar, GameObject countdownTime)
    {
        countdownBar.GetComponent<SpriteRenderer>().material.SetInt("_Arc2", (int)((1.0f - amount) * 360.0f));
        countdownTime.GetComponent<SpriteRenderer>().sprite = comboNumberArray[second == 3 ? second : second + 1];
    }

    public void SetActiveCountdown(bool isActive)
    {
        countdownObject.SetActive(isActive);
    }

    public void AnimationPause(bool isPause)
    {
        Time.timeScale = isPause ? 0.0f : 1.0f;
    }

    public void BGATextureDestroy()
    {
        bga.texture = null;
        layer.texture = null;
        for (int i = 0; i < bgSpriteArray.Length; i++)
        {
            if (bgSpriteArray[i] == transparentTexture) continue;
            Destroy(bgSpriteArray[i]);
        }
    }

    public void SkinTextureDestroy()
    {
        ObjectPool.poolInstance.NoteSpriteEmpty();
        assetPacker.Dispose();
    }
}
