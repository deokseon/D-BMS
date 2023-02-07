using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class SystemOptionManager : MonoBehaviour
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
    private TextMeshProUGUI frameValueText;
    [SerializeField]
    private GameObject frameSelectPanel;
    [SerializeField]
    private TextMeshProUGUI displayModeText;
    [SerializeField]
    private GameObject displayModeSelectPanel;

    private void Awake()
    {
        SetFrameText();
        SetDisplayModeText();
        StartCoroutine(PrepareVideo());
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
        yield return new WaitForSecondsRealtime(1.0f);
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator CoLoadScene(int scene)
    {
        fadeAnimator.gameObject.SetActive(true);
        fadeAnimator.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (frameSelectPanel.activeSelf) { frameSelectPanel.SetActive(false); }
            else if (displayModeSelectPanel.activeSelf) { displayModeSelectPanel.SetActive(false); }
            else { StartCoroutine(CoLoadScene(0)); }
        }
    }

    public void FrameButtonClick()
    {
        frameSelectPanel.SetActive(true);
    }
    public void DisplayModeButtonClick()
    {
        displayModeSelectPanel.SetActive(true);
    }

    public void FrameValueButtonClick(int index)
    {
        int syncCount = 1;
        int frame = -1;
        if (index > 0)
        {
            syncCount = 0;
            switch (index)
            {
                case 2: frame = 30; break;
                case 3: frame = 60; break;
                case 4: frame = 75; break;
                case 5: frame = 120; break;
                case 6: frame = 144; break;
                case 7: frame = 180; break;
                case 8: frame = 240; break;
                case 9: frame = 300; break;
            }
        }
        PlayerPrefs.SetInt("SyncCount", syncCount);
        PlayerPrefs.SetInt("FrameRate", frame);
        SetFrameText();
        frameSelectPanel.SetActive(false);
    }

    public void DisplayModeValueButtonClick(int index)
    {
        PlayerPrefs.SetInt("DisplayMode", index);
        SetDisplayModeText();
        displayModeSelectPanel.SetActive(false);
        Screen.fullScreenMode = (FullScreenMode)index;
    }

    private void SetFrameText()
    {
        if (PlayerPrefs.GetInt("SyncCount") == 1) { frameValueText.text = "V-Sync"; }
        else
        {
            int frame = PlayerPrefs.GetInt("FrameRate");
            if (frame == -1) { frameValueText.text = "Unlimited"; }
            else { frameValueText.text = frame + " FPS"; }
        }
    }

    private void SetDisplayModeText()
    {
        switch (PlayerPrefs.GetInt("DisplayMode"))
        {
            case 0: displayModeText.text = "Fullscreen"; break;
            case 1: displayModeText.text = "Fullscreen Window"; break;
            case 3: displayModeText.text = "Windowed"; break;
        }
    }
}
