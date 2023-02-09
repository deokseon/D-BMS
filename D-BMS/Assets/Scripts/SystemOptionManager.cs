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
    [SerializeField]
    private TextMeshProUGUI audioBufferText;
    [SerializeField]
    private GameObject audioBufferSelectPanel;
    [SerializeField]
    private TextMeshProUGUI masterVolumeText;
    [SerializeField]
    private Slider masterVolumeSlider;
    [SerializeField]
    private TextMeshProUGUI keySoundVolumeText;
    [SerializeField]
    private Slider keySoundVolumeSlider;
    [SerializeField]
    private TextMeshProUGUI bgmVolumeText;
    [SerializeField]
    private Slider bgmVolumeSlider;
    [SerializeField]
    private TextMeshProUGUI assistKeyUseText;
    [SerializeField]
    private TextMeshProUGUI[] keyText;
    [SerializeField]
    private GameObject waitKeyInputPanel;

    bool isChanging;

    private void Awake()
    {
        isChanging = false;

        SetFrameText();
        SetDisplayModeText();
        SetAudioBufferText();
        SetMasterVolumeText();
        SetKeySoundVolumeText();
        SetBGMVolumeText();
        SetAssistKeyUseText();
        for (int i = 0; i < 6; i++) { SetKeyText(i); }
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
        if (isChanging) { return; }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (frameSelectPanel.activeSelf) { frameSelectPanel.SetActive(false); }
            else if (displayModeSelectPanel.activeSelf) { displayModeSelectPanel.SetActive(false); }
            else if (audioBufferSelectPanel.activeSelf) { audioBufferSelectPanel.SetActive(false); }
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
    public void AudioBufferButtonClick()
    {
        audioBufferSelectPanel.SetActive(true);
    }

    public void FrameValueButtonClick(int value)
    {
        int syncCount = 1;
        int frame = -1;
        if (value > 0)
        {
            syncCount = 0;
            switch (value)
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

    public void DisplayModeValueButtonClick(int value)
    {
        PlayerPrefs.SetInt("DisplayMode", value);
        SetDisplayModeText();
        displayModeSelectPanel.SetActive(false);
        Screen.fullScreenMode = (FullScreenMode)value;
    }

    public void AudioBufferValueButtonClick(int value)
    {
        PlayerPrefs.SetInt("AudioBufferSize", value);
        SetAudioBufferText();
        audioBufferSelectPanel.SetActive(false);
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
    private void SetAudioBufferText()
    {
        switch (PlayerPrefs.GetInt("AudioBufferSize"))
        {
            case 256: audioBufferText.text = "Best Latency (256)"; break;
            case 512: audioBufferText.text = "Good Latency (512)"; break;
            case 1024: audioBufferText.text = "Best Performance (1024)"; break;
        }
    }

    public void MasterVolumeSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        SetMasterVolumeText();
    }

    public void SetMasterVolumeText()
    {
        float volumeValue = PlayerPrefs.GetFloat("MasterVolume");
        masterVolumeText.text = volumeValue.ToString("P0");
        if (volumeValue != masterVolumeSlider.value) { masterVolumeSlider.value = volumeValue; }
    }

    public void KeySoundVolumeSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("KeySoundVolume", value);
        SetKeySoundVolumeText();
    }

    public void SetKeySoundVolumeText()
    {
        float volumeValue = PlayerPrefs.GetFloat("KeySoundVolume");
        keySoundVolumeText.text = (volumeValue * 0.7f + 0.3f).ToString("P0");
        if (volumeValue != keySoundVolumeSlider.value) { keySoundVolumeSlider.value = volumeValue; }
    }

    public void BGMVolumeSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        SetBGMVolumeText();
    }

    public void SetBGMVolumeText()
    {
        float volumeValue = PlayerPrefs.GetFloat("BGMVolume");
        bgmVolumeText.text = (volumeValue * 0.7f + 0.3f).ToString("P0");
        if (volumeValue != bgmVolumeSlider.value) { bgmVolumeSlider.value = volumeValue; }
    }

    public void AssistKeyUseButtonClick()
    {
        PlayerPrefs.SetInt("AssistKeyUse", (PlayerPrefs.GetInt("AssistKeyUse") + 1) % 2);
        SetAssistKeyUseText();
    }

    private void SetAssistKeyUseText()
    {
        assistKeyUseText.text = PlayerPrefs.GetInt("AssistKeyUse") == 1 ? "Enabled" : "Disabled";
    }

    public void ChangeKey(int index)
    {
        if (!waitKeyInputPanel.activeSelf)
        {
            StartCoroutine(WaitKeyChange(index));
        }
    }

    private IEnumerator WaitKeyChange(int index)
    {
        isChanging = true;
        waitKeyInputPanel.SetActive(true);

        string key = PlayerPrefs.GetString($"Key{index + 1}");

        float timer = 0.0f;
        while (isChanging)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { isChanging = false; }
            #region keyinput
            else if (Input.GetKeyDown(KeyCode.BackQuote)) { key = "Backquote"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Backslash)) { key = "Backslash"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Equals)) { key = "Equals"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftBracket)) { key = "LeftBracket"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightBracket)) { key = "RightBracket"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Minus)) { key = "Minus"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Semicolon)) { key = "Semicolon"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Quote)) { key = "Quote"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Comma)) { key = "Comma"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Period)) { key = "Period"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Slash)) { key = "Slash"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftShift)) { key = "LeftShift"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightShift)) { key = "RightShift"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftAlt)) { key = "LeftAlt"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightAlt)) { key = "RightAlt"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftControl)) { key = "LeftCtrl"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightControl)) { key = "RightCtrl"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Space)) { key = "Space"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Return)) { key = "Enter"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Delete)) { key = "Delete"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.End)) { key = "End"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Home)) { key = "Home"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Insert)) { key = "Insert"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.PageDown)) { key = "PageDown"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.PageUp)) { key = "PageUp"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) { key = "LeftArrow"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) { key = "RightArrow"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) { key = "UpArrow"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) { key = "DownArrow"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadDivide)) { key = "NumpadDivide"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadEnter)) { key = "NumpadEnter"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadEquals)) { key = "NumpadEquals"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus)) { key = "NumpadMinus"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadMultiply)) { key = "NumpadMultiply"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadPeriod)) { key = "NumpadPeriod"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadPlus)) { key = "NumpadPlus"; isChanging = false; }
            else if (Input.anyKeyDown)
            {
                for (int i = 0; i < 26; i++)
                {
                    if (Input.GetKeyDown((KeyCode)(i + 'a')))
                    {
                        key = $"{(char)(i + 'A')}";
                        isChanging = false;
                        break;
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (Input.GetKeyDown((KeyCode)(i + 48)))
                    {
                        key = $"{i}";
                        isChanging = false;
                        break;
                    }
                    else if (Input.GetKeyDown((KeyCode)(i + 256)))
                    {
                        key = $"Numpad{i}";
                        isChanging = false;
                        break;
                    }
                }
            }
            #endregion

            timer += Time.deltaTime;
            if (timer >= 10.0f)
            {
                isChanging = false;
            }
            yield return null;
        }

        if (timer < 10.0f)
        {
            bool isDuplicate = false;
            for (int i = 1; i < 7; i++)
            {
                if (key.CompareTo(PlayerPrefs.GetString($"Key{i}")) == 0)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                PlayerPrefs.SetString($"Key{index + 1}", key);
            }
        }

        SetKeyText(index);
        waitKeyInputPanel.SetActive(false);
    }

    private void SetKeyText(int index)
    {
        keyText[index].text = PlayerPrefs.GetString($"Key{index + 1}");
    }
}
