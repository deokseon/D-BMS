using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using B83.Image.BMP;

public class SongSelectUIManager : MonoBehaviour
{
    public static float scrollValue = 1;
    public Scrollbar songScroll;

    [SerializeField]
    private GameObject songButtonPrefab;
    [SerializeField]
    private ToggleGroup toggleGroup;
    [SerializeField]
    private RectTransform songViewport;
    [SerializeField]
    private RawImage banner;
    [SerializeField]
    private Texture noBannerTexture;
    [SerializeField]
    private Texture noStageImageTexture;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI artistText;
    [SerializeField]
    private TextMeshProUGUI bpmText;
    [SerializeField]
    private TextMeshProUGUI noteSpeedText;
    [SerializeField]
    private TextMeshProUGUI randomEffectorText;
    [SerializeField]
    private TextMeshProUGUI levelText;

    private bool isReady = false;

    private BMPLoader loader;

    void Awake()
    {
        songScroll.value = scrollValue;

        loader = new BMPLoader();

        noteSpeedText.text = BMSGameManager.userSpeed.ToString("0.0");
        randomEffectorText.text = BMSGameManager.randomEffector.ToString();
    }

    void Update()
    {
        scrollValue = songScroll.value;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            GameStart();
        }
    }

    public void GameStart()
    {
        if (isReady)
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void DrawSongUI(BMSSongInfo[] songInfos)
    {
        int i = 0;
        songViewport.sizeDelta = new Vector2(0, 90 * songInfos.Length);
        foreach (BMSSongInfo info in songInfos)
        {
            GameObject t = Instantiate(songButtonPrefab, songViewport);
            t.gameObject.name = i.ToString();

            t.transform.localPosition = new Vector3(0, 40 - (89 * (++i)));

            t.GetComponentsInChildren<TextMeshProUGUI>()[0].text = info.songName + "  <size=25>" + info.header.subTitle + "</size>";
            if (info.header.maxBPM == info.header.minBPM) { t.GetComponentsInChildren<TextMeshProUGUI>()[1].text = info.header.artist + "        BPM: " + info.header.bpm.ToString(); }
            else { t.GetComponentsInChildren<TextMeshProUGUI>()[1].text = info.header.artist + "        BPM: " + info.header.minBPM.ToString() + " ~ " + info.header.maxBPM.ToString(); }
            t.GetComponentsInChildren<TextMeshProUGUI>()[2].text = info.header.level.ToString();

            StartCoroutine(LoadRawImage(t.GetComponentsInChildren<RawImage>()[0], info.header.musicFolderPath, info.header.stageFilePath, noStageImageTexture));

            t.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => { if (value) { DrawSongInfoUI(info); } });
            t.GetComponent<Toggle>().group = toggleGroup;
        }
    }

    public void DrawSongInfoUI(BMSSongInfo songInfo)
    {
        if (BMSFileSystem.selectedHeader == null || 
            BMSFileSystem.selectedHeader.textFolderPath.CompareTo(songInfo.header.textFolderPath) != 0)
        {
            StartCoroutine(LoadRawImage(banner, songInfo.header.musicFolderPath, songInfo.header.bannerPath, noBannerTexture));
        }

        BMSFileSystem.selectedHeader = songInfo.header;

        if (titleText.text.CompareTo(songInfo.songName) != 0)
        {
            titleText.text = songInfo.songName;
            artistText.text = songInfo.header.artist;
            if (songInfo.header.minBPM == songInfo.header.maxBPM) { bpmText.text = "BPM: " + songInfo.header.bpm.ToString(); }
            else { bpmText.text = "BPM: " + songInfo.header.minBPM.ToString() + " ~ " + songInfo.header.maxBPM.ToString(); }
            levelText.text = songInfo.header.level.ToString();
        }

        isReady = true;
    }

    public IEnumerator LoadRawImage(RawImage rawImage, string musicFolderPath, string path, Texture noImage)
    {
        if (string.IsNullOrEmpty(path)) { rawImage.texture = noImage; yield break; }

        string imagePath = $@"file:\\{musicFolderPath}{path}";

        Texture tex = null;
        if (imagePath.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
        {
            UnityWebRequest uwr = UnityWebRequest.Get(imagePath);
            yield return uwr.SendWebRequest();

            tex = loader.LoadBMP(uwr.downloadHandler.data).ToTexture2D();
        }
        else if (imagePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                 imagePath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
        {
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath);
            yield return uwr.SendWebRequest();

            tex = (uwr.downloadHandler as DownloadHandlerTexture).texture;
        }

        rawImage.texture = (tex != null ? tex : noImage);
    }

    public void NoteSpeedClick(float value)
    {
        BMSGameManager.userSpeed += value;

        if (BMSGameManager.userSpeed < 1.0f) { BMSGameManager.userSpeed = 1.0f; }
        else if (BMSGameManager.userSpeed > 20.0f) { BMSGameManager.userSpeed = 20.0f; }

        noteSpeedText.text = BMSGameManager.userSpeed.ToString("0.0");
    }

    public void RandomEffectorClick(int value)
    {
        int count = System.Enum.GetValues(typeof(RandomEffector)).Length;

        BMSGameManager.randomEffector = (RandomEffector)(((int)BMSGameManager.randomEffector + value + count) % count);

        if (BMSGameManager.randomEffector == RandomEffector.FRANDOM)
            randomEffectorText.text = "F-RANDOM";
        else if (BMSGameManager.randomEffector == RandomEffector.MFRANDOM)
            randomEffectorText.text = "MF-RANDOM";
        else
            randomEffectorText.text = BMSGameManager.randomEffector.ToString();
    }
}
