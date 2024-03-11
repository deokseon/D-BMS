using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CustomManager : MonoBehaviour
{
    public bool isPrepared = false;
    private GameUIManager gameUIManager = null;
    private ScoreGraph scoreGraph = null;
    private JudgementTracker judgementTracker = null;

    private Dictionary<string, float> limitValueDict;

    [SerializeField]
    private EarlyLate earlyLateManager;

    [SerializeField]
    private Slider comboPositionSlider;
    [SerializeField]
    private Slider judgePositionSlider;
    [SerializeField]
    private Slider earlyLatePositionSlider;
    [SerializeField]
    private Slider noteBombNScaleSlider;
    [SerializeField]
    private Slider noteBombLScaleSlider;
    [SerializeField]
    private Slider scoreDigitPositionXSlider;
    [SerializeField]
    private Slider scoreDigitPositionYSlider;
    [SerializeField]
    private Slider scoreImagePositionXSlider;
    [SerializeField]
    private Slider scoreImagePositionYSlider;
    [SerializeField]
    private Slider maxcomboDigitPositionXSlider;
    [SerializeField]
    private Slider maxcomboDigitPositionYSlider;
    [SerializeField]
    private Slider maxcomboImagePositionXSlider;
    [SerializeField]
    private Slider maxcomboImagePositionYSlider;
    [SerializeField]
    private Slider panelPositionSlider;
    [SerializeField]
    private Slider scoreGraphPositionXSlider;
    [SerializeField]
    private Slider scoreGraphPositionYSlider;
    [SerializeField]
    private Slider judgementTrackerPositionXSlider;
    [SerializeField]
    private Slider judgementTrackerPositionYSlider;
    [SerializeField]
    private Slider bgaPositionXSlider;
    [SerializeField]
    private Slider bgaPositionYSlider;
    [SerializeField]
    private Slider bgaWidthSlider;
    [SerializeField]
    private Slider bgaHeightSlider;
    [SerializeField]
    private Slider judgelinePositionSlider;

    [SerializeField]
    private Sprite[] noteBombLTriggerButtomSprite;
    [SerializeField]
    private Image noteBombLTriggerButtomImage;

    [SerializeField]
    private GameObject[] settingPanelArray;
    private int currentPanelIndex = 0;

    private void Awake()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
        scoreGraph = FindObjectOfType<ScoreGraph>();
        judgementTracker = FindObjectOfType<JudgementTracker>();
        limitValueDict = new Dictionary<string, float>(24);
        _ = SliderSetting();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameUIManager.fadeObject.activeSelf) { return; }
            _ = LoadStartScene();
        }
    }

    private async UniTask LoadStartScene()
    {
        gameUIManager.FadeIn();

        DataSaveManager.SaveData("Skin", "config.json", GameUIManager.config);

        await UniTask.Delay(1000);

        gameUIManager.SkinTextureDestroy();

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ChangeSettingPanel(int value)
    {
        settingPanelArray[currentPanelIndex].SetActive(false);
        currentPanelIndex = (currentPanelIndex + value + settingPanelArray.Length) % settingPanelArray.Length;
        settingPanelArray[currentPanelIndex].SetActive(true);
    }

    private void SetLimitValue()
    {
        limitValueDict.Add("MinComboPosition", GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("combo-0").bounds.size.y * 0.5f);
        limitValueDict.Add("MaxComboPosition", 7.5f - 0.085f - gameUIManager.assetPacker.GetSprite("combo-0").bounds.size.y * 0.5f - gameUIManager.assetPacker.GetSprite("text-combo").bounds.size.y); // 0.085f = distance between combo and comboTitle
        limitValueDict.Add("MinJudgePosition", GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("kool-0").bounds.size.y * 0.7f * 0.5f); // 0.7f = judgeScaleY
        limitValueDict.Add("MaxJudgePosition", 7.5f - gameUIManager.assetPacker.GetSprite("kool-0").bounds.size.y * 0.7f * 0.5f); // 0.7f = judgeScaleY
        limitValueDict.Add("MinEarlyLatePosition", GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("early").bounds.size.y * 0.26f * 0.5f); // 0.26f = earlylateScaleY
        limitValueDict.Add("MaxEarlyLatePosition", 7.5f - gameUIManager.assetPacker.GetSprite("early").bounds.size.y * 0.26f * 0.5f); // 0.26f = earlylateScaleY
        limitValueDict.Add("MinScoreDigitPositionX", gameUIManager.GetXPosition(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) + 6.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x);
        limitValueDict.Add("MaxScoreDigitPositionX", gameUIManager.GetXPosition(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x);
        limitValueDict.Add("MinScoreDigitPositionY", -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y);
        limitValueDict.Add("MaxScoreDigitPositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y);
        limitValueDict.Add("MinScoreImagePositionX", gameUIManager.GetXPosition(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) + 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.x);
        limitValueDict.Add("MaxScoreImagePositionX", gameUIManager.GetXPosition(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) - 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.x);
        limitValueDict.Add("MinScoreImagePositionY", -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.y);
        limitValueDict.Add("MaxScoreImagePositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.y);
        limitValueDict.Add("MinMaxcomboDigitPositionX", gameUIManager.GetXPosition(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) + 4.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x);
        limitValueDict.Add("MaxMaxcomboDigitPositionX", gameUIManager.GetXPosition(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x);
        limitValueDict.Add("MinMaxcomboDigitPositionY", -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y);
        limitValueDict.Add("MaxMaxcomboDigitPositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y);
        limitValueDict.Add("MinMaxcomboImagePositionX", gameUIManager.GetXPosition(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) + 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.x);
        limitValueDict.Add("MaxMaxcomboImagePositionX", gameUIManager.GetXPosition(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetXPosition(2) - 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.x);
        limitValueDict.Add("MinMaxcomboImagePositionY", -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.y);
        limitValueDict.Add("MaxMaxcomboImagePositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.y);
        limitValueDict.Add("MinPanelPosition", -8.889f + ObjectPool.poolInstance.GetNoteWidth() * 0.5f + gameUIManager.assetPacker.GetSprite("panel-left").bounds.size.x);
        limitValueDict.Add("MaxPanelPosition", 8.889f - ObjectPool.poolInstance.GetNoteWidth() * 4.5f - gameUIManager.assetPacker.GetSprite("panel-right").bounds.size.x);
        limitValueDict.Add("MinBGAPositionX", -960.0f + GameUIManager.config.bgaWidth * 0.5f);
        limitValueDict.Add("MaxBGAPositionX", 960.0f - GameUIManager.config.bgaWidth * 0.5f);
        limitValueDict.Add("MinBGAPositionY", -540.0f + GameUIManager.config.bgaHeight * 0.5f);
        limitValueDict.Add("MaxBGAPositionY", 540.0f - GameUIManager.config.bgaHeight * 0.5f);
    }

    private float GetSliderValue(float configValue, float minValue, float maxValue)
    {
        if (minValue == maxValue) return 1.0f;
        else return (configValue - minValue) / (maxValue - minValue);
    }

    private async UniTask SliderSetting()
    {
        await UniTask.WaitUntil(() => isPrepared);
        SetLimitValue();
        comboPositionSlider.value = GetSliderValue(GameUIManager.config.comboPosition, limitValueDict["MinComboPosition"], limitValueDict["MaxComboPosition"]);
        judgePositionSlider.value = GetSliderValue(GameUIManager.config.judgePosition, limitValueDict["MinJudgePosition"], limitValueDict["MaxJudgePosition"]);
        earlyLatePositionSlider.value = GetSliderValue(GameUIManager.config.earlyLatePosition, limitValueDict["MinEarlyLatePosition"], limitValueDict["MaxEarlyLatePosition"]);
        noteBombNScaleSlider.value = GameUIManager.config.noteBombNScale / 3.0f;
        noteBombLScaleSlider.value = GameUIManager.config.noteBombLScale / 3.0f;
        scoreDigitPositionXSlider.value = GetSliderValue(GameUIManager.config.scoreDigitPositionX, limitValueDict["MinScoreDigitPositionX"], limitValueDict["MaxScoreDigitPositionX"]);
        scoreDigitPositionYSlider.value = GetSliderValue(GameUIManager.config.scoreDigitPositionY, limitValueDict["MinScoreDigitPositionY"], limitValueDict["MaxScoreDigitPositionY"]);
        scoreImagePositionXSlider.value = GetSliderValue(GameUIManager.config.scoreImagePositionX, limitValueDict["MinScoreImagePositionX"], limitValueDict["MaxScoreImagePositionX"]);
        scoreImagePositionYSlider.value = GetSliderValue(GameUIManager.config.scoreImagePositionY, limitValueDict["MinScoreImagePositionY"], limitValueDict["MaxScoreImagePositionY"]);
        maxcomboDigitPositionXSlider.value = GetSliderValue(GameUIManager.config.maxcomboDigitPositionX, limitValueDict["MinMaxcomboDigitPositionX"], limitValueDict["MaxMaxcomboDigitPositionX"]);
        maxcomboDigitPositionYSlider.value = GetSliderValue(GameUIManager.config.maxcomboDigitPositionY, limitValueDict["MinMaxcomboDigitPositionY"], limitValueDict["MaxMaxcomboDigitPositionY"]);
        maxcomboImagePositionXSlider.value = GetSliderValue(GameUIManager.config.maxcomboImagePositionX, limitValueDict["MinMaxcomboImagePositionX"], limitValueDict["MaxMaxcomboImagePositionX"]);
        maxcomboImagePositionYSlider.value = GetSliderValue(GameUIManager.config.maxcomboImagePositionY, limitValueDict["MinMaxcomboImagePositionY"], limitValueDict["MaxMaxcomboImagePositionY"]);
        panelPositionSlider.value = GetSliderValue(GameUIManager.config.panelPosition, limitValueDict["MinPanelPosition"], limitValueDict["MaxPanelPosition"]);
        scoreGraphPositionXSlider.value = (GameUIManager.config.scoreGraphPositionOffsetX + 670.0f) / 1555.0f;
        scoreGraphPositionYSlider.value = (GameUIManager.config.scoreGraphPositionOffsetY + 370.0f) / 440.0f;
        judgementTrackerPositionXSlider.value = (GameUIManager.config.judgementTrackerPositionOffsetX + 670.0f) / 1555.0f;
        judgementTrackerPositionYSlider.value = (GameUIManager.config.judgementTrackerPositionOffsetY + 20.0f) / 740.0f;
        bgaPositionXSlider.value = GetSliderValue(GameUIManager.config.bgaPositionX, limitValueDict["MinBGAPositionX"], limitValueDict["MaxBGAPositionX"]);
        bgaPositionYSlider.value = GetSliderValue(GameUIManager.config.bgaPositionY, limitValueDict["MinBGAPositionY"], limitValueDict["MaxBGAPositionY"]);
        bgaWidthSlider.value = GameUIManager.config.bgaWidth / 1920.0f;
        bgaHeightSlider.value = GameUIManager.config.bgaWidth / 1080.0f;
        judgelinePositionSlider.value = (GameUIManager.config.judgeLinePosition + 1.0f) / 3.0f;

        for (int i = 1; i < settingPanelArray.Length; i++)
        {
            settingPanelArray[i].SetActive(false);
        }
    }

    public void ReturnButtonClick(int index)
    {
        switch (index)
        {
            case 0:
                comboPositionSlider.value = GetSliderValue(5.15f, limitValueDict["MinComboPosition"], limitValueDict["MaxComboPosition"]);
                break;
            case 1:
                judgePositionSlider.value = GetSliderValue(1.4f, limitValueDict["MinJudgePosition"], limitValueDict["MaxJudgePosition"]);
                break;
            case 2:
                earlyLatePositionSlider.value = GetSliderValue(2.17f, limitValueDict["MinEarlyLatePosition"], limitValueDict["MaxEarlyLatePosition"]);
                break;
            case 3:
                noteBombNScaleSlider.value = 1.5f / 3.0f;
                break;
            case 4:
                scoreDigitPositionXSlider.value = GetSliderValue(1.19f, limitValueDict["MinScoreDigitPositionX"], limitValueDict["MaxScoreDigitPositionX"]);
                break;
            case 5:
                scoreDigitPositionYSlider.value = GetSliderValue(-1.76f, limitValueDict["MinScoreDigitPositionY"], limitValueDict["MaxScoreDigitPositionY"]);
                break;
            case 6:
                scoreImagePositionXSlider.value = GetSliderValue(-0.95f, limitValueDict["MinScoreImagePositionX"], limitValueDict["MaxScoreImagePositionX"]);
                break;
            case 7:
                scoreImagePositionYSlider.value = GetSliderValue(-1.76f, limitValueDict["MinScoreImagePositionY"], limitValueDict["MaxScoreImagePositionY"]);
                break;
            case 8:
                maxcomboDigitPositionXSlider.value = GetSliderValue(1.19f, limitValueDict["MinMaxcomboDigitPositionX"], limitValueDict["MaxMaxcomboDigitPositionX"]);
                break;
            case 9:
                maxcomboDigitPositionYSlider.value = GetSliderValue(-2.17f, limitValueDict["MinMaxcomboDigitPositionY"], limitValueDict["MaxMaxcomboDigitPositionY"]);
                break;
            case 10:
                maxcomboImagePositionXSlider.value = GetSliderValue(-0.69f, limitValueDict["MinMaxcomboImagePositionX"], limitValueDict["MaxMaxcomboImagePositionX"]);
                break;
            case 11:
                maxcomboImagePositionYSlider.value = GetSliderValue(-2.17f, limitValueDict["MinMaxcomboImagePositionY"], limitValueDict["MaxMaxcomboImagePositionY"]);
                break;
            case 12:
                panelPositionSlider.value = GetSliderValue(-7.63f, limitValueDict["MinPanelPosition"], limitValueDict["MaxPanelPosition"]);
                break;
            case 13:
                scoreGraphPositionXSlider.value = 670.0f / 1555.0f;
                break;
            case 14:
                scoreGraphPositionYSlider.value = 370.0f / 440.0f;
                break;
            case 15:
                judgementTrackerPositionXSlider.value = 670.0f / 1555.0f;
                break;
            case 16:
                judgementTrackerPositionYSlider.value = 20.0f / 740.0f;
                break;
            case 17:
                bgaPositionXSlider.value = GetSliderValue(520.0f, limitValueDict["MinBGAPositionX"], limitValueDict["MaxBGAPositionX"]);
                break;
            case 18:
                bgaPositionYSlider.value = GetSliderValue(100.0f, limitValueDict["MinBGAPositionY"], limitValueDict["MaxBGAPositionY"]);
                break;
            case 19:
                bgaWidthSlider.value = 700.0f / 1920.0f;
                break;
            case 20:
                bgaHeightSlider.value = 700.0f / 1080.0f;
                break;
            case 21:
                judgelinePositionSlider.value = 1.0f / 3.0f;
                break;
            case 22:
                noteBombLScaleSlider.value = 1.5f / 3.0f;
                break;
        }
    }

    public void NoteBombNTriggerButtonClick()
    {
        gameUIManager.noteBombNAnimationIndex[2] = 0;
    }

    public void NoteBombLTriggerButtonClick()
    {
        noteBombLTriggerButtomImage.sprite = gameUIManager.isNoteBombLEffectActive[2] ? noteBombLTriggerButtomSprite[0] : noteBombLTriggerButtomSprite[1];
        gameUIManager.isNoteBombLEffectActive[2] = !gameUIManager.isNoteBombLEffectActive[2];
    }

    public void ComboPositionSliderValueChange(float value)
    {
        GameUIManager.config.comboPosition = value * (limitValueDict["MaxComboPosition"] - limitValueDict["MinComboPosition"]) + limitValueDict["MinComboPosition"];
        gameUIManager.SetCombo();
        gameUIManager.CustomSetting();
    }

    public void JudgePositionSliderValueChange(float value)
    {
        GameUIManager.config.judgePosition = value * (limitValueDict["MaxJudgePosition"] - limitValueDict["MinJudgePosition"]) + limitValueDict["MinJudgePosition"];
        gameUIManager.SetJudgePosition();
    }

    public void EarlyLatePositionSliderValueChange(float value)
    {
        GameUIManager.config.earlyLatePosition = value * (limitValueDict["MaxEarlyLatePosition"] - limitValueDict["MinEarlyLatePosition"]) + limitValueDict["MinEarlyLatePosition"];
        earlyLateManager.SetEarlyLatePosition();
    }

    public void NoteBombNScaleSliderValueChange(float value)
    {
        GameUIManager.config.noteBombNScale = value * 3.0f;
        gameUIManager.SetNoteBombScale();
    }

    public void NoteBombLScaleSliderValueChange(float value)
    {
        GameUIManager.config.noteBombLScale = value * 3.0f;
        gameUIManager.SetNoteBombScale();
    }

    public void ScoreDigitPositionXSliderValueChange(float value)
    {
        GameUIManager.config.scoreDigitPositionX = value * (limitValueDict["MaxScoreDigitPositionX"] - limitValueDict["MinScoreDigitPositionX"]) + limitValueDict["MinScoreDigitPositionX"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void ScoreDigitPositionYSliderValueChange(float value)
    {
        GameUIManager.config.scoreDigitPositionY = value * (limitValueDict["MaxScoreDigitPositionY"] - limitValueDict["MinScoreDigitPositionY"]) + limitValueDict["MinScoreDigitPositionY"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void ScoreImagePositionXSliderValueChange(float value)
    {
        GameUIManager.config.scoreImagePositionX = value * (limitValueDict["MaxScoreImagePositionX"] - limitValueDict["MinScoreImagePositionX"]) + limitValueDict["MinScoreImagePositionX"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void ScoreImagePositionYSliderValueChange(float value)
    {
        GameUIManager.config.scoreImagePositionY = value * (limitValueDict["MaxScoreImagePositionY"] - limitValueDict["MinScoreImagePositionY"]) + limitValueDict["MinScoreImagePositionY"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void MaxcomboDigitPositionXSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboDigitPositionX = value * (limitValueDict["MaxMaxcomboDigitPositionX"] - limitValueDict["MinMaxcomboDigitPositionX"]) + limitValueDict["MinMaxcomboDigitPositionX"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void MaxcomboDigitPositionYSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboDigitPositionY = value * (limitValueDict["MaxMaxcomboDigitPositionY"] - limitValueDict["MinMaxcomboDigitPositionY"]) + limitValueDict["MinMaxcomboDigitPositionY"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void MaxcomboImagePositionXSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboImagePositionX = value * (limitValueDict["MaxMaxcomboImagePositionX"] - limitValueDict["MinMaxcomboImagePositionX"]) + limitValueDict["MinMaxcomboImagePositionX"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void MaxcomboImagePositionYSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboImagePositionY = value * (limitValueDict["MaxMaxcomboImagePositionY"] - limitValueDict["MinMaxcomboImagePositionY"]) + limitValueDict["MinMaxcomboImagePositionY"];
        gameUIManager.SetScoreAndMaxcombo();
    }

    public void PanelPositionSliderValueChange(float value)
    {
        GameUIManager.config.panelPosition = value * (limitValueDict["MaxPanelPosition"] - limitValueDict["MinPanelPosition"]) + limitValueDict["MinPanelPosition"];
        gameUIManager.SetCombo();
        gameUIManager.SetGamePanel();
        gameUIManager.SetJudgeLine();
        gameUIManager.SetJudgePosition();
        gameUIManager.SetNoteBombPosition();
        gameUIManager.SetKeyFeedback();
        gameUIManager.SetScoreAndMaxcombo();
        earlyLateManager.SetEarlyLatePosition();
        gameUIManager.CustomSetting();
    }

    public void ScoreGraphPositionXSliderValueChange(float value)
    {
        float newOffset = value * 1555.0f - 670.0f;
        scoreGraph.SetScoreGraphPosition(newOffset - GameUIManager.config.scoreGraphPositionOffsetX, 0.0f);
        GameUIManager.config.scoreGraphPositionOffsetX = newOffset;
    }

    public void ScoreGraphPositionYSliderValueChange(float value)
    {
        float newOffset = value * 440.0f - 370.0f;
        scoreGraph.SetScoreGraphPosition(0.0f, newOffset - GameUIManager.config.scoreGraphPositionOffsetY);
        GameUIManager.config.scoreGraphPositionOffsetY = newOffset;
    }

    public void JudgementTrackerPositionXSliderValueChange(float value)
    {
        float newOffset = value * 1555.0f - 670.0f;
        judgementTracker.SetJudgementTrackerPosition(newOffset - GameUIManager.config.judgementTrackerPositionOffsetX, 0.0f);
        GameUIManager.config.judgementTrackerPositionOffsetX = newOffset;
    }

    public void JudgementTrackerPositionYSliderValueChange(float value)
    {
        float newOffset = value * 740.0f - 20.0f;
        judgementTracker.SetJudgementTrackerPosition(0.0f, newOffset - GameUIManager.config.judgementTrackerPositionOffsetY);
        GameUIManager.config.judgementTrackerPositionOffsetY = newOffset;
    }

    public void BGAPositionXSliderValueChange(float value)
    {
        GameUIManager.config.bgaPositionX = value * (limitValueDict["MaxBGAPositionX"] - limitValueDict["MinBGAPositionX"]) + limitValueDict["MinBGAPositionX"];
        gameUIManager.SetBGAPanel();
    }

    public void BGAPositionYSliderValueChange(float value)
    {
        GameUIManager.config.bgaPositionY = value * (limitValueDict["MaxBGAPositionY"] - limitValueDict["MinBGAPositionY"]) + limitValueDict["MinBGAPositionY"];
        gameUIManager.SetBGAPanel();
    }

    public void BGAWidthSliderValueChange(float value)
    {
        GameUIManager.config.bgaWidth = value * 1920.0f;
        SetLimitValue("MinBGAPositionX", -960.0f + GameUIManager.config.bgaWidth * 0.5f, GameUIManager.config.bgaPositionX, 0.0f, bgaPositionXSlider, BGAPositionXSliderValueChange);
        SetLimitValue("MaxBGAPositionX", 960.0f - GameUIManager.config.bgaWidth * 0.5f, GameUIManager.config.bgaPositionX, 1.0f, bgaPositionXSlider, BGAPositionXSliderValueChange);
        gameUIManager.SetBGAPanel();
    }

    public void BGAHeigthSliderValueChange(float value)
    {
        GameUIManager.config.bgaHeight = value * 1080.0f;
        SetLimitValue("MinBGAPositionY", -540.0f + GameUIManager.config.bgaHeight * 0.5f, GameUIManager.config.bgaPositionY, 0.0f, bgaPositionYSlider, BGAPositionYSliderValueChange);
        SetLimitValue("MaxBGAPositionY", 540.0f - GameUIManager.config.bgaHeight * 0.5f, GameUIManager.config.bgaPositionY, 1.0f, bgaPositionYSlider, BGAPositionYSliderValueChange);
        gameUIManager.SetBGAPanel();
    }

    public void JudgelinePositionSliderValueChange(float value)
    {
        GameUIManager.config.judgeLinePosition = value * 3.0f - 1.0f;
        SetLimitValue("MinComboPosition", GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("combo-0").bounds.size.y * 0.5f,
                      GameUIManager.config.comboPosition, 0.0f, comboPositionSlider, ComboPositionSliderValueChange);
        SetLimitValue("MinJudgePosition", GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("kool-0").bounds.size.y * 0.7f * 0.5f,
                      GameUIManager.config.judgePosition, 0.0f, judgePositionSlider, JudgePositionSliderValueChange);
        SetLimitValue("MinEarlyLatePosition", GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("early").bounds.size.y * 0.26f * 0.5f,
                      GameUIManager.config.earlyLatePosition, 0.0f, earlyLatePositionSlider, EarlyLatePositionSliderValueChange);

        SetLimitValue("MaxScoreDigitPositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y,
                      GameUIManager.config.scoreDigitPositionY, 1.0f, scoreDigitPositionYSlider, ScoreDigitPositionYSliderValueChange);
        SetLimitValue("MaxScoreImagePositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.y,
                      GameUIManager.config.scoreImagePositionY, 1.0f, scoreImagePositionYSlider, ScoreImagePositionYSliderValueChange);
        SetLimitValue("MaxMaxcomboDigitPositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y,
                      GameUIManager.config.maxcomboDigitPositionY, 1.0f, maxcomboDigitPositionYSlider, MaxcomboDigitPositionYSliderValueChange);
        SetLimitValue("MaxMaxcomboImagePositionY", GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.y,
                      GameUIManager.config.maxcomboImagePositionY, 1.0f, maxcomboImagePositionYSlider, MaxcomboImagePositionYSliderValueChange);

        gameUIManager.SetGamePanel();
        gameUIManager.SetJudgeLine();
        gameUIManager.SetNoteBombPosition();
        gameUIManager.SetKeyFeedback();
    }

    private void SetLimitValue(string key, float value, float comparingValue, float sliderValue, Slider slider, Action<float> sliderFunc)
    {
        limitValueDict[key] = value;
        if (sliderValue == 0.0f ? comparingValue <= limitValueDict[key] : comparingValue >= limitValueDict[key])
        {
            slider.value = sliderValue;
            sliderFunc(sliderValue);
        }
    }
}
