using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    private SoundManager soundManager;
    private BMSGameManager bmsGameManager;
    public GameUIManager gameUIManager;

    public BMSPattern pattern { get; private set; }
    public BMSHeader header { get; private set; }

    private string[] bmsFile { get; set; }
    private Dictionary<string, double> exBpms;

    private List<int> lineIndex;

    private void Awake()
    {
        exBpms = new Dictionary<string, double>();
        soundManager = GetComponent<SoundManager>();
        bmsGameManager = GetComponent<BMSGameManager>();
        lineIndex = new List<int>();
        SetLineIndex();
    }

    private void SetLineIndex()
    {
        int randomValue = -1;

        if (BMSGameManager.randomEffector == RandomEffector.NONE)
        {
            for (int i = 0; i < 5; i++) { lineIndex.Add(i); }
        }
        else if (BMSGameManager.randomEffector == RandomEffector.RANDOM)
        {
            for (int i = 0; i < 5; i++)
            {
                do { randomValue = Random.Range(0, 5); } while (lineIndex.Contains(randomValue));
                lineIndex.Add(randomValue);
            }
        }
        else if (BMSGameManager.randomEffector == RandomEffector.MIRROR)
        {
            for (int i = 4; i >= 0; i--) { lineIndex.Add(i); }
        }
        else if (BMSGameManager.randomEffector == RandomEffector.FRANDOM)
        {
            for (int i = 0; i < 4; i += 3)
            {
                randomValue = Random.Range(i, i + 2);
                lineIndex.Add(randomValue);
                lineIndex.Add(randomValue == i ? i + 1 : i);
                if (!lineIndex.Contains(2)) { lineIndex.Add(2); };
            }
        }
        else if (BMSGameManager.randomEffector == RandomEffector.MFRANDOM)
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

    public void Parse()
    {
        pattern = new BMSPattern();
        GetFile();
        ParseGameHeader();
        ParseMainData();
    }

    public void GetFile()
    {
        header = BMSGameManager.header;
        bmsFile = File.ReadAllLines(header.textFolderPath);
    }

    private void ParseGameHeader()
    {
        int len = bmsFile.Length;
        for (int i = 0; i < len; i++)
        {
            if (bmsFile[i].Length <= 3) { continue; }

            if (bmsFile[i].Length >= 4 && bmsFile[i].Substring(0, 4).CompareTo("#WAV") == 0)
            {
                int key = Decode36(bmsFile[i].Substring(4, 2));
                string path = bmsFile[i].Substring(7, bmsFile[i].Length - 11);
                soundManager.pathes.Add(new KeyValuePair<int, string>(key, path));
            }
            else if (bmsFile[i].Length >= 6 && bmsFile[i].Substring(0, 6).CompareTo("#LNOBJ") == 0)
            {
                header.lnobj = Decode36(bmsFile[i].Substring(7, 2));
                header.lnType |= Lntype.LNOBJ;
            }
            else if (bmsFile[i].Length >= 6 && bmsFile[i].Substring(0, 4).CompareTo("#BMP") == 0)
            {
                string key = bmsFile[i].Substring(4, 2);
                string extend = bmsFile[i].Substring(bmsFile[i].Length - 3, 3);
                string path = bmsFile[i].Substring(7, bmsFile[i].Length - 7);
                if (extend.CompareTo("mpg") == 0)
                {
                    pattern.bgVideoTable.Add(key, path);
                }
                else if (extend.CompareTo("bmp") == 0 || extend.CompareTo("png") == 0)
                {
                    gameUIManager.bgImageTable.Add(key, path);
                }
            }
            else if (bmsFile[i].Length >= 6 && bmsFile[i].Substring(0, 4).CompareTo("#BPM") == 0)
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
            else if (bmsFile[i].Length >= 30 && bmsFile[i].CompareTo("*---------------------- MAIN DATA FIELD") == 0) break;
        }
    }

    private void ParseMainData()
    {
        double beatC = 1.0d;

        int lnBits = 0;

        int len = bmsFile.Length;
        for (int i = 0; i < len; i++)
        {
            if (bmsFile[i].Length == 0) { continue; }
            if (bmsFile[i][0] != '#') { continue; }

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
                            pattern.AddNote(line, bar, (k - 7) / 2, beatLength, -1, 1);
                            lnBits &= ~(1 << line);
                            continue;
                        }
                        else { lnBits |= (1 << line); }
                    }
                    if (header.lnType.HasFlag(Lntype.LNOBJ) && keySound == header.lnobj)
                    {
                        PushLongNoteTick(line, bar, (((double)(k - 7) / 2.0d) / (double)beatLength) * 4.0d);
                        pattern.AddNote(line, bar, (k - 7) / 2, beatLength, keySound, 1);
                    }
                    else
                    {
                        pattern.AddNote(line, bar, (k - 7) / 2, beatLength, keySound, 0);
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
                            pattern.AddBGSound(bar, (k - 7) / 2, beatLength, keySound);
                        }
                    }
                }
                else if (bmsFile[i][5] == '2')
                {
                    beatC = double.Parse(bmsFile[i].Substring(7));
                    pattern.AddNewBeatC(bar, beatC);
                }
                else if (bmsFile[i][5] == '3')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;

                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        double bpm = int.Parse(bmsFile[i].Substring(k, 2), System.Globalization.NumberStyles.HexNumber);

                        if (bpm != 0) { pattern.AddBPM(bar, (k - 7) / 2, beatLength, bpm); }
                    }
                }
                else if (bmsFile[i][5] == '4')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;
                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        string key = bmsFile[i].Substring(k, 2);
                        if (key.CompareTo("00") != 0)
                        {
                            if (pattern.bgVideoTable.ContainsKey(key))
                            {
                                pattern.AddBGAChange(bar, (k - 7) / 2, beatLength, key);
                            }
                            else
                            {
                                pattern.AddBGAChange(bar, (k - 7) / 2, beatLength, key, true);
                            }
                        }
                    }
                }
                else if (bmsFile[i][5] == '8')
                {
                    beatLength = (bmsFile[i].Length - 7) / 2;
                    //int idx = Decode36(s.Substring(7, 2)) - 1;
                    int iLen = bmsFile[i].Length - 1;
                    for (int k = 7; k < iLen; k += 2)
                    {
                        string key = bmsFile[i].Substring(k, 2);
                        if (key.CompareTo("00") != 0) pattern.AddBPM(bar, (k - 7) / 2, beatLength, exBpms[key]);
                    }
                }
            }
        }
        len = pattern.barCount + 2;
        for (int i = 0; i < len; i++)
        {
            pattern.AddBar(i, 0, 1, 0, 3);
        }
    }

    private void PushLongNoteTick(int line, int bar, double beat)
    {
        double currentBeat = 0.0d;
        int currentBar = pattern.lines[line].noteList.Peek.bar;
        double prevBeat = pattern.lines[line].noteList.Peek.beat;
        double endBeat = beat - 0.25d;

        while (true)
        {
            currentBeat += 0.25d;
            if (currentBeat <= prevBeat) { continue; }

            if (currentBeat >= 4.0d)
            {
                currentBeat = 0.0d;
                currentBar++;
            }

            if (currentBar == bar && currentBeat > endBeat) { break; }

            pattern.AddNote(line, currentBar, currentBeat, 0, 2);
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
}
