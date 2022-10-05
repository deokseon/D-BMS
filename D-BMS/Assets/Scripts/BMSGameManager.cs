﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BMSGameManager : MonoBehaviour
{
    public static BMSResult bmsResult;
    public static float userSpeed = 7.2f;
    public static float gameSpeed = 0.65f;
    public static RandomEffector randomEffector = RandomEffector.NONE;
    public static int judgeAdjValue = 0;
    public static bool isClear;
    public static bool isPaused;
    public static Texture stageTexture;

    [SerializeField]
    private BMSDrawer bmsDrawer;
    [SerializeField]
    private GameUIManager gameUIManager;
    [SerializeField]
    private SoundManager soundManager;
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField]
    private KeyInput keyInput;
    [SerializeField]
    private Transform noteParent;

    private double currentBeat = 0;
    private double currentScrollTime = 0;
    private int currentTimeSample = 0;
    private double currentTime = 0;
    private int accuracySum = 0;
    private int currentCount = 0;
    private int totalLoading = 0;
    private float divideTotalLoading;
    public static int currentLoading = 0;
    private Coroutine coSongEndCheck;

    private const double divide60 = 1.0d / 60.0d;
    private const double divide44100 = 1.0d / 44100.0d;
    private float divideBPM;
    private double[] divideTable;
    private double[] maxScoreTable;

    public static BMSHeader header;
    private Gauge gauge;
    private JudgeManager judge;
    private BMSPattern pattern;
    private WaitForSeconds wait3Sec;
    private WaitForSeconds wait1Sec;
    private double currentBPM;
    private int combo = 0;
    private bool isBGAVideoSupported = false;

    public int[] currentNote;

    private AudioSource timeSampleAudio;

    private double koolAddScore;
    private double coolAddScore;
    private double goodAddScore;
    private double currentScore = 0.0d;

    [SerializeField]
    private GameObject[] longNotePress;

    private JudgeType currentLongNoteJudge;
    private bool[] currentButtonPressed = { false, false, false, false, false };

    private int notePoolMaxCount;
    private int longNotePoolMaxCount;
    private int barPoolMaxCount;

    private readonly float[] xPosition = { -7.7f, -7.13f, -6.56f, -5.99f, -5.42f };
    private int bgaChangeListCount;
    private int bgSoundsListCount;
    private int bpmsListCount;
    private int[] notesListCount;
    private int[] longNoteListCount;
    private int[] normalNoteListCount;
    private int barListCount;
    private List<BGChange> bgaChangeList;
    private List<Note> bgSoundsList;
    private List<BPM> bpmsList;
    private List<Note>[] notesList;
    private List<Note>[] longNoteList;
    private List<Note>[] normalNoteList;
    private List<Note> barList;

    public void CalulateSpeed()
    {
        gameSpeed = (userSpeed * 131.3f * divideBPM);
    }

    private IEnumerator PreLoad(bool isRestart)
    {
        ObjectPool.poolInstance.Init();

        BMSParser.instance.Parse(isRestart);
        pattern = BMSParser.instance.pattern;

        pattern.GetBeatsAndTimings();

        bgaChangeList = pattern.bgaChanges;
        bgSoundsList = pattern.bgSounds;
        bpmsList = pattern.bpms;
        for (int i = 0; i < 5; i++) 
        { 
            notesList[i] = pattern.lines[i].noteList;
            longNoteList[i] = pattern.longNote[i];
            normalNoteList[i] = pattern.normalNote[i];

            notesListCount[i] = notesList[i].Count;
            longNoteListCount[i] = longNoteList[i].Count;
            normalNoteListCount[i] = normalNoteList[i].Count;
        }
        barList = pattern.barLine.noteList;

        bgaChangeListCount = bgaChangeList.Count;
        bgSoundsListCount = bgSoundsList.Count;
        bpmsListCount = bpmsList.Count;
        barListCount = barList.Count;
        
        for (int i = 0; i < 5; i++) { if (notesListCount[i] > 0) { currentNote[i] = notesList[i][notesListCount[i] - 1].keySound; } }

        divideBPM = (float)(1.0f / header.bpm);
        CalulateSpeed();
        bmsDrawer.DrawNotes();
        
        currentBPM = bpmsList[bpmsListCount - 1].bpm;
        --bpmsListCount;

        bmsResult.noteCount = pattern.noteCount;
        bmsResult.judgeList = new double[bmsResult.noteCount + 1];
        for (int i = bmsResult.judgeList.Length - 1; i >= 1; i--) { bmsResult.judgeList[i] = 200.0d; }

        currentTime += (judgeAdjValue * 0.001f);

        coSongEndCheck = StartCoroutine(SongEndCheck());
        StartCoroutine(TimerStart());

        if (!isRestart)
        {
            keyInput.KeySetting();

            koolAddScore = 1100000.0d / pattern.noteCount * 1000.0d;
            coolAddScore = koolAddScore * 0.7d;
            goodAddScore = koolAddScore * 0.2d;

            MakeTable();

            gauge = new Gauge();

            Time.fixedDeltaTime = 0.001f;

            gameUIManager.SetLoading();
            stageTexture = null;
            totalLoading = gameUIManager.bgImageTable.Count + soundManager.pathes.Count;
            for (int i = bgaChangeListCount - 1; i > -1; i--) { if (!bgaChangeList[i].isPic) { totalLoading++; break; } }
            divideTotalLoading = 1.0f / totalLoading;
            StartCoroutine(Loading());
            gameUIManager.LoadImages();
            soundManager.AddAudioClips();
            if (videoPlayer.isActiveAndEnabled)
            {
                for (int i = bgaChangeListCount - 1; i > -1; i--)
                {
                    if (!bgaChangeList[i].isPic)
                    {
                        videoPlayer.url = "file://" + header.musicFolderPath + pattern.bgVideoTable[pattern.bgaChanges[i].key];
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(videoPlayer.url))
                {
                    bool errorFlag = false;
                    videoPlayer.errorReceived += (a, b) => errorFlag = true;
                    videoPlayer.Prepare();
                    yield return new WaitUntil(() => (videoPlayer.isPrepared || errorFlag));
                    currentLoading++;
                    isBGAVideoSupported = !errorFlag;
                }
            }
            yield return new WaitUntil(() => gameUIManager.isPrepared);
            yield return new WaitUntil(() => soundManager.isPrepared);
            yield return wait3Sec;
        }

        gameUIManager.bga.texture = videoPlayer.texture;
        gameUIManager.bga.color = Color.white;

        gameUIManager.UpdateBPMText(currentBPM);
        gameUIManager.TextUpdate(bmsResult, 0, JudgeType.IGNORE, 0);
        gameUIManager.UpdateScore(gauge.hp, 100.0f, 0.0d, 0.0d);

        System.GC.Collect();

        if (isRestart)
        {
            StartCoroutine(gameUIManager.FadeOut());
            yield return wait1Sec;
        }

        isPaused = false;
    }

    public IEnumerator GameRestart()
    {
        if (isPaused) { yield break; }

        isPaused = true;
        gameUIManager.FadeIn();
        soundManager.AudioAllStop();
        StopCoroutine(coSongEndCheck);
        timeSampleAudio.Stop();
        timeSampleAudio.timeSamples = 0;
        if (videoPlayer.isPlaying) { videoPlayer.Pause(); videoPlayer.time = 0.0d; }
        yield return wait1Sec;

        ReturnAllNotes();
        gameUIManager.bga.texture = null;
        bmsResult = null;
        pattern = null;
        bgaChangeList = null;
        bgSoundsList = null;
        bpmsList = null;
        for (int i = 0; i < 5; i++)
        {
            notesList[i] = null;
            longNoteList[i] = null;
            normalNoteList[i] = null;
        }
        notesList = null;
        longNoteList = null;
        normalNoteList = null;
        barList = null;

        isClear = true;
        currentBeat = 0;
        currentScrollTime = 0;
        currentTimeSample = 0;
        currentTime = 0;
        accuracySum = 0;
        currentCount = 0;
        combo = 0;
        currentScore = 0.0d;
        gauge.hp = 1.0f;
        gameUIManager.maxCombo = 0;
        for (int i = 0; i < 5; i++) { currentButtonPressed[i] = false; }

        bmsResult = new BMSResult();
        notesList = new List<Note>[5];
        longNoteList = new List<Note>[5];
        normalNoteList = new List<Note>[5];

        StartCoroutine(PreLoad(true));
    }

    private void ReturnAllNotes()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = normalNoteListCount[i] - 1; j >= 0; j--)
            {
                if (normalNoteList[i][j].model == null) { break; }
                normalNoteList[i][j].model.SetActive(false);
                ObjectPool.poolInstance.ReturnNoteInPool(i, normalNoteList[i][j].model);
            }
            for (int j = longNoteListCount[i] - 1; j >= 0; j -= 3)
            {
                if (longNoteList[i][j].model == null) { break; }
                for (int k = 0; k < 3; k++)
                {
                    longNoteList[i][j - k].model.SetActive(false);
                    ObjectPool.poolInstance.ReturnLongNoteInPool(i, (k == 2 ? 0 : k + 1), longNoteList[i][j - k].model);
                }
            }
        }
        for (int i = barListCount - 1; i >= 0; i--)
        {
            if (barList[i].model == null) { break; }
            barList[i].model.SetActive(false);
            ObjectPool.poolInstance.ReturnBarInPool(barList[i].model);
        }
    }

    private IEnumerator Loading()
    {
        while (currentLoading < totalLoading)
        {
            gameUIManager.SetLoadingSlider(currentLoading * divideTotalLoading);
            yield return null;
        }
        gameUIManager.SetLoadingSlider(1.0f);
        yield return new WaitForSeconds(0.5f);
        gameUIManager.CloseLoading();
    }

    private void Awake()
    {
        isPaused = true;
        isClear = true;
        judge = JudgeManager.instance;
        bmsResult = new BMSResult();

        header = BMSFileSystem.selectedHeader;
        BMSFileSystem.selectedHeader = null;

        gameUIManager.UpdateInfoText();
        gameUIManager.UpdateSpeedText();

        currentNote = new int[5] { 0, 0, 0, 0, 0 };
        notesListCount = new int[5];
        longNoteListCount = new int[5];
        normalNoteListCount = new int[5];
        notesList = new List<Note>[5];
        longNoteList = new List<Note>[5];
        normalNoteList = new List<Note>[5];

        currentLoading = 0;

        notePoolMaxCount = ObjectPool.poolInstance.maxNoteCount;
        longNotePoolMaxCount = ObjectPool.poolInstance.maxLongNoteCount;
        barPoolMaxCount = ObjectPool.poolInstance.maxBarCount;

        timeSampleAudio = GetComponent<AudioSource>();

        wait3Sec = new WaitForSeconds(3.0f);
        wait1Sec = new WaitForSeconds(1.5f);

        gameUIManager.bga.color = new Color(1, 1, 1, 0);

        StartCoroutine(PreLoad(false));
    }

    private void Update()
    {
        if (isPaused) { return; }

        while (bgaChangeListCount > 0 && bgaChangeList[bgaChangeListCount - 1].timing - ((!bgaChangeList[bgaChangeListCount - 1].isPic) ? 0.4 : 0) <= currentTime)
        {
            if (isBGAVideoSupported && !bgaChangeList[bgaChangeListCount - 1].isPic) { videoPlayer.Play(); }
            else { gameUIManager.ChangeBGA(bgaChangeList[bgaChangeListCount - 1].key); }
            --bgaChangeListCount;
        }

        PlayNotes();

        int tempTimeSample = timeSampleAudio.timeSamples;
        double frameTime = (tempTimeSample - currentTimeSample) * divide44100;
        currentTimeSample = tempTimeSample;
        currentTime = currentTimeSample * divide44100;

        double avg = currentBPM * frameTime;

        BPM next = null;
        bool flag = false;

        if (bpmsListCount > 0)
        {
            BPM bpm = bpmsList[bpmsListCount - 1];
            if (next == null) { next = bpm; }
            else if (bpm.beat <= next.beat) { next = bpm; }

            if (next.timing < currentScrollTime + frameTime)
            {
                flag = true;
                avg = 0;
            }
        }

        double prevTime = currentScrollTime;
        while (next != null && next.timing < currentScrollTime + frameTime)
        {
            double diff = next.timing - prevTime;
            avg += currentBPM * diff;
            currentBPM = next.bpm;
            gameUIManager.UpdateBPMText(currentBPM);
            prevTime = next.timing;
            --bpmsListCount;

            next = null;
            if (bpmsListCount > 0) { next = bpmsList[bpmsListCount - 1]; }
        }

        if (flag && prevTime <= currentScrollTime + frameTime)
        {
            avg += currentBPM * (currentScrollTime + frameTime - prevTime);
        }

        avg *= divide60;
        currentBeat += avg;
        currentScrollTime += frameTime;
        noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed), 0.0f);
    }

    private void HandleNote(List<Note> noteList, int idx)
    {
        Note n = noteList[notesListCount[idx] - 1];
        double diff = (currentTime - n.timing) * 1000.0d;
        JudgeType result = judge.Judge(diff);
        if (result == JudgeType.IGNORE) { return; }

        currentCount++;
        --notesListCount[idx];

        if (notesListCount[idx] > 0) { currentNote[idx] = noteList[notesListCount[idx] - 1].keySound; }

        bool nextCheck = true;
        if (normalNoteListCount[idx] > 0 && n.beat == normalNoteList[idx][normalNoteListCount[idx] - 1].beat)
        {
            --normalNoteListCount[idx];
            int tempPeek = normalNoteListCount[idx] - notePoolMaxCount;
            if (tempPeek >= 0)
            {
                n.modelTransform.localPosition = new Vector3(xPosition[idx], (float)(normalNoteList[idx][tempPeek].beat * gameSpeed), 0.0f);
                normalNoteList[idx][tempPeek].model = n.model;
                normalNoteList[idx][tempPeek].modelTransform = n.modelTransform;
            }
            else
            {
                n.model.SetActive(false);
                ObjectPool.poolInstance.ReturnNoteInPool(idx, n.model);
            }
            nextCheck = false;
        }

        if (nextCheck)
        { 
            currentLongNoteJudge = result;
            if (currentLongNoteJudge == JudgeType.COOL) { currentLongNoteJudge = JudgeType.KOOL; }
            longNotePress[idx].SetActive(true);
        }

        if (result <= JudgeType.MISS) { combo = -1; }
        else { bmsResult.judgeList[currentCount] = diff; }
        gameUIManager.TextUpdate(bmsResult, ++combo, result, idx);

        if (result != JudgeType.FAIL) { gameUIManager.UpdateFSText((float)diff, idx); }

        UpdateResult(result);
    }

    private void HandleLongNoteTick(List<Note> noteList, int idx)
    {
        currentCount++;
        --notesListCount[idx];

        if (notesListCount[idx] > 0) { currentNote[idx] = noteList[notesListCount[idx] - 1].keySound; }

        if (!currentButtonPressed[idx]) { currentLongNoteJudge = JudgeType.FAIL; }
        else { if (currentLongNoteJudge == JudgeType.FAIL) { currentLongNoteJudge = JudgeType.GOOD; } }

        JudgeType result = currentLongNoteJudge;

        if (result <= JudgeType.MISS) { combo = -1; }
        gameUIManager.TextUpdate(bmsResult, ++combo, result, idx);

        UpdateResult(result);
    }

    private void PlayNotes()
    {
        while (bgSoundsListCount > 0 && bgSoundsList[bgSoundsListCount - 1].timing <= currentTime)  // 배경음 재생
        {
            soundManager.PlayBGSound(bgSoundsList[bgSoundsListCount - 1].keySound);
            --bgSoundsListCount;
        }

        for (int i = 0; i < 5; i++)
        {
            while (longNoteListCount[i] > 0 && longNoteList[i][longNoteListCount[i] - 1].timing <= currentTime)
            {
                longNotePress[i].SetActive(false);
                for (int j = 0; j < 3; j++)
                {
                    Note tempNote = longNoteList[i][longNoteListCount[i] - 1];
                    --longNoteListCount[i];

                    int len = longNoteListCount[i] - (3 * longNotePoolMaxCount);
                    if (len >= 0)
                    {
                        tempNote.modelTransform.localPosition = new Vector3(xPosition[i], (float)(longNoteList[i][len].beat * gameSpeed), 0.0f);
                        longNoteList[i][len].model = tempNote.model;
                        longNoteList[i][len].modelTransform = tempNote.modelTransform;
                        if (j == 1)
                        {
                            longNoteList[i][len].modelTransform.localScale =
                                new Vector3(0.3f, ((float)(longNoteList[i][len + 1].beat - longNoteList[i][len - 1].beat) * gameSpeed - 0.3f) * 1.219512f, 1.0f);
                        }
                    }
                    else
                    {
                        tempNote.model.SetActive(false);
                        ObjectPool.poolInstance.ReturnLongNoteInPool(i, (j == 2 ? 0 : j + 1), tempNote.model);
                    }
                }
            }
        }

        for (int i = 0; i < 5; i++)  // 롱노트 틱 검사
        {
            while (notesListCount[i] > 0 && notesList[i][notesListCount[i] - 1].extra == 2 && notesList[i][notesListCount[i] - 1].timing <= currentTime)
            {
                HandleLongNoteTick(notesList[i], i);
            }
        }

        for (int i = 0; i < 5; i++)  // 놓친 노트 검사
        {
            while (notesListCount[i] > 0 && judge.Judge(notesList[i][notesListCount[i] - 1], currentTime) == JudgeType.FAIL)
            {
                HandleNote(notesList[i], i);
            }
        }

        while (barListCount > 0 && barList[barListCount - 1].timing + 0.175f <= currentTime)
        {
            Note bar = barList[barListCount - 1];
            --barListCount;
            int len = barListCount - barPoolMaxCount;
            if (len >= 0)
            {
                bar.modelTransform.localPosition = new Vector3(-6.56f, (float)(barList[len].beat * gameSpeed) - 0.285f, 0.0f);
                barList[len].model = bar.model;
                barList[len].modelTransform = bar.modelTransform;
            }
            else
            {
                bar.model.SetActive(false);
                ObjectPool.poolInstance.ReturnBarInPool(bar.model);
            }
        }

        // auto
        for (int i = 0; i < 5; i++)
        {
            currentButtonPressed[i] = true;
            while (notesListCount[i] > 0 && notesList[i][notesListCount[i] - 1].extra != 2 && notesList[i][notesListCount[i] - 1].timing <= currentTime)
            {
                soundManager.PlayKeySound(notesList[i][notesListCount[i] - 1].keySound);
                HandleNote(notesList[i], i);
            }
        }
    }

    public void KeyDown(int index)
    {
        currentButtonPressed[index] = true;
        if (notesListCount[index] <= 0 || notesList[index][notesListCount[index] - 1].extra == 2) { return; }
        HandleNote(notesList[index], index);
    }

    public void KeyUp(int index)
    {
        currentButtonPressed[index] = false;
    }

    public void UpdateResult(JudgeType judge)
    {
        switch (judge)
        {
            case JudgeType.KOOL:
                bmsResult.koolCount++; 
                accuracySum += 100;
                currentScore += koolAddScore;
                gauge.hp += gauge.koolHealAmount;
                break;
            case JudgeType.COOL:
                bmsResult.coolCount++; 
                accuracySum += 70;
                currentScore += coolAddScore;
                gauge.hp += gauge.coolHealAmount;
                break;
            case JudgeType.GOOD:
                bmsResult.goodCount++; 
                accuracySum += 20;
                currentScore += goodAddScore;
                gauge.hp += gauge.goodHealAmount;
                break;
            case JudgeType.MISS:
                bmsResult.missCount++;
                gauge.hp -= gauge.missDamage;
                break;
            case JudgeType.FAIL:
                bmsResult.failCount++;
                gauge.hp -= gauge.failDamage;
                break;
        }

        if (gauge.hp > 1.0f) { gauge.hp = 1.0f; }
        else if (gauge.hp <= 0.0f) { StartCoroutine(GameEnd(false)); }

        gameUIManager.UpdateScore(gauge.hp, (float)(accuracySum * divideTable[currentCount]), currentScore * 0.001d, maxScoreTable[currentCount]);

        if (currentCount >= pattern.noteCount)
            gameUIManager.UpdateSongEndText(bmsResult.koolCount, bmsResult.coolCount, bmsResult.goodCount);
    }

    private IEnumerator TimerStart()
    {
        while (isPaused) { yield return null; }

        timeSampleAudio.Play();
    }

    public IEnumerator GameEnd(bool clear)
    {
        if (isPaused) { yield break; }
        isPaused = !clear;
        isClear = clear;

        soundManager.AudioAllStop();

        noteParent.gameObject.SetActive(clear);

        Time.fixedDeltaTime = 0.02f;

        keyInput.KeyDisable();

        bmsResult.accuracy = accuracySum * divideTable[currentCount] * 0.01f;
        bmsResult.score = currentScore * 0.001f;
        bmsResult.maxCombo = gameUIManager.maxCombo;

        if (isClear) { yield return wait3Sec; }
        else { yield return new WaitForSeconds(1.0f); }
        gameUIManager.FadeIn();
        yield return new WaitForSeconds(1.0f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    private IEnumerator SongEndCheck()
    {
        while (true)
        {
            if (currentCount >= pattern.noteCount && 
                ((!isBGAVideoSupported && bgaChangeListCount == 0) || 
                (isBGAVideoSupported && !videoPlayer.isPlaying)))
            {
                StartCoroutine(GameEnd(true));
                yield break;
            }
            yield return wait3Sec;
        }
    }

    private void MakeTable()
    {
        int len = pattern.noteCount + 1;
        divideTable = new double[len];
        for (int i = 1; i < len; i++) { divideTable[i] = 1.0d / i; }

        double divide20000 = 1.0d / 20000.0d;
        double divide6250 = 1.0d / 6250.0d;
        maxScoreTable = new double[len];
        for (int i = 1; i < len; i++) 
        { 
            maxScoreTable[i] = koolAddScore * i * 0.001d;
            double under60 = (maxScoreTable[i] > 600000.0d ? 600000.0d : maxScoreTable[i]) * divide20000;
            double up60 = (maxScoreTable[i] > 600000.0d ? maxScoreTable[i] - 600000.0d : 0.0d) * divide6250;
            maxScoreTable[i] = under60 + up60;
        }

        gameUIManager.MakeStringTable();
    }

    public void ChangeSpeed(float value)
    {
        if (isPaused) { return; }

        userSpeed += value;
        if (userSpeed > 20.0f) { userSpeed = 20.0f; }
        else if (userSpeed < 1.0f) { userSpeed = 1.0f; }
        gameUIManager.UpdateSpeedText();
        CalulateSpeed();

        for (int i = 0; i < 5; i++)
        {
            for (int j = normalNoteListCount[i] - 1; j >= 0; j--)
            {
                if (normalNoteList[i][j].model == null) { break; }

                normalNoteList[i][j].modelTransform.localPosition = new Vector3(xPosition[i], (float)(normalNoteList[i][j].beat * gameSpeed), 0.0f);
            }

            for (int j = longNoteListCount[i] - 1; j >= 0; j -= 3)
            {
                if (longNoteList[i][j].model == null) { break; }

                for (int k = 0; k < 3; k++)
                {
                    longNoteList[i][j - k].modelTransform.localPosition = new Vector3(xPosition[i], (float)(longNoteList[i][j - k].beat * gameSpeed), 0.0f);

                    if (k == 1)
                    {
                        longNoteList[i][j - k].modelTransform.localScale =
                                new Vector3(0.3f, ((float)(longNoteList[i][j].beat - longNoteList[i][j - 2].beat) * gameSpeed - 0.3f) * 1.219512f, 1.0f);
                    }
                }
            }
        }
        for (int i = barListCount - 1; i >= 0; i--)
        {
            if (barList[i].model == null) { break; }

            barList[i].modelTransform.localPosition = new Vector3(-6.56f, (float)(barList[i].beat * gameSpeed) - 0.285f, 0.0f);
        }
        noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed), 0.0f);
    }
}
