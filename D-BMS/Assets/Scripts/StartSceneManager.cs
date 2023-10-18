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
    [SerializeField]
    private Image fadeImage;
    [SerializeField]
    private ToggleGroup toggleGroup;
    [SerializeField]
    private Toggle[] toggleArray;
    [SerializeField]
    private TextMeshProUGUI[] toggleTextArray;
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

        SetBackground();
    }


    private void SetBackground()
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\start-bg";
        if (File.Exists(filePath + ".jpg"))
        {
            StartCoroutine(LoadBG(filePath + ".jpg"));
        }
        else if (File.Exists(filePath + ".png"))
        {
            StartCoroutine(LoadBG(filePath + ".png"));
        }
        else if (File.Exists(filePath + ".mp4"))
        {
            StartCoroutine(PrepareVideo(filePath + ".mp4"));
        }
        else if (File.Exists(filePath + ".avi"))
        {
            StartCoroutine(PrepareVideo(filePath + ".avi"));
        }
        else if (File.Exists(filePath + ".wmv"))
        {
            StartCoroutine(PrepareVideo(filePath + ".wmv"));
        }
        else if (File.Exists(filePath + ".mpeg"))
        {
            StartCoroutine(PrepareVideo(filePath + ".mpeg"));
        }
        else if (File.Exists(filePath + ".mpg"))
        {
            StartCoroutine(PrepareVideo(filePath + ".mpg"));
        }
    }

    private IEnumerator LoadBG(string path)
    {
        string imagePath = $@"file:\\{path}";

        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath);
        yield return uwr.SendWebRequest();

        GameObject.Find("Screen").GetComponent<RawImage>().texture = (uwr.downloadHandler as DownloadHandlerTexture).texture;

        StartCoroutine(CoFadeOut());
    }


    private IEnumerator PrepareVideo(string path)
    {
        VideoPlayer videoPlayer = GameObject.Find("VideoPlayer").GetComponent<VideoPlayer>();
        videoPlayer.url = $"file://{path}";

        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);

        GameObject.Find("Screen").GetComponent<RawImage>().texture = videoPlayer.texture;

        videoPlayer.Play();

        StartCoroutine(CoFadeOut());
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

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
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
            StartCoroutine(CoLoadScene(1));
        }
        else if (currentIndex == 1)
        {
            StartCoroutine(CoLoadScene(4));
        }
        else if (currentIndex == 2)
        {
            StartCoroutine(CoLoadScene(5));
        }
        else
        {
            ExitButtonClick();
        }
    }

    private IEnumerator CoLoadScene(int scene)
    {
        fadeImage.GetComponent<Animator>().SetTrigger("FadeIn");

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
