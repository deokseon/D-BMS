using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Video;
using System;
using System.IO;

public class BMSGameManager : MonoBehaviour
{
    public static BMSResult bmsResult;
    public static bool isClear;
    private bool isPaused;
    private bool isStarted;
    private bool isCountdown;

    [HideInInspector] public float longNoteOffset;
    [HideInInspector] public float longNoteLength;
    private float verticalLine;
    private int userSpeed;
    private float gameSpeed;
    private int displayDelayCorrectionValue;
    private int earlyLateThreshold;
    private float judgeLineYPosition;

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
    [SerializeField]
    private PauseManager pauseManager;
    [SerializeField]
    private Transform inputBlockLine;

    private System.Diagnostics.Stopwatch stopwatch;
    private double currentBeat = 0;
    private double currentScrollTime = 0;
    private long currentTicks = 0;
    private long resumeTicks = 0;
    private long resumeCountTicks = 0;
    private double currentTime = 0;
    public int accuracySum = 0;
    public int currentCount = 0;
    private int totalLoading = 0;
    private bool isFadeEnd = false;
    private float divideTotalLoading;
    private double displayDelayCorrectionBeat;
    private float longNoteEndOffset;
    public static int currentLoading = 0;
    private int currentRankIndex;
    public int endCount;

    private const double divide20000 = 1.0d / 20000.0d;
    private const double height60 = 600000.0d * divide20000;
    private const double divide6250 = 1.0d / 6250.0d;
    private const double divide60 = 1.0d / 60.0d;
    private float divideBPM;
    public double[] divideTable;
    public float[] maxScoreTable;

    public static BMSHeader header;
    public Gauge gauge;
    private JudgeManager judge;
    private BMSPattern pattern;
    private WaitForSeconds wait3Sec;
    private WaitForSeconds wait1Sec;
    private double currentBPM;
    private int combo = 0;
    private bool isBGAVideoSupported = false;
    private Coroutine songEndCheckCoroutine;

    private int[] currentNote;

    private double koolAddScore;
    private double coolAddScore;
    private double goodAddScore;
    public double currentScore = 0.0d;

    private JudgeType[] currentLongNoteJudge = { JudgeType.KOOL, JudgeType.KOOL, JudgeType.KOOL, JudgeType.KOOL, JudgeType.KOOL };
    private bool[] isCurrentLongNote = { false, false, false, false, false };
    private int[] longNoteHandleCount = { 0, 0, 0, 0, 0 };
    private int[] normalNoteHandleCount = { 0, 0, 0, 0, 0 };

    private int notePoolMaxCount;
    private int longNotePoolMaxCount;
    private int barPoolMaxCount;

    [HideInInspector] public float[] xPosition;
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

    private Thread bgmThread;
    private TimeSpan threadFrequency;
    [HideInInspector] public object inputHandleLock = new object();
    private bool[] isKeyDown;
    private bool[] isNoteBombActive;
    private int[] noteBombState;
    [HideInInspector] public bool isChangeRankImage = false;
    [HideInInspector] public bool isJudgementTrackerUpdate = false;
    [HideInInspector] public bool isScoreGraphUpdate = false;
    [HideInInspector] public bool[] fsUpdate;
    [HideInInspector] public int[] fsStates;
    [HideInInspector] public int[] fsCount;
    private bool isGameUIUpdate = false;
    private bool isGameEnd = false;
    [HideInInspector] public int isEndJudgeInfoUpdate = 0;
    private bool isGameOver = false;
    private JudgeType currentJudge;

    public float CalulateSpeed()
    {
        //return (userSpeed * 13.35f * divideBPM);
        return (userSpeed * 13.0f * divideBPM);
    }

    private IEnumerator PreLoad(bool isRestart)
    {
        stopwatch.Reset();
        soundManager.AudioPause(false);
        bgmThread = new Thread(BGMPlayThread);

        gameUIManager.bga.color = new Color(1, 1, 1, 0);
        gameUIManager.bga.texture = null;

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

            notesListCount[i] = notesList[i].Count - 1;
            longNoteListCount[i] = longNoteList[i].Count;
            normalNoteListCount[i] = normalNoteList[i].Count;
        }
        barList = pattern.barLine.noteList;

        bgaChangeListCount = bgaChangeList.Count - 1;
        bgSoundsListCount = bgSoundsList.Count - 1;
        bpmsListCount = bpmsList.Count;
        barListCount = barList.Count - 1;

        divideBPM = (float)(1.0f / header.bpm);
        gameSpeed = CalulateSpeed();

        displayDelayCorrectionBeat = 0.001d * header.bpm * divide60;
        longNoteEndOffset = Mathf.Max((float)(displayDelayCorrectionBeat * -displayDelayCorrectionValue), 0.0f);
        currentBeat += (displayDelayCorrectionBeat * displayDelayCorrectionValue);
        noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed) + judgeLineYPosition, 0.0f);

        currentBPM = bpmsList[bpmsListCount - 1].bpm;
        --bpmsListCount;

        bmsResult.noteCount = pattern.noteCount;
        bmsResult.judgeList = new double[bmsResult.noteCount + 1];
        bmsResult.scoreGraphArray = new float[bmsResult.noteCount + 2]; bmsResult.scoreGraphArray[0] = 0.0f;
        for (int i = bmsResult.judgeList.Length - 1; i >= 1; i--) { bmsResult.judgeList[i] = 2000000.0d; }

        if (!isRestart)
        {
            koolAddScore = 1100000.0d / pattern.noteCount;
            coolAddScore = koolAddScore * 0.7d;
            goodAddScore = koolAddScore * 0.2d;

            MakeTable();

            gauge = new Gauge();

            GameUIManager.isCreateReady = false;
            gameUIManager.SetLoading();
            totalLoading = gameUIManager.bgImageTable.Count + soundManager.pathes.Count + Directory.GetFiles($@"{Directory.GetParent(Application.dataPath)}\Skin\GameObject").Length;
            for (int i = bgaChangeListCount; i > -1; i--) { if (!bgaChangeList[i].isPic) { totalLoading++; break; } }
            divideTotalLoading = 1.0f / totalLoading;
            StartCoroutine(Loading());
            gameUIManager.LoadImages();
            soundManager.AddAudioClips();
            if (videoPlayer.isActiveAndEnabled)
            {
                for (int i = bgaChangeListCount; i > -1; i--)
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
            yield return new WaitUntil(() => gameUIManager.isPrepared == 2);
            keyInput.KeySetting();
            yield return new WaitUntil(() => soundManager.isPrepared == soundManager.threadCount);
            yield return new WaitUntil(() => isFadeEnd);
            yield return wait3Sec;
        }
        else
        {
            bmsDrawer.DrawNotes(xPosition);
        }

        inputBlockLine.localPosition = new Vector3(xPosition[2], -200.0f, 0.0f);

        for (int i = 0; i < 5; i++) { if (notesListCount[i] >= 0) { currentNote[i] = notesList[i][notesListCount[i]].keySound; } }

        gameUIManager.bga.texture = videoPlayer.texture;
        if (bgaChangeListCount == -1 || (!string.IsNullOrEmpty(videoPlayer.url) && !isBGAVideoSupported)) 
        {
            gameUIManager.bga.color = Color.black;
            bgaChangeList.Add(new BGChange(0, "00", 0, 0, true));
            bgaChangeListCount = 0;
            bgaChangeList[0].timing = 2000000000.0d;
        }
        else { gameUIManager.bga.color = Color.white; }

        for (int i = 0; i < 5; i++)
        {
            gameUIManager.KeyInputImageSetActive(false, i);
        }
        gameUIManager.GameUIUpdate(0, JudgeType.IGNORE);
        isJudgementTrackerUpdate = true;
        isScoreGraphUpdate = true;
        isChangeRankImage = true;

        GC.Collect();

        if (isRestart)
        {
            StartCoroutine(gameUIManager.FadeOut());
            yield return wait1Sec;
        }

        isStarted = true;
        isPaused = false;
        if (bgSoundsListCount >= 0)
        {
            bgmThread.Start();
        }
        keyInput.InputThreadStart();
        stopwatch.Start();
    }

    public IEnumerator GameRestart()
    {
        if (isCountdown || isClear || !isStarted) { yield break; }

        if (songEndCheckCoroutine != null)
        {
            StopCoroutine(songEndCheckCoroutine);
        }
        pauseManager.Pause_SetActive(false);
        bgmThread.Abort();
        keyInput.InputThreadAbort();
        stopwatch.Stop();
        videoPlayer.Pause();
        isPaused = true;
        isStarted = false;
        gameUIManager.AnimationPause(false);
        gameUIManager.FadeIn();
        soundManager.AudioAllStop();
        yield return wait1Sec;

        if (isBGAVideoSupported)
        {
            videoPlayer.Stop();
            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);
        }
        ReturnAllNotes();
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

        isClear = false;
        currentBeat = 0;
        currentScrollTime = 0;
        currentTicks = 0;
        resumeTicks = 0;
        resumeCountTicks = 0;
        currentTime = 0;
        accuracySum = 0;
        currentCount = 0;
        combo = 0;
        currentScore = 0.0d;
        gauge.hp = 1.0f;
        currentRankIndex = 0;
        endCount = 0;

        bmsResult.koolCount = 0;
        bmsResult.coolCount = 0;
        bmsResult.goodCount = 0;
        bmsResult.missCount = 0;
        bmsResult.failCount = 0;
        bmsResult.maxCombo = 0;
        bmsResult.rankIndex = 0;
        bmsResult.score = 0.0d;
        bmsResult.accuracy = 0.0d;
        notesList = new List<Note>[5];
        longNoteList = new List<Note>[5];
        normalNoteList = new List<Note>[5];

        for (int i = 0; i < 5; i++)
        {
            isKeyDown[i] = false;
            isNoteBombActive[i] = false;
            noteBombState[i] = 0;
            isCurrentLongNote[i] = false;
            longNoteHandleCount[i] = 0;
            normalNoteHandleCount[i] = 0;
        }
        isEndJudgeInfoUpdate = 1;
        isGameUIUpdate = false;
        isJudgementTrackerUpdate = false;
        isScoreGraphUpdate = false;
        isChangeRankImage = false;
        isGameEnd = false;
        isGameOver = false;
        for (int i = 0; i < 2; i++)
        {
            fsUpdate[i] = false;
            fsCount[i] = 0;
        }

        StartCoroutine(PreLoad(true));
    }

    private void ReturnAllNotes()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = longNoteList[i].Count - 1; j >= 0; j -= 3)
            {
                if (longNoteList[i][j].model == null || !longNoteList[i][j].model.activeSelf) { continue; }
                for (int k = 0; k < 3; k++)
                {
                    longNoteList[i][j - k].model.SetActive(false);
                    ObjectPool.poolInstance.ReturnLongNoteInPool(i, (k == 2 ? 0 : k + 1), longNoteList[i][j - k].model);
                }
            }
            for (int j = normalNoteList[i].Count - 1; j >= 0; j--)
            {
                if (normalNoteList[i][j].model == null || !normalNoteList[i][j].model.activeSelf) { continue; }
                normalNoteList[i][j].model.SetActive(false);
                ObjectPool.poolInstance.ReturnNoteInPool(i, normalNoteList[i][j].model);
            }
        }
        for (int i = barList.Count - 1; i >= 0; i--)
        {
            if (barList[i].model == null || !barList[i].model.activeSelf) { continue; }
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
        gameUIManager.FadeIn();
        yield return new WaitForSeconds(1.0f);
        GameUIManager.isCreateReady = true;
        gameUIManager.CloseLoading();
        yield return new WaitUntil(() => gameUIManager.isPrepared == 2);
        gameUIManager.GameUIUpdate(0, JudgeType.IGNORE);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(gameUIManager.FadeOut());
        yield return new WaitForSeconds(0.5f);
        isFadeEnd = true;
    }

    private void Awake()
    {
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("SyncCount");
        Application.targetFrameRate = PlayerPrefs.GetInt("FrameRate");
        userSpeed = PlayerPrefs.GetInt("NoteSpeed");
        displayDelayCorrectionValue = PlayerPrefs.GetInt("DisplayDelayCorrection");
        earlyLateThreshold = PlayerPrefs.GetInt("EarlyLateThreshold");
        verticalLine = PlayerPrefs.GetFloat("VerticalLine") * 0.018f;
        judgeLineYPosition = PlayerPrefs.GetInt("JudgeLine") == 0 ? 0.0f : -0.24f;

        stopwatch = new System.Diagnostics.Stopwatch();
        isPaused = true;
        isStarted = false;
        isClear = false;
        isCountdown = false;
        judge = JudgeManager.instance;
        bmsResult = new BMSResult();

        header = BMSFileSystem.selectedHeader;
        BMSFileSystem.selectedHeader = null;

        currentNote = new int[5] { 0, 0, 0, 0, 0 };
        notesListCount = new int[5];
        longNoteListCount = new int[5];
        normalNoteListCount = new int[5];
        notesList = new List<Note>[5];
        longNoteList = new List<Note>[5];
        normalNoteList = new List<Note>[5];

        endCount = 0;
        currentLoading = 0;
        currentRankIndex = -1;

        notePoolMaxCount = ObjectPool.poolInstance.maxNoteCount;
        longNotePoolMaxCount = ObjectPool.poolInstance.maxLongNoteCount;
        barPoolMaxCount = ObjectPool.poolInstance.maxBarCount;

        wait3Sec = new WaitForSeconds(3.0f);
        wait1Sec = new WaitForSeconds(1.5f);

        isKeyDown = new bool[5] { false, false, false, false, false };
        isNoteBombActive = new bool[5] { false, false, false, false, false };
        noteBombState = new int[5] { 0, 0, 0, 0, 0 };
        fsUpdate = new bool[2] { false, false };
        fsStates = new int[2] { 0, 0 };
        fsCount = new int[2] { 0, 0 };
        threadFrequency = new TimeSpan(10000000 / PlayerPrefs.GetInt("PollingRate"));

        StartCoroutine(PreLoad(false));
    }

    private void Update()
    {
        if (!isStarted) { return; }

        while (bgaChangeList[bgaChangeListCount].timing <= currentTime)
        {
            if (isBGAVideoSupported && !bgaChangeList[bgaChangeListCount].isPic) 
            { 
                videoPlayer.Play();
            }
            else 
            {
                gameUIManager.ChangeBGA(bgaChangeList[bgaChangeListCount].key);
            }
            if (bgaChangeListCount > 0)
            {
                bgaChangeListCount--;
            }
            else
            {
                bgaChangeList[bgaChangeListCount].timing = 2000000000.0d;
            }
        }

        currentTicks = stopwatch.ElapsedTicks - resumeCountTicks;
        double tempTime = currentTicks * 0.0000001d;
        double frameTime = tempTime - currentTime;
        currentTime = tempTime;

        double avg = currentBPM * frameTime;

        BPM next = null;
        bool flag = false;

        if (bpmsListCount > 0)
        {
            next = bpmsList[bpmsListCount - 1];

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

        lock (inputHandleLock) 
        { 
            PlayNotes();
            ChangeUI();
        }

        noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed) + judgeLineYPosition, 0.0f);
    }

    private void BGMPlayThread()
    {
        while (true)
        {
            while (bgSoundsList[bgSoundsListCount].timing <= stopwatch.ElapsedTicks)
            {
                soundManager.PlayBGM(bgSoundsList[bgSoundsListCount].keySound);
                if (bgSoundsListCount > 0)
                {
                    bgSoundsListCount--;
                }
                else
                {
                    bgmThread.Abort();
                }
            }
            Thread.Sleep(threadFrequency);
        }
    }

    private void ChangeUI()
    {
        #region Key Feedback Check
        if (isKeyDown[0])
        {
            gameUIManager.KeyInputImageSetActive(keyInput.prevKeyState[0], 0);
            isKeyDown[0] = false;
        }
        if (isKeyDown[1])
        {
            gameUIManager.KeyInputImageSetActive(keyInput.prevKeyState[1], 1);
            isKeyDown[1] = false;
        }
        if (isKeyDown[2])
        {
            gameUIManager.KeyInputImageSetActive(keyInput.prevKeyState[2] || keyInput.prevKeyState[5], 2);
            isKeyDown[2] = false;
        }
        if (isKeyDown[3])
        {
            gameUIManager.KeyInputImageSetActive(keyInput.prevKeyState[3], 3);
            isKeyDown[3] = false;
        }
        if (isKeyDown[4])
        {
            gameUIManager.KeyInputImageSetActive(keyInput.prevKeyState[4], 4);
            isKeyDown[4] = false;
        }
        #endregion

        #region Notebomb Check
        if (isNoteBombActive[0])
        {
            gameUIManager.NoteBombActive(0, noteBombState[0]);
            isNoteBombActive[0] = false;
        }
        if (isNoteBombActive[1])
        {
            gameUIManager.NoteBombActive(1, noteBombState[1]);
            isNoteBombActive[1] = false;
        }
        if (isNoteBombActive[2])
        {
            gameUIManager.NoteBombActive(2, noteBombState[2]);
            isNoteBombActive[2] = false;
        }
        if (isNoteBombActive[3])
        {
            gameUIManager.NoteBombActive(3, noteBombState[3]);
            isNoteBombActive[3] = false;
        }
        if (isNoteBombActive[4])
        {
            gameUIManager.NoteBombActive(4, noteBombState[4]);
            isNoteBombActive[4] = false;
        }
        #endregion

        if (isGameUIUpdate)
        {
            gameUIManager.GameUIUpdate(combo, currentJudge);
            isGameUIUpdate = false;
        }

        if (isGameEnd)
        {
            songEndCheckCoroutine = StartCoroutine(SongEndCheck());
            isEndJudgeInfoUpdate = 2;
            isGameEnd = false;
        }

        if (isGameOver)
        {
            GamePause(1);
            isGameOver = false;
        }
    }

    private void HandleNote(List<Note> noteList, int idx, double time)
    {
        Note n = noteList[notesListCount[idx]];
        double diff = time - n.timing;
        JudgeType result = judge.Judge(diff);
        if (result == JudgeType.IGNORE) { return; }

        currentCount++;
        --notesListCount[idx];

        if (notesListCount[idx] >= 0)
        {
            currentNote[idx] = noteList[notesListCount[idx]].keySound;
        }

        if (normalNoteListCount[idx] > 0 && n.beat == normalNoteList[idx][normalNoteListCount[idx] - normalNoteHandleCount[idx] - 1].beat)
        {
            normalNoteHandleCount[idx]++;
        }
        else
        {
            if (result == JudgeType.COOL) { result = JudgeType.KOOL; }
            currentLongNoteJudge[idx] = result;
            if (isCurrentLongNote[idx])
            {
                longNoteHandleCount[idx]++;
            }
            else
            {
                isCurrentLongNote[idx] = true;
            }
        }

        if (result <= JudgeType.MISS) { combo = -1; }
        else
        {
            bmsResult.judgeList[currentCount] = diff;
            if (result != JudgeType.GOOD) 
            { 
                isNoteBombActive[idx] = true;
                noteBombState[idx] = 0;
            }
        }

        currentJudge = result;

        UpdateResult(result);
        combo++;
        if (bmsResult.maxCombo <= combo)
        {
            bmsResult.maxCombo = combo;
        }
        isGameUIUpdate = true;

        if (result == JudgeType.FAIL) { return; }

        if ((earlyLateThreshold == 22 && result != JudgeType.KOOL) || (earlyLateThreshold < 22 && (diff > earlyLateThreshold || diff < -earlyLateThreshold)))
        {
            int fsState = diff < 0 ? 0 : 1;
            int index = idx < 2 ? 0 : 1;
            fsUpdate[index] = true;
            fsStates[index] = fsState;
            fsCount[fsState]++;
        }
    }

    private void HandleLongNoteTick(List<Note> noteList, int idx)
    {
        currentCount++;
        --notesListCount[idx];

        if (notesListCount[idx] >= 0) { currentNote[idx] = noteList[notesListCount[idx]].keySound; }

        if (!keyInput.prevKeyState[idx]) { currentLongNoteJudge[idx] = JudgeType.FAIL; }
        else { if (currentLongNoteJudge[idx] == JudgeType.FAIL) { currentLongNoteJudge[idx] = JudgeType.GOOD; } }

        JudgeType result = currentLongNoteJudge[idx];

        if (result <= JudgeType.MISS) { combo = -1; }
        else if (result >= JudgeType.COOL) 
        { 
            isNoteBombActive[idx] = true;
            noteBombState[idx] = 1;
        }

        currentJudge = currentLongNoteJudge[idx];

        UpdateResult(result);
        combo++;
        if (bmsResult.maxCombo <= combo)
        {
            bmsResult.maxCombo = combo;
        }
        isGameUIUpdate = true;
    }

    private void PlayNotes()
    {
        for (int i = 0; i < 5; i++)
        {
            while (normalNoteHandleCount[i] > 0)
            {
                normalNoteHandleCount[i]--;
                normalNoteListCount[i]--;
                int tempPeek = normalNoteListCount[i] - notePoolMaxCount;
                if (tempPeek >= 0)
                {
                    normalNoteList[i][normalNoteListCount[i]].modelTransform.localPosition = new Vector3(xPosition[i], (float)(normalNoteList[i][tempPeek].beat * gameSpeed), 0.0f);
                    normalNoteList[i][tempPeek].model = normalNoteList[i][normalNoteListCount[i]].model;
                    normalNoteList[i][tempPeek].modelTransform = normalNoteList[i][normalNoteListCount[i]].modelTransform;
                }
                else
                {
                    normalNoteList[i][normalNoteListCount[i]].model.SetActive(false);
                    ObjectPool.poolInstance.ReturnNoteInPool(i, normalNoteList[i][normalNoteListCount[i]].model);
                }
            }

            while (longNoteHandleCount[i] > 0)
            {
                longNoteHandleCount[i]--;
                for (int j = 0; j < 3; j++)
                {
                    Note tempNote = longNoteList[i][longNoteListCount[i] - 1];
                    longNoteListCount[i]--;

                    int len = longNoteListCount[i] - (3 * longNotePoolMaxCount);
                    if (len >= 0)
                    {
                        float yPos = (j == 1 ? (float)longNoteList[i][len + 1].beat : (float)longNoteList[i][len].beat) * gameSpeed;
                        tempNote.modelTransform.localPosition = new Vector3(xPosition[i], yPos, 0.0f);
                        longNoteList[i][len].model = tempNote.model;
                        longNoteList[i][len].modelTransform = tempNote.modelTransform;
                        if (j == 1)
                        {
                            longNoteList[i][len].modelTransform.localScale =
                                new Vector3(1.0f, ((float)pattern.longNote[i][len].beat * gameSpeed - longNoteOffset) * longNoteLength, 1.0f);
                        }
                    }
                    else
                    {
                        tempNote.model.SetActive(false);
                        ObjectPool.poolInstance.ReturnLongNoteInPool(i, (j == 2 ? 0 : j + 1), tempNote.model);
                    }
                }
            }

            while (notesListCount[i] >= 0 && notesList[i][notesListCount[i]].timing + 1750000.0d < currentTicks)
            {
                HandleNote(notesList[i], i, currentTicks);
            }

            while (notesListCount[i] >= 0 && notesList[i][notesListCount[i]].extra == 2 && notesList[i][notesListCount[i]].timing <= currentTicks)
            {
                HandleLongNoteTick(notesList[i], i);
            }

            if (!isCurrentLongNote[i]) { continue; }

            float longNoteNextYscale = ((float)(longNoteList[i][longNoteListCount[i] - 1].beat - currentBeat) * gameSpeed - longNoteOffset) * longNoteLength;
            if (longNoteNextYscale > longNoteEndOffset)
            {
                longNoteList[i][longNoteListCount[i] - 3].modelTransform.localPosition = new Vector3(xPosition[i], (float)currentBeat * gameSpeed, 0.0f);
                longNoteList[i][longNoteListCount[i] - 2].modelTransform.localScale = new Vector3(1.0f, longNoteNextYscale, 1.0f);
            }
            else
            {
                isCurrentLongNote[i] = false;
                longNoteHandleCount[i]++;
            }
        }

        while (barList[barListCount].timing <= currentTime)
        {
            Note bar = barList[barListCount];
            int len = barListCount - barPoolMaxCount;
            if (barListCount > 0)
            {
                barListCount--;
            }
            else
            {
                barList[barListCount].timing = 2000000000.0d;
            }
            if (len >= 0)
            {
                bar.modelTransform.localPosition = new Vector3(xPosition[2], (float)(barList[len].beat * gameSpeed), 0.0f);
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
        /*for (int i = 0; i < 5; i++)
        {
            while (notesListCount[i] >= 0 && notesList[i][notesListCount[i]].extra != 2 && 
                notesList[i][notesListCount[i]].timing <= currentTicks)
            {
                soundManager.PlayKeySound(notesList[i][notesListCount[i]].keySound);
                HandleNote(notesList[i], i, currentTicks);
            }
        }*/
    }

    public void KeyDown(int index)
    {
        long keyDownTime = stopwatch.ElapsedTicks - resumeCountTicks;
        if (isClear) { soundManager.PlayKeySoundEnd(currentNote[index]); }
        else { soundManager.PlayKeySound(currentNote[index]); }
        isKeyDown[index] = true;
        if (notesListCount[index] < 0 || notesList[index][notesListCount[index]].extra == 2) { return; }
        lock (inputHandleLock) { HandleNote(notesList[index], index, keyDownTime); }
    }

    public void KeyUp(int index)
    {
        isKeyDown[index] = true;
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
        isJudgementTrackerUpdate = true;

        double under60 = height60;
        double up60 = 0.0d;
        if (currentScore <= 600000.0d) { under60 = currentScore * divide20000; }
        else { up60 = (currentScore - 600000.0d) * divide6250; }
        float scoreStickHeight = (float)(under60 + up60);
        bmsResult.scoreGraphArray[currentCount] = scoreStickHeight;

        switch ((float)currentScore)
        {
            case float n when (n >= 0.0f && n < 550000.0f): bmsResult.rankIndex = 0; break;
            case float n when (n >= 550000.0f && n < 650000.0f): bmsResult.rankIndex = 1; break;
            case float n when (n >= 650000.0f && n < 750000.0f): bmsResult.rankIndex = 2; break;
            case float n when (n >= 750000.0f && n < 850000.0f): bmsResult.rankIndex = 3; break;
            case float n when (n >= 850000.0f && n < 900000.0f): bmsResult.rankIndex = 4; break;
            case float n when (n >= 900000.0f && n < 950000.0f): bmsResult.rankIndex = 5; break;
            case float n when (n >= 950000.0f && n < 1000000.0f): bmsResult.rankIndex = 6; break;
            case float n when (n >= 1000000.0f && n < 1025000.0f): bmsResult.rankIndex = 7; break;
            case float n when (n >= 1025000.0f && n < 1050000.0f): bmsResult.rankIndex = 8; break;
            case float n when (n >= 1050000.0f && n < 1090000.0f): bmsResult.rankIndex = 9; break;
            case float n when (n >= 1090000.0f): bmsResult.rankIndex = 10; break;
        }
        if (currentRankIndex != bmsResult.rankIndex) 
        { 
            currentRankIndex = bmsResult.rankIndex;
            isChangeRankImage = true;
        }

        if (gauge.hp > 1.0f) { gauge.hp = 1.0f; }
        else if (gauge.hp <= 0.0f) { isGameOver = true; }

        if (currentCount >= pattern.noteCount && gauge.hp > 0.0f)
        {
            isGameEnd = true;
            isPaused = true;
            bmsResult.score = currentScore + bmsResult.maxCombo;
            under60 = height60;
            up60 = 0.0d;
            if (bmsResult.score <= 600000.0d) { under60 = bmsResult.score * divide20000; }
            else { up60 = (bmsResult.score - 600000.0d) * divide6250; }
            scoreStickHeight = (float)(under60 + up60);
            bmsResult.scoreGraphArray[currentCount + 1] = scoreStickHeight;
            endCount++;
        }
        isScoreGraphUpdate = true;
    }

    public void GamePause(int set)
    {
        lock (inputHandleLock)
        {
            if (isPaused) { return; }
            isPaused = true;
            gameUIManager.AnimationPause(true);
            stopwatch.Stop();
            soundManager.AudioPause(true);
            keyInput.InputThreadAbort();
            if (bgmThread.IsAlive)
            {
                bgmThread.Abort();
            }
            if (isBGAVideoSupported) { videoPlayer.Pause(); }
            pauseManager.PausePanelSetting(set);
            pauseManager.Pause_SetActive(true);
            gameUIManager.SetPausePanelNoteSpeedText();
        }
    }

    public IEnumerator GameEnd(bool clear)
    {
        pauseManager.Pause_SetActive(false);
        keyInput.KeyDisable();
        isPaused = true;
        isClear = clear;

        if (isClear)
        {
            int maxWaitTime = 0;
            while (soundManager.IsPlayingAudio()) 
            {
                maxWaitTime++;
                if (maxWaitTime >= 10) { break; }
                yield return wait1Sec;
            }
        }
        else
        {
            if (bgmThread.IsAlive)
            {
                bgmThread.Abort();
            }
        }
        soundManager.AudioAllStop();

        for (int i = bmsResult.judgeList.Length - 1; i >= 1; i--) { bmsResult.judgeList[i] *= 0.0001d; }
        bmsResult.score = currentScore + bmsResult.maxCombo;
        bmsResult.accuracy = bmsResult.score / (1100000.0d + pattern.noteCount);

        if (isClear) { yield return wait3Sec; }
        StartCoroutine(CoLoadScene(3));
    }

    public IEnumerator CoLoadScene(int scene)
    {
        gameUIManager.AnimationPause(false);
        pauseManager.Pause_SetActive(false);
        gameUIManager.FadeIn();
        yield return new WaitForSecondsRealtime(1.0f);
        ReturnAllNotes();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void GameResume()
    {
        for (int i = 0; i <= bgSoundsListCount; i++)
        {
            bgSoundsList[i].timing += 30000000;
        }
        bgmThread = new Thread(BGMPlayThread);
        if (bgSoundsListCount >= 0)
        {
            bgmThread.Start();
        }
        combo = 0;
        inputBlockLine.localPosition = new Vector3(xPosition[2], (float)(currentBeat * gameSpeed), 0.0f);
        resumeTicks = currentTicks;
        resumeCountTicks += 30000000;
        currentTicks = stopwatch.ElapsedTicks - resumeCountTicks;
        pauseManager.Pause_SetActive(false);
        stopwatch.Start();
        StartCoroutine(CoCountDown3());
        Thread bgmResumeThread = new Thread(ResumeBGM);
        bgmResumeThread.Start();
    }

    private IEnumerator CoCountDown3()
    {
        isCountdown = true;
        gameUIManager.SetActiveCountdown(true);
        GameObject countdownBar = GameObject.Find("CountDown_Bar");
        GameObject countdownTime = GameObject.Find("CountDown_Time");
        while (true)
        {
            long countdown = resumeTicks - currentTicks;
            if (countdown <= 0)
            {
                gameUIManager.SetActiveCountdown(false);
                gameUIManager.AnimationPause(false);
                keyInput.InputThreadStart();
                if (isBGAVideoSupported) { videoPlayer.Play(); }
                isPaused = false;
                isCountdown = false;
                gameUIManager.GameUIUpdate(0, JudgeType.IGNORE);
                break;
            }
            else
            {
                float countdownMS = countdown * 0.0000001f;
                int second = (int)countdownMS;
                gameUIManager.SetCountdown(countdownMS - second, second, countdownBar, countdownTime);
            }
            yield return null;
        }
    }

    private void ResumeBGM()
    {
        while (resumeTicks > currentTicks)
        {
            Thread.Sleep(threadFrequency);
        }
        soundManager.AudioPause(false);
    }

    private IEnumerator SongEndCheck()
    {
        while (true)
        {
            if (((!isBGAVideoSupported && bgaChangeListCount == 0) || (isBGAVideoSupported && !videoPlayer.isPlaying)))
            {
                StartCoroutine(GameEnd(true));
                songEndCheckCoroutine = null;
                yield break;
            }
            yield return null;
        }
    }

    private void MakeTable()
    {
        int len = pattern.noteCount + 1;
        divideTable = new double[len];
        for (int i = 1; i < len; i++) { divideTable[i] = 1.0d / i; }

        maxScoreTable = new float[len + 1];
        if (SongSelectUIManager.resultData.rankIndex == 11)
        {
            for (int i = 1; i < len + 1; i++) { maxScoreTable[i] = 0; }
        }
        else
        {
            for (int i = 1; i < len + 1; i++) { maxScoreTable[i] = SongSelectUIManager.scoreGraphData.scoreGraphList[i]; }
        }
    }

    public void ChangeSpeed(int value)
    {
        lock (inputHandleLock)
        {
            userSpeed += value;
            if (userSpeed > 200) { userSpeed = 200; }
            else if (userSpeed < 10) { userSpeed = 10; }
            PlayerPrefs.SetInt("NoteSpeed", userSpeed);
            if (pauseManager.enabled) { gameUIManager.SetPausePanelNoteSpeedText(); }
            gameSpeed = CalulateSpeed();

            float verticalLineLength = verticalLine * userSpeed;

            for (int i = 0; i < 5; i++)
            {
                for (int j = normalNoteListCount[i] - 1; j >= 0; j--)
                {
                    if (normalNoteList[i][j].model == null) { break; }

                    normalNoteList[i][j].modelTransform.localPosition = new Vector3(xPosition[i], (float)(normalNoteList[i][j].beat * gameSpeed), 0.0f);
                    normalNoteList[i][j].modelTransform.GetChild(0).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                    normalNoteList[i][j].modelTransform.GetChild(1).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                }

                bool isCurrent = true;
                for (int j = longNoteListCount[i] - 1; j >= 0; j -= 3)
                {
                    if (longNoteList[i][j].model == null) { break; }

                    for (int k = 0; k < 3; k++)
                    {
                        float yPos;
                        if (k == 2)
                        {
                            if (isCurrentLongNote[i] && isCurrent) { yPos = (float)currentBeat * gameSpeed; }
                            else { yPos = (float)longNoteList[i][j - 2].beat * gameSpeed; }
                        }
                        else { yPos = (float)longNoteList[i][j].beat * gameSpeed; }

                        longNoteList[i][j - k].modelTransform.localPosition = new Vector3(xPosition[i], yPos, 0.0f);

                        if (k == 1)
                        {
                            float scale;
                            if (isCurrentLongNote[i] && isCurrent) { scale = ((float)(longNoteList[i][j].beat - currentBeat) * gameSpeed - longNoteOffset) * longNoteLength; }
                            else { scale = ((float)longNoteList[i][j - 1].beat * gameSpeed - longNoteOffset) * longNoteLength; }
                            longNoteList[i][j - k].modelTransform.localScale = new Vector3(1.0f, scale, 1.0f);
                        }
                        else
                        {
                            longNoteList[i][j - k].modelTransform.GetChild(0).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                            longNoteList[i][j - k].modelTransform.GetChild(1).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                        }
                    }
                    isCurrent = false;
                }
            }
            for (int i = barListCount; i >= 0; i--)
            {
                if (barList[i].model == null) { break; }

                barList[i].modelTransform.localPosition = new Vector3(xPosition[2], (float)(barList[i].beat * gameSpeed), 0.0f);
            }
            noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed), 0.0f);
        }
    }

    public void ChangeJudgeAdjValue(int value)
    {
        displayDelayCorrectionValue += value;
        if (displayDelayCorrectionValue > 80) 
        { 
            displayDelayCorrectionValue = 80; 
            value = 0;
        }
        else if (displayDelayCorrectionValue < -80) 
        {
            displayDelayCorrectionValue = -80;
            value = 0;
        }

        currentBeat += (displayDelayCorrectionBeat * value);
        longNoteEndOffset = Mathf.Max((float)(displayDelayCorrectionBeat * -displayDelayCorrectionValue), 0.0f);

        PlayerPrefs.SetInt("DisplayDelayCorrection", displayDelayCorrectionValue);

        gameUIManager.CoUpdateJudgeAdjText();
    }

    private void OnDestroy()
    {
        keyInput.KeyDisable();
        keyInput.InputThreadAbort();
        if (bgmThread.IsAlive)
        {
            bgmThread.Abort();
        }
        soundManager.SoundAndChannelRelease();
    }
}
