using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField]
    private Animator fadeAnimator;
    [SerializeField]
    private Image fadeImage;
    [SerializeField]
    private RawImage screen;
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField]
    private Toggle[] toggleArray;
    private int currentIndex;

    private void Awake()
    {
        currentIndex = 0;
        InitializeSystemOption();
        InitializeGamePlayOption();
        SetSystemOption();

        StartCoroutine(PrepareVideo());
    }


    private void Update()
    {
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
            PlayerPrefs.SetFloat("BGMVolume", 0.8f);

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
            PlayerPrefs.SetInt("NoteSkin", 0);
            PlayerPrefs.SetInt("JudgeLine", 0);
        }
    }

    private void SetSystemOption()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        Screen.SetResolution(1920, 1080, true);
        Screen.fullScreenMode = (FullScreenMode)(PlayerPrefs.GetInt("DisplayMode"));
    }

    private IEnumerator PrepareVideo()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared) { yield return null; }

        screen.texture = videoPlayer.texture;

        videoPlayer.Play();

        StartCoroutine(CoFadeOut());
    }

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeAnimator.SetTrigger("FadeOut");
    }

    public void ToggleChange(int index)
    {
        if (fadeImage.IsActive()) { return; }
        if (index >= 0 && index < toggleArray.Length)
        {
            currentIndex = index;
            toggleArray[currentIndex].isOn = true;
        }
    }

    public void ToggleExecute()
    {
        if (fadeImage.IsActive()) { return; }
        if (currentIndex == 0)
        {
            StartCoroutine(CoLoadScene(1));
        }
        else if (currentIndex == 1)
        {
            StartCoroutine(CoLoadScene(4));
        }
        else
        {
            ExitButtonClick();
        }
    }

    private IEnumerator CoLoadScene(int scene)
    {
        fadeAnimator.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

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
