using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScrollIndexCallback1 : MonoBehaviour
{
    private SongSelectUIManager songSelectUIManager = null;

    [SerializeField]
    private Toggle thisObjectToggle;
    [SerializeField]
    private TextMeshProUGUI toggleTitleText;
    [SerializeField]
    private TextMeshProUGUI toggleSubtitleText;
    [SerializeField]
    private TextMeshProUGUI toggleArtistText;
    [SerializeField]
    private TextMeshProUGUI toggleBPMText;
    [SerializeField]
    private TextMeshProUGUI toggleLevelText;
    [SerializeField]
    private Image clearLampImage;

    private Color32 normalTextColor;

    void Awake()
    {
        normalTextColor = new Color32(180, 180, 180, 255);
    }

    void Start()
    {
        if (songSelectUIManager == null) { songSelectUIManager = FindObjectOfType<SongSelectUIManager>(); }
        thisObjectToggle.group = songSelectUIManager.songToggleGroup;
    }

    void ScrollCellIndex (int idx) 
    {
        if (songSelectUIManager == null) { songSelectUIManager = FindObjectOfType<SongSelectUIManager>(); }

        thisObjectToggle.onValueChanged.RemoveAllListeners();

        gameObject.name = idx.ToString();

        int len = BMSFileSystem.selectedCategoryHeaderList.Count;
        int tidx = idx;
        while (tidx < 0) { tidx += len; }
        tidx %= len;

        BMSHeader header = BMSFileSystem.selectedCategoryHeaderList[tidx];
        toggleTitleText.text = header.title;
        toggleTitleText.fontSize = 28;
        toggleSubtitleText.rectTransform.localPosition = new Vector3(-445.0f + toggleTitleText.preferredWidth, 13.0f, 0.0f);
        toggleSubtitleText.text = header.subTitle;
        toggleSubtitleText.fontSize = 17;
        while (toggleTitleText.preferredWidth + toggleSubtitleText.preferredWidth > 835.0f)
        {
            toggleTitleText.fontSize -= 0.1f;
            toggleSubtitleText.rectTransform.localPosition = new Vector3(-445.0f + toggleTitleText.preferredWidth, 13.0f, 0.0f);
            toggleSubtitleText.fontSize -= 0.1f;
        }

        toggleArtistText.text = header.artist;
        toggleArtistText.fontSize = 15;
        toggleBPMText.rectTransform.localPosition = new Vector3(-420.0f + toggleArtistText.preferredWidth, -20.0f, 0.0f);
        if (header.maxBPM == header.minBPM) { toggleBPMText.text = "BPM " + header.bpm.ToString(); }
        else { toggleBPMText.text = "BPM " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
        toggleBPMText.fontSize = 15;
        while (toggleArtistText.preferredWidth + toggleBPMText.preferredWidth > 835.0f)
        {
            toggleArtistText.fontSize -= 0.1f;
            toggleBPMText.rectTransform.localPosition = new Vector3(-420.0f + toggleArtistText.preferredWidth, -20.0f, 0.0f);
            toggleBPMText.fontSize -= 0.1f;
        }

        toggleLevelText.text = header.level.ToString();
        int clearlamp;
        bool check = BMSFileSystem.songClearLamp.clearLampDict.TryGetValue(header.fileName, out clearlamp);
        if (!check) clearlamp = -1;
        switch (clearlamp)
        {
            case -1:
                clearLampImage.sprite = songSelectUIManager.clearlamp_normal;
                clearLampImage.color = Color.gray;
                break;
            case 0:
                clearLampImage.sprite = songSelectUIManager.clearlamp_normal;
                clearLampImage.color = Color.red;
                break;
            case 1:
                clearLampImage.sprite = songSelectUIManager.clearlamp_normal;
                clearLampImage.color = new Color(0.0f, 215.0f / 255.0f, 1.0f);
                break;
            case 2:
                clearLampImage.sprite = songSelectUIManager.clearlamp_normal;
                clearLampImage.color = Color.yellow;
                break;
            case 3:
                clearLampImage.sprite = songSelectUIManager.clearlamp_allcool;
                clearLampImage.color = Color.white;
                break;
        }

        if (gameObject.CompareTag("CurrentContent")) { songSelectUIManager.songToggleGroup.allowSwitchOff = (idx != songSelectUIManager.currentIndex); }

        thisObjectToggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                songSelectUIManager.songToggleGroup.allowSwitchOff = false;
                songSelectUIManager.DrawSongInfoUI(header);
                if (idx != songSelectUIManager.currentIndex) 
                { 
                    songSelectUIManager.MoveToIndex(idx);
                    DataSaveManager.LoadResultData(header.fileName);
                    songSelectUIManager.SetSongRecord();
                }
                if (songSelectUIManager.currentContent != null) { songSelectUIManager.currentContent.tag = "Untagged"; }
                gameObject.tag = "CurrentContent";
                songSelectUIManager.currentContent = gameObject;
                toggleLevelText.color = Color.white;
            }
            else
            {
                toggleLevelText.color = normalTextColor;
            }
        });

        if (idx == songSelectUIManager.currentIndex) { thisObjectToggle.isOn = true; }
        else { thisObjectToggle.isOn = false; }
    }
}
