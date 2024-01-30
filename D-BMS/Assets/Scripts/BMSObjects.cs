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
    public int bar;
    public double beat;
    public double timing;

    public BMSObject() { }

    public BMSObject(int bar, double beat, double beatLength)
    {
        this.bar = bar;
        this.beat = (beat / beatLength) * 4.0d;
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
    public int key;
    public bool isPic;
    public BGChange(int bar, int key, double beat, double beatLength, bool isPic) : base(bar, beat, beatLength)
    {
        this.key = key;
        this.isPic = isPic;
    }
}

public class AbstractNote : BMSObject
{
    public int keySound;
    public int extra;

    public AbstractNote() { }

    public AbstractNote(int bar, double beat, double beatLength) : base(bar, beat, beatLength) { }

    public AbstractNote(Note note)
    {
        bar = note.bar;
        beat = note.beat;
        timing = note.timing;
        keySound = note.keySound;
        extra = note.extra;
    }
}

public class Note : AbstractNote
{
    public double failTiming;
    public double tickTiming;
    public GameObject model;
    public Transform modelTransform;

    public Note(double time, int extra)
    {
        timing = time;
        this.extra = extra;
    }

    public Note(int bar, int keySound, double beat, double beatLength, int extra) : base(bar, beat, beatLength)
    {
        this.keySound = keySound;
        this.extra = extra;
    }

    public Note(int bar, int keySound, double beat, int extra)
    {
        this.bar = bar;
        this.beat = beat;
        this.keySound = keySound;
        this.extra = extra;
    }

    public Note(AbstractNote abstractNote)
    {
        bar = abstractNote.bar;
        beat = abstractNote.beat;
        timing = abstractNote.timing;
        keySound = abstractNote.keySound;
        extra = abstractNote.extra;
    }
}

public class ReplayNote : AbstractNote
{
    public double diff;
    public GameObject model;

    public ReplayNote(ReplayNoteData data)
    {
        timing = data.timing;
        diff = data.diff;
        extra = data.extra;
        model = null;
    }
}

public struct ReplayNoteData
{
    public double timing;
    public double diff;
    public int extra;

    public ReplayNoteData(double timing, double diff, int extra)
    {
        this.timing = timing;
        this.diff = diff;
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

public struct KeySoundChange
{
    public double timing;
    public int keySound;

    public KeySoundChange (double timing, int keySound)
    {
        this.timing = timing;
        this.keySound = keySound;
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