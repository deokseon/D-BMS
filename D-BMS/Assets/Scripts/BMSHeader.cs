using System;

[Flags]
public enum Lntype { NONE = 0, LN1 = 1 << 1, LN2 = 1 << 2, LNOBJ = 1 << 3 }

public enum Category { NONE = 0, AERY = 1, SEORI = 2 }

public enum SortBy { LEVEL = 0, TITLE = 1, BPM = 2 }

public class BMSHeader
{
    public string musicFolderPath { get; set; }
    public string textFolderPath { get; set; }
    public string fileName { get; set; }
    public Lntype lnType { get; set; }

    public Category songCategory { get; set; } = Category.NONE;

    public int lnobj { get; set; }
    public int level { get; set; }
    public string stageFilePath { get; set; }
    public string artist { get; set; }
    public string title { get; set; }
    public string subTitle { get; set; }
    public double bpm { get; set; }
    public double minBPM { get; set; }
    public double maxBPM { get; set; }
}