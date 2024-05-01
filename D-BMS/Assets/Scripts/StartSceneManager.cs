﻿using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartSceneManager : MonoBehaviour
{
    private Texture bgImageTexture;

    [SerializeField]
    private Image fadeImage;
    [SerializeField]
    private ToggleGroup toggleGroup;
    [SerializeField]
    private Toggle[] toggleArray;
    [SerializeField]
    private TextMeshProUGUI[] toggleTextArray;
    [SerializeField]
    private GameObject convertingNoticeBoard;
    private int currentIndex;

    private void Awake()
    {
        currentIndex = 0;
        InitializeSystemOption();
        InitializeGamePlayOption();
        SetSystemOption();

        for (int i = toggleArray.Length - 1; i >= 0; i--)
        {
            AddToggleListener(toggleArray[i], i, toggleTextArray[i]);
        }

        toggleArray[currentIndex].isOn = true;
        toggleGroup.allowSwitchOff = false;

        _ = SetBackground();
    }

    private async UniTask SetBackground()
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\start-bg";

        bgImageTexture = await FindObjectOfType<TextureDownloadManager>().GetTexture(filePath);
        if (bgImageTexture != null)
        {
            GameObject.Find("Screen").GetComponent<RawImage>().texture = bgImageTexture;
        }
        else
        {
            await FindObjectOfType<TextureDownloadManager>().PrepareVideo(filePath, "VideoPlayer", "Screen");
        }
        _ = FadeOut();
    }

    private void Update()
    {
        if (convertingNoticeBoard.activeSelf)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitButtonClick();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleExecute();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ToggleChange(currentIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ToggleChange(currentIndex - 1);
        }
    }

    public void OptionReset()
    {
        PlayerPrefs.SetInt("SystemOption", 0);
        PlayerPrefs.SetInt("GamePlayOption", 0);
        InitializeSystemOption();
        InitializeGamePlayOption();
    }

    private void InitializeSystemOption()
    {
        if (PlayerPrefs.GetInt("SystemOption") == 0)
        {
            PlayerPrefs.SetInt("SystemOption", 1);

            PlayerPrefs.SetInt("DisplayMode", 1);
            PlayerPrefs.SetInt("FrameRate", -1);
            PlayerPrefs.SetInt("SyncCount", 1);

            PlayerPrefs.SetString("OutputType", "WASAPI");
            PlayerPrefs.SetString("DriverName", "None");
            PlayerPrefs.SetInt("AudioBufferSize", 64);
            PlayerPrefs.SetFloat("MasterVolume", 1.0f);
            PlayerPrefs.SetFloat("KeySoundVolume", 1.0f);
            PlayerPrefs.SetFloat("BGMVolume", 5.0f / 7.0f);

            PlayerPrefs.SetInt("AssistKeyUse", 1);
            PlayerPrefs.SetInt("PollingRate", 1000);
            PlayerPrefs.SetInt("Key1", 0x44);
            PlayerPrefs.SetInt("Key2", 0x46);
            PlayerPrefs.SetInt("Key3", 0x4A);
            PlayerPrefs.SetInt("Key4", 0x4B);
            PlayerPrefs.SetInt("Key5", 0x4C);
            PlayerPrefs.SetInt("Key6", 0x47);
            PlayerPrefs.SetInt("SpeedUp1", 0x71);
            PlayerPrefs.SetInt("SpeedDown1", 0x70);
            PlayerPrefs.SetInt("SpeedUp2", 0x79);
            PlayerPrefs.SetInt("SpeedDown2", 0x78);
        }
    }

    private void InitializeGamePlayOption()
    {
        if (PlayerPrefs.GetInt("GamePlayOption") == 0)
        {
            PlayerPrefs.SetInt("GamePlayOption", 1);

            PlayerPrefs.SetInt("SortBy", 0);
            PlayerPrefs.SetInt("Category", 0);
            PlayerPrefs.SetInt("Category0Index", 0);
            PlayerPrefs.SetInt("Category1Index", 0);
            PlayerPrefs.SetInt("Category2Index", 0);
            PlayerPrefs.SetInt("NoteSpeed", 50);
            PlayerPrefs.SetInt("BGAOpacity", 10);
            PlayerPrefs.SetInt("RandomEffector", 0);
            PlayerPrefs.SetInt("DisplayDelayCorrection", 0);
            PlayerPrefs.SetInt("EarlyLateThreshold", 22);
            PlayerPrefs.SetFloat("VerticalLine", 0.0f);
            PlayerPrefs.SetFloat("KeyFeedbackOpacity", 0.7f);
            PlayerPrefs.SetFloat("OddKeyFeedbackColorR", 1.0f);
            PlayerPrefs.SetFloat("OddKeyFeedbackColorG", 1.0f);
            PlayerPrefs.SetFloat("OddKeyFeedbackColorB", 1.0f);
            PlayerPrefs.SetFloat("EvenKeyFeedbackColorR", 1.0f);
            PlayerPrefs.SetFloat("EvenKeyFeedbackColorG", 0.0f);
            PlayerPrefs.SetFloat("EvenKeyFeedbackColorB", 0.0f);
            PlayerPrefs.SetInt("JudgeLine", 0);
            PlayerPrefs.SetFloat("FadeIn", 0.0f);
            PlayerPrefs.SetInt("JudgementTracker", 1);
            PlayerPrefs.SetInt("ScoreGraph", 1);
            PlayerPrefs.SetInt("EarlyLate", 1);

            PlayerPrefs.SetFloat("NoteXPosition", -7.63f);
        }
    }

    private void SetSystemOption()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        Screen.SetResolution(1920, 1080, true);
        Screen.fullScreenMode = (FullScreenMode)(PlayerPrefs.GetInt("DisplayMode"));
    }

    private async UniTask FadeOut()
    {
        await UniTask.Delay(500);
        fadeImage.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private void AddToggleListener(Toggle toggle, int index, TextMeshProUGUI toggleText)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                currentIndex = index;
                toggleText.color = new Color32(0, 255, 255, 255);
                if (toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick)
                {
                    toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick = false;
                    ToggleExecute();
                }
            }
            else
            {
                toggleText.color = new Color32(220, 220, 220, 255);
            }
        });
    }

    private void ToggleChange(int index)
    {
        if (fadeImage.IsActive()) { return; }
        currentIndex = (index + toggleArray.Length) % toggleArray.Length;
        toggleArray[currentIndex].isOn = true;
    }

    private void ToggleExecute()
    {
        if (fadeImage.IsActive()) { return; }
        if (currentIndex == 0)
        {
            _ = LoadScene(1);
        }
        else if (currentIndex == 1)
        {
            _ = LoadScene(4);
        }
        else if (currentIndex == 2)
        {
            _ = LoadScene(5);
        }
        else
        {
            ExitButtonClick();
        }
    }

    private void TextureDestroy()
    {
        if (bgImageTexture != null)
        {
            Destroy(bgImageTexture);
        }
    }

    private async UniTask LoadScene(int scene)
    {
        fadeImage.GetComponent<Animator>().SetTrigger("FadeIn");

        await UniTask.Delay(1000);

        TextureDestroy();

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void ExitButtonClick()
    {
        if (fadeImage.IsActive()) { return; }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
