using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongInfoObject : MonoBehaviour
{
    private SongSelectUIManager songSelectUIManager = null;

    [SerializeField]
    private Toggle songSelectToggle;
    [SerializeField]
    private TextMeshProUGUI songTitleText;
    [SerializeField]
    private TextMeshProUGUI songSubtitleText;
    [SerializeField]
    private TextMeshProUGUI songArtistText;
    [SerializeField]
    private TextMeshProUGUI songBPMText;
    [SerializeField]
    private TextMeshProUGUI songLevelText;
    [SerializeField]
    private Image clearLampImage;
    [SerializeField]
    private Sprite normalClearLampSprite;
    [SerializeField]
    private Sprite allCoolClearLampSprite;
    [SerializeField]
    private Toggle favoriteToggle;

    public void FindSelectManager()
    {
        if (songSelectUIManager == null)
        {
            songSelectUIManager = FindObjectOfType<SongSelectUIManager>();
        }
    }

    public void SetSelectToggleGroup()
    {
        songSelectToggle.group = songSelectUIManager.songToggleGroup;
    }

    public void SetSongInfo(BMSHeader header)
    {
        songTitleText.text = header.title;
        songSubtitleText.text = header.subTitle;
        SetTextFontSize(songTitleText, songSubtitleText, 28, 17, -445.0f, 13.0f, 800.0f);

        songArtistText.text = header.artist;
        if (header.maxBPM == header.minBPM) { songBPMText.text = "BPM " + header.bpm.ToString(); }
        else { songBPMText.text = "BPM " + header.minBPM.ToString() + " ~ " + header.maxBPM.ToString(); }
        SetTextFontSize(songArtistText, songBPMText, 15, 15, -420.0f, -20.0f, 800.0f);

        songLevelText.text = header.level.ToString();
    }

    private void SetTextFontSize(TextMeshProUGUI frontText, TextMeshProUGUI backText, int frontTextFontSize, int backTextFontSize, float textStartPosX, float textPosY, float maxTextLength)
    {
        frontText.fontSize = frontTextFontSize;
        backText.fontSize = backTextFontSize;
        backText.rectTransform.localPosition = new Vector3(textStartPosX + frontText.preferredWidth, textPosY, 0.0f);
        while (frontText.preferredWidth + backText.preferredWidth > maxTextLength)
        {
            frontText.fontSize -= 0.1f;
            backText.rectTransform.localPosition = new Vector3(textStartPosX + frontText.preferredWidth, textPosY, 0.0f);
            backText.fontSize -= 0.1f;
        }
    }

    public void SetClearLamp(BMSHeader header)
    {
        int clearlamp;
        if (!BMSFileSystem.songClearLamp.clearLampDict.TryGetValue(header.fileName, out clearlamp))
        {
            clearlamp = -1;
        }
        switch (clearlamp)
        {
            case -1:
                clearLampImage.sprite = normalClearLampSprite;
                clearLampImage.color = Color.gray;
                break;
            case 0:
                clearLampImage.sprite = normalClearLampSprite;
                clearLampImage.color = Color.red;
                break;
            case 1:
                clearLampImage.sprite = normalClearLampSprite;
                clearLampImage.color = new Color(0.0f, 215.0f / 255.0f, 1.0f);
                break;
            case 2:
                clearLampImage.sprite = normalClearLampSprite;
                clearLampImage.color = Color.yellow;
                break;
            case 3:
                clearLampImage.sprite = allCoolClearLampSprite;
                clearLampImage.color = Color.white;
                break;
        }
    }

    public void CheckObjectTag(int index)
    {
        if (gameObject.CompareTag("CurrentContent")) { songSelectUIManager.songToggleGroup.allowSwitchOff = (index != songSelectUIManager.currentIndex); }
    }

    public void CheckSongIndex(int index)
    {
        if (index == songSelectUIManager.currentIndex) 
        { 
            songSelectToggle.isOn = true;
        }
        else 
        { 
            songSelectToggle.isOn = false;
        }
    }

    public void RemoveAllToggleListener()
    {
        songSelectToggle.onValueChanged.RemoveAllListeners();
        favoriteToggle.onValueChanged.RemoveAllListeners();
    }

    public void SelectToggleAddListener(BMSHeader header, int index)
    {
        songSelectToggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                songSelectUIManager.songToggleGroup.allowSwitchOff = false;
                songSelectUIManager.DrawSongInfoUI(header);
                if (index != songSelectUIManager.currentIndex)
                {
                    songSelectUIManager.MoveToIndex(index);
                    SongSelectUIManager.resultData = DataSaveManager.LoadData<ResultData>("DataSave", header.fileName + ".json") ?? new ResultData(11);
                    songSelectUIManager.SetSongRecord();
                }
                if (songSelectUIManager.currentContent != null) { songSelectUIManager.currentContent.tag = "Untagged"; }
                gameObject.tag = "CurrentContent";
                songSelectUIManager.currentContent = gameObject;
                songLevelText.color = Color.white;
            }
            else
            {
                songLevelText.color = new Color32(180, 180, 180, 255);
            }
        });
    }

    public void FavoriteToggleAddListener(BMSHeader header)
    {
        favoriteToggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                BMSFileSystem.favoriteSong.favoriteSongSet.Add(header.fileName);
            }
            else
            {
                BMSFileSystem.favoriteSong.favoriteSongSet.Remove(header.fileName);
            }
            DataSaveManager.SaveData("DataSave", "FavoriteSong.json", BMSFileSystem.favoriteSong);
        });
    }

    public void SetFavoriteToggle(BMSHeader header)
    {
        favoriteToggle.isOn = BMSFileSystem.favoriteSong.favoriteSongSet.Contains(header.fileName);
    }
}
