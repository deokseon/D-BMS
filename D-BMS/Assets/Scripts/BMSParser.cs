using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class BMSParser : MonoBehaviour
{
    private static BMSParser inst;
    public static BMSParser instance
    {
        get
        {
            if (!inst) { inst = FindObjectOfType<BMSParser>(); }
            return inst;
        }
    }

    [SerializeField]
    private SoundManager soundManager;
    public GameUIManager gameUIManager;

    public BMSPattern pattern { get; private set; }
    public BMSHeader header { get; private set; }

    private string[] bmsFile { get; set; }
    private Dictionary<string, double> exBpms;
    private List<int> lineIndex;

    private void SetLineIndex()
    {
        int randomValue = -1;

        RandomEffector effector = (RandomEffector)PlayerPrefs.GetInt("RandomEffector");

        if (effector == RandomEffector.NONE)
        {
            for (int i = 0; i < 5; i++) { lineIndex.Add(i); }
        }
        else if (effector == RandomEffector.RANDOM)
        {
            for (int i = 0; i < 5; i++)
            {
                do { randomValue = Random.Range(0, 5); } while (lineIndex.Contains(randomValue));
                lineIndex.Add(randomValue);
            }
        }
        else if (effector == RandomEffector.MIRROR)
        {
            for (int i = 4; i >= 0; i--) { lineIndex.Add(i); }
        }
        else if (effector == RandomEffector.FRANDOM)
        {
            for (int i = 0; i < 4; i += 3)
            {
                randomValue = Random.Range(i, i + 2);
                lineIndex.Add(randomValue);
                lineIndex.Add(randomValue == i ? i + 1 : i);
                if (!lineIndex.Contains(2)) { lineIndex.Add(2); };
            }
        }
        else if (effector == RandomEffector.MFRANDOM)
        {
            for (int i = 3; i >= 0; i -= 3)
            {
                randomValue = Random.Range(i, i + 2);
                lineIndex.Add(randomValue);
                lineIndex.Add(randomValue == i ? i + 1 : i);
                if (!lineIndex.Contains(2)) { lineIndex.Add(2); };
            }
        }
    }

    public void Parse(bool isRestart)
    {
        exBpms = null;
        lineIndex = null;
        pattern = null;
        exBpms = new Dictionary<string, double>(50);
        lineIndex = new List<int>(5);
        pattern = new BMSPattern();
        SetLineIndex();
        GetFile();
        ParseGameHeader(isRestart);
        ParseMainData();
        SetReplayNoteList();
    }

    public void GetFile()
    {
        header = BMSGameManager.header;
        bmsFile = null;
        bmsFile = File.ReadAllLines(header.textFolderPath, Encoding.GetEncoding(932));
    }

    private void ParseGameHeader(bool isRestart)
    {
        bool isIfBlockOpen = false;
        bool isValid = false;
        int randomValue = 1;
        System.Random rand = new System.Random();

        int len = bmsFile.Length;
        for (int i = 0; i < len; i++)
        {
            if (bmsFile[i].Length <= 3) { continue; }

            if (bmsFile[i].Length >= 9 && string.Compare(bmsFile[i].Substring(0, 7), "#random", true) == 0)
            {
                randomValue = rand.Next(1, int.Parse(bmsFile[i].Substring(8)) + 1);
                continue;
            }

            if (bmsFile[i].Length >= 3 && string.Compare(bmsFile[i].Substring(0, 3), "#if", true) == 0)
            {
                isIfBlockOpen = true;
                if (int.Parse(bmsFile[i].Substring(4)) == randomValue) { isValid = true; }
                continue;
            }

            if (bmsFile[i].Length >= 6 && string.Compare(bmsFile[i].Substring(0, 6), "#endif", true) == 0)
            {
                isIfBlockOpen = false;
                isValid = false;
                continue;
            }
            if (isIfBlockOpen && !isValid) { continue; }

            if (bmsFile[i].Length >= 4 && string.Compare(bmsFile[i].Substring(0, 4), "#WAV", true) == 0 && !isRestart)
            {
                int key = Decode36(bmsFile[i].Substring(4, 2));
                string path = bmsFile[i].Substring(7, bmsFile[i].Length - 11);
                soundManager.pathes.Add(new KeyValuePair<int, string>(key, path));
            }
            else if (bmsFile[i].Length >= 6 && string.Compare(bmsFile[i].Substring(0, 6), "#LNOBJ", true) == 0 && !isRestart)
            {
                header.lnobj = Decode36(bmsFile[i].Substring(7, 2));
                header.lnType |= Lntype.LNOBJ;
            }
            else if (bmsFile[i].Length >= 6 && string.Compare(bmsFile[i].Substring(0, 4), "#BMP", true) == 0)
            {
                int key = Decode36(bmsFile[i].Substring(4, 2));
                gameUIManager.bgaTextureArrayLength = gameUIManager.bgaTextureArrayLength > key ? gameUIManager.bgaTextureArrayLength : key;
                string extend = bmsFile[i].Substring(bmsFile[i].IndexOf('.', 0) + 1).ToLower();
                string path = bmsFile[i].Substring(7, bmsFile[i].Length - 7);
                if (string.Compare(extend, "mpg", true) == 0 || string.Compare(extend, "mpeg", true) == 0 ||
                    string.Compare(extend, "wmv", true) == 0 || string.Compare(extend, "avi", true) == 0 || string.Compare(extend, "mp4", true) == 0)
                {
                    pattern.bgVideoTable.Add(key, path);
                }
                else if ((string.Compare(extend, "bmp", true) == 0 || string.Compare(extend, "png", true) == 0 ||
                          string.Compare(extend, "jpg", true) == 0) && !isRestart)
                {
                    gameUIManager.bgImageList.Add(new KeyValuePair<int, string>(key, path));
                }
            }
            else if (bmsFile[i].Length >= 6 && string.Compare(bmsFile[i].Substring(0, 4), "#BPM", true) == 0)
            {
                if (bmsFile[i][4] == ' ')
                    header.bpm = double.Parse(bmsFile[i].Substring(5));
                else
                {
                    string key = bmsFile[i].Substring(4, 2);
                    double bpm = double.Parse(bmsFile[i].Substring(7));
                    exBpms.Add(key, bpm);
                }
            }
            else if (bmsFile[i].Length >= 30 && string.Compare(bmsFile[i], "*---------------------- MAIN DATA FIELD", true) == 0) break;
        }
    }

    private void ParseMainData()
    {
        bool isIfBlockOpen = false;
        bool isValid = false;
        int randomValue = 1;
        System.Random rand = new System.Random();

        double beatC = 1.0d;

        int lnBits = 0;

        int len = bmsFile.Length;
        for (int i = 0; i < len; i++)
        {
            if (bmsFile[i].Length == 0) { continue; }
            if (bmsFile[i][0] != '#') { continue; }

            if (bmsFile[i].Length >= 9 && string.Compare(bmsFile[i].Substring(0, 7), "#random", true) == 0)
            {
                randomValue = rand.Next(1, int.Parse(bmsFile[i].Substring(8)) + 1);
                continue;
            }

            if (bmsFile[i].Length >= 3 && string.Compare(bmsFile[i].Substring(0, 3), "#if", true) == 0)
            {
                isIfBlockOpen = true;
                if (int.Parse(bmsFile[i].Substring(4)) == randomValue) { isValid = true; }
                continue;
            }

            if (bmsFile[i].Length >= 6 && string.Compare(bmsFile[i].Substring(0, 6), "#endif", true) == 0)
            {
                isIfBlockOpen = false;
                isValid = false;
                continue;
            }
            if (isIfBlockOpen && !isValid) { continue; }

            int bar;
            if (!int.TryParse(bmsFile[i].Substring(1, 3), out bar)) { continue; }

            if (pattern.barCount < bar) { pattern.barCount = bar; }

            if (bmsFile[i][4] == '1' || bmsFile[i][4] == '5')
            {
                int line, beatLength;
                line = lineIndex[bmsFile[i][5] - '1'];
                beatLength = (bmsFile[i].Length - 7) / 2;

                int iLen = bmsFile[i].Length - 1;
                for (int k = 7; k < iLen; k += 2)
                {
                    int keySound = Decode36(bmsFile[i].Substring(k, 2));

                    if (keySound == 0) { continue; }

                    if (bmsFile[i][4] == '5')
                    {
                        if ((lnBits & (1 << line)) != 0)
                        {
                            pattern.AddNote(line, bar + 2, (k - 7) / 2, beatLength, -1, 1);
                            lnBits &= ~(1 << line);
                            continue;
                        }
                        else { lnBits |= (1 << line); }
                    }
                    if (header.lnType.HasFlag(Lntype.LNOBJ) && keySound == header.lnobj)
                    {
                        pattern.AddNote(line, bar + 2, (k - 7) / 2, beatLength, keySound, 1);
                    }
                    else
                    {
                        pattern.AddNote(line, bar + 2, (k - 7) / 2, beatLength, keySound, 0);
                    }
                }
            }
            else if (bmsFile[i][4] == '0')
            {
                int beatLength;
                if (bmsFile[i][5] == '1')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;

                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        int keySound = Decode36(bmsFile[i].Substring(k, 2));

                        if (keySound != 0)
                        {
                            pattern.AddBGSound(bar + 2, (k - 7) / 2, beatLength, keySound);
                        }
                    }
                }
                else if (bmsFile[i][5] == '2')
                {
                    beatC = double.Parse(bmsFile[i].Substring(7));
                    pattern.AddNewBeatC(bar + 2, beatC);
                }
                else if (bmsFile[i][5] == '3')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;

                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        double bpm = int.Parse(bmsFile[i].Substring(k, 2), System.Globalization.NumberStyles.HexNumber);

                        if (bpm != 0) { pattern.AddBPM(bar + 2, (k - 7) / 2, beatLength, bpm); }
                    }
                }
                else if (bmsFile[i][5] == '4')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;
                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        int key = Decode36(bmsFile[i].Substring(k, 2));
                        gameUIManager.bgaTextureArrayLength = gameUIManager.bgaTextureArrayLength > key ? gameUIManager.bgaTextureArrayLength : key;
                        if (key != 0)
                        {
                            if (pattern.bgVideoTable.ContainsKey(key))
                            {
                                pattern.AddBGAChange(bar + 2, (k - 7) / 2, beatLength, key);
                            }
                            else
                            {
                                pattern.AddBGAChange(bar + 2, (k - 7) / 2, beatLength, key, true);
                            }
                        }
                    }
                }
                else if (bmsFile[i][5] == '7')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;
                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        int key = Decode36(bmsFile[i].Substring(k, 2));
                        gameUIManager.bgaTextureArrayLength = gameUIManager.bgaTextureArrayLength > key ? gameUIManager.bgaTextureArrayLength : key;
                        if (key != 0)
                        {
                            if (pattern.bgVideoTable.ContainsKey(key))
                            {
                                pattern.AddBGAChange(bar + 2, (k - 7) / 2, beatLength, key);
                            }
                            else
                            {
                                gameUIManager.layerImageSet.Add(key);
                                pattern.AddLayerChange(bar + 2, (k - 7) / 2, beatLength, key, true);
                            }
                        }
                    }
                }
                else if (bmsFile[i][5] == '8')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;
                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        string key = bmsFile[i].Substring(k, 2);
                        if (key.CompareTo("00") != 0) pattern.AddBPM(bar + 2, (k - 7) / 2, beatLength, exBpms[key]);
                    }
                }
            }
        }
        len = pattern.barCount + 20;
        for (int i = 0; i < len; i++)
        {
            pattern.AddBar(i, 0, 1, 0, 3);
        }
    }

    public static int Decode36(string str)
    {
        if (str.Length != 2) return -1;

        int result = 0;
        if (str[1] >= 'A')
            result += str[1] - 'A' + 10;
        else
            result += str[1] - '0';
        if (str[0] >= 'A')
            result += (str[0] - 'A' + 10) * 36;
        else
            result += (str[0] - '0') * 36;

        return result;
    }

    private void SetReplayNoteList()
    {
        if (!BMSGameManager.isReplay) { return; }
        for (int i = 0; i < 5; i++)
        {
            pattern.lines[i].noteList = new List<Note>(BMSGameManager.replayData.noteList[i].Count);
            for (int j = 0; j < BMSGameManager.replayData.noteList[i].Count; j++)
            {
                pattern.lines[i].noteList.Add(new Note(BMSGameManager.replayData.noteList[i][j]));
            }
        }
    }
}
