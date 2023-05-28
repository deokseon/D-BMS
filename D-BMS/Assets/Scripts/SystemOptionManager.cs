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
    [SerializeField]
    private TextMeshProUGUI pollingRateValueText;
    [SerializeField]
    private GameObject pollingRateSelectPanel;
    [SerializeField]
    private GameObject[] audioOutputDeviceButton;
    [SerializeField]
    private GameObject audioOutputDeviceSelectPanel;
    [SerializeField]
    private TextMeshProUGUI audioOutputDeviceText;
    private int wasapiCount;
    private int asioCount;

    private readonly string[] keySettingString = { "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "SpeedUp1", "SpeedDown1", "SpeedUp2", "SpeedDown2" };
    bool isChanging;

    private void Awake()
    {
        isChanging = false;

        SetFrameText();
        SetDisplayModeText();
        SetAudioOutputDevice();
        SetAudioOutputDeviceText();
        SetAudioBufferText();
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        SetMasterVolumeText();
        keySoundVolumeSlider.value = PlayerPrefs.GetFloat("KeySoundVolume");
        SetKeySoundVolumeText();
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume");
        SetBGMVolumeText();
        SetAssistKeyUseText();
        SetPollingRateText();
        for (int i = 0; i < keyText.Length; i++) { SetKeyText(i); }
        StartCoroutine(PrepareVideo());
    }

    private void SetAudioOutputDevice()
    {
        for (int i = 0; i < 8; i++)
        {
            audioOutputDeviceButton[i].SetActive(false);
        }
        string originalOutputType = PlayerPrefs.GetString("OutputType");
        string originalDriverName = PlayerPrefs.GetString("DriverName");
        PlayerPrefs.SetString("OutputType", "WASAPI");
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }
        FMODUnity.RuntimeManager.CoreSystem.getNumDrivers(out wasapiCount);
        for (int i = 0; i < wasapiCount; i++)
        {
            if (i > 7) { break; }
            string name;
            FMOD.SPEAKERMODE speakerMode;
            int channels, systemrate;
            System.Guid guid;
            FMODUnity.RuntimeManager.CoreSystem.getDriverInfo(i, out name, 50, out guid, out systemrate, out speakerMode, out channels);
            audioOutputDeviceButton[i].SetActive(true);
            audioOutputDeviceButton[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
        }

        PlayerPrefs.SetString("OutputType", "ASIO");
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }
        FMOD.OUTPUTTYPE currentOutputType;
        FMODUnity.RuntimeManager.CoreSystem.getOutput(out currentOutputType);
        if (currentOutputType == FMOD.OUTPUTTYPE.ASIO)
        {
            FMODUnity.RuntimeManager.CoreSystem.getNumDrivers(out asioCount);
            for (int i = 0; i < asioCount; i++)
            {
                if (i + wasapiCount > 7) { break; }
                string name;
                FMOD.SPEAKERMODE speakerMode;
                int channels, systemrate;
                System.Guid guid;
                FMODUnity.RuntimeManager.CoreSystem.getDriverInfo(i, out name, 50, out guid, out systemrate, out speakerMode, out channels);
                audioOutputDeviceButton[i + wasapiCount].SetActive(true);
                audioOutputDeviceButton[i + wasapiCount].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "ASIO : " + name;
            }
        }

        PlayerPrefs.SetString("OutputType", originalOutputType);
        PlayerPrefs.SetString("DriverName", originalDriverName);
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }
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

    private IEnumerator CoLoadScene(int scene)
    {
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
            else if (audioOutputDeviceSelectPanel.activeSelf) { audioOutputDeviceSelectPanel.SetActive(false); }
            else if (audioBufferSelectPanel.activeSelf) { audioBufferSelectPanel.SetActive(false); }
            else if (pollingRateSelectPanel.activeSelf) { pollingRateSelectPanel.SetActive(false); }
            else 
            {
                if (fadeImage.IsActive()) { return; }
                StartCoroutine(CoLoadScene(0));
            }
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
    public void AudioOutputDeviceButtonClick()
    {
        audioOutputDeviceSelectPanel.SetActive(true);
    }
    public void AudioBufferButtonClick()
    {
        audioBufferSelectPanel.SetActive(true);
    }
    public void PollingRateButtonClick()
    {
        pollingRateSelectPanel.SetActive(true);
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

    public void AudioOutputDeviceValueButtonClick(int index)
    {
        string buttonText = audioOutputDeviceButton[index].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text;
        if (buttonText.Substring(0, 7).CompareTo("ASIO : ") == 0)
        {
            PlayerPrefs.SetString("OutputType", "ASIO");
            PlayerPrefs.SetString("DriverName", buttonText.Substring(7));
        }
        else
        {
            PlayerPrefs.SetString("OutputType", "WASAPI");
            PlayerPrefs.SetString("DriverName", buttonText.Substring(0));
        }
        SetAudioOutputDeviceText();
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }
        audioOutputDeviceSelectPanel.SetActive(false);
    }

    public void AudioBufferValueButtonClick(int value)
    {
        PlayerPrefs.SetInt("AudioBufferSize", value);
        SetAudioBufferText();
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }
        audioBufferSelectPanel.SetActive(false);
    }

    public void PollingRateValueButtonClick(int value)
    {
        PlayerPrefs.SetInt("PollingRate", value);
        SetPollingRateText();
        pollingRateSelectPanel.SetActive(false);
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
    private void SetAudioOutputDeviceText()
    {
        audioOutputDeviceText.text = PlayerPrefs.GetString("OutputType").CompareTo("ASIO") == 0 ? "ASIO : " + PlayerPrefs.GetString("DriverName") :
                                                                                                    PlayerPrefs.GetString("DriverName");
    }
    private void SetAudioBufferText()
    {
        switch (PlayerPrefs.GetInt("AudioBufferSize"))
        {
            case 16: audioBufferText.text = "Extreme (32)"; break;
            case 32: audioBufferText.text = "Ultra Low (64)"; break;
            case 64: audioBufferText.text = "Very Low (128)"; break;
            case 128: audioBufferText.text = "Low (256)"; break;
            case 256: audioBufferText.text = "Medium (512)"; break;
            case 384: audioBufferText.text = "High (768)"; break;
            case 512: audioBufferText.text = "Very High (1024)"; break;
        }
    }

    public void MasterVolumeSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        SetMasterVolumeText();
    }

    private void SetMasterVolumeText()
    {
        masterVolumeText.text = PlayerPrefs.GetFloat("MasterVolume").ToString("P0");
    }

    public void KeySoundVolumeSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("KeySoundVolume", value);
        SetKeySoundVolumeText();
    }

    private void SetKeySoundVolumeText()
    {
        keySoundVolumeText.text = (PlayerPrefs.GetFloat("KeySoundVolume") * 0.7f + 0.3f).ToString("P0");
    }

    public void BGMVolumeSliderValueChange(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        SetBGMVolumeText();
    }

    private void SetBGMVolumeText()
    {
        bgmVolumeText.text = (PlayerPrefs.GetFloat("BGMVolume") * 0.7f + 0.3f).ToString("P0");
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

    private void SetPollingRateText()
    {
        pollingRateValueText.text = PlayerPrefs.GetInt("PollingRate") + "Hz";
    }

    public void ChangeKey(int index)
    {
        if (!waitKeyInputPanel.activeSelf)
        {
            StartCoroutine(WaitKeyChange(index));
        }
    }

    /*private IEnumerator WaitKeyChange(int index)
    {
        isChanging = true;
        waitKeyInputPanel.SetActive(true);

        string key = PlayerPrefs.GetString(keySettingString[index]);

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
            else if (Input.GetKeyDown(KeyCode.F1)) { key = "F1"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F2)) { key = "F2"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F3)) { key = "F3"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F4)) { key = "F4"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F6)) { key = "F6"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F9)) { key = "F9"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F10)) { key = "F10"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F11)) { key = "F11"; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F12)) { key = "F12"; isChanging = false; }
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
            for (int i = 0; i < keyText.Length; i++)
            {
                if (key.CompareTo(PlayerPrefs.GetString(keySettingString[i])) == 0)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                PlayerPrefs.SetString(keySettingString[index], key);
            }
        }

        SetKeyText(index);
        waitKeyInputPanel.SetActive(false);
    }*/

    private IEnumerator WaitKeyChange(int index)
    {
        isChanging = true;
        waitKeyInputPanel.SetActive(true);

        int key = PlayerPrefs.GetInt(keySettingString[index]);

        float timer = 0.0f;
        while (isChanging)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { isChanging = false; }
            #region keyinput
            else if (Input.GetKeyDown(KeyCode.BackQuote)) { key = 0xC0; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Backslash)) { key = 0xDC; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Equals)) { key = 0xBB; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftBracket)) { key = 0xDB; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightBracket)) { key = 0xDD; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Minus)) { key = 0xBD; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Semicolon)) { key = 0xBA; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Quote)) { key = 0xDE; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Comma)) { key = 0xBC; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Period)) { key = 0xBE; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Slash)) { key = 0xBF; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftShift)) { key = 0xA0; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightShift)) { key = 0xA1; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftAlt)) { key = 0xA4; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightAlt)) { key = 0xA5; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftControl)) { key = 0xA2; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightControl)) { key = 0xA3; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Space)) { key = 0x20; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Return)) { key = 0x0D; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Delete)) { key = 0x2E; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.End)) { key = 0x23; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Home)) { key = 0x24; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Insert)) { key = 0x2D; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.PageDown)) { key = 0x22; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.PageUp)) { key = 0x21; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) { key = 0x25; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) { key = 0x27; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) { key = 0x26; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) { key = 0x28; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadDivide)) { key = 0x6F; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadEnter)) { key = 0x6C; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus)) { key = 0x6D; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadMultiply)) { key = 0x6A; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadPeriod)) { key = 0x6E; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.KeypadPlus)) { key = 0x6B; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F1)) { key = 0x70; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F2)) { key = 0x71; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F3)) { key = 0x72; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F4)) { key = 0x73; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F6)) { key = 0x75; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F9)) { key = 0x78; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F10)) { key = 0x79; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F11)) { key = 0x7A; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F12)) { key = 0x7B; isChanging = false; }
            else if (Input.anyKeyDown)
            {
                for (int i = 0; i < 26; i++)
                {
                    if (Input.GetKeyDown((KeyCode)(i + 'a')))
                    {
                        key = 0x41 + ((int)(KeyCode)(i + 'a') - (int)KeyCode.A);
                        isChanging = false;
                        break;
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (Input.GetKeyDown((KeyCode)(i + 48)))
                    {
                        key = 0x30 + ((int)(KeyCode)(i + 48) - (int)KeyCode.Alpha0);
                        isChanging = false;
                        break;
                    }
                    else if (Input.GetKeyDown((KeyCode)(i + 256)))
                    {
                        key = 0x60 + ((int)(KeyCode)(i + 256) - (int)KeyCode.Keypad0);
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
            for (int i = 0; i < keyText.Length; i++)
            {
                if (key.CompareTo(PlayerPrefs.GetInt(keySettingString[i])) == 0)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                PlayerPrefs.SetInt(keySettingString[index], key);
            }
        }

        SetKeyText(index);
        waitKeyInputPanel.SetActive(false);
    }

    private void SetKeyText(int index)
    {
        keyText[index].text = KeyCodeToString(PlayerPrefs.GetInt(keySettingString[index]));
    }

    private string KeyCodeToString(int key)
    {
        string value = "";
        switch (key)
        {
            case 0x0D: value = "Enter"; break;
            case 0x20: value = "Space"; break;
            case 0x21: value = "PageUp"; break;
            case 0x22: value = "PageDown"; break;
            case 0x23: value = "End"; break;
            case 0x24: value = "Home"; break;
            case 0x25: value = "LeftArrow"; break;
            case 0x26: value = "UpArrow"; break;
            case 0x27: value = "RightArrow"; break;
            case 0x28: value = "DownArrow"; break;
            case 0x2D: value = "Insert"; break;
            case 0x2E: value = "Delete"; break;
            case 0x30: value = "0"; break;
            case 0x31: value = "1"; break;
            case 0x32: value = "2"; break;
            case 0x33: value = "3"; break;
            case 0x34: value = "4"; break;
            case 0x35: value = "5"; break;
            case 0x36: value = "6"; break;
            case 0x37: value = "7"; break;
            case 0x38: value = "8"; break;
            case 0x39: value = "9"; break;
            case 0x41: value = "A"; break;
            case 0x42: value = "B"; break;
            case 0x43: value = "C"; break;
            case 0x44: value = "D"; break;
            case 0x45: value = "E"; break;
            case 0x46: value = "F"; break;
            case 0x47: value = "G"; break;
            case 0x48: value = "H"; break;
            case 0x49: value = "I"; break;
            case 0x4A: value = "J"; break;
            case 0x4B: value = "K"; break;
            case 0x4C: value = "L"; break;
            case 0x4D: value = "M"; break;
            case 0x4E: value = "N"; break;
            case 0x4F: value = "O"; break;
            case 0x50: value = "P"; break;
            case 0x51: value = "Q"; break;
            case 0x52: value = "R"; break;
            case 0x53: value = "S"; break;
            case 0x54: value = "T"; break;
            case 0x55: value = "U"; break;
            case 0x56: value = "V"; break;
            case 0x57: value = "W"; break;
            case 0x58: value = "X"; break;
            case 0x59: value = "Y"; break;
            case 0x5A: value = "Z"; break;
            case 0x60: value = "NumPad0"; break;
            case 0x61: value = "NumPad1"; break;
            case 0x62: value = "NumPad2"; break;
            case 0x63: value = "NumPad3"; break;
            case 0x64: value = "NumPad4"; break;
            case 0x65: value = "NumPad5"; break;
            case 0x66: value = "NumPad6"; break;
            case 0x67: value = "NumPad7"; break;
            case 0x68: value = "NumPad8"; break;
            case 0x69: value = "NumPad9"; break;
            case 0x6A: value = "NumPadMultiply"; break;
            case 0x6B: value = "NumpadPlus"; break;
            case 0x6C: value = "NumpadEnter"; break;
            case 0x6D: value = "NumpadMinus"; break;
            case 0x6E: value = "NumpadPeriod"; break;
            case 0x6F: value = "NumpadDivide"; break;
            case 0x70: value = "F1"; break;
            case 0x71: value = "F2"; break;
            case 0x72: value = "F3"; break;
            case 0x73: value = "F4"; break;
            case 0x75: value = "F6"; break;
            case 0x78: value = "F9"; break;
            case 0x79: value = "F10"; break;
            case 0x7A: value = "F11"; break;
            case 0x7B: value = "F12"; break;
            case 0xA0: value = "LeftShift"; break;
            case 0xA1: value = "RightShift"; break;
            case 0xA2: value = "LeftCtrl"; break;
            case 0xA3: value = "RightCtrl"; break;
            case 0xA4: value = "LeftAlt"; break;
            case 0xA5: value = "RightAlt"; break;
            case 0xBA: value = "Semicolon"; break;
            case 0xBB: value = "Equals"; break;
            case 0xBC: value = "Comma"; break;
            case 0xBD: value = "Minus"; break;
            case 0xBE: value = "Period"; break;
            case 0xBF: value = "Slash"; break;
            case 0xC0: value = "Backquote"; break;
            case 0xDB: value = "LeftBracket"; break;
            case 0xDC: value = "Backslash"; break;
            case 0xDD: value = "RightBracket"; break;
            case 0xDE: value = "Quote"; break;
        }
        return value;
    }
}
