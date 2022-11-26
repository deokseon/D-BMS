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
            int bmsFileCount = 0;
            for (int i = 0; i < len; i++)
            {
                string extend = fileInfos[i].Name.Substring(fileInfos[i].Name.IndexOf('.', 0) + 1).ToLower();
                if (extend.CompareTo("bms") == 0 || extend.CompareTo("bme") == 0 || extend.CompareTo("bml") == 0) { bmsFileCount++; }
            }
            headers = new BMSHeader[bmsFileCount];
            selectedCategoryHeaderList = new List<BMSHeader>(bmsFileCount);

            int headersIndex = 0;
            for (int i = 0; i < len; i++) 
            {
                string extend = fileInfos[i].Name.Substring(fileInfos[i].Name.IndexOf('.', 0) + 1).ToLower();
                if (extend.CompareTo("bms") != 0 && extend.CompareTo("bme") != 0 && extend.CompareTo("bml") != 0) { continue; }
                ParseHeader(fileInfos[i].Name, out headers[headersIndex]);
                selectedCategoryHeaderList.Add(headers[headersIndex++]);
            }
            selectedCategoryHeaderList.Sort((x, y) => {
                int result = x.level.CompareTo(y.level);
                return result != 0 ? result : string.Compare(x.title, y.title);
            });
        }
        initOnStart.DrawSongUI();
    }

    private void ParseHeader(string sname, out BMSHeader header)
    {
        header = new BMSHeader();

        StreamReader reader = new StreamReader($@"{rootPath}\TextFolder\{sname}", Encoding.GetEncoding(932));

        header.musicFolderPath = $@"{rootPath}\MusicFolder\{sname.Substring(0, sname.Length - 9)}\";
        header.textFolderPath = $@"{rootPath}\TextFolder\{sname}";

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length <= 3) { continue; }

            string temp = line.Substring(0, 4).ToUpper();
            try
            {
                switch (temp)
                {
                    case "#STA": header.stageFilePath = line.Substring(11); break;   // STAGEFILE
                    case "#BAC": header.backBMPPath = line.Substring(9); break;      // BACKBMP
                    case "#ART": header.artist = line.Substring(8); break;           // ARTIST
                    case "#BAN": header.bannerPath = line.Substring(8); break;       // BANNER
                    case "#LNT": header.lnType |= (Lntype)(1 << (line[8] - '0')); break;  // LNTYPE
                    case "#TIT":   // TITLE
                        header.title = line.Substring(7);
                        if (!string.IsNullOrEmpty(header.title))
                        {
                            int idx;
                            if ((idx = header.title.LastIndexOf('[')) >= 0)
                            {
                                header.subTitle = header.title.Substring(idx).Trim('[', ']');
                                header.title = header.title.Remove(idx);
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
                        if (string.Compare(line, "*---------------------- MAIN DATA FIELD", true) == 0) { return; }
                        break;
                    case "#PLA":  // PLAYLEVEL
                        if (line[5] == 'L')
                        {
                            int lvl = 0;
                            int.TryParse(line.Substring(11), out lvl);
                            header.level = lvl;
                        } break;
                    case "#SUB":  // SUBTITLE
                        if (line[4] == 'T') { header.subTitle = line.Substring(10).Trim('[', ']'); }
                        break;
                    default: break;
                }
            }
            catch (System.Exception e) { Debug.LogWarning(e); break; }
        }
    }
}
