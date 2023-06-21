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
    private Image checkmark;

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
            toggleTitleText.fontSize--;
            toggleSubtitleText.rectTransform.localPosition = new Vector3(-445.0f + toggleTitleText.preferredWidth, 13.0f, 0.0f);
            toggleSubtitleText.fontSize--;
        }

        toggleArtistText.text = header.artist;
        toggleArtistText.fontSize = 15;
        toggleBPMText.rectTransform.localPosition = new Vector3(-445.0f + toggleArtistText.preferredWidth, -20.0f, 0.0f);
        if (header.maxBPM == header.minBPM) { toggleBPMText.text = "BPM " + header.bpm.ToString(); }
        else { toggleBPMText.text = "BPM " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
        toggleBPMText.fontSize = 15;
        while (toggleArtistText.preferredWidth + toggleBPMText.preferredWidth > 835.0f)
        {
            toggleArtistText.fontSize--;
            toggleBPMText.rectTransform.localPosition = new Vector3(-445.0f + toggleArtistText.preferredWidth, -20.0f, 0.0f);
            toggleBPMText.fontSize--;
        }

        toggleLevelText.text = header.level.ToString();

        switch (header.songCategory)
        {
            case Category.AERY: checkmark.sprite = songSelectUIManager.aeryToggleSprite; break;
            case Category.SEORI: checkmark.sprite = songSelectUIManager.seoriToggleSprite; break;
            default: checkmark.sprite = songSelectUIManager.seoriToggleSprite; break;
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
                    songSelectUIManager.MoveCurrentIndex(idx);
                    DataSaveManager.LoadResultData(header.fileName);
                    songSelectUIManager.SetSongRecord();
                }
                if (songSelectUIManager.currentContent != null) { songSelectUIManager.currentContent.tag = "Untagged"; }
                gameObject.tag = "CurrentContent";
                songSelectUIManager.currentContent = gameObject;
            }
        });

        if (idx == songSelectUIManager.currentIndex) { thisObjectToggle.isOn = true; }
        else { thisObjectToggle.isOn = false; }
    }
}
