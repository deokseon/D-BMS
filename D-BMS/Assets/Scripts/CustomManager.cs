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

    [SerializeField]
    private EarlyLate earlyLateManager;

    [SerializeField]
    private Slider[] sliderArray;
    private CustomValue[] customValueArray;

    [SerializeField]
    private Sprite[] noteBombLTriggerButtomSprite;
    [SerializeField]
    private Image noteBombLTriggerButtomImage;

    [SerializeField]
    private GameObject[] settingPanelArray;
    private int currentPanelIndex = 0;

    public struct CustomValue
    {
        public float configValue;
        public float initValue;
        public float minValue;
        public float maxValue;

        public CustomValue(float configValue, float initValue, float minValue, float maxValue)
        {
            this.configValue = configValue;
            this.initValue = initValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
    }

    private void Awake()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
        scoreGraph = FindObjectOfType<ScoreGraph>();
        judgementTracker = FindObjectOfType<JudgementTracker>();
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

    private void SetCustomValue()
    {
        customValueArray = new CustomValue[sliderArray.Length];
        customValueArray[0] = new CustomValue(GameUIManager.config.comboPosition, 5.15f, GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("combo-0").bounds.size.y * 0.5f, 
                                                7.5f - 0.085f - gameUIManager.assetPacker.GetSprite("combo-0").bounds.size.y * 0.5f - gameUIManager.assetPacker.GetSprite("text-combo").bounds.size.y);
        customValueArray[1] = new CustomValue(GameUIManager.config.judgePosition, 1.4f, GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("kool-0").bounds.size.y * 0.7f * 0.5f,
                                                7.5f - gameUIManager.assetPacker.GetSprite("kool-0").bounds.size.y * 0.7f * 0.5f);
        customValueArray[2] = new CustomValue(GameUIManager.config.earlyLatePosition, 2.17f, GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("early").bounds.size.y * 0.26f * 0.5f,
                                                7.5f - gameUIManager.assetPacker.GetSprite("early").bounds.size.y * 0.26f * 0.5f);
        customValueArray[3] = new CustomValue(GameUIManager.config.noteBombNScale, 1.5f, 0.0f, 3.0f);
        customValueArray[4] = new CustomValue(GameUIManager.config.noteBombLScale, 1.5f, 0.0f, 3.0f);
        customValueArray[5] = new CustomValue(GameUIManager.config.scoreDigitPositionX, 1.19f, gameUIManager.GetLinePositionX(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) + 6.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x,
                                                gameUIManager.GetLinePositionX(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x);
        customValueArray[6] = new CustomValue(GameUIManager.config.scoreDigitPositionY, -1.76f, -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y,
                                                GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y);
        customValueArray[7] = new CustomValue(GameUIManager.config.scoreImagePositionX, -0.95f, gameUIManager.GetLinePositionX(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) + 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.x,
                                                gameUIManager.GetLinePositionX(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) - 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.x);
        customValueArray[8] = new CustomValue(GameUIManager.config.scoreImagePositionY, -1.76f, -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.y,
                                                GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.y);
        customValueArray[9] = new CustomValue(GameUIManager.config.maxcomboDigitPositionX, 1.19f, gameUIManager.GetLinePositionX(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) + 4.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x,
                                                gameUIManager.GetLinePositionX(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.x);
        customValueArray[10] = new CustomValue(GameUIManager.config.maxcomboDigitPositionY, -2.17f, -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y,
                                                GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y);
        customValueArray[11] = new CustomValue(GameUIManager.config.maxcomboImagePositionX, -0.69f, gameUIManager.GetLinePositionX(0) - ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) + 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.x,
                                                gameUIManager.GetLinePositionX(4) + ObjectPool.poolInstance.GetLineWidth() * 0.5f - gameUIManager.GetLinePositionX(2) - 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.x);
        customValueArray[12] = new CustomValue(GameUIManager.config.maxcomboImagePositionY, -2.17f, -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.y,
                                                GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.y);
        customValueArray[13] = new CustomValue(GameUIManager.config.hpbarBGPositionX, 0.0f, -8.889f + 0.5f * gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.x, 
                                                8.889f - 0.5f * gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.x);
        customValueArray[14] = new CustomValue(GameUIManager.config.hpbarBGPositionY, 0.0f, -2.5f + 0.5f * gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.y,
                                                7.5f - 0.5f * gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.y);
        customValueArray[15] = new CustomValue(GameUIManager.config.hpbarPositionX, 0.0f, gameUIManager.assetPacker.GetSprite("hpbar-00").bounds.size.x * 0.5f - gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.x * 0.5f,
                                                gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.x * 0.5f - gameUIManager.assetPacker.GetSprite("hpbar-00").bounds.size.x * 0.5f);
        customValueArray[16] = new CustomValue(GameUIManager.config.hpbarPositionY, gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.y * -0.5f, gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.y * -0.5f,
                                                gameUIManager.assetPacker.GetSprite("hpbarbg").bounds.size.y * 0.5f - gameUIManager.assetPacker.GetSprite("hpbar-00").bounds.size.y);
        customValueArray[17] = new CustomValue(GameUIManager.config.panelPosition, -7.63f, -8.889f + ObjectPool.poolInstance.GetLineWidth() * 0.5f + gameUIManager.assetPacker.GetSprite("panel-left").bounds.size.x,
                                                8.889f - ObjectPool.poolInstance.GetLineWidth() * 4.5f - gameUIManager.assetPacker.GetSprite("panel-right").bounds.size.x);
        customValueArray[18] = new CustomValue(GameUIManager.config.judgeLinePosition, 0.0f, -1.0f, 2.0f);
        customValueArray[19] = new CustomValue(GameUIManager.config.panelBottomScaleX, 1.0f, 0.0f, 3.0f);
        customValueArray[20] = new CustomValue(GameUIManager.config.scoreGraphPositionOffsetX, 0.0f, -670.0f, 885.0f);
        customValueArray[21] = new CustomValue(GameUIManager.config.scoreGraphPositionOffsetY, 0.0f, -370.0f, 70.0f);
        customValueArray[22] = new CustomValue(GameUIManager.config.judgementTrackerPositionOffsetX, 0.0f, -670.0f, 885.0f);
        customValueArray[23] = new CustomValue(GameUIManager.config.judgementTrackerPositionOffsetY, 0.0f, -20.0f, 720.0f);
        customValueArray[24] = new CustomValue(GameUIManager.config.bgaPositionX, 520.0f, -960.0f + GameUIManager.config.bgaWidth * 0.5f, 960.0f - GameUIManager.config.bgaWidth * 0.5f);
        customValueArray[25] = new CustomValue(GameUIManager.config.bgaPositionY, 100.0f, -540.0f + GameUIManager.config.bgaHeight * 0.5f, 540.0f - GameUIManager.config.bgaHeight * 0.5f);
        customValueArray[26] = new CustomValue(GameUIManager.config.bgaWidth, 700.0f, 0.0f, 1980.0f);
        customValueArray[27] = new CustomValue(GameUIManager.config.bgaHeight, 700.0f, 0.0f, 1080.0f);
    }

    private float GetSliderValue(float configValue, float minValue, float maxValue)
    {
        if (minValue == maxValue) return 1.0f;
        else return (configValue - minValue) / (maxValue - minValue);
    }

    private async UniTask SliderSetting()
    {
        await UniTask.WaitUntil(() => isPrepared);
        SetCustomValue();
        for (int i = 0; i < customValueArray.Length; i++)
        {
            sliderArray[i].value = GetSliderValue(customValueArray[i].configValue, customValueArray[i].minValue, customValueArray[i].maxValue);
        }

        for (int i = 1; i < settingPanelArray.Length; i++)
        {
            settingPanelArray[i].SetActive(false);
        }
    }

    public void ReturnButtonClick(int index)
    {
        sliderArray[index].value = GetSliderValue(customValueArray[index].initValue, customValueArray[index].minValue, customValueArray[index].maxValue);
    }

    public void NoteBombNTriggerButtonClick()
    {
        gameUIManager.noteBomb.noteBombNAnimationIndex[2] = 0;
    }

    public void NoteBombLTriggerButtonClick()
    {
        noteBombLTriggerButtomImage.sprite = gameUIManager.noteBomb.isNoteBombLEffectActive[2] ? noteBombLTriggerButtomSprite[0] : noteBombLTriggerButtomSprite[1];
        gameUIManager.noteBomb.isNoteBombLEffectActive[2] = !gameUIManager.noteBomb.isNoteBombLEffectActive[2];
    }

    public void ComboPositionSliderValueChange(float value)
    {
        GameUIManager.config.comboPosition = value * (customValueArray[0].maxValue - customValueArray[0].minValue) + customValueArray[0].minValue;
        gameUIManager.comboScore.SetCombo();
        gameUIManager.GameObjectSettingInCustomScene();
    }

    public void JudgePositionSliderValueChange(float value)
    {
        GameUIManager.config.judgePosition = value * (customValueArray[1].maxValue - customValueArray[1].minValue) + customValueArray[1].minValue;
        gameUIManager.judgeEffect.SetJudgePosition();
    }

    public void EarlyLatePositionSliderValueChange(float value)
    {
        GameUIManager.config.earlyLatePosition = value * (customValueArray[2].maxValue - customValueArray[2].minValue) + customValueArray[2].minValue;
        earlyLateManager.SetEarlyLatePosition();
    }

    public void NoteBombNScaleSliderValueChange(float value)
    {
        GameUIManager.config.noteBombNScale = value * (customValueArray[3].maxValue - customValueArray[3].minValue) + customValueArray[3].minValue;
        gameUIManager.noteBomb.SetNoteBombScale();
    }

    public void NoteBombLScaleSliderValueChange(float value)
    {
        GameUIManager.config.noteBombLScale = value * (customValueArray[4].maxValue - customValueArray[4].minValue) + customValueArray[4].minValue;
        gameUIManager.noteBomb.SetNoteBombScale();
    }

    public void ScoreDigitPositionXSliderValueChange(float value)
    {
        GameUIManager.config.scoreDigitPositionX = value * (customValueArray[5].maxValue - customValueArray[5].minValue) + customValueArray[5].minValue;
        gameUIManager.comboScore.SetScore();
    }

    public void ScoreDigitPositionYSliderValueChange(float value)
    {
        GameUIManager.config.scoreDigitPositionY = value * (customValueArray[6].maxValue - customValueArray[6].minValue) + customValueArray[6].minValue;
        gameUIManager.comboScore.SetScore();
    }

    public void ScoreImagePositionXSliderValueChange(float value)
    {
        GameUIManager.config.scoreImagePositionX = value * (customValueArray[7].maxValue - customValueArray[7].minValue) + customValueArray[7].minValue;
        gameUIManager.comboScore.SetScore();
    }

    public void ScoreImagePositionYSliderValueChange(float value)
    {
        GameUIManager.config.scoreImagePositionY = value * (customValueArray[8].maxValue - customValueArray[8].minValue) + customValueArray[8].minValue;
        gameUIManager.comboScore.SetScore();
    }

    public void MaxcomboDigitPositionXSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboDigitPositionX = value * (customValueArray[9].maxValue - customValueArray[9].minValue) + customValueArray[9].minValue;
        gameUIManager.comboScore.SetMaxcombo();
    }

    public void MaxcomboDigitPositionYSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboDigitPositionY = value * (customValueArray[10].maxValue - customValueArray[10].minValue) + customValueArray[10].minValue;
        gameUIManager.comboScore.SetMaxcombo();
    }

    public void MaxcomboImagePositionXSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboImagePositionX = value * (customValueArray[11].maxValue - customValueArray[11].minValue) + customValueArray[11].minValue;
        gameUIManager.comboScore.SetMaxcombo();
    }

    public void MaxcomboImagePositionYSliderValueChange(float value)
    {
        GameUIManager.config.maxcomboImagePositionY = value * (customValueArray[12].maxValue - customValueArray[12].minValue) + customValueArray[12].minValue;
        gameUIManager.comboScore.SetMaxcombo();
    }

    public void HPbarBGPositionXSliderValueChange(float value)
    {
        GameUIManager.config.hpbarBGPositionX = value * (customValueArray[13].maxValue - customValueArray[13].minValue) + customValueArray[13].minValue;
        gameUIManager.gamePanel.SetHPbar();
    }

    public void HPbarBGPositionYSliderValueChange(float value)
    {
        GameUIManager.config.hpbarBGPositionY = value * (customValueArray[14].maxValue - customValueArray[14].minValue) + customValueArray[14].minValue;
        gameUIManager.gamePanel.SetHPbar();
    }

    public void HPbarPositionXSliderValueChange(float value)
    {
        GameUIManager.config.hpbarPositionX = value * (customValueArray[15].maxValue - customValueArray[15].minValue) + customValueArray[15].minValue;
        gameUIManager.gamePanel.SetHPbar();
    }

    public void HPbarPositionYSliderValueChange(float value)
    {
        GameUIManager.config.hpbarPositionY = value * (customValueArray[16].maxValue - customValueArray[16].minValue) + customValueArray[16].minValue;
        gameUIManager.gamePanel.SetHPbar();
    }

    public void PanelPositionSliderValueChange(float value)
    {
        GameUIManager.config.panelPosition = value * (customValueArray[17].maxValue - customValueArray[17].minValue) + customValueArray[17].minValue;
        gameUIManager.comboScore.SetCombo();
        gameUIManager.gamePanel.SetGamePanel();
        gameUIManager.gamePanel.SetKeyObject();
        gameUIManager.gamePanel.SetJudgeLine();
        gameUIManager.judgeEffect.SetJudgePosition();
        gameUIManager.noteBomb.SetNoteBombPosition();
        gameUIManager.gamePanel.SetKeyFeedback();
        gameUIManager.comboScore.SetScore();
        gameUIManager.comboScore.SetMaxcombo();
        earlyLateManager.SetEarlyLatePosition();
        gameUIManager.GameObjectSettingInCustomScene();
    }

    public void JudgelinePositionSliderValueChange(float value)
    {
        GameUIManager.config.judgeLinePosition = value * (customValueArray[18].maxValue - customValueArray[18].minValue) + customValueArray[18].minValue;
        SetLimitValue(0, GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("combo-0").bounds.size.y * 0.5f,
                      GameUIManager.config.comboPosition, 0.0f, ComboPositionSliderValueChange);
        SetLimitValue(1, GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("kool-0").bounds.size.y * 0.7f * 0.5f,
                      GameUIManager.config.judgePosition, 0.0f, JudgePositionSliderValueChange);
        SetLimitValue(2, GameUIManager.config.judgeLinePosition - 0.24f + gameUIManager.assetPacker.GetSprite("early").bounds.size.y * 0.26f * 0.5f,
                      GameUIManager.config.earlyLatePosition, 0.0f, EarlyLatePositionSliderValueChange);

        SetLimitValue(6, GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y,
                      GameUIManager.config.scoreDigitPositionY, 1.0f, ScoreDigitPositionYSliderValueChange);
        SetLimitValue(8, GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-score").bounds.size.y,
                      GameUIManager.config.scoreImagePositionY, 1.0f, ScoreImagePositionYSliderValueChange);
        SetLimitValue(10, GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("default-0").bounds.size.y,
                      GameUIManager.config.maxcomboDigitPositionY, 1.0f, MaxcomboDigitPositionYSliderValueChange);
        SetLimitValue(12, GameUIManager.config.judgeLinePosition - 0.24f - 0.5f * gameUIManager.assetPacker.GetSprite("text-maxcombo").bounds.size.y,
                      GameUIManager.config.maxcomboImagePositionY, 1.0f, MaxcomboImagePositionYSliderValueChange);

        gameUIManager.gamePanel.SetGamePanel();
        gameUIManager.gamePanel.SetKeyObject();
        gameUIManager.gamePanel.SetJudgeLine();
        gameUIManager.noteBomb.SetNoteBombPosition();
        gameUIManager.gamePanel.SetKeyFeedback();
    }

    public void PanelBottomScaleXSliderValueChange(float value)
    {
        GameUIManager.config.panelBottomScaleX = value * (customValueArray[19].maxValue - customValueArray[19].minValue) + customValueArray[19].minValue;
        gameUIManager.gamePanel.SetGamePanel();
    }

    public void ScoreGraphPositionXSliderValueChange(float value)
    {
        float newOffset = value * (customValueArray[20].maxValue - customValueArray[20].minValue) + customValueArray[20].minValue;
        scoreGraph.SetScoreGraphPosition(newOffset - GameUIManager.config.scoreGraphPositionOffsetX, 0.0f);
        GameUIManager.config.scoreGraphPositionOffsetX = newOffset;
    }

    public void ScoreGraphPositionYSliderValueChange(float value)
    {
        float newOffset = value * (customValueArray[21].maxValue - customValueArray[21].minValue) + customValueArray[21].minValue;
        scoreGraph.SetScoreGraphPosition(0.0f, newOffset - GameUIManager.config.scoreGraphPositionOffsetY);
        GameUIManager.config.scoreGraphPositionOffsetY = newOffset;
    }

    public void JudgementTrackerPositionXSliderValueChange(float value)
    {
        float newOffset = value * (customValueArray[22].maxValue - customValueArray[22].minValue) + customValueArray[22].minValue;
        judgementTracker.SetJudgementTrackerPosition(newOffset - GameUIManager.config.judgementTrackerPositionOffsetX, 0.0f);
        GameUIManager.config.judgementTrackerPositionOffsetX = newOffset;
    }

    public void JudgementTrackerPositionYSliderValueChange(float value)
    {
        float newOffset = value * (customValueArray[23].maxValue - customValueArray[23].minValue) + customValueArray[23].minValue;
        judgementTracker.SetJudgementTrackerPosition(0.0f, newOffset - GameUIManager.config.judgementTrackerPositionOffsetY);
        GameUIManager.config.judgementTrackerPositionOffsetY = newOffset;
    }

    public void BGAPositionXSliderValueChange(float value)
    {
        GameUIManager.config.bgaPositionX = value * (customValueArray[24].maxValue - customValueArray[24].minValue) + customValueArray[24].minValue;
        gameUIManager.SetBGAPanel();
    }

    public void BGAPositionYSliderValueChange(float value)
    {
        GameUIManager.config.bgaPositionY = value * (customValueArray[25].maxValue - customValueArray[25].minValue) + customValueArray[25].minValue;
        gameUIManager.SetBGAPanel();
    }

    public void BGAWidthSliderValueChange(float value)
    {
        GameUIManager.config.bgaWidth = value * (customValueArray[26].maxValue - customValueArray[26].minValue) + customValueArray[26].minValue;
        SetLimitValue(24, -960.0f + GameUIManager.config.bgaWidth * 0.5f, GameUIManager.config.bgaPositionX, 0.0f, BGAPositionXSliderValueChange);
        SetLimitValue(24, 960.0f - GameUIManager.config.bgaWidth * 0.5f, GameUIManager.config.bgaPositionX, 1.0f, BGAPositionXSliderValueChange);
        gameUIManager.SetBGAPanel();
    }

    public void BGAHeigthSliderValueChange(float value)
    {
        GameUIManager.config.bgaHeight = value * (customValueArray[27].maxValue - customValueArray[27].minValue) + customValueArray[27].minValue;
        SetLimitValue(25, -540.0f + GameUIManager.config.bgaHeight * 0.5f, GameUIManager.config.bgaPositionY, 0.0f, BGAPositionYSliderValueChange);
        SetLimitValue(25, 540.0f - GameUIManager.config.bgaHeight * 0.5f, GameUIManager.config.bgaPositionY, 1.0f, BGAPositionYSliderValueChange);
        gameUIManager.SetBGAPanel();
    }

    private void SetLimitValue(int index, float value, float comparingValue, float sliderValue, Action<float> sliderFunc)
    {
        if (sliderValue == 1.0f)
        {
            customValueArray[index].maxValue = value;
        }
        else
        {
            customValueArray[index].minValue = value;
        }
        if (sliderValue == 0.0f ? comparingValue <= value : comparingValue >= value)
        {
            sliderArray[index].value = sliderValue;
            sliderFunc(sliderValue);
        }
    }
}
