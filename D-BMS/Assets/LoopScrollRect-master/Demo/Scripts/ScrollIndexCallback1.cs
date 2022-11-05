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
    private TextMeshProUGUI toggleArtistBPMText;
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
        string title = header.title;
        toggleTitleText.text = title + "  <size=20>" + header.subTitle + "</size>";
        if (header.maxBPM == header.minBPM) { toggleArtistBPMText.text = header.artist + "        BPM: " + header.bpm.ToString(); }
        else { toggleArtistBPMText.text = header.artist + "        BPM: " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
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
                if (idx != songSelectUIManager.currentIndex) { songSelectUIManager.MoveCurrentIndex(idx); }
                if (songSelectUIManager.currentContent != null) { songSelectUIManager.currentContent.tag = "Untagged"; }
                gameObject.tag = "CurrentContent";
                songSelectUIManager.currentContent = gameObject;
            }
        });

        if (idx == songSelectUIManager.currentIndex) { thisObjectToggle.isOn = true; }
        else { thisObjectToggle.isOn = false; }
    }
}
