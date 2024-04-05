using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePanel : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private GameUIManager gameUIManager = null;

    [SerializeField]
    private SpriteRenderer panelBottom;
    private Sprite[] panelBottomSpriteArray;
    private int panelBottomSpriteArrayLength;
    private int panelBottomAnimationIndex;
    private TimeSpan panelBottomAnimationDelay;

    [SerializeField]
    private Transform hpBarMask;
    private Sprite[] hpBarSpriteArray;
    private int hpbarSpriteArrayLength;
    private TimeSpan hpbarSpriteAnimationDelay;
    private TimeSpan hpbarPulseAnimationDelay;
    private int hpbarSpriteAnimationIndex;
    private int hpbarPulseAnimationIndex;
    private const double animationMaxSpeed = 1.0d / 300.0d;
    [SerializeField]
    private GameObject[] keyFeedback;
    [SerializeField]
    private SpriteRenderer[] keyboard;
    private Sprite[] keyInitImage;
    private Sprite[] keyPressedImage;

    private void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
    }

    private void ObjectSetting(SpriteRenderer changeObject, Sprite changeSprite, Vector3 positionVector, Vector3 scaleVector)
    {
        changeObject.sprite = changeSprite;
        changeObject.transform.localPosition = positionVector;
        changeObject.transform.localScale = scaleVector;
    }

    public void SetGamePanel()
    {
        float cameraSize = Camera.main.orthographicSize;
        float noteWidth = ObjectPool.poolInstance.GetLineWidth();

        ObjectSetting(GameObject.Find("Panel_Left").GetComponent<SpriteRenderer>(), gameUIManager.assetPacker.GetSprite("panel-left"), new Vector3(gameUIManager.GetLinePositionX(0) - noteWidth * 0.5f, 2.5f, 0.0f), 
                        new Vector3(1.0f, (cameraSize * 2.0f) / gameUIManager.assetPacker.GetSprite("panel-left").bounds.size.y, 1.0f));

        ObjectSetting(GameObject.Find("Panel_Right").GetComponent<SpriteRenderer>(), gameUIManager.assetPacker.GetSprite("panel-right"), new Vector3(gameUIManager.GetLinePositionX(4) + noteWidth * 0.5f, 2.5f, 0.0f),
                        new Vector3(1.0f, (cameraSize * 2.0f) / gameUIManager.assetPacker.GetSprite("panel-right").bounds.size.y, 1.0f));

        ObjectSetting(GameObject.Find("Panel_BG").GetComponent<SpriteRenderer>(), gameUIManager.assetPacker.GetSprite("panel-bg"), new Vector3(gameUIManager.GetLinePositionX(2), GameUIManager.config.judgeLinePosition - 0.24f, 0.0f),
                        new Vector3(noteWidth * 5.0f / gameUIManager.assetPacker.GetSprite("panel-bg").bounds.size.x, (cameraSize + 2.74f - GameUIManager.config.judgeLinePosition) / gameUIManager.assetPacker.GetSprite("panel-bg").bounds.size.y, 1.0f));

        if (panelBottomSpriteArray == null)
        {
            panelBottomSpriteArray = gameUIManager.assetPacker.GetSprites("panelbottom-");
            panelBottomSpriteArrayLength = panelBottomSpriteArray.Length - 1;
        }
        ObjectSetting(panelBottom, panelBottomSpriteArray[0], new Vector3(gameUIManager.GetLinePositionX(2), GameUIManager.config.judgeLinePosition - 0.24f, 0.0f),
                        new Vector3(GameUIManager.config.panelBottomScaleX, (GameUIManager.config.judgeLinePosition + 2.26f) / panelBottomSpriteArray[0].bounds.size.y, 0.0f));

        GameObject panelFade = GameObject.Find("Panel_Fade");
        if (panelFade == null) { return; }
        float fadeInSize = PlayerPrefs.GetFloat("FadeIn");
        if (fadeInSize == 0.0f)
        {
            panelFade.gameObject.SetActive(false);
        }
        else
        {
            panelFade.GetComponent<SpriteMask>().sprite = gameUIManager.assetPacker.GetSprite("panel-bg");
            panelFade.transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), 2.5f + cameraSize, 0.0f);
            panelFade.transform.localScale = new Vector3(noteWidth * 5.0f / gameUIManager.assetPacker.GetSprite("panel-bg").bounds.size.x, 4.0f / gameUIManager.assetPacker.GetSprite("panel-bg").bounds.size.y * fadeInSize, 1.0f);
        }
    }

    public void SetHPbar()
    {
        Sprite hpBarBGSprite = gameUIManager.assetPacker.GetSprite("hpbarbg");
        GameObject hpBarBackground = GameObject.Find("HPbar_bg");
        hpBarBackground.GetComponent<SpriteRenderer>().sprite = hpBarBGSprite;
        hpBarBackground.transform.localPosition = new Vector3(GameUIManager.config.hpbarBGPositionX, GameUIManager.config.hpbarBGPositionY, 0.0f);

        GameObject hpBar = GameObject.Find("HPbar");
        if (hpBarSpriteArray == null)
        {
            hpBarSpriteArray = gameUIManager.assetPacker.GetSprites("hpbar-");
            hpbarSpriteArrayLength = hpBarSpriteArray.Length - 1;
        }
        hpBar.GetComponent<SpriteRenderer>().sprite = hpBarSpriteArray[0];
        hpBarMask.GetComponent<SpriteMask>().sprite = hpBarSpriteArray[0];
        hpBar.transform.localPosition = new Vector3(GameUIManager.config.hpbarPositionX, GameUIManager.config.hpbarPositionY, 0.0f);
    }

    public void SetJudgeLine()
    {
        float judgeLineYPosition = PlayerPrefs.GetInt("JudgeLine") == 0 ? GameUIManager.config.judgeLinePosition : GameUIManager.config.judgeLinePosition - 0.24f;
        Sprite judgelineSprite = gameUIManager.assetPacker.GetSprite("judgeline");
        float judgelinesize = ObjectPool.poolInstance.GetLineWidth() / judgelineSprite.bounds.size.x;
        for (int i = 1; i < 6; i++)
        {
            ObjectSetting(GameObject.Find($"JudgeLine{i}").GetComponent<SpriteRenderer>(), judgelineSprite, new Vector3(gameUIManager.GetLinePositionX(i - 1), judgeLineYPosition, 0.0f),
                        new Vector3(judgelinesize, judgelinesize, 1.0f));
        }
    }

    public void SetKeyObject()
    {
        keyInitImage = new Sprite[5];
        keyPressedImage = new Sprite[5];
        float keyboardWidth = ObjectPool.poolInstance.GetLineWidth() / gameUIManager.assetPacker.GetSprite("key1-init").bounds.size.x;
        float keyboardHeight = (GameUIManager.config.judgeLinePosition + 2.26f) / gameUIManager.assetPacker.GetSprite("key1-init").bounds.size.y;
        for (int i = 0; i < 5; i++)
        {
            keyInitImage[i] = gameUIManager.assetPacker.GetSprite($"key{(i % 2) + 1}-init");
            keyPressedImage[i] = gameUIManager.assetPacker.GetSprite($"key{(i % 2) + 1}-pressed");
            ObjectSetting(keyboard[i], keyInitImage[i], new Vector3(gameUIManager.GetLinePositionX(i), GameUIManager.config.judgeLinePosition - 0.24f, 0.0f), 
                        new Vector3(keyboardWidth, keyboardHeight, 1.0f));
        }
    }

    public void SetKeyFeedback()
    {
        Sprite keyfeedbackSprite = gameUIManager.assetPacker.GetSprite("keyfeedback");
        Color oddColor = new Color(PlayerPrefs.GetFloat("OddKeyFeedbackColorR"), PlayerPrefs.GetFloat("OddKeyFeedbackColorG"),
                                   PlayerPrefs.GetFloat("OddKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        Color evenColor = new Color(PlayerPrefs.GetFloat("EvenKeyFeedbackColorR"), PlayerPrefs.GetFloat("EvenKeyFeedbackColorG"),
                                    PlayerPrefs.GetFloat("EvenKeyFeedbackColorB"), PlayerPrefs.GetFloat("KeyFeedbackOpacity"));
        float xScale = ObjectPool.poolInstance.GetLineWidth() / keyfeedbackSprite.bounds.size.x;
        float yScale = (7.74f - GameUIManager.config.judgeLinePosition) / keyfeedbackSprite.bounds.size.y;
        for (int i = 0; i < 5; i++)
        {
            keyFeedback[i].GetComponent<SpriteRenderer>().color = i % 2 == 0 ? oddColor : evenColor;
            ObjectSetting(keyFeedback[i].GetComponent<SpriteRenderer>(), keyfeedbackSprite, new Vector3(gameUIManager.GetLinePositionX(i), GameUIManager.config.judgeLinePosition - 0.24f, 0.0f),
                        new Vector3(xScale, yScale, 1.0f));
        }
    }

    public void KeyInputImageSetActive(bool active, int index)
    {
        keyFeedback[index].SetActive(active);
        keyboard[index].sprite = active ? keyPressedImage[index] : keyInitImage[index];
    }

    public void SetHPbarValue(float hp)
    {
        hpBarMask.localScale = new Vector3(1.0f, hp, 1.0f);
    }

    public void SetAnimationSpeed(double bpm)
    {
        double panelBottpmAnimationDelayValue = panelBottomSpriteArrayLength == 0 ? 0.0d : 60.0d / bpm / panelBottomSpriteArrayLength;
        double hpbarSpriteAnimationDelayValue = hpbarSpriteArrayLength == 0 ? 0.0d : 60.0d / bpm / hpbarSpriteArrayLength;
        double hpbarPulseAnimationDelayValue = 3.0d / bpm;
        panelBottomAnimationDelay = TimeSpan.FromSeconds(panelBottpmAnimationDelayValue > animationMaxSpeed ? panelBottpmAnimationDelayValue : animationMaxSpeed);
        hpbarSpriteAnimationDelay = TimeSpan.FromSeconds(hpbarSpriteAnimationDelayValue > animationMaxSpeed ? hpbarSpriteAnimationDelayValue : animationMaxSpeed);
        hpbarPulseAnimationDelay = TimeSpan.FromSeconds(hpbarPulseAnimationDelayValue > animationMaxSpeed ? hpbarPulseAnimationDelayValue : animationMaxSpeed);
    }

    public void SetGamePanelAnimationIndex()
    {
        panelBottomAnimationIndex = panelBottomSpriteArrayLength;
        hpbarSpriteAnimationIndex = hpbarSpriteArrayLength;
        hpbarPulseAnimationIndex = 19;
    }

    public void GamePanelAnimationStart()
    {
        if (hpbarSpriteArrayLength > 0)
        {
            _ = HPbarSpriteAnimation();
        }
        if (panelBottomSpriteArrayLength > 0)
        {
            _ = PanelBottomSpriteAnimation();
        }
        _ = HPbarPulseAnimation();
    }

    private async UniTask PanelBottomSpriteAnimation()
    {
        var token = this.GetCancellationTokenOnDestroy();
        panelBottomAnimationIndex = -1;
        while (true)
        {
            while (panelBottomAnimationIndex >= 0)
            {
                panelBottom.sprite = panelBottomSpriteArray[panelBottomAnimationIndex--];
                await UniTask.Delay(panelBottomAnimationDelay, cancellationToken: token);
            }
            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask HPbarSpriteAnimation()
    {
        var token = this.GetCancellationTokenOnDestroy();
        SpriteRenderer hpBar = GameObject.Find("HPbar").GetComponent<SpriteRenderer>();
        Gauge gauge = bmsGameManager.gauge;
        hpbarSpriteAnimationIndex = -1;
        while (true)
        {
            while (hpbarSpriteAnimationIndex >= 0)
            {
                hpBar.sprite = gauge.hp == 1.0f ? hpBarSpriteArray[hpbarSpriteAnimationIndex--] : hpBarSpriteArray[0];
                await UniTask.Delay(hpbarSpriteAnimationDelay, cancellationToken: token);
            }
            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask HPbarPulseAnimation()
    {
        var token = this.GetCancellationTokenOnDestroy();
        Gauge gauge = bmsGameManager.gauge;
        hpbarPulseAnimationIndex = -1;
        while (true)
        {
            while (hpbarPulseAnimationIndex >= 0)
            {
                hpBarMask.localScale = new Vector3(1.0f, gauge.hp + 0.005f * hpbarPulseAnimationIndex--, 1.0f);
                await UniTask.Delay(hpbarPulseAnimationDelay, cancellationToken: token);
            }
            await UniTask.Yield(cancellationToken: token);
        }
    }
}
