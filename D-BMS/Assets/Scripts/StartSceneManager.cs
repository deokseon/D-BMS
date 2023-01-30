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
        SetFrameRate();

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

    private void SetFrameRate()
    {
        if (PlayerPrefs.GetInt("FrameRate") == 0)
        {
            PlayerPrefs.SetInt("FrameRate", -1);
            PlayerPrefs.SetInt("SyncCount", 1);
            SetFrameRate();
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
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
