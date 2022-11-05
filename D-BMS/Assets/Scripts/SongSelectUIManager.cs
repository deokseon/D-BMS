using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using B83.Image.BMP;

public class SongSelectUIManager : MonoBehaviour
{
    [SerializeField]
    private RawImage banner;
    [SerializeField]
    private Texture noBannerTexture;
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
    private int randomEffectorCount;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI songIndexText;
    [SerializeField]
    private Scrollbar scrollbar;

    [SerializeField]
    private Toggle[] categoryToggles;
    private int currentCategoryIndex;
    private int categoryCount;
    [SerializeField]
    private TextMeshProUGUI sortByText;
    private SortBy currentSortBy;
    private int sortByCount;

    public LoopVerticalScrollRect lvScrollRect;
    public ToggleGroup songToggleGroup;
    public GameObject currentContent;
    public int currentIndex = 0;
    private int convertedIndex = 0;
    private int currentHeaderListCount;
    public Sprite seoriToggleSprite;
    public Sprite aeryToggleSprite;

    private bool isReady = false;

    private BMPLoader loader;

    void Awake()
    {
        loader = new BMPLoader();

        noteSpeedText.text = BMSGameManager.userSpeed.ToString("0.0");
        randomEffectorText.text = BMSGameManager.randomEffector.ToString();
        randomEffectorCount = System.Enum.GetValues(typeof(RandomEffector)).Length;

        for (int i = categoryToggles.Length - 1; i >= 0; i--) { AddToggleListener(categoryToggles[i]); }
        currentCategoryIndex = 0;
        categoryCount = System.Enum.GetValues(typeof(Category)).Length;
        sortByCount = System.Enum.GetValues(typeof(SortBy)).Length;
        currentHeaderListCount = BMSFileSystem.selectedCategoryHeaderList.Count;
        SongIndexUpdate();
        ScrollbarSetting();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            GameStart();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            MoveCurrentIndex(currentIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            MoveCurrentIndex(currentIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentCategoryIndex = (currentCategoryIndex - 1 + categoryCount) % categoryCount;
            categoryToggles[currentCategoryIndex].isOn = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            currentCategoryIndex = (currentCategoryIndex + 1) % categoryCount;
            categoryToggles[currentCategoryIndex].isOn = true;
        }
    }

    public void MoveCurrentIndex(int index)
    {
        currentIndex = index;
        lvScrollRect.ScrollToCellWithinTime(currentIndex, 0.05f);
        ConvertIndex();
        scrollbar.value = convertedIndex / (currentHeaderListCount - 1.0f);
        SongIndexUpdate();
    }

    public void GameStart()
    {
        if (isReady)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
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

        if (titleText.text.CompareTo(header.title) != 0 || levelText.text.CompareTo(header.level.ToString()) != 0)
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
        BMSGameManager.randomEffector = (RandomEffector)(((int)BMSGameManager.randomEffector + value + randomEffectorCount) % randomEffectorCount);

        if (BMSGameManager.randomEffector == RandomEffector.FRANDOM)
            randomEffectorText.text = "F-RANDOM";
        else if (BMSGameManager.randomEffector == RandomEffector.MFRANDOM)
            randomEffectorText.text = "MF-RANDOM";
        else
            randomEffectorText.text = BMSGameManager.randomEffector.ToString();
    }

    public void SortByClick(int value)
    {
        currentSortBy = (SortBy)(((int)currentSortBy + value + sortByCount) % sortByCount);

        if (currentSortBy == SortBy.LEVEL) { sortByText.text = "Sort by Level"; }
        else if (currentSortBy == SortBy.TITLE) { sortByText.text = "Sort by Title"; }
        else { sortByText.text = "Sort by BPM"; }

        SortHeaderList();
    }

    private void AddToggleListener(Toggle toggle)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                switch (toggle.tag)
                {
                    case "Category_All": currentCategoryIndex = 0; break;
                    case "Category_Aery": currentCategoryIndex = 1; break;
                    case "Category_SeoRi": currentCategoryIndex = 2; break;
                }
                ChangeCategory();
            }
        });
    }

    private void ScrollbarSetting()
    {
        scrollbar.onValueChanged.RemoveAllListeners();
        scrollbar.onValueChanged.AddListener((float value) =>
        {
            int tempValue = Mathf.RoundToInt(value * (currentHeaderListCount - 1));
            if (tempValue != convertedIndex) { MoveCurrentIndex(tempValue); }
        });
        scrollbar.size = 1.0f / currentHeaderListCount;
        scrollbar.value = 0.0f;
    }

    private void ChangeCategory()
    {
        StopAllCoroutines();
        BMSFileSystem.selectedCategoryHeaderList.Clear();

        Category currentCategory = (Category)(currentCategoryIndex);

        int headerCount = BMSFileSystem.headers.Length;
        for (int i = 0; i < headerCount; i++)
        {
            if (currentCategory == Category.NONE || BMSFileSystem.headers[i].songCategory == currentCategory)
            {
                BMSFileSystem.selectedCategoryHeaderList.Add(BMSFileSystem.headers[i]);
            }
        }

        SortHeaderList();
        ScrollbarSetting();
    }

    private void SortHeaderList()
    {
        BMSFileSystem.selectedCategoryHeaderList.Sort((x, y) => {
            if (currentSortBy == SortBy.LEVEL)
            {
                int result = x.level.CompareTo(y.level);
                return result != 0 ? result : string.Compare(x.title, y.title);
            }
            else if (currentSortBy == SortBy.TITLE)
            {
                int result = string.Compare(x.title, y.title);
                return result != 0 ? result : x.level.CompareTo(y.level);
            }
            else
            {
                int result1 = x.maxBPM.CompareTo(y.maxBPM);
                int result2 = result1 != 0 ? result1 : x.level.CompareTo(y.level);
                return result2 != 0 ? result2 : string.Compare(x.title, y.title);
            }
        });

        currentIndex = 0;
        if (currentContent != null) { currentContent.tag = "Untagged"; }
        currentContent = null;

        lvScrollRect.RefillCells(-4);

        currentHeaderListCount = BMSFileSystem.selectedCategoryHeaderList.Count;
        SongIndexUpdate();
    }

    public void SongIndexUpdate()
    {
        if (currentHeaderListCount == 0) { return; }
        int tempIndex = convertedIndex + 1;
        songIndexText.text = tempIndex.ToString() + " / " + currentHeaderListCount.ToString();
    }

    private void ConvertIndex()
    {
        convertedIndex = currentIndex;
        while (convertedIndex < 0) { convertedIndex += (currentHeaderListCount * 10); }
        convertedIndex %= currentHeaderListCount;
    }
}
