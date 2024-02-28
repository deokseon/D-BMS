using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class BMSFileSystem : MonoBehaviour
{
    private FileInfo[] fileInfos;
    private List<string> bmsTextFileList;
    public static BMSHeader[] headers;
    public static List<BMSHeader> selectedCategoryHeaderList;
    public static BMSHeader selectedHeader;

    [SerializeField]
    private Demo.InitOnStart initOnStart;
    private static string rootPath;

    public static SongClearLamp songClearLamp;
    public static FavoriteSong favoriteSong;

    private int threadCount = 4;
    private int completeThreadCount;
    private bool isCompleteHeadersSort = true;
    private readonly object threadLock = new object();

    void Awake()
    {
        if (string.IsNullOrEmpty(rootPath))
        {
            completeThreadCount = 0;
            rootPath = $@"{Directory.GetParent(Application.dataPath)}\BMSFiles";
            fileInfos = new DirectoryInfo($@"{rootPath}\TextFolder").GetFiles();
            bmsTextFileList = new List<string>(fileInfos.Length);
            for (int i = fileInfos.Length - 1; i >= 0; i--)
            {
                string extend = fileInfos[i].Name.Substring(fileInfos[i].Name.IndexOf('.', 0) + 1).ToLower();
                if (extend.CompareTo("bms") == 0 || extend.CompareTo("bme") == 0 || extend.CompareTo("bml") == 0) 
                {
                    bmsTextFileList.Add(fileInfos[i].Name);
                }
            }
            headers = new BMSHeader[bmsTextFileList.Count];
            selectedCategoryHeaderList = new List<BMSHeader>(bmsTextFileList.Count);

            for (int i = 0; i < threadCount; i++)
            {
                AddHeaderAsync(i);
            }

            _ = WaitHeadersSort();
        }

        songClearLamp = songClearLamp ?? DataSaveManager.LoadData<SongClearLamp>("DataSave", "ClearLamp.json") ?? new SongClearLamp();
        favoriteSong = favoriteSong ?? DataSaveManager.LoadData<FavoriteSong>("DataSave", "FavoriteSong.json") ?? new FavoriteSong();
        _ = WaitDrawSongUI();
    }

    async private void AddHeaderAsync(int value)
    {
        await Task.Run(() =>
        {
            int start = (int)(value / (double)threadCount * bmsTextFileList.Count);
            int end = (int)((value + 1) / (double)threadCount * bmsTextFileList.Count);
            for (int i = start; i < end; i++)
            {
                ParseHeader(bmsTextFileList[i], out headers[i]);
                lock (threadLock)
                {
                    selectedCategoryHeaderList.Add(headers[i]);
                }
            }
            lock (threadLock)
            {
                completeThreadCount++;
            }
        });
    }

    private async UniTask WaitHeadersSort()
    {
        isCompleteHeadersSort = false;
        await UniTask.WaitUntil(() => completeThreadCount == threadCount);

        selectedCategoryHeaderList.Sort((x, y) => {
            int result = x.level.CompareTo(y.level);
            return result != 0 ? result : string.Compare(x.title, y.title);
        });

        isCompleteHeadersSort = true;
    }

    private async UniTask WaitDrawSongUI()
    {
        await UniTask.WaitUntil(() => isCompleteHeadersSort);

        initOnStart.DrawSongUI();

        FindObjectOfType<SongSelectUIManager>().SetSongScrollView();
    }

    private void ParseHeader(string sname, out BMSHeader header)
    {
        header = new BMSHeader();
        header.musicFolderPath = $@"{rootPath}\MusicFolder\{sname.Substring(0, sname.Length - 9)}\";
        header.textFolderPath = $@"{rootPath}\TextFolder\{sname}";
        header.fileName = sname.Substring(0, sname.Length - 4);

        StreamReader reader = new StreamReader($@"{rootPath}\TextFolder\{sname}", Encoding.GetEncoding(932));
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length <= 3) { continue; }

            string temp = line.Substring(0, 4).ToUpper();
            try
            {
                switch (temp)
                {
                    case "#STA": header.stageFilePath = line.Substring(11, line.Length - 15); break;   // STAGEFILE
                    case "#ART": header.artist = line.Substring(8); break;           // ARTIST
                    case "#LNT": header.lnType |= (Lntype)(1 << (line[8] - '0')); break;  // LNTYPE
                    case "#SUB":  // SUBTITLE
                        if (line[4] == 'T') { header.subTitle = line.Substring(10).Trim('[', ']'); }
                        break;
                    case "#PLA":  // PLAYLEVEL
                        if (line[5] == 'L')
                        {
                            int lvl = 0;
                            int.TryParse(line.Substring(11), out lvl);
                            header.level = lvl;
                        } break;
                    case "#TIT":   // TITLE
                        header.title = line.Substring(7);
                        if (!string.IsNullOrEmpty(header.title))
                        {
                            int idx;
                            if ((idx = header.title.LastIndexOf('[')) >= 0)
                            {
                                header.subTitle = header.title.Substring(idx).Trim('[', ']');
                                header.title = header.title.Remove(idx);
                                if (header.title == null || header.title.Length == 0)
                                {
                                    header.title = "[" + header.subTitle + "]";
                                    header.subTitle = null;
                                }
                            }
                        } break;
                    case "#BPM":  // BPM
                        if (line[4] == ' ')
                        {
                            header.bpm = double.Parse(line.Substring(5));
                            header.minBPM = header.bpm;
                            header.maxBPM = header.bpm;
                        }
                        else
                        {
                            double tempBPM = double.Parse(line.Substring(7));
                            if (header.minBPM > tempBPM) { header.minBPM = tempBPM; }
                            if (header.maxBPM < tempBPM) { header.maxBPM = tempBPM; }
                        } break;
                    case "#CAT":  // CATEGORY
                        switch (line.Substring(10))
                        {
                            case "AERY": header.songCategory = Category.AERY; break;
                            case "SeoRi": header.songCategory = Category.SEORI; break;
                            default: header.songCategory = Category.NONE; break;
                        } break;
                    case "*---":
                        if (string.Compare(line, "*---------------------- MAIN DATA FIELD", true) == 0) { ParseBPM(reader, header); return; }
                        break;
                    default: break;
                }
            }
            catch (System.Exception e) { Debug.LogWarning(e); break; }
        }
    }

    private void ParseBPM(StreamReader reader, BMSHeader header)
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            int len = line.Length;
            if (len < 9 || line[0] != '#' || line[5] != '3' || line[4] != '0') { continue; }

            int iLen = len - 1;
            for (int k = 7; k < iLen; k += 2)
            {
                double bpm = int.Parse(line.Substring(k, 2), System.Globalization.NumberStyles.HexNumber);

                if (bpm == 0) { continue; } 

                if (header.minBPM > bpm) { header.minBPM = bpm; }
                if (header.maxBPM < bpm) { header.maxBPM = bpm; }
            }
        }
    }
}
