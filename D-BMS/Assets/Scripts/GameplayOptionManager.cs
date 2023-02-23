﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private int randomEffectorCount;

    private void Awake()
    {
        randomEffectorCount = System.Enum.GetValues(typeof(RandomEffector)).Length;
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
        SetRandomEffector(PlayerPrefs.GetInt("RandomEffector"));
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
}
