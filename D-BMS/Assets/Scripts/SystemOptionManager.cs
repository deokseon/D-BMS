using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System;
using System.IO;
using UnityEngine.Networking;

public class SystemOptionManager : MonoBehaviour
{
    private Texture bgImageTexture;

    [SerializeField]
    private Image fadeImage;
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
    private int outputDeviceCount;

    [SerializeField]
    private Scrollbar scrollbar;
    [SerializeField]
    private RectTransform[] optionToggleRT;
    [SerializeField]
    private Toggle[] optionToggleArray;
    [SerializeField]
    private Toggle[] frameToggleArray;
    [SerializeField]
    private Toggle[] displayModeToggleArray;
    [SerializeField]
    private Toggle[] outputDeviceToggleArray;
    [SerializeField]
    private Toggle[] audioBufferToggleArray;
    [SerializeField]
    private Toggle[] pollingRateToggleArray;
    private List<Toggle[]> toggleArrayList;
    private List<Action<int>> valueToggleFunctionList;
    private int currentOptionIndex;
    private int[] currentValueToggleIndex = { 0, 0, 0, 0, 0 };
    private bool isUpDownPress = false;

    private readonly string[] keySettingString = { "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "SpeedUp1", "SpeedDown1", "SpeedUp2", "SpeedDown2" };
    private bool isChanging;

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

        for (int i = optionToggleArray.Length - 1; i >= 0; i--)
        {
            AddOptionToggleListener(optionToggleArray[i], i);
        }

        toggleArrayList = new List<Toggle[]>(5);
        toggleArrayList.Add(frameToggleArray);
        toggleArrayList.Add(displayModeToggleArray);
        toggleArrayList.Add(outputDeviceToggleArray);
        toggleArrayList.Add(audioBufferToggleArray);
        toggleArrayList.Add(pollingRateToggleArray);

        valueToggleFunctionList = new List<Action<int>>(5);
        valueToggleFunctionList.Add(FrameValueToggleClick);
        valueToggleFunctionList.Add(DisplayModeValueToggleClick);
        valueToggleFunctionList.Add(AudioOutputDeviceValueToggleClick);
        valueToggleFunctionList.Add(AudioBufferValueToggleClick);
        valueToggleFunctionList.Add(PollingRateValueToggleClick);

        for (int i = 0; i < 5; i++)
        {
            for (int j = toggleArrayList[i].Length - 1; j >= 0; j--)
            {
                AddValueToggleListener(toggleArrayList[i][j], i, j);
            }
        }

        //StartCoroutine(PrepareVideo());
        SetBackground();
    }
    private void SetBackground()
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\option-bg";
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

        bgImageTexture = (uwr.downloadHandler as DownloadHandlerTexture).texture;

        GameObject.Find("Screen").GetComponent<RawImage>().texture = bgImageTexture;

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

    private void FitTextSize(TextMeshProUGUI text, int initFontSize, int maxSize)
    {
        text.fontSize = initFontSize;
        while (text.preferredWidth > maxSize)
        {
            text.fontSize -= 0.1f;
        }
    }

    private void AddOptionToggleListener(Toggle toggle, int index)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                currentOptionIndex = index;
                if (isUpDownPress)
                {
                    scrollbar.value = Mathf.Abs(optionToggleRT[optionToggleRT.Length - 1].localPosition.y - optionToggleRT[index].localPosition.y) / 1685.0f;
                    isUpDownPress = false;
                }
                if (toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick)
                {
                    toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick = false;
                    OptionToggleExecute();
                }
            }
        });
    }

    private void OptionToggleChange(int index)
    {
        if (fadeImage.IsActive()) { return; }
        currentOptionIndex = (index + optionToggleArray.Length) % optionToggleArray.Length;
        optionToggleArray[currentOptionIndex].isOn = true;
    }

    private void OptionToggleExecute()
    {
        if (fadeImage.IsActive()) { return; }
        switch (currentOptionIndex)
        {
            case 0: StartCoroutine(ChangeValue(frameSelectPanel, 0, frameToggleArray.Length)); break;
            case 1: StartCoroutine(ChangeValue(displayModeSelectPanel, 1, displayModeToggleArray.Length)); break;
            case 3: StartCoroutine(ChangeValue(audioOutputDeviceSelectPanel, 2, outputDeviceCount)); break;
            case 4: StartCoroutine(ChangeValue(audioBufferSelectPanel, 3, audioBufferToggleArray.Length)); break;
            case 8: AssistKeyUseToggleClick(); break;
            case 9: StartCoroutine(ChangeValue(pollingRateSelectPanel, 4, pollingRateToggleArray.Length)); break;
            case 10: ChangeKey(0); break;
            case 11: ChangeKey(1); break;
            case 12: ChangeKey(2); break;
            case 13: ChangeKey(5); break;
            case 14: ChangeKey(3); break;
            case 15: ChangeKey(4); break;
            case 16: ChangeKey(6); break;
            case 17: ChangeKey(7); break;
            case 18: ChangeKey(8); break;
            case 19: ChangeKey(9); break;
        }
    }

    private void AddValueToggleListener(Toggle toggle, int category, int index)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                currentValueToggleIndex[category] = index;
                if (toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick)
                {
                    toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick = false;
                    valueToggleFunctionList[category](index);
                }
            }
        });
    }

    private void SetAudioOutputDevice()
    {
        for (int i = 0; i < 10; i++)
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
            if (i > 9) { break; }
            string name;
            FMOD.SPEAKERMODE speakerMode;
            int channels, systemrate;
            System.Guid guid;
            FMODUnity.RuntimeManager.CoreSystem.getDriverInfo(i, out name, 50, out guid, out systemrate, out speakerMode, out channels);
            audioOutputDeviceButton[i].SetActive(true);
            audioOutputDeviceButton[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = name;
            if (originalDriverName.CompareTo("None") == 0)
            {
                originalOutputType = "WASAPI";
                originalDriverName = name;
            }
            FitTextSize(audioOutputDeviceButton[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>(), 20, 420);
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
                if (i + wasapiCount > 9) { break; }
                string name;
                FMOD.SPEAKERMODE speakerMode;
                int channels, systemrate;
                System.Guid guid;
                FMODUnity.RuntimeManager.CoreSystem.getDriverInfo(i, out name, 50, out guid, out systemrate, out speakerMode, out channels);
                audioOutputDeviceButton[i + wasapiCount].SetActive(true);
                audioOutputDeviceButton[i + wasapiCount].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "ASIO : " + name;
                if (originalDriverName.CompareTo("None") == 0)
                {
                    originalOutputType = "ASIO";
                    originalDriverName = name;
                }
                FitTextSize(audioOutputDeviceButton[i + wasapiCount].transform.GetChild(3).GetComponent<TextMeshProUGUI>(), 20, 420);
            }
        }

        PlayerPrefs.SetString("OutputType", originalOutputType);
        PlayerPrefs.SetString("DriverName", originalDriverName);
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }

        outputDeviceCount = wasapiCount + asioCount > 10 ? 10 : wasapiCount + asioCount;
    }

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeImage.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private void TextureDestroy()
    {
        if (bgImageTexture != null)
        {
            Destroy(bgImageTexture);
        }
    }

    private IEnumerator CoLoadScene(int scene)
    {
        fadeImage.GetComponent<Animator>().SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        TextureDestroy();

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    void Update()
    {
        if (isChanging || fadeImage.IsActive()) { return; }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(CoLoadScene(0));
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            OptionToggleExecute();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isUpDownPress = true;
            OptionToggleChange(currentOptionIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isUpDownPress = true;
            OptionToggleChange(currentOptionIndex - 1);
        }

        if (currentOptionIndex < 5 || currentOptionIndex > 7) { return; }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            VolumeChange(-0.01f);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            VolumeChange(0.01f);
        }
    }

    private IEnumerator ChangeValue(GameObject panel, int category, int toggleArrayLength)
    {
        if (isChanging) yield break;
        isChanging = true;
        panel.SetActive(true);

        SetCurrentValueToggle(category);
        toggleArrayList[category][currentValueToggleIndex[category]].isOn = true;

        yield return new WaitForSecondsRealtime(0.1f);

        while (isChanging)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.Return)) { valueToggleFunctionList[category](currentValueToggleIndex[category]); }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) { ValueToggleChange(category, 1, toggleArrayLength); }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) { ValueToggleChange(category, -1, toggleArrayLength); }
            yield return null;
        }

        panel.SetActive(false);
    }

    private void ValueToggleChange(int category, int value, int toggleArrayLength)
    {
        currentValueToggleIndex[category] = (currentValueToggleIndex[category] + value + toggleArrayLength) % toggleArrayLength;
        toggleArrayList[category][currentValueToggleIndex[category]].isOn = true;
    }

    private void SetCurrentValueToggle(int category)
    {
        switch (category)
        {
            case 0:
                if (PlayerPrefs.GetInt("SyncCount") == 1) { currentValueToggleIndex[category] = 0; }
                else
                {
                    switch (PlayerPrefs.GetInt("FrameRate"))
                    {
                        case -1: currentValueToggleIndex[category] = 1; break;
                        case 30: currentValueToggleIndex[category] = 2; break;
                        case 60: currentValueToggleIndex[category] = 3; break;
                        case 75: currentValueToggleIndex[category] = 4; break;
                        case 120: currentValueToggleIndex[category] = 5; break;
                        case 144: currentValueToggleIndex[category] = 6; break;
                        case 180: currentValueToggleIndex[category] = 7; break;
                        case 240: currentValueToggleIndex[category] = 8; break;
                        case 300: currentValueToggleIndex[category] = 9; break;
                    }
                }
                break;
            case 1: 
                currentValueToggleIndex[category] = PlayerPrefs.GetInt("DisplayMode");
                break;
            case 2:
                currentValueToggleIndex[category] = 0;
                for (int i = 0; i < outputDeviceCount; i++)
                {
                    string buttonText = audioOutputDeviceButton[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text;
                    if (buttonText.Substring(0, 4).CompareTo(PlayerPrefs.GetString("OutputType")) == 0 &&
                        buttonText.Substring(7).CompareTo(PlayerPrefs.GetString("DriverName")) == 0)
                    {
                        currentValueToggleIndex[category] = i;
                        i = outputDeviceCount;
                    }
                }
                break;
            case 3:
                switch (PlayerPrefs.GetInt("AudioBufferSize"))
                {
                    case 16: currentValueToggleIndex[category] = 0; break;
                    case 32: currentValueToggleIndex[category] = 1; break;
                    case 64: currentValueToggleIndex[category] = 2; break;
                    case 128: currentValueToggleIndex[category] = 3; break;
                    case 256: currentValueToggleIndex[category] = 4; break;
                    case 384: currentValueToggleIndex[category] = 5; break;
                    case 512: currentValueToggleIndex[category] = 6; break;
                }
                break;
            case 4:
                switch (PlayerPrefs.GetInt("PollingRate"))
                {
                    case 1000: currentValueToggleIndex[category] = 0; break;
                    case 2000: currentValueToggleIndex[category] = 1; break;
                    case 4000: currentValueToggleIndex[category] = 2; break;
                    case 8000: currentValueToggleIndex[category] = 3; break;
                }
                break;
        }
    }

    private void AssistKeyUseToggleClick()
    {
        PlayerPrefs.SetInt("AssistKeyUse", (PlayerPrefs.GetInt("AssistKeyUse") + 1) % 2);
        SetAssistKeyUseText();
    }

    public void FrameValueToggleClick(int value)
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
        isChanging = false;
    }

    public void DisplayModeValueToggleClick(int value)
    {
        PlayerPrefs.SetInt("DisplayMode", value);
        SetDisplayModeText();
        Screen.fullScreenMode = value == 2 ? FullScreenMode.Windowed : (FullScreenMode)value;
        isChanging = false;
    }

    public void AudioOutputDeviceValueToggleClick(int index)
    {
        string buttonText = audioOutputDeviceButton[index].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text;
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
        isChanging = false;
    }

    public void AudioBufferValueToggleClick(int index)
    {
        int value = 0;
        switch (index)
        {
            case 0: value = 16; break;
            case 1: value = 32; break;
            case 2: value = 64; break;
            case 3: value = 128; break;
            case 4: value = 256; break;
            case 5: value = 384; break;
            case 6: value = 512; break;
        }
        PlayerPrefs.SetInt("AudioBufferSize", value);
        SetAudioBufferText();
        foreach (FMODUnity.RuntimeManager manager in Resources.FindObjectsOfTypeAll<FMODUnity.RuntimeManager>())
        {
            DestroyImmediate(manager.gameObject);
        }
        isChanging = false;
    }

    public void PollingRateValueToggleClick(int index)
    {
        int value = 0;
        switch (index)
        {
            case 0: value = 1000; break;
            case 1: value = 2000; break;
            case 2: value = 4000; break;
            case 3: value = 8000; break;
        }
        PlayerPrefs.SetInt("PollingRate", value);
        SetPollingRateText();
        isChanging = false;
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
            case 2: displayModeText.text = "Windowed"; break;
        }
    }
    private void SetAudioOutputDeviceText()
    {
        audioOutputDeviceText.text = PlayerPrefs.GetString("OutputType").CompareTo("ASIO") == 0 ? "ASIO : " + PlayerPrefs.GetString("DriverName") :
                                                                                                    PlayerPrefs.GetString("DriverName");
        FitTextSize(audioOutputDeviceText, 20, 310);
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

    private void VolumeChange(float value)
    {
        switch (currentOptionIndex)
        {
            case 5:
                masterVolumeSlider.value += value;
                break;
            case 6:
                keySoundVolumeSlider.value += value;
                break;
            case 7:
                bgmVolumeSlider.value += value;
                break;
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

    private void SetAssistKeyUseText()
    {
        assistKeyUseText.text = PlayerPrefs.GetInt("AssistKeyUse") == 1 ? "Enabled" : "Disabled";
    }

    private void SetPollingRateText()
    {
        pollingRateValueText.text = PlayerPrefs.GetInt("PollingRate") + "Hz";
    }

    #region KeySetting
    private void ChangeKey(int index)
    {
        if (!isChanging)
        {
            StartCoroutine(WaitKeyChange(index));
        }
    }

    private IEnumerator WaitKeyChange(int index)
    {
        isChanging = true;
        waitKeyInputPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(0.1f);

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
            else if (Input.GetKeyDown(KeyCode.F6)) { key = 0x75; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F7)) { key = 0x76; isChanging = false; }
            else if (Input.GetKeyDown(KeyCode.F8)) { key = 0x77; isChanging = false; }
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
            case 0x25: value = "Left"; break;
            case 0x26: value = "Up"; break;
            case 0x27: value = "Right"; break;
            case 0x28: value = "Down"; break;
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
            case 0x60: value = "NumPad 0"; break;
            case 0x61: value = "NumPad 1"; break;
            case 0x62: value = "NumPad 2"; break;
            case 0x63: value = "NumPad 3"; break;
            case 0x64: value = "NumPad 4"; break;
            case 0x65: value = "NumPad 5"; break;
            case 0x66: value = "NumPad 6"; break;
            case 0x67: value = "NumPad 7"; break;
            case 0x68: value = "NumPad 8"; break;
            case 0x69: value = "NumPad 9"; break;
            case 0x6A: value = "NumPad *"; break;
            case 0x6B: value = "Numpad +"; break;
            case 0x6C: value = "Numpad Enter"; break;
            case 0x6D: value = "Numpad -"; break;
            case 0x6E: value = "Numpad ."; break;
            case 0x6F: value = "Numpad /"; break;
            case 0x70: value = "F1"; break;
            case 0x71: value = "F2"; break;
            case 0x75: value = "F6"; break;
            case 0x76: value = "F7"; break;
            case 0x77: value = "F8"; break;
            case 0x78: value = "F9"; break;
            case 0x79: value = "F10"; break;
            case 0x7A: value = "F11"; break;
            case 0x7B: value = "F12"; break;
            case 0xA0: value = "Left Shift"; break;
            case 0xA1: value = "Right Shift"; break;
            case 0xA2: value = "Left Ctrl"; break;
            case 0xA3: value = "Right Ctrl"; break;
            case 0xA4: value = "Left Alt"; break;
            case 0xA5: value = "Right Alt"; break;
            case 0xBA: value = ";"; break;
            case 0xBB: value = "="; break;
            case 0xBC: value = ","; break;
            case 0xBD: value = "-"; break;
            case 0xBE: value = "."; break;
            case 0xBF: value = "/"; break;
            case 0xC0: value = "`"; break;
            case 0xDB: value = "["; break;
            case 0xDC: value = "\\"; break;
            case 0xDD: value = "]"; break;
            case 0xDE: value = "'"; break;
        }
        return value;
    }
    #endregion
}
