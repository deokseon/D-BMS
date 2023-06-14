using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HSVPicker;

public class GameplayOptionManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI noteSpeedValueText;
    [SerializeField]
    private Slider noteSpeedSlider;
    [SerializeField]
    private TextMeshProUGUI randomEffectorValueText;
    [SerializeField]
    private Image[] currentRandomEffectorImage;
    [SerializeField]
    private TextMeshProUGUI displayDelayCorrectionValueText;
    [SerializeField]
    private Slider displayDelayCorrectionSlider;
    [SerializeField]
    private TextMeshProUGUI earlyLateThresholdValueText;
    [SerializeField]
    private Slider earlyLateThresholdSlider;
    [SerializeField]
    private TextMeshProUGUI verticalLineValueText;
    [SerializeField]
    private Slider verticalLineSlider;
    [SerializeField]
    private TextMeshProUGUI keyFeedbackOpacityValueText;
    [SerializeField]
    private Slider keyFeedbackOpacitySlider;
    [SerializeField]
    private Image oddKeyFeedbackColorPreviewImage;
    [SerializeField]
    private ColorPicker oddKeyFeedbackColorPicker;
    [SerializeField]
    private Image evenKeyFeedbackColorPreviewImage;
    [SerializeField]
    private ColorPicker evenKeyFeedbackColorPicker;
    [SerializeField]
    private TextMeshProUGUI fadeInValueText;
    [SerializeField]
    private Slider fadeInSlider;
    [SerializeField]
    private Toggle[] noteSkinToggles;
    [SerializeField]
    private Toggle[] judgeLineToggles;

    private int randomEffectorCount;

    private void Awake()
    {
        randomEffectorCount = System.Enum.GetValues(typeof(RandomEffector)).Length;

        oddKeyFeedbackColorPicker.onValueChanged.AddListener(color =>
        {
            oddKeyFeedbackColorPreviewImage.color = color;
            PlayerPrefs.SetFloat("OddKeyFeedbackColorR", color.r);
            PlayerPrefs.SetFloat("OddKeyFeedbackColorG", color.g);
            PlayerPrefs.SetFloat("OddKeyFeedbackColorB", color.b);
        });
        evenKeyFeedbackColorPicker.onValueChanged.AddListener(color =>
        {
            evenKeyFeedbackColorPreviewImage.color = color;
            PlayerPrefs.SetFloat("EvenKeyFeedbackColorR", color.r);
            PlayerPrefs.SetFloat("EvenKeyFeedbackColorG", color.g);
            PlayerPrefs.SetFloat("EvenKeyFeedbackColorB", color.b);
        });
    }

    public void SetGameplayOption()
    {
        noteSpeedSlider.value = (PlayerPrefs.GetInt("NoteSpeed") - 10.0f) / 190.0f;
        SetNoteSpeedValueText();
        displayDelayCorrectionSlider.value = (PlayerPrefs.GetInt("DisplayDelayCorrection") + 80.0f) / 160.0f;
        SetDisplayDelayCorrectionValueText();
        earlyLateThresholdSlider.value = (PlayerPrefs.GetInt("EarlyLateThreshold") - 1.0f) / 21.0f;
        SetEarlyLateThresholdValueText();
        verticalLineSlider.value = PlayerPrefs.GetFloat("VerticalLine");
        SetVerticalLineValueText();
        keyFeedbackOpacitySlider.value = PlayerPrefs.GetFloat("KeyFeedbackOpacity");
        SetKeyFeedbackOpacityValueText();
        oddKeyFeedbackColorPicker.CurrentColor = new Color(PlayerPrefs.GetFloat("OddKeyFeedbackColorR"), 
                                                           PlayerPrefs.GetFloat("OddKeyFeedbackColorG"), 
                                                           PlayerPrefs.GetFloat("OddKeyFeedbackColorB"));
        evenKeyFeedbackColorPicker.CurrentColor = new Color(PlayerPrefs.GetFloat("EvenKeyFeedbackColorR"), 
                                                            PlayerPrefs.GetFloat("EvenKeyFeedbackColorG"), 
                                                            PlayerPrefs.GetFloat("EvenKeyFeedbackColorB"));
        SetRandomEffector(PlayerPrefs.GetInt("RandomEffector"));
        fadeInSlider.value = PlayerPrefs.GetFloat("FadeIn");
        SetFadeInValueText();
        noteSkinToggles[PlayerPrefs.GetInt("NoteSkin")].isOn = true;
        judgeLineToggles[PlayerPrefs.GetInt("JudgeLine")].isOn = true;
    }

    public void NoteSpeedSliderValueChange(float value)
    {
        PlayerPrefs.SetInt("NoteSpeed", Mathf.RoundToInt(value * 190.0f + 10.0f));
        SetNoteSpeedValueText();
    }
    private void SetNoteSpeedValueText()
    {
        noteSpeedValueText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
    }

    public void ChangeRandomEffector(int value)
    {
        SetRandomEffector((PlayerPrefs.GetInt("RandomEffector") + value + randomEffectorCount) % randomEffectorCount);
    }
    private void SetRandomEffector(int index)
    {
        switch (index)
        {
            case 0: randomEffectorValueText.text = "NONE"; break;
            case 1: randomEffectorValueText.text = "RANDOM"; break;
            case 2: randomEffectorValueText.text = "MIRROR"; break;
            case 3: randomEffectorValueText.text = "FLIP RANDOM"; break;
            case 4: randomEffectorValueText.text = "MIRROR FLIP RANDOM"; break;
        }

        float colorValue = 100.0f / 255.0f;
        for (int i = 0; i < randomEffectorCount; i++)
        {
            currentRandomEffectorImage[i].color = new Color(colorValue, colorValue, colorValue);
        }
        currentRandomEffectorImage[index].color = new Color(1, 1, 1);

        PlayerPrefs.SetInt("RandomEffector", index);
    }

    public void DisplayDelayCorrectionSliderValueChange(float value)
    {
        PlayerPrefs.SetInt("DisplayDelayCorrection", Mathf.RoundToInt(value * 160.0f - 80.0f));
        SetDisplayDelayCorrectionValueText();
    }
    private void SetDisplayDelayCorrectionValueText()
    {
        int value = PlayerPrefs.GetInt("DisplayDelayCorrection");
        displayDelayCorrectionValueText.text = (value > 0 ? "+" : "") + value.ToString() + "ms";
    }

    public void EarlyLateThresholdSliderValueChange(float value)
    {
        PlayerPrefs.SetInt("EarlyLateThreshold", Mathf.RoundToInt(value * 21.0f + 1.0f));
        SetEarlyLateThresholdValueText();
    }
    private void SetEarlyLateThresholdValueText()
    {
        earlyLateThresholdValueText.text = PlayerPrefs.GetInt("EarlyLateThreshold").ToString() + "ms";
    }

    public void VerticalLineSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("VerticalLine", value);
        SetVerticalLineValueText();
    }
    private void SetVerticalLineValueText()
    {
        verticalLineValueText.text = PlayerPrefs.GetFloat("VerticalLine").ToString("P0");
    }

    public void KeyFeedbackOpacitySliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("KeyFeedbackOpacity", value);
        SetKeyFeedbackOpacityValueText();
    }
    private void SetKeyFeedbackOpacityValueText()
    {
        keyFeedbackOpacityValueText.text = PlayerPrefs.GetFloat("KeyFeedbackOpacity").ToString("P0");
    }

    public void FadeInSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("FadeIn", value);
        SetFadeInValueText();
    }
    private void SetFadeInValueText()
    {
        fadeInValueText.text = PlayerPrefs.GetFloat("FadeIn").ToString("P0");
    }

    public void SetNoteSkin(int index)
    {
        PlayerPrefs.SetInt("NoteSkin", index);
    }

    public void SetJudgeLine(int index)
    {
        PlayerPrefs.SetInt("JudgeLine", index);
    }
}
