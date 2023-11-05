using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScrollIndexCallback1 : MonoBehaviour
{
    private SongInfoObject songInfoObject;

    void Awake()
    {
        songInfoObject = GetComponent<SongInfoObject>();
    }

    void Start()
    {
        songInfoObject.SetSelectToggleGroup();
    }

    void ScrollCellIndex (int idx) 
    {
        songInfoObject.FindSelectManager();
        songInfoObject.RemoveAllToggleListener();

        gameObject.name = idx.ToString();

        int len = BMSFileSystem.selectedCategoryHeaderList.Count;
        int tidx = idx;
        while (tidx < 0) { tidx += len; }
        tidx %= len;

        songInfoObject.SetSongInfo(BMSFileSystem.selectedCategoryHeaderList[tidx]);
        songInfoObject.SetClearLamp(BMSFileSystem.selectedCategoryHeaderList[tidx]);
        songInfoObject.CheckObjectTag(idx);
        songInfoObject.SelectToggleAddListener(BMSFileSystem.selectedCategoryHeaderList[tidx], idx);
        songInfoObject.SetFavoriteToggle(BMSFileSystem.selectedCategoryHeaderList[tidx]);
        songInfoObject.FavoriteToggleAddListener(BMSFileSystem.selectedCategoryHeaderList[tidx]);
        songInfoObject.CheckSongIndex(idx);
    }
}
