using System;

public class BMSSongInfo
{
    public BMSHeader header;
    public string songName;
    public BMSSongInfo()
    {
        header = new BMSHeader();
    }
}

[Flags]
public enum Lntype { NONE = 0, LN1 = 1 << 1, LN2 = 1 << 2, LNOBJ = 1 << 3 }

public class BMSHeader
{
    public string bmpPath { get; set; }
    public string musicFolderPath { get; set; }
    public string textFolderPath { get; set; }
    public Lntype lnType { get; set; }

    public int rank { get; set; }
    public int lnobj { get; set; }
    public int level { get; set; }
    public int player { get; set; }
    public string stageFilePath { get; set; }
    public string backBMPPath { get; set; }
    public string bannerPath { get; set; }
    public string artist { get; set; }
    public string genre { get; set; }
    public string title { get; set; }
    public string subTitle { get; set; }
    public float total { get; set; } = 400;
    public double bpm { get; set; }
    public double minBPM { get; set; }
    public double maxBPM { get; set; }
}