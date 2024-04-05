using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DaVikingCode.AssetPacker;
using System.IO;
using Cysharp.Threading.Tasks;

public class GameUIManager : MonoBehaviour
{
    [HideInInspector] public int isPrepared { get; set; } = 0;
    public static bool isCreateReady = false;
    public RawImage bga;
    public RawImage layer;
    public Image bgaOpacity;
    [SerializeField]
    private Texture2D transparentTexture;
    [HideInInspector] public int bgaTextureArrayLength;
    public HashSet<int> layerImageSet { get; set; }
    public List<KeyValuePair<int, string>> bgImageList;
    private Texture2D[] bgaTextureArray;
    [HideInInspector] public int taskCount;

    private BMSDrawer bmsDrawer = null;
    private BMSGameManager bmsGameManager = null;
    private TextureDownloadManager textureDownloadManger = null;
    public NoteBomb noteBomb;
    public GamePanel gamePanel;
    public Countdown countdown;
    public JudgeEffect judgeEffect;
    public ComboScore comboScore;

    [SerializeField]
    private GameObject infoPanel;
    [SerializeField]
    private TextMeshProUGUI noteSpeedText;
    [SerializeField]
    private TextMeshProUGUI bgaOpacityText;
    [SerializeField]
    private TextMeshProUGUI displayDelayCorrectionText;

    private RawImage loadingPanelBG;
    private RawImage stageImage;
    [SerializeField]
    private Texture noStageImage;

    public GameObject fadeObject;

    private Coroutine infoPanelCoroutine;
    private Coroutine displayDelayCorrectionCoroutine;

    [HideInInspector]
    public AssetPacker assetPacker;

    public static ConfigData config;

    private void Awake()
    {
        assetPacker = GetComponent<AssetPacker>();
        bmsGameManager = FindObjectOfType<BMSGameManager>();

        assetPacker.AddTexturesToPack(Directory.GetFiles($@"{Directory.GetParent(Application.dataPath)}\Skin\GameObject"));
        assetPacker.Process();
        assetPacker.OnProcessCompleted.AddListener(GameObjectSpriteSetting);

        config = config ?? DataSaveManager.LoadData<ConfigData>("Skin", "config.json") ?? new ConfigData();

        if (bmsGameManager == null) 
        {
            isCreateReady = true;
            return;
        }

        bmsDrawer = FindObjectOfType<BMSDrawer>();
        textureDownloadManger = FindObjectOfType<TextureDownloadManager>();
        loadingPanelBG = GameObject.Find("Loading_Panel").GetComponent<RawImage>();
        stageImage = GameObject.Find("Loading_StageImage").GetComponent<RawImage>();

        bgImageList = new List<KeyValuePair<int, string>>(500);
        layerImageSet = new HashSet<int>();
        taskCount = Mathf.Max((int)(SystemInfo.processorCount * 0.5f) - 2, 1);

        SetReplayFunctionText();
    }

    private void GameObjectSpriteSetting()
    {
        _ = GameObjectSpriteSettingTask();
    }

    private async UniTask GameObjectSpriteSettingTask()
    {
        ObjectPool.poolInstance.SetComponent();
        ObjectPool.poolInstance.SetVerticalLineSprite();
        ObjectPool.poolInstance.SetNoteSprite();
        ObjectPool.poolInstance.SetVerticalLine();

        SetBGAPanel();
        comboScore.SetNumberSpriteArray();
        comboScore.SetCombo();
        comboScore.SetScore();
        comboScore.SetMaxcombo();
        gamePanel.SetGamePanel();
        gamePanel.SetKeyObject();
        gamePanel.SetHPbar();
        countdown.SetCountdownObject();
        gamePanel.SetJudgeLine();
        noteBomb.SetNoteBomb();
        judgeEffect.SetJudgeEffect();
        gamePanel.SetKeyFeedback();
        SetInputBlock();

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
            bmsDrawer.DrawNotes(false);

            isPrepared++;
            gamePanel.GamePanelAnimationStart();
            judgeEffect.JudgeEffectSpriteAnimationStart();
        }
        else
        {
            FindObjectOfType<EarlyLate>().ObjectSetting();
            comboScore.ScoreComboUpdate(0, 789, 123456);
            gamePanel.SetHPbarValue(1.0f);
            await UniTask.Delay(200);
            GameObjectSettingInCustomScene();
            _ = FadeOut();
        }
    }

    public void GameObjectSettingInCustomScene()
    {
        comboScore.ComboScoreCustomSetting();
        judgeEffect.SetJudgeEffectSprite(4, 0);
        FindObjectOfType<EarlyLate>().CustomSetting();
        FindObjectOfType<CustomManager>().isPrepared = true;
    }

    public float GetLinePositionX(int index)
    {
        return ObjectPool.poolInstance.GetLineWidth() * index + config.panelPosition;
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

    private void SetInputBlock()
    {
        Sprite inputBlockSprite = assetPacker.GetSprite("inputblock");
        GameObject inputBlockLine = GameObject.Find("InputBlockLine");
        inputBlockLine.GetComponent<SpriteRenderer>().sprite = inputBlockSprite;
        inputBlockLine.transform.localScale = new Vector3(ObjectPool.poolInstance.GetLineWidth() * 5 / inputBlockSprite.bounds.size.x, 0.75f, 1.0f);
    }

    public void LoadBGATextures()
    {
        bgaTextureArray = new Texture2D[bgaTextureArrayLength + 1];
        for (int i = 0; i < taskCount; i++)
        {
            _ = LoadBGATexturesTask(i);
        }
    }

    private async UniTask LoadBGATexturesTask(int value)
    {
        int start = (int)(value / (double)taskCount * bgImageList.Count);
        int end = (int)((value + 1) / (double)taskCount * bgImageList.Count);
        for (int i = start; i < end; i++)
        {
            Texture2D texture2D = await textureDownloadManger.GetTexture(BMSGameManager.header.musicFolderPath + bgImageList[i].Value.Substring(0, bgImageList[i].Value.Length - 4));
            if (texture2D != null)
            {
                texture2D.filterMode = FilterMode.Point;
                if (layerImageSet.Contains(bgImageList[i].Key))
                {
                    var colors = texture2D.GetPixels32();

                    for (int k = colors.Length - 1; k >= 0; k--)
                    {
                        if (colors[k].a != 0 && colors[k].r + colors[k].g + colors[k].b == 0) colors[k].a = 0;
                    }

                    texture2D.SetPixels32(colors);
                }
                texture2D.Apply();
            }
            bgaTextureArray[bgImageList[i].Key] = texture2D;
            lock (bmsGameManager.threadLock)
            {
                BMSGameManager.currentLoading++;
            }
        }
        isPrepared++;
    }

    public void SetNullBGATextureArray()
    {
        for (int i = 0; i < bgaTextureArray.Length; i++)
        {
            if (bgaTextureArray[i] == null) bgaTextureArray[i] = transparentTexture;
        }
    }

    public void ChangeBGA(int key)
    {
        bga.texture = bgaTextureArray[key];
    }

    public void ChangeLayer(int key)
    {
        layer.texture = bgaTextureArray[key];
    }

    public void InitBGALayer()
    {
        bga.texture = transparentTexture;
        layer.texture = transparentTexture;
    }

    public void FadeIn()
    {
        fadeObject.SetActive(true);
        fadeObject.GetComponent<Animator>().SetTrigger("FadeIn");
    }

    public async UniTask FadeOut()
    {
        fadeObject.GetComponent<Animator>().SetTrigger("FadeOut");
        await UniTask.Delay(1000);
        fadeObject.SetActive(false);
    }

    public void InfoPanelUpdateCoroutine(bool isStart)
    {
        if (infoPanelCoroutine != null)
        {
            StopCoroutine(infoPanelCoroutine);
            infoPanelCoroutine = null;
        }
        if (isStart)
        {
            infoPanelCoroutine = StartCoroutine(InfoPanelUpdate());
        }
    }

    public void SetActiveInfoPanel(bool isActive)
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

    private IEnumerator InfoPanelUpdate()
    {
        SetNoteSpeedText();
        SetBGAOpacityText();
        SetActiveInfoPanel(true);
        yield return new WaitForSecondsRealtime(1.0f);
        SetActiveInfoPanel(false);
        infoPanelCoroutine = null;
    }

    public void DisplayDelayCorrectionTextUpdateCoroutine()
    {
        if (displayDelayCorrectionCoroutine != null)
        {
            StopCoroutine(displayDelayCorrectionCoroutine);
            displayDelayCorrectionCoroutine = null;
        }
        displayDelayCorrectionCoroutine = StartCoroutine(DisplayDelayCorrectionTextUpdate());
    }

    private IEnumerator DisplayDelayCorrectionTextUpdate()
    {
        displayDelayCorrectionText.text = "Display Delay Correction : " + PlayerPrefs.GetInt("DisplayDelayCorrection") + "ms";
        displayDelayCorrectionText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.0f);
        displayDelayCorrectionText.gameObject.SetActive(false);
        displayDelayCorrectionCoroutine = null;
    }

    public void SetLoadingPanel()
    {
        _ = LoadImage(loadingPanelBG, $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\loading-bg");
        _ = LoadImage(stageImage, $"{BMSGameManager.header.musicFolderPath}{BMSGameManager.header.stageFilePath}");

        TextMeshProUGUI titleText = GameObject.Find("Loading_Title").GetComponent<TextMeshProUGUI>();
        titleText.text = BMSGameManager.header.title;
        titleText.fontSize = 35;
        while (titleText.preferredWidth > 775.0f)
        {
            titleText.fontSize -= 0.1f;
        }
        GameObject.Find("Loading_Artist").GetComponent<TextMeshProUGUI>().text = BMSGameManager.header.artist;
        GameObject.Find("Loading_NoteSpeed").GetComponent<TextMeshProUGUI>().text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(GameObject.Find("Loading_Random").GetComponent<TextMeshProUGUI>(), BMSGameManager.isReplay ? BMSGameManager.replayData.randomEffector : PlayerPrefs.GetInt("RandomEffector"));
        GameObject.Find("Loading_Fader").GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat("FadeIn") == 0.0f ? "NONE" : $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";
    }

    private async UniTask LoadImage(RawImage rawImage, string path)
    {
        Texture tex = await textureDownloadManger.GetTexture(path);
        rawImage.texture = tex ?? noStageImage;
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

    public void SetLoadingSlider(float loadingValue)
    {
        GameObject.Find("LoadingBar").GetComponent<Slider>().value = loadingValue;
        GameObject.Find("LoadingValue").GetComponent<TextMeshProUGUI>().text = ((int)(loadingValue * 100.0f)).ToString() + "%";
    }

    public void CloseLoadingPanel()
    {
        GameObject.Find("Loading_Panel").SetActive(false);
    }

    public void AnimationPause(bool isPause)
    {
        Time.timeScale = isPause ? 0.0f : 1.0f;
    }

    private void SetReplayFunctionText()
    {
        if (!BMSGameManager.isReplay)
        {
            GameObject.Find("ReplayFuncTextBG").SetActive(false);
            GameObject.Find("ReplayFuncKeyTextBG").SetActive(false);
            GameObject.Find("ReplayFuncText").SetActive(false);
            GameObject.Find("ReplayFuncKeyText").SetActive(false);
        }
    }

    public void BGATextureDestroy()
    {
        bga.texture = null;
        layer.texture = null;
        for (int i = 0; i < bgaTextureArray.Length; i++)
        {
            if (bgaTextureArray[i] == transparentTexture) continue;
            Destroy(bgaTextureArray[i]);
        }
        if (loadingPanelBG.texture != noStageImage)
        {
            Destroy(loadingPanelBG.texture);
        }
        if (stageImage.texture != noStageImage)
        {
            Destroy(stageImage.texture);
        }
    }

    public void SkinTextureDestroy()
    {
        ObjectPool.poolInstance.NoteSpriteEmpty();
        assetPacker.Dispose();
    }
}
