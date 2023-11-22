using System.Collections.Generic;
using UnityEngine;

public class BMSPattern
{
    public int noteCount = 0;
    public int barCount { get; set; } = 0;
    public List<BGChange> bgaChanges { get; set; }
    public List<BGChange> layerChanges { get; set; }
    public List<Note> bgSounds { get; set; }
    public List<BPM> bpms { get; set; }
    public Dictionary<int, double> beatCTable { get; set; }
    public Dictionary<int, string> bgVideoTable { get; set; }
    public Line[] lines { get; set; }
    public Line barLine { get; set; }  // 마디선 리스트
    public List<Note>[] longNote { get; set; }
    public List<Note>[] normalNote { get; set; }

    public BMSPattern()
    {
        bgSounds = new List<Note>(5000);
        bpms = new List<BPM>(50);
        beatCTable = new Dictionary<int, double>(100);
        bgVideoTable = new Dictionary<int, string>(5);
        bgaChanges = new List<BGChange>(1000);
        layerChanges = new List<BGChange>(1000);
        lines = new Line[5];
        for (int i = 0; i < 5; i++) { lines[i] = new Line(); }

        barLine = new Line();
        longNote = new List<Note>[5];
        for (int i = 0; i < 5; i++) { longNote[i] = new List<Note>(500); }
        normalNote = new List<Note>[5];
        for (int i = 0; i < 5; i++) { normalNote[i] = new List<Note>(1000); }
    }

    public void AddBGAChange(int bar, double beat, double beatLength, int key, bool isPic = false)
        => bgaChanges.Add(new BGChange(bar, key, beat, beatLength, isPic));

    public void AddLayerChange(int bar, double beat, double beatLength, int key, bool isPic = false)
        => layerChanges.Add(new BGChange(bar, key, beat, beatLength, isPic));

    public void AddNote(int line, int bar, double beat, double beatLength, int keySound, int extra)
    {
        //if (extra != 1) { this.noteCount++; }
        lines[line].noteList.Add(new Note(bar, keySound, beat, beatLength, extra));
    }

    public void AddNote(int line, int bar, double beat, int keySound, int extra)
    {
        //if (extra != 1) { this.noteCount++; }
        lines[line].noteList.Add(new Note(bar, keySound, beat, extra));
    }

    public void AddBar(int bar, double beat, double beatLength, int keySound, int extra)
    {
        barLine.noteList.Add(new Note(bar, keySound, beat, beatLength, extra));
    }

    public void AddBGSound(int bar, double beat, double beatLength, int keySound)
        => bgSounds.Add(new Note(bar, keySound, beat, beatLength, 0));

    public void AddNewBeatC(int bar, double beatC)
        => beatCTable.Add(bar, beatC);

    public void AddBPM(int bar, double beat, double beatLength, double bpm)
        => bpms.Add(new BPM(bar, bpm, beat, beatLength));

    public double GetBeatC(int bar) => beatCTable.ContainsKey(bar) ? beatCTable[bar] : 1.0d;

    public void GetBeatsAndTimings()
    {
        // GET BPM
        int len = bpms.Count;
        for (int i = 0; i < len; i++) 
        { 
            bpms[i].CalculateBeat(GetPreviousBarBeatSum(bpms[i].bar), GetBeatC(bpms[i].bar));
        }
        bpms.Sort();
        if (len == 0 || (len > 0 && bpms[len - 1].beat != 0)) { AddBPM(0, 0, 1, BMSGameManager.header.bpm); }
        len = bpms.Count;
        bpms[len - 1].timing = 0;
        for (int i = len - 2; i > -1; --i)
        {
            bpms[i].timing = bpms[i + 1].timing + (bpms[i].beat - bpms[i + 1].beat) / (bpms[i + 1].bpm / 60);
        }

        len = bgaChanges.Count;
        for (int i = 0; i < len; i++)
        {
            bgaChanges[i].CalculateBeat(GetPreviousBarBeatSum(bgaChanges[i].bar), GetBeatC(bgaChanges[i].bar));
            bgaChanges[i].timing = GetTimingInSecond(bgaChanges[i]);
        }
        bgaChanges.Sort();
        for (int i = bgaChanges.Count - 1; i >= 0; i--)
        {
            if (!bgaChanges[i].isPic)
            {
                bgaChanges[i].timing -= 0.4d;
            }
        }

        for (int i = 0; i < layerChanges.Count; i++)
        {
            layerChanges[i].CalculateBeat(GetPreviousBarBeatSum(layerChanges[i].bar), GetBeatC(layerChanges[i].bar));
            layerChanges[i].timing = GetTimingInSecond(layerChanges[i]);
        }
        layerChanges.Sort();

        // GET BGSOUND
        CalculateTimingsInListExtension(bgSounds);
        for (int i = bgSounds.Count - 1; i >= 0; i--) { bgSounds[i].timing *= 10000000.0d; }

        // GET NOTES
        for (int i = 0; i < 5; i++) { CalculateTimingsInListExtension(lines[i].noteList); }

        AddLongNoteTick();
        SetNoteTiming();
        NoteDivideSave();

        CalculateTimingsInListExtension(barLine.noteList);
        barLine.noteList.RemoveAt(barLine.noteList.Count - 1);
        for (int i = barLine.noteList.Count - 1; i >= 0; i--)
        {
            barLine.noteList[i].timing += 0.175d;
        }
    }

    private void AddLongNoteTick()
    {
        for (int i = 0; i < 5; i++)
        {
            List<Note> tempList = new List<Note>();
            for (int j = 0; j < lines[i].noteList.Count; j++)
            {
                tempList.Add(lines[i].noteList[j]);
                if (lines[i].noteList[j].extra == 1)
                {
                    if (lines[i].noteList[j].timing - 0.07d <= lines[i].noteList[j + 1].timing) { continue; }
                    Note tempNote = new Note(lines[i].noteList[j].timing - 0.055d);
                    tempNote.beat = GetBeatFromTiming(tempNote);
                    tempList.Add(tempNote);

                    tempNote = new Note(0, 0, tempList[tempList.Count - 1].beat - 0.25d, 2);
                    tempNote.timing = GetTimingInSecond(tempNote);
                    while (tempNote.beat > lines[i].noteList[j + 1].beat)
                    {
                        tempList.Add(tempNote);
                        tempNote = new Note(0, 0, tempList[tempList.Count - 1].beat - 0.25d, 2);
                        tempNote.timing = GetTimingInSecond(tempNote);
                    }
                }
            }
            lines[i].noteList = tempList;
        }
    }

    private void SetNoteTiming()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = lines[i].noteList.Count - 1; j >= 0; j--)
            {
                lines[i].noteList[j].timing *= 10000000.0d;
                if (lines[i].noteList[j].extra == 0)
                {
                    lines[i].noteList[j].failTiming = lines[i].noteList[j].timing + 1750000.0d;
                    lines[i].noteList[j].tickTiming = 20000000000.0d;
                }
                else
                {
                    lines[i].noteList[j].failTiming = 20000000000.0d;
                    lines[i].noteList[j].tickTiming = lines[i].noteList[j].timing;
                    lines[i].noteList[j].timing = 20000000000.0d;
                }
            }
        }
    }

    private void NoteDivideSave()
    {
        for (int i = 0; i < 5; i++)
        {
            int len = lines[i].noteList.Count;
            for (int j = 0; j < len; j++)
            {
                if (lines[i].noteList[j].extra == 1)
                {
                    int k = 0;
                    for (k = j; k < len; k++) { if (lines[i].noteList[k].extra == 0) { break; } }
                    longNote[i].Add(lines[i].noteList[k]);
                    longNote[i].Add(new Note(lines[i].noteList[k].bar, 0, lines[i].noteList[j].beat - lines[i].noteList[k].beat, 2));
                    longNote[i].Add(lines[i].noteList[j]);
                    j = k;
                }
                else
                {
                    normalNote[i].Add(lines[i].noteList[j]);
                }
            }

            for (int j = 0; j < lines[i].noteList.Count; j++)
            {
                if (lines[i].noteList[j].extra == 1)
                {
                    lines[i].noteList.RemoveAt(j--);
                }
            }
            noteCount += lines[i].noteList.Count;
        }
    }

    public void CalculateTimingsInListExtension(List<Note> list)
    {
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            list[i].CalculateBeat(GetPreviousBarBeatSum(list[i].bar), GetBeatC(list[i].bar));
            list[i].timing = GetTimingInSecond(list[i]);
        }
        list.Sort();
    }

    private double GetBPM(double beat)
    {
        if (bpms.Count == 1) { return bpms[0].bpm; }
        int idx = bpms.Count - 1;
        while (idx > 0 && beat >= bpms[--idx].beat) ;
        return bpms[idx + 1].bpm;
    }

    private double GetTimingInSecond(BMSObject obj)
    {
        double timing = 0;
        int i;
        for (i = bpms.Count - 1; i > 0 && obj.beat > bpms[i - 1].beat; --i)
        {
            timing += (bpms[i - 1].beat - bpms[i].beat) / bpms[i].bpm * 60;
        }
        timing += (obj.beat - bpms[i].beat) / bpms[i].bpm * 60;
        return timing;
    }

    private double GetBeatFromTiming(BMSObject obj)  // 리플레이용
    {
        double beat = 0;
        int i;
        for (i = bpms.Count - 1; i > 0 && obj.timing > bpms[i - 1].timing; i--)
        {
            beat += (bpms[i - 1].timing - bpms[i].timing) * bpms[i].bpm / 60;
        }
        beat += (obj.timing - bpms[i].timing) * bpms[i].bpm / 60;
        return beat;
    }

    public double GetPreviousBarBeatSum(int bar)
    {
        double sum = 0;
        for (int i = 0; i < bar; i++) { sum += 4.0d * GetBeatC(i); }
        return sum;
    }
}