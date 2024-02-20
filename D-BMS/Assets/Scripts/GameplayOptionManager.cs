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
    private TextMeshProUGUI bgaOpacityValueText;
    [SerializeField]
    private Slider bgaOpacitySlider;
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
    private ToggleController judgementTrackerToggle;
    [SerializeField]
    private ToggleController scoreGraphToggle;
    [SerializeField]
    private ToggleController judgeLineToggle;
    [SerializeField]
    private ToggleController earlyLateToggle;

    [SerializeField]
    private Toggle[] optionToggleArray;
    private int currentOptionIndex;

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

        for (int i = optionToggleArray.Length - 1; i >= 0; i--)
        {
            AddOptionToggleListener(optionToggleArray[i], i);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OptionToggleChange(currentOptionIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OptionToggleChange(currentOptionIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OptionValueChange(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OptionValueChange(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleSwitching();
        }
    }

    private void AddOptionToggleListener(Toggle toggle, int index)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                currentOptionIndex = index;
            }
        });
    }

    private void OptionToggleChange(int index)
    {
        currentOptionIndex = (index + optionToggleArray.Length) % optionToggleArray.Length;
        optionToggleArray[currentOptionIndex].isOn = true;
    }

    private void OptionValueChange(int value)
    {
        switch (currentOptionIndex)
        {
            case 0: noteSpeedSlider.value = (PlayerPrefs.GetInt("NoteSpeed") - 10.0f + value) / 190.0f; break;
            case 1: bgaOpacitySlider.value = (PlayerPrefs.GetInt("BGAOpacity") + value) / 10.0f; break;
            case 2: displayDelayCorrectionSlider.value = (PlayerPrefs.GetInt("DisplayDelayCorrection") + 100.0f + value) / 200.0f; break;
            case 3: earlyLateThresholdSlider.value = (PlayerPrefs.GetInt("EarlyLateThreshold") - 1.0f + value) / 21.0f; break;
            case 4: verticalLineSlider.value += value * 0.01f; break;
            case 5: keyFeedbackOpacitySlider.value += value * 0.01f; break;
            case 7: ChangeRandomEffector(value); break;
            case 8: fadeInSlider.value += value * 0.01f; break;
        }
    }

    private void ToggleSwitching()
    {
        switch (currentOptionIndex)
        {
            case 9: judgementTrackerToggle.switching = true; break;
            case 10: scoreGraphToggle.switching = true; break;
            case 11: judgeLineToggle.switching = true; break;
            case 12: earlyLateToggle.switching = true; break;
        }
    }

    public void SetGameplayOption()
    {
        noteSpeedSlider.value = (PlayerPrefs.GetInt("NoteSpeed") - 10.0f) / 190.0f;
        SetNoteSpeedValueText();
        bgaOpacitySlider.value = PlayerPrefs.GetInt("BGAOpacity") / 10.0f;
        SetBGAOpacityValueText();
        displayDelayCorrectionSlider.value = (PlayerPrefs.GetInt("DisplayDelayCorrection") + 100.0f) / 200.0f;
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

    public void BGAOpacitySliderValueChange(float value)
    {
        PlayerPrefs.SetInt("BGAOpacity", Mathf.RoundToInt(value * 10.0f));
        SetBGAOpacityValueText();
    }
    private void SetBGAOpacityValueText()
    {
        bgaOpacityValueText.text = (PlayerPrefs.GetInt("BGAOpacity") * 10) + "%";
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
        PlayerPrefs.SetInt("DisplayDelayCorrection", Mathf.RoundToInt(value * 200.0f - 100.0f));
        SetDisplayDelayCorrectionValueText();
    }
    private void SetDisplayDelayCorrectionValueText()
    {
        displayDelayCorrectionValueText.text = PlayerPrefs.GetInt("DisplayDelayCorrection") + "ms";
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
        verticalLineValueText.text = $"{(int)(PlayerPrefs.GetFloat("VerticalLine") * 100.0f)}%";
    }

    public void KeyFeedbackOpacitySliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("KeyFeedbackOpacity", value);
        SetKeyFeedbackOpacityValueText();
    }
    private void SetKeyFeedbackOpacityValueText()
    {
        keyFeedbackOpacityValueText.text = $"{(int)(PlayerPrefs.GetFloat("KeyFeedbackOpacity") * 100.0f)}%";
    }

    public void FadeInSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("FadeIn", value);
        SetFadeInValueText();
    }
    private void SetFadeInValueText()
    {
        fadeInValueText.text = $"{(int)(PlayerPrefs.GetFloat("FadeIn") * 100.0f)}%";
    }
}
