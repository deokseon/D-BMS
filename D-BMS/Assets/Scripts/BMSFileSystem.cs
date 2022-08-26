using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class BMSFileSystem : MonoBehaviour
{
    public static string[] pathes;
    public static BMSSongInfo[] songInfos;
    public static BMSHeader selectedHeader;
    public static string selectedPath;

    [SerializeField]
    private SongSelectUIManager songSelectUIManager;

    void Awake()
    {
        DirectoryInfo direcInfo = new DirectoryInfo("Assets/Resources/BMSFiles/TextFolder");

        int len = direcInfo.GetFiles().Length / 2;
        songInfos = new BMSSongInfo[len];
        pathes = new string[len];

        int k = 0;
        foreach (FileInfo file in direcInfo.GetFiles())
        {
            if (file.Name.Substring(file.Name.Length - 4, 4).CompareTo("meta") == 0) { continue; }
            pathes[k] = file.Name;
            ParseHeader(pathes[k], out songInfos[k++]);
        }

        songSelectUIManager.DrawSongUI(songInfos);
    }

    private void ParseHeader(string sname, out BMSSongInfo songInfo)
    {
        songInfo = new BMSSongInfo();

        string[] file = File.ReadAllLines("Assets/Resources/BMSFiles/TextFolder/" + sname, Encoding.GetEncoding(932));

        int len = file.Length;
        if (len == 0) { return; }

        songInfo.header.bmpPath = "Assets/Resources/BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/";
        songInfo.header.musicFolderPath = "BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/";
        songInfo.header.textFolderPath = "Assets/Resources/BMSFiles/TextFolder/" + sname;

        for (int i = 0; i < len; i++)
        {
            if (file[i].Length <= 3) { continue; }

            try
            {
                if (file[i].Length > 10 && file[i].Substring(0, 10).CompareTo("#PLAYLEVEL") == 0)
                {
                    int lvl = 0;
                    int.TryParse(file[i].Substring(11), out lvl);
                    songInfo.header.level = lvl;
                }
                else if (file[i].Length > 11 && file[i].Substring(0, 10).CompareTo("#STAGEFILE") == 0)
                {
                    if (file[i].EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
                        songInfo.header.stageFilePath = "Assets/Resources/BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/" + file[i].Substring(11, file[i].Length - 11);
                    else
                        songInfo.header.stageFilePath = "BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/" + file[i].Substring(11, file[i].Length - 15);
                }
                else if (file[i].Length >= 9 && file[i].Substring(0, 9).CompareTo("#SUBTITLE") == 0) songInfo.header.subTitle = file[i].Substring(10).Trim('[', ']');
                else if (file[i].Length >= 8 && file[i].Substring(0, 8).CompareTo("#BACKBMP") == 0)
                {
                    if (file[i].EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
                        songInfo.header.backBMPPath = "Assets/Resources/BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/" + file[i].Substring(9, file[i].Length - 9);
                    else
                        songInfo.header.backBMPPath = "BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/" + file[i].Substring(9, file[i].Length - 13);
                }
                else if (file[i].Length >= 8 && file[i].Substring(0, 7).CompareTo("#PLAYER") == 0) songInfo.header.player = file[i][8] - '0';
                else if (file[i].Length >= 7 && file[i].Substring(0, 7).CompareTo("#ARTIST") == 0) songInfo.header.artist = file[i].Substring(8);
                else if (file[i].Length >= 7 && file[i].Substring(0, 7).CompareTo("#BANNER") == 0)
                {
                    if (file[i].EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
                        songInfo.header.bannerPath = "BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/" + file[i].Substring(8, file[i].Length - 8);
                    else
                        songInfo.header.bannerPath = "BMSFiles/MusicFolder/" + sname.Substring(0, sname.Length - 6) + "/" + file[i].Substring(8, file[i].Length - 12);
                }
                else if (file[i].Length >= 7 && file[i].Substring(0, 7).CompareTo("#LNTYPE") == 0) songInfo.header.lnType |= (Lntype)(1 << (file[i][8] - '0'));
                else if (file[i].Length >= 7 && file[i].Substring(0, 6).CompareTo("#GENRE") == 0) songInfo.header.genre = file[i].Substring(7);
                else if (file[i].Length >= 6 && file[i].Substring(0, 6).CompareTo("#TITLE") == 0)
                {
                    songInfo.header.title = file[i].Substring(7);
                    if (!string.IsNullOrEmpty(songInfo.header.title))
                    {
                        int idx;
                        if ((idx = songInfo.header.title.LastIndexOf('[')) >= 0)
                        {
                            string name = songInfo.header.title.Remove(idx);
                            if (string.IsNullOrEmpty(songInfo.songName) || songInfo.songName.Length > name.Length)
                                songInfo.songName = name;
                            songInfo.header.subTitle = songInfo.header.title.Substring(idx).Trim('[', ']');
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(songInfo.songName) || songInfo.songName.Length > songInfo.header.title.Length)
                                songInfo.songName = songInfo.header.title;
                        }
                    }
                    songInfo.header.title = songInfo.songName;
                }
                else if (file[i].Length >= 6 && file[i].Substring(0, 6).CompareTo("#TOTAL") == 0)
                {
                    float tot = 160;
                    float.TryParse(file[i].Substring(7), out tot);
                    songInfo.header.total = tot;
                }
                else if (file[i].Length >= 5 && file[i].Substring(0, 5).CompareTo("#RANK") == 0) songInfo.header.rank = int.Parse(file[i].Substring(6));
                else if (file[i].Length >= 6 && file[i].Substring(0, 4).CompareTo("#BPM") == 0)
                {
                    if (file[i][4] == ' ')
                    {
                        songInfo.header.bpm = double.Parse(file[i].Substring(5));
                        songInfo.header.minBPM = songInfo.header.bpm;
                        songInfo.header.maxBPM = songInfo.header.bpm;
                    }
                    else
                    {
                        double tempBPM = double.Parse(file[i].Substring(7));
                        if (songInfo.header.minBPM > tempBPM) { songInfo.header.minBPM = tempBPM; }
                        if (songInfo.header.maxBPM < tempBPM) { songInfo.header.maxBPM = tempBPM; }
                    }
                }
                else if (file[i].Length >= 30 && file[i].CompareTo("*---------------------- MAIN DATA FIELD") == 0) break;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                break;
            }
        }
    }
}
