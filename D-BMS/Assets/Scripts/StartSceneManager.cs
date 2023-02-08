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

            PlayerPrefs.SetInt("AudioBufferSize", 256);
            PlayerPrefs.SetFloat("MasterVolume", 1.0f);
            PlayerPrefs.SetFloat("KeySoundVolume", 1.0f);
            PlayerPrefs.SetFloat("BGMVolume", 0.8f);

            PlayerPrefs.SetInt("AssistKeyUse", 1);
            PlayerPrefs.SetString("Key1", "D");
            PlayerPrefs.SetString("Key2", "F");
            PlayerPrefs.SetString("Key3", "J");
            PlayerPrefs.SetString("Key4", "K");
            PlayerPrefs.SetString("Key5", "L");
            PlayerPrefs.SetString("Key6", "G");
        }
    }

    private void SetSystemOption()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        Screen.SetResolution(1920, 1080, true);
        Screen.fullScreenMode = (FullScreenMode)(PlayerPrefs.GetInt("DisplayMode"));

        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        audioConfig.dspBufferSize = PlayerPrefs.GetInt("AudioBufferSize");
        AudioSettings.Reset(audioConfig);

        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
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
