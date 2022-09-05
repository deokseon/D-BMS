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
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }

    public void DrawSongUI(BMSHeader[] headers)
    {
        int i = 0;
        songViewport.sizeDelta = new Vector2(0, 90 * headers.Length);
        foreach (BMSHeader header in headers)
        {
            GameObject t = Instantiate(songButtonPrefab, songViewport);
            t.gameObject.name = i.ToString();

            t.transform.localPosition = new Vector3(0, 40 - (89 * (++i)));

            t.GetComponentsInChildren<TextMeshProUGUI>()[0].text = header.title + "  <size=20>" + header.subTitle + "</size>";
            if (header.maxBPM == header.minBPM) { t.GetComponentsInChildren<TextMeshProUGUI>()[1].text = header.artist + "        BPM: " + header.bpm.ToString(); }
            else { t.GetComponentsInChildren<TextMeshProUGUI>()[1].text = header.artist + "        BPM: " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
            t.GetComponentsInChildren<TextMeshProUGUI>()[2].text = header.level.ToString();

            StartCoroutine(LoadRawImage(t.GetComponentsInChildren<RawImage>()[0], header.musicFolderPath, header.stageFilePath, noStageImageTexture));

            t.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => 
            { 
                if (value)
                {
                    BMSGameManager.stageTexture = t.GetComponentsInChildren<RawImage>()[0].texture;
                    DrawSongInfoUI(header);
                } 
            });
            t.GetComponent<Toggle>().group = toggleGroup;
        }
    }

    public void DrawSongInfoUI(BMSHeader header)
    {
        if (BMSFileSystem.selectedHeader == null || 
            BMSFileSystem.selectedHeader.textFolderPath.CompareTo(header.textFolderPath) != 0)
        {
            StartCoroutine(LoadRawImage(banner, header.musicFolderPath, header.bannerPath, noBannerTexture));
        }

        BMSFileSystem.selectedHeader = header;

        if (titleText.text.CompareTo(header.title) != 0)
        {
            titleText.text = header.title;
            artistText.text = header.artist;
            if (header.minBPM == header.maxBPM) { bpmText.text = "BPM: " + header.bpm.ToString(); }
            else { bpmText.text = "BPM: " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
            levelText.text = header.level.ToString();
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

        rawImage.texture = (tex ?? noImage);
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
