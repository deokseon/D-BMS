using UnityEngine;
using System.Collections.Generic;

public class Line
{
    public List<Note> noteList;
    public Line()
    {
        noteList = new List<Note>() { Capacity = 250 };
    }
}

public abstract class BMSObject : System.IComparable<BMSObject>
{
    public int bar { get; protected set; }
    public double beat;
    public double timing;

    public BMSObject(int bar, double beat, double beatLength)
    {
        this.bar = bar;
        this.beat = (beat / beatLength) * 4.0d;
    }
    public BMSObject(int bar, double beat)
    {
        this.bar = bar;
        this.beat = beat;
    }
    public void CalculateBeat(double prevBeats, double beatC)
    {
        this.beat = this.beat * beatC + prevBeats;
    }

    public int CompareTo(BMSObject other)
    {
        if (this.beat < other.beat) { return 1; }
        if (this.beat == other.beat) { return 0; }
        return -1;
    }
}

public class BGChange : BMSObject
{
    public string key { get; private set; }
    public bool isPic;
    public BGChange(int bar, string key, double beat, double beatLength, bool isPic) : base(bar, beat, beatLength)
    {
        this.key = key;
        this.isPic = isPic;
    }
}
public class Note : BMSObject
{
    public int keySound;
    public int extra;
    public GameObject model;
    public Transform modelTransform;

    public Note(int bar, int keySound, double beat, double beatLength, int extra) : base(bar, beat, beatLength)
    {
        this.keySound = keySound;
        this.extra = extra;
    }

    public Note(int bar, int keySound, double beat, int extra) : base(bar, beat)
    {
        this.keySound = keySound;
        this.extra = extra;
    }
}


public class BPM : BMSObject
{
    public double bpm;
    public BPM(int bar, double bpm, double beat, double beatLength):base(bar, beat, beatLength)
    {
        this.bpm = bpm;
    }
}

public class Utility
{
    public static double Dabs(double value) => (value > 0) ? value : -value;

    public static int CeilToInt(double value) => (int)(value + 1);
}

public class BMSResult
{
    public int noteCount;
    public ResultData resultData;
    public ScoreGraphData scoreGraphData;
    public double[] judgeList;
}

public class Gauge
{
    public readonly float koolHealAmount;
    public readonly float coolHealAmount;
    public readonly float goodHealAmount;
    public readonly float missDamage;
    public readonly float failDamage;
    public float hp;

    public Gauge()
    {
        hp = 1.0f;
        koolHealAmount = 0.002f;
        coolHealAmount = 0.0014f;
        goodHealAmount = 0.0004f;
        missDamage = 0.02f;
        failDamage = 0.05f;
    }
}

public enum RandomEffector
{
    NONE,
    RANDOM,
    MIRROR,
    FRANDOM,
    MFRANDOM
}