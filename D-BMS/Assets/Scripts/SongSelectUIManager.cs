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
    private TextMeshProUGUI koolText;
    [SerializeField]
    private TextMeshProUGUI coolText;
    [SerializeField]
    private TextMeshProUGUI goodText;
    [SerializeField]
    private TextMeshProUGUI missText;
    [SerializeField]
    private TextMeshProUGUI failText;
    [SerializeField]
    private TextMeshProUGUI accuracyText;
    [SerializeField]
    private TextMeshProUGUI maxComboText;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private Image rankImage;
    [SerializeField]
    private Sprite[] rankImageArray;
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
    private Canvas optionCanvas;
    [SerializeField]
    private Animator fadeAnimator;
    [SerializeField]
    private GameplayOptionManager gameplayOptionManager;

    [SerializeField]
    private Toggle[] categoryToggles;
    [SerializeField]
    private ToggleGroup categoryToggleGroup;
    private int categoryCount;
    [SerializeField]
    private TextMeshProUGUI sortByText;
    private int sortByCount;

    public LoopVerticalScrollRect lvScrollRect;
    public ToggleGroup songToggleGroup;
    public GameObject currentContent;
    public int currentIndex = 0;
    private int convertedIndex = 0;
    private int currentHeaderListCount;
    public Sprite seoriToggleSprite;
    public Sprite aeryToggleSprite;

    private WaitForSeconds wait100ms;
    private bool isUpArrowPressed;
    private bool isDownArrowPressed;

    private char prevFindAlphabet;
    private int findSequence;

    private bool isReady = false;
    private static bool isStart = false;
    private bool isExit = false;

    public static SaveData songRecordData;

    private BMPLoader loader;

    void Awake()
    {
        if (songRecordData == null) { songRecordData = new SaveData(); }
        loader = new BMPLoader();

        noteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
        SetRandomEffectorText(PlayerPrefs.GetInt("RandomEffector"));
        randomEffectorCount = System.Enum.GetValues(typeof(RandomEffector)).Length;

        for (int i = categoryToggles.Length - 1; i >= 0; i--) { AddToggleListener(categoryToggles[i]); }
        categoryCount = System.Enum.GetValues(typeof(Category)).Length;
        sortByCount = System.Enum.GetValues(typeof(SortBy)).Length;
        currentHeaderListCount = BMSFileSystem.selectedCategoryHeaderList.Count;

        SongIndexUpdate();
        ScrollbarSetting();

        categoryToggleGroup.allowSwitchOff = true;
        categoryToggleGroup.SetAllTogglesOff(false);
        categoryToggles[PlayerPrefs.GetInt("Category")].isOn = true;
        categoryToggleGroup.allowSwitchOff = false;

        wait100ms = new WaitForSeconds(0.1f);
        isUpArrowPressed = false;
        isDownArrowPressed = false;
        prevFindAlphabet = '.';
        findSequence = 0;

        StartCoroutine(CoFadeOut());
    }

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.5f);
        isStart = false;
    }

    private IEnumerator CoLoadStartScene()
    {
        isExit = true;
        fadeAnimator.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (isStart || isExit) { return; }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleOptionCanvas();
        }
        if (optionCanvas.enabled) { return; }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(CoLoadStartScene());
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
        {
            GameStart();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && !isUpArrowPressed)
        {
            isDownArrowPressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            isDownArrowPressed = false;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && !isDownArrowPressed)
        {
            isUpArrowPressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            isUpArrowPressed = false;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            MoveCurrentIndex(currentIndex + 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            MoveCurrentIndex(currentIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", currentIndex);
            PlayerPrefs.SetInt("Category", (PlayerPrefs.GetInt("Category") - 1 + categoryCount) % categoryCount);
            categoryToggles[PlayerPrefs.GetInt("Category")].isOn = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", currentIndex);
            PlayerPrefs.SetInt("Category", (PlayerPrefs.GetInt("Category") + 1) % categoryCount);
            categoryToggles[PlayerPrefs.GetInt("Category")].isOn = true;
        }
        else if (Input.anyKeyDown)
        {
            int code, index;
            if (!CheckKeyCode(out code)) { return; }

            if (code >= 97 && code <= 122) { index = FindSongTitleIndex((char)code); }
            else { index = (int)(currentHeaderListCount * 0.1f) * code; }

            if (index == -1) { return; }

            StartCoroutine(CoMoveIndex(index));
        }
    }

    private IEnumerator CoMoveIndex(int index)
    {
        MoveCurrentIndex(index - 1);
        yield return new WaitForSeconds(0.05f);
        MoveCurrentIndex(index);
    }

    private bool CheckKeyCode(out int code)
    {
        code = -1;
        for (int i = 0; i < 26; i++)
        {
            if (Input.GetKeyDown((KeyCode)(i + 'a'))) {
                code = i + 'a';
                return true;
            }
        }
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown((KeyCode)(i + 48)) || Input.GetKeyDown((KeyCode)(i + 256)))
            {
                code = i;
                return true;
            }
        }
        return false;
    }

    private IEnumerator CheckUpDownArrowPress()
    {
        while (true)
        {
            while (isDownArrowPressed)
            {
                MoveCurrentIndex(currentIndex + 1);
                yield return wait100ms;
            }
            while (isUpArrowPressed)
            {
                MoveCurrentIndex(currentIndex - 1);
                yield return wait100ms;
            }
            yield return null;
        }
    }

    private int FindSongTitleIndex(char findTitleChar)
    {
        int index = -1;
        findSequence = (findTitleChar == prevFindAlphabet ? findSequence + 1 : 0);
        prevFindAlphabet = findTitleChar;
        int firstIndex = -1;
        int currentSequence = 0;
        for (int i = 0; i < currentHeaderListCount; i++)
        {
            if (BMSFileSystem.selectedCategoryHeaderList[i].title.ToLower()[0] != findTitleChar) { continue; }
            if (currentSequence == findSequence)
            {
                index = i;
                break;
            }
            if (currentSequence == 0) { firstIndex = i; }
            currentSequence++;
        }
        if (index == -1)
        {
            index = firstIndex;
            findSequence = 0;
        }
        return index;
    }

    public void MoveCurrentIndex(int index)
    {
        currentIndex = index;
        lvScrollRect.ScrollToCellWithinTime(currentIndex, 0.05f);
        ConvertIndex();
        scrollbar.value = convertedIndex / (currentHeaderListCount - 1.0f);
        SongIndexUpdate();
        DataSaveManager.LoadResultData(BMSFileSystem.selectedCategoryHeaderList[convertedIndex].fileName);
        SetSongRecord();
    }

    public void GameStart()
    {
        if (isReady)
        {
            isStart = true;
            PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", currentIndex);
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        }
    }

    public void DrawSongInfoUI(BMSHeader header)
    {
        if (BMSFileSystem.selectedHeader == null || banner.texture == null ||
            BMSFileSystem.selectedHeader.textFolderPath.CompareTo(header.textFolderPath) != 0)
        {
            StartCoroutine(LoadRawImage(banner, header.musicFolderPath, header.bannerPath, noBannerTexture));
        }

        BMSFileSystem.selectedHeader = header;

        if (titleText.text.CompareTo(header.title) != 0 || levelText.text.CompareTo(header.level.ToString()) != 0)
        {
            if (header.title.Length > 30) { titleText.fontSize = 20; }
            else if (header.title.Length <= 30 && header.title.Length >= 15) { titleText.fontSize = 30; }
            else { titleText.fontSize = 60; }
            titleText.text = header.title;
            if (header.artist.Length >= 30) { artistText.fontSize = 20; }
            else { artistText.fontSize = 35; }
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

    public void NoteSpeedClick(int value)
    {
        int userSpeed = PlayerPrefs.GetInt("NoteSpeed");
        userSpeed += value;

        if (userSpeed < 10) { userSpeed = 10; }
        else if (userSpeed > 200) { userSpeed = 200; }

        noteSpeedText.text = (userSpeed * 0.1f).ToString("0.0");
        PlayerPrefs.SetInt("NoteSpeed", userSpeed);
    }

    public void RandomEffectorClick(int value)
    {
        int index = (PlayerPrefs.GetInt("RandomEffector") + value + randomEffectorCount) % randomEffectorCount;
        PlayerPrefs.SetInt("RandomEffector", index);
        SetRandomEffectorText(index);
    }

    private void SetRandomEffectorText(int index)
    {
        switch (index)
        {
            case 0: randomEffectorText.text = "NONE"; break;
            case 1: randomEffectorText.text = "RANDOM"; break;
            case 2: randomEffectorText.text = "MIRROR"; break;
            case 3: randomEffectorText.text = "F-RANDOM"; break;
            case 4: randomEffectorText.text = "MF-RANDOM"; break;
        }
    }

    public void SortByClick(int value)
    {
        string currentTitle = BMSFileSystem.selectedHeader.title;
        int currentLevel = BMSFileSystem.selectedHeader.level;
        PlayerPrefs.SetInt("SortBy", (PlayerPrefs.GetInt("SortBy") + value + sortByCount) % sortByCount);
        SortHeaderList();
        PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", FindCurrentSongIndex(currentTitle, currentLevel));
        StartCoroutine(CoMoveIndex(PlayerPrefs.GetInt($"Category{PlayerPrefs.GetInt("Category")}Index")));
    }

    private int FindCurrentSongIndex(string title, int level)
    {
        for (int i = 0; i < currentHeaderListCount; i++)
        {
            if (BMSFileSystem.selectedCategoryHeaderList[i].title.CompareTo(title) == 0 && BMSFileSystem.selectedCategoryHeaderList[i].level == level)
            {
                return i;
            }
        }
        return 0;
    }

    private void AddToggleListener(Toggle toggle)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                int curCategory = PlayerPrefs.GetInt("Category");
                int nextCategory = 0;
                switch (toggle.tag)
                {
                    case "Category_All": PlayerPrefs.SetInt("Category", 0); nextCategory = 0; break;
                    case "Category_Aery": PlayerPrefs.SetInt("Category", 1); nextCategory = 1; break;
                    case "Category_SeoRi": PlayerPrefs.SetInt("Category", 2); nextCategory = 2; break;
                }
                if (curCategory != nextCategory) { PlayerPrefs.SetInt($"Category{curCategory}Index", currentIndex); }
                ChangeCategory();
                StartCoroutine(CheckUpDownArrowPress());
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

        Category currentCategory = (Category)PlayerPrefs.GetInt("Category");

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
        StartCoroutine(CoMoveIndex(PlayerPrefs.GetInt($"Category{PlayerPrefs.GetInt("Category")}Index")));
    }

    private void SortHeaderList()
    {
        switch((SortBy)PlayerPrefs.GetInt("SortBy"))
        {
            case SortBy.LEVEL:
                sortByText.text = "Sort by Level";
                BMSFileSystem.selectedCategoryHeaderList.Sort((x, y) => {
                    int result = x.level.CompareTo(y.level);
                    return result != 0 ? result : string.Compare(x.title, y.title);
                }); break;
            case SortBy.TITLE:
                sortByText.text = "Sort by Title";
                BMSFileSystem.selectedCategoryHeaderList.Sort((x, y) => {
                    int result = string.Compare(x.title, y.title);
                    return result != 0 ? result : x.level.CompareTo(y.level);
                }); break;
            case SortBy.BPM:
                sortByText.text = "Sort by BPM";
                BMSFileSystem.selectedCategoryHeaderList.Sort((x, y) => {
                    int result1 = x.maxBPM.CompareTo(y.maxBPM);
                    int result2 = result1 != 0 ? result1 : x.level.CompareTo(y.level);
                    return result2 != 0 ? result2 : string.Compare(x.title, y.title);
                }); break;
        }

        currentIndex = 0;
        if (currentContent != null) { currentContent.tag = "Untagged"; }
        currentContent = null;

        lvScrollRect.RefillCells(-4);

        currentHeaderListCount = BMSFileSystem.selectedCategoryHeaderList.Count;
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

    private void ToggleOptionCanvas()
    {
        optionCanvas.enabled = !optionCanvas.enabled;
        if (optionCanvas.enabled)
        {
            gameplayOptionManager.SetGameplayOption();
        }
        else
        {
            noteSpeedText.text = (PlayerPrefs.GetInt("NoteSpeed") * 0.1f).ToString("0.0");
            SetRandomEffectorText(PlayerPrefs.GetInt("RandomEffector"));
        }
    }

    public void SetSongRecord()
    {
        koolText.text = songRecordData.koolCount.ToString();
        coolText.text = songRecordData.coolCount.ToString();
        goodText.text = songRecordData.goodCount.ToString();
        missText.text = songRecordData.missCount.ToString();
        failText.text = songRecordData.failCount.ToString();
        accuracyText.text = ((float)songRecordData.accuracy).ToString("P");
        maxComboText.text = songRecordData.maxCombo.ToString();
        scoreText.text = ((int)((float)songRecordData.score)).ToString();
        rankImage.sprite = rankImageArray[songRecordData.rankIndex];
    }
}
