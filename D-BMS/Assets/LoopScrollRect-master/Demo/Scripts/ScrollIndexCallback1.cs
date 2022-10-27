using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScrollIndexCallback1 : MonoBehaviour 
{
    private Toggle thisObjectToggle = null;
    private SongSelectUIManager songSelectUIManager = null;
    private LoopVerticalScrollRect lvScrollRect = null;
    private ToggleGroup toggleGroup = null;

    [SerializeField]
    private TextMeshProUGUI toggleTitleText;
    [SerializeField]
    private TextMeshProUGUI toggleArtistBPMText;
    [SerializeField]
    private TextMeshProUGUI toggleLevelText;

    void Start()
    {
        if (thisObjectToggle == null) { thisObjectToggle = GetComponent<Toggle>(); }
        if (songSelectUIManager == null) { songSelectUIManager = FindObjectOfType<SongSelectUIManager>(); }
        if (lvScrollRect == null) { lvScrollRect = FindObjectOfType<LoopVerticalScrollRect>(); }
        if (toggleGroup == null) { toggleGroup = FindObjectOfType<ToggleGroup>(); }
        thisObjectToggle.group = toggleGroup;
    }

    void ScrollCellIndex (int idx) 
    {
        if (thisObjectToggle == null) { thisObjectToggle = GetComponent<Toggle>(); }
        if (songSelectUIManager == null) { songSelectUIManager = FindObjectOfType<SongSelectUIManager>(); }
        if (lvScrollRect == null) { lvScrollRect = FindObjectOfType<LoopVerticalScrollRect>(); }
        if (toggleGroup == null) { toggleGroup = FindObjectOfType<ToggleGroup>(); }

        thisObjectToggle.onValueChanged.RemoveAllListeners();

        gameObject.name = idx.ToString();

        int len = BMSFileSystem.headers.Length;
        int tidx = idx;
        while (tidx < 0) { tidx += len; }
        tidx %= len;

        BMSHeader header = BMSFileSystem.headers[tidx];
        string title = header.title;
        toggleTitleText.text = title + "  <size=20>" + header.subTitle + "</size>";
        if (header.maxBPM == header.minBPM) { toggleArtistBPMText.text = header.artist + "        BPM: " + header.bpm.ToString(); }
        else { toggleArtistBPMText.text = header.artist + "        BPM: " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
        toggleLevelText.text = header.level.ToString();

        if (gameObject.CompareTag("CurrentContent")) { toggleGroup.allowSwitchOff = (idx != songSelectUIManager.currentIndex); }

        thisObjectToggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                toggleGroup.allowSwitchOff = false;
                songSelectUIManager.DrawSongInfoUI(header);
                if (idx != songSelectUIManager.currentIndex) 
                { 
                    songSelectUIManager.currentIndex = idx;
                    lvScrollRect.ScrollToCellWithinTime(idx, 0.05f);
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
