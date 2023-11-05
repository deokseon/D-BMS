using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using B83.Image.BMP;
using System.IO;
using UnityEngine.Video;

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
    private TextMeshProUGUI earlyText;
    [SerializeField]
    private TextMeshProUGUI lateText;
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
    private RawImage stageImage;
    [SerializeField]
    private Animator stageImageAnimator;
    [SerializeField]
    private Texture noneTexture;
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
    private Image fadeImage;
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

    private readonly int hashRightRotate = Animator.StringToHash("RightRotate");
    private readonly int hashLeftRotate = Animator.StringToHash("LeftRotate");

    public LoopVerticalScrollRect lvScrollRect;
    public ToggleGroup songToggleGroup;
    public GameObject currentContent;
    public int currentIndex = 0;
    private int convertedIndex = 0;
    private int currentHeaderListCount;

    private bool isUpPressed;
    private bool isDownPressed;
    private Coroutine upDownCoroutine;

    private char prevFindAlphabet;
    private int findSequence;

    public static ResultData resultData;

    private BMPLoader loader;

    void Awake()
    {
        if (resultData == null) { resultData = new ResultData(11); }
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

        isUpPressed = false;
        isDownPressed = false;
        prevFindAlphabet = '.';
        findSequence = 0;

        SetBackground();
    }

    private void SetBackground()
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\select-bg";
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

        GameObject.Find("Screen").GetComponent<RawImage>().texture = (uwr.downloadHandler as DownloadHandlerTexture).texture;

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

    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeImage.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private IEnumerator CoLoadStartScene()
    {
        PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", currentIndex);
        fadeImage.GetComponent<Animator>().SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (fadeImage.IsActive()) { return; }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleOptionCanvas();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionCanvas.enabled)
            {
                ToggleOptionCanvas();
            }
            else
            {
                StartCoroutine(CoLoadStartScene());
            }
        }
        if (optionCanvas.enabled) { return; }


        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
        {
            GameStart();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && isDownPressed)
        {
            isDownPressed = false;
            StopCoroutine(upDownCoroutine);
            upDownCoroutine = null;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && !isUpPressed)
        {
            if (upDownCoroutine != null)
            {
                StopCoroutine(upDownCoroutine);
                upDownCoroutine = null;
            }
            MoveToIndex(currentIndex + 1);
            upDownCoroutine = StartCoroutine(UpDownPressing(1));
            isDownPressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) && isUpPressed)
        {
            isUpPressed = false;
            StopCoroutine(upDownCoroutine);
            upDownCoroutine = null;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && !isDownPressed)
        {
            if (upDownCoroutine != null)
            {
                StopCoroutine(upDownCoroutine);
                upDownCoroutine = null;
            }
            MoveToIndex(currentIndex - 1);
            upDownCoroutine = StartCoroutine(UpDownPressing(-1));
            isUpPressed = true;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            MoveToIndex(currentIndex + 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            MoveToIndex(currentIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            CategoryChangeButtonClick(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            CategoryChangeButtonClick(1);
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            SongRandomSelect();
        }
        else if (Input.anyKeyDown)
        {
            int code, index;
            if (!CheckKeyCode(out code)) { return; }

            if (code >= 97 && code <= 122) { index = FindSongTitleIndex((char)code); }
            else { index = (int)(currentHeaderListCount * 0.1f) * code; }

            if (index == -1) { return; }

            MoveToIndex(index);
        }
    }

    public void CategoryChangeButtonClick(int value)
    {
        PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", currentIndex);
        PlayerPrefs.SetInt("Category", (PlayerPrefs.GetInt("Category") + value + categoryCount) % categoryCount);
        categoryToggles[PlayerPrefs.GetInt("Category")].isOn = true;
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

    private IEnumerator UpDownPressing(int direction)
    {
        yield return new WaitForSecondsRealtime(0.3f);
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            MoveToIndex(currentIndex + direction);
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
            if (BMSFileSystem.selectedCategoryHeaderList[i].title.Length == 0 || 
                BMSFileSystem.selectedCategoryHeaderList[i].title.ToLower()[0] != findTitleChar) { continue; }
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

    public void MoveToIndex(int index)
    {
        currentIndex = index;
        lvScrollRect.ScrollToCellWithinTime(currentIndex, 0.05f);
        ConvertIndex();
        scrollbar.value = currentHeaderListCount == 1 ? 1.0f : convertedIndex / (currentHeaderListCount - 1.0f);
        SongIndexUpdate();
        resultData = DataSaveManager.LoadData<ResultData>("DataSave", BMSFileSystem.selectedCategoryHeaderList[convertedIndex].fileName + ".json") ?? new ResultData(11);
        SetSongRecord();
    }

    public void GameStart()
    {
        if (BMSFileSystem.selectedHeader != null)
        {
            PlayerPrefs.SetInt($"Category{PlayerPrefs.GetInt("Category")}Index", currentIndex);
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        }
    }

    public void DrawSongInfoUI(BMSHeader header)
    {
        if (BMSFileSystem.selectedHeader == null || stageImage.texture == null ||
            BMSFileSystem.selectedHeader.textFolderPath.CompareTo(header.textFolderPath) != 0)
        {
            StartCoroutine(LoadRawImage(stageImage, header.musicFolderPath, header.stageFilePath, noneTexture));
        }

        BMSFileSystem.selectedHeader = header;

        levelText.text = header.level.ToString();
    }

    public IEnumerator LoadRawImage(RawImage rawImage, string musicFolderPath, string path, Texture noImage)
    {
        if (string.IsNullOrEmpty(path)) 
        {
            rawImage.texture = noImage;
            rawImage.color = new Color32(0, 0, 0, 230);
            yield break;
        }

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

        if (tex == null)
        {
            rawImage.texture = noImage;
            rawImage.color = new Color32(0, 0, 0, 230);
        }
        else
        {
            rawImage.texture = tex;
            rawImage.color = new Color32(255, 255, 255, 255);
        }

        stageImageAnimator.SetTrigger(Random.Range(0, 2) == 0 ? hashLeftRotate : hashRightRotate);
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
        MoveToIndex(PlayerPrefs.GetInt($"Category{PlayerPrefs.GetInt("Category")}Index"));
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
            TextMeshProUGUI toggleText = toggle.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (value)
            {
                int curCategory = PlayerPrefs.GetInt("Category");
                int nextCategory = 0;
                switch (toggle.tag)
                {
                    case "Category_All": PlayerPrefs.SetInt("Category", 0); nextCategory = 0; break;
                    case "Category_Aery": PlayerPrefs.SetInt("Category", 1); nextCategory = 1; break;
                    case "Category_SeoRi": PlayerPrefs.SetInt("Category", 2); nextCategory = 2; break;
                    case "Category_Favorite": PlayerPrefs.SetInt("Category", 3); nextCategory = 3; break;
                }
                if (curCategory != nextCategory) { PlayerPrefs.SetInt($"Category{curCategory}Index", currentIndex); }
                toggleText.fontSize = 25;
                toggleText.color = Color.white;
                ChangeCategory();
            }
            else
            {
                toggleText.fontSize = 20;
                toggleText.color = new Color32(180, 180, 180, 255);
            }
        });
    }

    private void ScrollbarSetting()
    {
        scrollbar.onValueChanged.RemoveAllListeners();
        scrollbar.onValueChanged.AddListener((float value) =>
        {
            int tempValue = Mathf.RoundToInt(value * (currentHeaderListCount - 1));
            if (tempValue != convertedIndex) { MoveToIndex(tempValue); }
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
        if ((int)currentCategory == 3)
        {
            if (BMSFileSystem.favoriteSong.favoriteSongSet.Count == 0)
            {
                categoryToggles[0].isOn = true;
                return;
            }
            for (int i = 0; i < headerCount; i++)
            {
                if (BMSFileSystem.favoriteSong.favoriteSongSet.Contains(BMSFileSystem.headers[i].fileName))
                {
                    BMSFileSystem.selectedCategoryHeaderList.Add(BMSFileSystem.headers[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < headerCount; i++)
            {
                if (currentCategory == Category.NONE || BMSFileSystem.headers[i].songCategory == currentCategory)
                {
                    BMSFileSystem.selectedCategoryHeaderList.Add(BMSFileSystem.headers[i]);
                }
            }
        }

        SortHeaderList();
        ScrollbarSetting();
        MoveToIndex(PlayerPrefs.GetInt($"Category{PlayerPrefs.GetInt("Category")}Index"));
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

    public void ToggleOptionCanvas()
    {
        optionCanvas.enabled = !optionCanvas.enabled;
        gameplayOptionManager.enabled = optionCanvas.enabled;
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
        koolText.text = resultData.koolCount.ToString();
        coolText.text = resultData.coolCount.ToString();
        goodText.text = resultData.goodCount.ToString();
        missText.text = resultData.missCount.ToString();
        failText.text = resultData.failCount.ToString();
        earlyText.text = resultData.earlyCount.ToString();
        lateText.text = resultData.lateCount.ToString();
        accuracyText.text = ((float)resultData.accuracy).ToString("P");
        maxComboText.text = resultData.maxCombo.ToString();
        scoreText.text = ((int)((float)resultData.score)).ToString();
        rankImage.sprite = rankImageArray[resultData.rankIndex];
    }

    private void SongRandomSelect()
    {
        MoveToIndex(Random.Range(0, currentHeaderListCount));
    }

    public void Check()
    {
        if (BMSFileSystem.selectedHeader.title.CompareTo(BMSFileSystem.selectedCategoryHeaderList[convertedIndex].title) != 0)
        {
            Debug.Log("Change");
        }
    }
}
