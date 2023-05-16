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
    private Button startButton;

    private bool isLoadScene;

    private void Awake()
    {
        InitializeSystemOption();
        InitializeGamePlayOption();
        SetSystemOption();

        isLoadScene = false;

        StartCoroutine(PrepareVideo());
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitButtonClick();
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

        isLoadScene = true;
        StartCoroutine(CoFadeOut());
        startButton.Select();
    }

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(1.0f);
        fadeImage.gameObject.SetActive(false);
        isLoadScene = false;
    }

    public void ButtonClick(int scene)
    {
        if (isLoadScene) { return; }
        isLoadScene = true;
        StartCoroutine(CoLoadScene(scene));
    }

    private IEnumerator CoLoadScene(int scene)
    {
        fadeAnimator.gameObject.SetActive(true);
        fadeAnimator.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void ExitButtonClick()
    {
        if (isLoadScene) { return; }
        isLoadScene = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
