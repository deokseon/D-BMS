using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using TMPro;

public class BMSFileSystem : MonoBehaviour
{
    public static BMSHeader[] headers;
    public static List<BMSHeader> selectedCategoryHeaderList;
    public static BMSHeader selectedHeader;

    [SerializeField]
    private Demo.InitOnStart initOnStart;
    private static string rootPath;

    void Awake()
    {
        if (string.IsNullOrEmpty(rootPath))
        {
#if UNITY_EDITOR
            rootPath = $@"{Directory.GetParent((Directory.GetParent(Directory.GetParent(Application.dataPath).ToString())).ToString())}\BMSFiles";
#else
            rootPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\BMSFiles";
#endif
            FileInfo[] fileInfos = new DirectoryInfo($@"{rootPath}\TextFolder").GetFiles();
            int len = fileInfos.Length;
            headers = new BMSHeader[len];
            selectedCategoryHeaderList = new List<BMSHeader>(len);

            for (int i = 0; i < len; i++) 
            { 
                ParseHeader(fileInfos[i].Name, out headers[i]);
                selectedCategoryHeaderList.Add(headers[i]);
            }
            selectedCategoryHeaderList.Sort((x, y) => { return x.level.CompareTo(y.level); });
        }

        initOnStart.DrawSongUI();
    }

    private void ParseHeader(string sname, out BMSHeader header)
    {
        header = new BMSHeader();

        StreamReader reader = new StreamReader($@"{rootPath}\TextFolder\{sname}", Encoding.GetEncoding(932));
        string line;

        header.musicFolderPath = $@"{rootPath}\MusicFolder\{sname.Substring(0, sname.Length - 6)}\";
        header.textFolderPath = $@"{rootPath}\TextFolder\{sname}";

        while((line = reader.ReadLine()) != null)
        {
            if (line.Length <= 3) { continue; }

            try
            {
                if (line.Length > 10 && line.Substring(0, 10).CompareTo("#PLAYLEVEL") == 0)
                {
                    int lvl = 0;
                    int.TryParse(line.Substring(11), out lvl);
                    header.level = lvl;
                }
                else if (line.Length > 11 && line.Substring(0, 10).CompareTo("#STAGEFILE") == 0) { header.stageFilePath = line.Substring(11); }
                else if (line.Length >= 9 && line.Substring(0, 9).CompareTo("#SUBTITLE") == 0) { header.subTitle = line.Substring(10).Trim('[', ']'); }
                else if (line.Length >= 8 && line.Substring(0, 8).CompareTo("#BACKBMP") == 0) { header.backBMPPath = line.Substring(9); }
                else if (line.Length >= 7 && line.Substring(0, 7).CompareTo("#ARTIST") == 0) { header.artist = line.Substring(8); }
                else if (line.Length >= 7 && line.Substring(0, 7).CompareTo("#BANNER") == 0) { header.bannerPath = line.Substring(8); }
                else if (line.Length >= 7 && line.Substring(0, 7).CompareTo("#LNTYPE") == 0) { header.lnType |= (Lntype)(1 << (line[8] - '0')); }
                else if (line.Length >= 6 && line.Substring(0, 6).CompareTo("#TITLE") == 0)
                {
                    header.title = line.Substring(7);
                    if (!string.IsNullOrEmpty(header.title))
                    {
                        int idx;
                        if ((idx = header.title.LastIndexOf('[')) >= 0)
                        {
                            header.subTitle = header.title.Substring(idx).Trim('[', ']');
                            header.title = header.title.Remove(idx);
                        }
                    }
                }
                else if (line.Length >= 6 && line.Substring(0, 4).CompareTo("#BPM") == 0)
                {
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
                    }
                }
                else if (line.Length >= 9 && line.Substring(0, 9).CompareTo("#CATEGORY") == 0)
                {
                    switch (line.Substring(10))
                    {
                        case "AERY": header.songCategory = Category.AERY; break;
                        case "SeoRi": header.songCategory = Category.SEORI; break;
                        default: header.songCategory = Category.NONE; break;
                    }
                }
                else if (line.Length >= 30 && line.CompareTo("*---------------------- MAIN DATA FIELD") == 0) break;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                break;
            }
        }
    }
}
