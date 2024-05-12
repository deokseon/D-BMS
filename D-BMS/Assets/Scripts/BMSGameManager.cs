using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Video;
using System;
using System.IO;
using Cysharp.Threading.Tasks;

public class BMSGameManager : MonoBehaviour
{
    public static ReplayData replayData;
    public static BMSResult bmsResult;
    public static bool isClear;
    public static bool isReplay;
    private bool isPaused = true;
    private bool isStarted = false;
    private bool isCountdown = false;
    private bool isSongEndChecking = false;

    [HideInInspector] public float longNoteOffset;
    [HideInInspector] public float longNoteLength;
    private float verticalLine;
    private int userSpeed;
    private float gameSpeed;
    [HideInInspector] public int earlyLateThreshold;
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
    [SerializeField]
    private NoteBomb noteBomb;
    [SerializeField]
    private GamePanel gamePanel;
    [SerializeField]
    private JudgeEffect judgeEffect;
    [SerializeField]
    private ComboScore comboScore;
    private GameObject replayNoteParent;

    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    private double currentBeat = 0;
    private double currentScrollTime = 0;
    private long currentTicks = 0;
    private long resumeTicks = 0;
    private long resumeCountTicks = 0;
    private double currentTime = 0;
    [HideInInspector] public int accuracySum = 0;
    [HideInInspector] public int currentCount = 0;
    private int totalLoading = 0;
    private bool isFadeEnd = false;
    private float divideTotalLoading;
    public static int currentLoading = 0;
    private int currentRankIndex = -1;
    [HideInInspector] public int endCount = 0;

    private const double divide20000 = 1.0d / 20000.0d;
    private const double height60 = 600000.0d * divide20000;
    private const double divide6250 = 1.0d / 6250.0d;
    private const double divide60 = 1.0d / 60.0d;
    private float divideBPM;
    [HideInInspector] public double[] divideTable;

    public static BMSHeader header;
    public Gauge gauge;
    private JudgeManager judge = JudgeManager.instance;
    private BMSPattern pattern;
    private double currentBPM;
    private int combo = 0;
    private bool isBGAVideoSupported = false;

    private int[] currentKeySound = new int[5] { 0, 0, 0, 0, 0 };

    private double koolAddScore;
    private double coolAddScore;
    private double goodAddScore;
    [HideInInspector] public double currentScore = 0.0d;

    private JudgeType[] currentLongNoteJudge = { JudgeType.KOOL, JudgeType.KOOL, JudgeType.KOOL, JudgeType.KOOL, JudgeType.KOOL };
    private bool[] isCurrentLongNote = { false, false, false, false, false };
    private int[] longNoteHandleCount = { 0, 0, 0, 0, 0 };
    private int[] normalNoteHandleCount = { 0, 0, 0, 0, 0 };

    private int notePoolMaxCount;
    private int longNotePoolMaxCount;
    private int barPoolMaxCount;

    [HideInInspector] public float[] xPosition;
    private int bgaChangeArrayCount;
    private int layerChangeArrayCount;
    private int bgSoundArrayCount;
    private int bpmArrayCount;
    private int fourBeatArrayCount;
    private int[] noteArrayCount = new int[5];
    private int[] longNoteArrayCount = new int[5];
    private int[] normalNoteArrayCount = new int[5];
    private int[] keySoundChangeArrayCount = new int[5];
    private int barArrayCount;
    private BGChange[] bgaChangeArray;
    private BGChange[] layerChangeArray;
    private Note[] bgSoundArray;
    private BPM[] bpmArray;
    private Note[][] noteArray = new Note[5][];
    private Note[][] longNoteArray = new Note[5][];
    private Note[][] normalNoteArray = new Note[5][];
    private Note[] barArray;
    private KeySoundChange[][] keySoundChangeArray = new KeySoundChange[5][];
    private double[] fourBeatArray;
    private int[] replayNoteArrayCount = new int[5];
    public ReplayNote[][] replayNoteArray = new ReplayNote[5][];
    private List<ReplayNoteData>[] replayNoteDataList = new List<ReplayNoteData>[5];

    private Thread bgmThread;
    private Thread keySoundChangeThread;
    private Thread replayInputThread;
    private TimeSpan threadFrequency;
    [HideInInspector] public readonly object threadLock = new object();
    private bool[] isKeyDown = new bool[5] { false, false, false, false, false };
    [HideInInspector] public bool isChangeRankImage = false;
    [HideInInspector] public bool isJudgementTrackerUpdate = false;
    [HideInInspector] public bool isScoreGraphUpdate = false;
    [HideInInspector] public bool[] fsUpdate;
    [HideInInspector] public int[] fsStates;
    private bool isGameUIUpdate = false;
    private bool isGameEnd = false;
    [HideInInspector] public int isEndJudgeInfoUpdate = 0;
    private bool isGameOver = false;
    private JudgeType currentJudge;

    private void Awake()
    {
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("SyncCount");
        Application.targetFrameRate = PlayerPrefs.GetInt("FrameRate");
        userSpeed = PlayerPrefs.GetInt("NoteSpeed");
        earlyLateThreshold = PlayerPrefs.GetInt("EarlyLateThreshold") * 10000;
        verticalLine = PlayerPrefs.GetFloat("VerticalLine") * 0.018f;
        judgeLineYPosition = PlayerPrefs.GetInt("JudgeLine") == 0 ? GameUIManager.config.judgeLinePosition : GameUIManager.config.judgeLinePosition - 0.24f;

        isClear = false;
        bmsResult = new BMSResult();

        header = BMSFileSystem.selectedHeader;
        BMSFileSystem.selectedHeader = null;

        currentLoading = 0;

        notePoolMaxCount = ObjectPool.poolInstance.maxNoteCount;
        longNotePoolMaxCount = ObjectPool.poolInstance.maxLongNoteCount;
        barPoolMaxCount = ObjectPool.poolInstance.maxBarCount;

        fsUpdate = new bool[2] { false, false };
        fsStates = new int[2] { 0, 0 };
        threadFrequency = new TimeSpan(10000000 / PlayerPrefs.GetInt("PollingRate"));

        if (replayData == null)
        {
            replayData = new ReplayData();
            replayData.noteList = new List<AbstractNote>[5];
        }

        _ = PreLoad(false);
    }

    public float ConvertSpeed()
    {
        return (userSpeed * 13.0f * divideBPM);
    }

    private async UniTask PreLoad(bool isRestart)
    {
        stopwatch.Reset();
        soundManager.AudioPause(false);
        bgmThread = new Thread(BGMPlayThread);
        keySoundChangeThread = new Thread(KeySoundChangeThread);

        gameUIManager.bgaOpacity.color = new Color(0, 0, 0, 0);

        ObjectPool.poolInstance.Init();

        BMSParser.instance.Parse(isRestart);
        pattern = BMSParser.instance.pattern;
        SaveReplayNoteList();

        pattern.GetBeatsAndTimings();
        SetReplayNoteArray();

        bgaChangeArray = pattern.bgaChanges.ToArray();
        layerChangeArray = pattern.layerChanges.ToArray();
        bgSoundArray = pattern.bgSounds.ToArray();
        bpmArray = pattern.bpms.ToArray();
        for (int i = 0; i < 5; i++)
        {
            noteArray[i] = pattern.lines[i].noteList.ToArray();
            longNoteArray[i] = pattern.longNote[i].ToArray();
            normalNoteArray[i] = pattern.normalNote[i].ToArray();
            keySoundChangeArray[i] = pattern.keySoundChangeTimingList[i].ToArray();

            noteArrayCount[i] = noteArray[i].Length - 1;
            longNoteArrayCount[i] = longNoteArray[i].Length;
            normalNoteArrayCount[i] = normalNoteArray[i].Length;
            keySoundChangeArrayCount[i] = keySoundChangeArray[i].Length - 1;

            if (keySoundChangeArrayCount[i] == -1)
            {
                keySoundChangeArray[i] = new KeySoundChange[1] { new KeySoundChange(20000000000.0d, 0) };
                keySoundChangeArrayCount[i] = 0;
            }

            replayNoteDataList[i] = new List<ReplayNoteData>(noteArrayCount[i] * 2 + 200);
        }
        barArray = pattern.barLine.noteList.ToArray();
        fourBeatArray = pattern.fourBeatList.ToArray();

        bgaChangeArrayCount = bgaChangeArray.Length - 1;
        layerChangeArrayCount = layerChangeArray.Length - 1;
        bgSoundArrayCount = bgSoundArray.Length - 1;
        bpmArrayCount = bpmArray.Length;
        barArrayCount = barArray.Length - 1;
        fourBeatArrayCount = fourBeatArray.Length - 1;

        divideBPM = (float)(1.0f / header.bpm);
        gameSpeed = ConvertSpeed();

        SetDisplayDelayCorrection();

        noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed) + judgeLineYPosition, 0.0f);

        currentBPM = bpmArray[bpmArrayCount - 1].bpm;
        --bpmArrayCount;

        bmsResult.noteCount = pattern.noteCount;
        bmsResult.resultData = new ResultData(0);
        bmsResult.scoreGraphData = new ScoreGraphData(bmsResult.noteCount + 2);
        bmsResult.judgeList = new double[bmsResult.noteCount + 1];
        for (int i = bmsResult.judgeList.Length - 1; i >= 1; i--) { bmsResult.judgeList[i] = 2000000.0d; }

        if (!isRestart)
        {
            koolAddScore = 1100000.0d / pattern.noteCount;
            coolAddScore = koolAddScore * 0.7d;
            goodAddScore = koolAddScore * 0.2d;

            MakeTable();
            FindObjectOfType<ScoreGraph>().SetMaxScoreGraph(pattern.noteCount + 2);
            gauge = new Gauge();

            GameUIManager.isCreateReady = false;
            gameUIManager.SetLoadingPanel();
            totalLoading = gameUIManager.bgImageList.Count + soundManager.pathes.Count + Directory.GetFiles($@"{Directory.GetParent(Application.dataPath)}\Skin\GameObject").Length;
            for (int i = bgaChangeArrayCount; i > -1; i--) { if (!bgaChangeArray[i].isPic) { totalLoading++; break; } }
            divideTotalLoading = 1.0f / totalLoading;
            _ = Loading();
            gameUIManager.LoadBGATextures();
            soundManager.AddAudioClips();
            if (videoPlayer.isActiveAndEnabled)
            {
                for (int i = bgaChangeArrayCount; i > -1; i--)
                {
                    if (!bgaChangeArray[i].isPic && File.Exists(header.musicFolderPath + pattern.bgVideoTable[pattern.bgaChanges[i].key]))
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
                    await UniTask.WaitUntil(() => videoPlayer.isPrepared || errorFlag);
                    isBGAVideoSupported = !errorFlag;
                    if (isBGAVideoSupported)
                    {
                        videoPlayer.Play();
                        await UniTask.WaitUntil(() => videoPlayer.frame != -1);
                        videoPlayer.Pause();
                    }
                    currentLoading++;
                }
            }
            await UniTask.WaitUntil(() => gameUIManager.isPrepared == gameUIManager.taskCount + 1);
            if (isReplay)
            {
                replayNoteParent = GameObject.Find("ReplayNoteParent");
            }
            keyInput.KeySetting();
            gameUIManager.SetNullBGATextureArray();
            await UniTask.WaitUntil(() => soundManager.isPrepared == soundManager.threadCount);
            await UniTask.WaitUntil(() => isFadeEnd);
        }
        else
        {
            bmsDrawer.DrawNotes(true);
        }

        inputBlockLine.localPosition = new Vector3(xPosition[2], -200.0f, 0.0f);

        for (int i = 0; i < 5; i++) { currentKeySound[i] = 0; }

        if (bgaChangeArrayCount + layerChangeArrayCount == -2 || ((!string.IsNullOrEmpty(videoPlayer.url) || gameUIManager.bgImageList.Count == 0) && !isBGAVideoSupported) ||
            (!isBGAVideoSupported && gameUIManager.CheckBGATextureEmpty()))
        {
            gameUIManager.bgaOpacity.color = Color.black;
        }
        else
        {
            gameUIManager.bgaOpacity.color = new Color(0, 0, 0, (10 - PlayerPrefs.GetInt("BGAOpacity")) * 0.1f);
        }
        if (bgaChangeArrayCount == -1 || ((!string.IsNullOrEmpty(videoPlayer.url) || gameUIManager.bgImageList.Count == 0) && !isBGAVideoSupported) || 
            (!isBGAVideoSupported && gameUIManager.CheckBGATextureEmpty()))
        {
            gameUIManager.bga.gameObject.SetActive(false);
            bgaChangeArray = new BGChange[1] { new BGChange(0, 0, 0, 0, true) };
            bgaChangeArrayCount = 0;
            bgaChangeArray[0].timing = 20000000000.0d;
        }
        if (layerChangeArrayCount == -1)
        {
            gameUIManager.layer.gameObject.SetActive(false);
            layerChangeArray = new BGChange[1] { new BGChange(0, 0, 0, 0, true) };
            layerChangeArrayCount = 0;
            layerChangeArray[0].timing = 20000000000.0d;
        }


        for (int i = 0; i < 5; i++)
        {
            gamePanel.KeyInputImageSetActive(false, i);
        }
        comboScore.ScoreComboUpdate(0, bmsResult.resultData.maxCombo, (int)(float)currentScore);
        gamePanel.SetHPbarValue(1.0f);
        isJudgementTrackerUpdate = true;
        isScoreGraphUpdate = true;
        isChangeRankImage = true;

        gamePanel.SetAnimationSpeed(currentBPM);

        GC.Collect();

        if (isRestart)
        {
            _ = gameUIManager.FadeOut();
            await UniTask.Delay(1000);
        }

        isStarted = true;
        isPaused = false;
        if (bgSoundArrayCount >= 0)
        {
            bgmThread.Start();
        }
        keySoundChangeThread.Start();
        InputThreadStart();
        stopwatch.Start();
    }

    public async UniTask GameRestart()
    {
        if (isCountdown || isClear || !isStarted) { return; }

        pauseManager.Pause_SetActive(false);
        gameUIManager.InfoPanelUpdateCoroutine(false);
        gameUIManager.SetActiveInfoPanel(false);
        bgmThread.Abort();
        keySoundChangeThread.Abort();
        InputThreadAbort();
        stopwatch.Stop();
        videoPlayer.Pause();
        isPaused = true;
        isStarted = false;
        isGameEnd = false;
        isSongEndChecking = false;
        noteBomb.NoteBombLEffectOff();
        gameUIManager.AnimationPause(false);
        gameUIManager.FadeIn();
        soundManager.AudioAllStop();
        await UniTask.Delay(1000);

        if (isBGAVideoSupported)
        {
            videoPlayer.Stop();
            videoPlayer.Prepare();
            await UniTask.WaitUntil(() => videoPlayer.isPrepared);
            videoPlayer.Play();
            await UniTask.WaitUntil(() => videoPlayer.frame != -1);
            videoPlayer.Pause();
        }
        gameUIManager.InitBGALayer();
        ReturnAllNotes();

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

        for (int i = 0; i < 5; i++)
        {
            isKeyDown[i] = false;
            isCurrentLongNote[i] = false;
            longNoteHandleCount[i] = 0;
            normalNoteHandleCount[i] = 0;
        }
        isEndJudgeInfoUpdate = 1;
        isGameUIUpdate = false;
        isJudgementTrackerUpdate = false;
        isScoreGraphUpdate = false;
        isChangeRankImage = false;
        isGameOver = false;
        for (int i = 0; i < 2; i++)
        {
            fsUpdate[i] = false;
        }

        _ = PreLoad(true);
    }

    private void ReturnAllNotes()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = longNoteArray[i].Length - 1; j >= 0; j -= 3)
            {
                if (longNoteArray[i][j].model == null || !longNoteArray[i][j].model.activeSelf) { continue; }
                for (int k = 0; k < 3; k++)
                {
                    longNoteArray[i][j - k].model.SetActive(false);
                    ObjectPool.poolInstance.ReturnLongNoteInPool(i, (k == 2 ? 0 : k + 1), longNoteArray[i][j - k].model);
                }
            }
            for (int j = normalNoteArray[i].Length - 1; j >= 0; j--)
            {
                if (normalNoteArray[i][j].model == null || !normalNoteArray[i][j].model.activeSelf) { continue; }
                normalNoteArray[i][j].model.SetActive(false);
                ObjectPool.poolInstance.ReturnNoteInPool(i, normalNoteArray[i][j].model);
            }
        }
        for (int i = barArray.Length - 1; i >= 0; i--)
        {
            if (barArray[i].model == null || !barArray[i].model.activeSelf) { continue; }
            barArray[i].model.SetActive(false);
            ObjectPool.poolInstance.ReturnBarInPool(barArray[i].model);
        }
    }

    private async UniTask Loading()
    {
        while (currentLoading < totalLoading)
        {
            gameUIManager.SetLoadingSlider(currentLoading * divideTotalLoading);
            await UniTask.Yield();
        }
        gameUIManager.SetLoadingSlider(1.0f);
        gameUIManager.FadeIn();
        await UniTask.Delay(1000);
        GameUIManager.isCreateReady = true;
        gameUIManager.CloseLoadingPanel();
        gamePanel.SetHPbarValue(1.0f);
        await UniTask.WaitUntil(() => gameUIManager.isPrepared == gameUIManager.taskCount + 1);
        comboScore.ScoreComboUpdate(0, bmsResult.resultData.maxCombo, (int)(float)currentScore);
        await UniTask.Delay(500);
        _ = gameUIManager.FadeOut();
        await UniTask.Delay(500);
        isFadeEnd = true;
    }

    private void Update()
    {
        if (!isStarted) { return; }

        while (bgaChangeArray[bgaChangeArrayCount].timing <= currentTime)
        {
            if (isBGAVideoSupported && !bgaChangeArray[bgaChangeArrayCount].isPic) 
            {
                gameUIManager.bga.texture = videoPlayer.texture;
                videoPlayer.Play();
            }
            else 
            {
                gameUIManager.ChangeBGA(bgaChangeArray[bgaChangeArrayCount].key);
            }
            if (bgaChangeArrayCount > 0)
            {
                bgaChangeArrayCount--;
            }
            else
            {
                bgaChangeArray[bgaChangeArrayCount].timing = 20000000000.0d;
            }
        }

        while (layerChangeArray[layerChangeArrayCount].timing <= currentTime)
        {
            gameUIManager.ChangeLayer(layerChangeArray[layerChangeArrayCount].key);
            if (layerChangeArrayCount > 0)
            {
                layerChangeArrayCount--;
            }
            else
            {
                layerChangeArray[layerChangeArrayCount].timing = 20000000000.0d;
            }
        }
        while (fourBeatArray[fourBeatArrayCount] <= currentTime)
        {
            gamePanel.SetGamePanelAnimationIndex();
            if (fourBeatArrayCount > 0)
            {
                fourBeatArrayCount--;
            }
            else
            {
                fourBeatArray[fourBeatArrayCount] = 20000000000.0d;
            }
        }

        currentTicks = stopwatch.ElapsedTicks - resumeCountTicks;
        double tempTime = currentTicks * 0.0000001d;
        double frameTime = tempTime - currentTime;
        currentTime = tempTime;

        double avg = currentBPM * frameTime;

        BPM next = null;
        bool flag = false;

        if (bpmArrayCount > 0)
        {
            next = bpmArray[bpmArrayCount - 1];

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
            --bpmArrayCount;
            gamePanel.SetAnimationSpeed(currentBPM);

            next = null;
            if (bpmArrayCount > 0) { next = bpmArray[bpmArrayCount - 1]; }
        }

        if (flag && prevTime <= currentScrollTime + frameTime)
        {
            avg += currentBPM * (currentScrollTime + frameTime - prevTime);
        }

        avg *= divide60;
        currentBeat += avg;
        currentScrollTime += frameTime;

        lock (threadLock) 
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
            while (bgSoundArray[bgSoundArrayCount].timing <= stopwatch.ElapsedTicks)
            {
                soundManager.PlayBGM(bgSoundArray[bgSoundArrayCount].keySound);
                bgSoundArrayCount--;
                if (bgSoundArrayCount < 0)
                {
                    bgmThread.Abort();
                }
            }
            Thread.Sleep(threadFrequency);
        }
    }

    private void KeySoundChangeThread()
    {
        while (true)
        {
            for (int i = 0; i < 5; i++)
            {
                if (keySoundChangeArray[i][keySoundChangeArrayCount[i]].timing <= stopwatch.ElapsedTicks)
                {
                    lock (threadLock)
                    {
                        currentKeySound[i] = keySoundChangeArray[i][keySoundChangeArrayCount[i]].keySound;
                    }
                    if (keySoundChangeArrayCount[i] > 0)
                    {
                        keySoundChangeArrayCount[i]--;
                    }
                    else
                    {
                        keySoundChangeArray[i][keySoundChangeArrayCount[i]].timing = 20000000000.0d;
                    }
                }
            }
            Thread.Sleep(threadFrequency);
        }
    }

    private void ReplayInputCheck()
    {
        while (true)
        {
            for (int i = 0; i < 5; i++)
            {
                if (replayNoteArray[i][replayNoteArrayCount[i]].timing <= stopwatch.ElapsedTicks)
                {
                    lock (threadLock)
                    {
                        switch (replayNoteArray[i][replayNoteArrayCount[i]].extra)
                        {
                            case 0:
                            case 5:
                                keyInput.prevKeyState[i] = true;
                                isKeyDown[i] = true;
                                if (isClear) { soundManager.PlayKeySoundEnd(currentKeySound[i]); }
                                else { soundManager.PlayKeySound(currentKeySound[i]); }
                                if (replayNoteArray[i][replayNoteArrayCount[i]].diff >= -1750000.0d)
                                {
                                    HandleNote(i, replayNoteArray[i][replayNoteArrayCount[i]].timing);
                                }
                                break;
                            case 1:
                                keyInput.prevKeyState[i] = false;
                                isKeyDown[i] = true;
                                break;
                            case 2:
                                HandleReplayLongNoteTick(i, replayNoteArray[i][replayNoteArrayCount[i]].diff);
                                break;
                            case 3:
                                HandleNote(i, replayNoteArray[i][replayNoteArrayCount[i]].timing);
                                break;
                            case 4:
                                combo = 0;
                                isGameUIUpdate = true;
                                break;
                        }
                    }
                    if (replayNoteArrayCount[i] > 0)
                    {
                        replayNoteArrayCount[i]--;
                    }
                    else
                    {
                        replayNoteArray[i][replayNoteArrayCount[i]].timing = 20000000000.0d;
                    }
                }
            }
            Thread.Sleep(1);
        }
    }

    private void ChangeUI()
    {
        #region Key Feedback Check
        if (isKeyDown[0])
        {
            gamePanel.KeyInputImageSetActive(keyInput.prevKeyState[0], 0);
            isKeyDown[0] = false;
        }
        if (isKeyDown[1])
        {
            gamePanel.KeyInputImageSetActive(keyInput.prevKeyState[1], 1);
            isKeyDown[1] = false;
        }
        if (isKeyDown[2])
        {
            gamePanel.KeyInputImageSetActive(keyInput.prevKeyState[2] || keyInput.prevKeyState[5], 2);
            isKeyDown[2] = false;
        }
        if (isKeyDown[3])
        {
            gamePanel.KeyInputImageSetActive(keyInput.prevKeyState[3], 3);
            isKeyDown[3] = false;
        }
        if (isKeyDown[4])
        {
            gamePanel.KeyInputImageSetActive(keyInput.prevKeyState[4], 4);
            isKeyDown[4] = false;
        }
        #endregion

        if (isGameUIUpdate)
        {
            comboScore.ScoreComboUpdate(combo, bmsResult.resultData.maxCombo, (int)(float)currentScore);
            judgeEffect.JudgeEffectAnimationActive(currentJudge);
            isGameUIUpdate = false;
        }

        if (isGameEnd)
        {
            _ = SongEndCheck();
            isEndJudgeInfoUpdate = 2;
            isGameEnd = false;
        }

        if (isGameOver)
        {
            GamePause(1);
            gamePanel.SetHPbarValue(0.0f);
            isGameOver = false;
        }
    }

    private void HandleNote(int idx, double time)
    {
        Note n = noteArray[idx][noteArrayCount[idx]];
        double diff = time - n.timing;
        JudgeType result = judge.Judge(diff);
        int replayExtra = 0;
        if (result == JudgeType.IGNORE) { replayNoteDataList[idx].Add(new ReplayNoteData(time, diff, 0)); return; }

        currentCount++;

        if (noteArrayCount[idx] > 0) --noteArrayCount[idx];
        else
        {
            noteArray[idx][noteArrayCount[idx]].timing = 20000000000.0d;
            noteArray[idx][noteArrayCount[idx]].tickTiming = 20000000000.0d;
            noteArray[idx][noteArrayCount[idx]].failTiming = 20000000000.0d;
        }

        //if (normalNoteArrayCount[idx] > 0 && n.beat == normalNoteArray[idx][normalNoteArrayCount[idx] - normalNoteHandleCount[idx] - 1].beat)
        if (n.extra == 0)
        {
            normalNoteHandleCount[idx]++;
        }
        else
        {
            if (result == JudgeType.COOL) 
            {
                result = JudgeType.KOOL;
                replayExtra = 5; 
            }
            currentLongNoteJudge[idx] = result;
            if (isCurrentLongNote[idx])
            {
                longNoteHandleCount[idx]++;
            }
            else
            {
                noteBomb.isNoteBombLEffectActive[idx] = true;
                isCurrentLongNote[idx] = true;
            }
        }

        if (result <= JudgeType.MISS) 
        {
            combo = -1;
            noteBomb.isNoteBombLEffectActive[idx] = false;
        }
        else
        {
            bmsResult.judgeList[currentCount] = diff;
            if (result != JudgeType.GOOD) 
            {
                noteBomb.noteBombNAnimationIndex[idx] = 0;
            }
        }

        currentJudge = result;

        UpdateResult(result);

        if (result == JudgeType.FAIL) { replayNoteDataList[idx].Add(new ReplayNoteData(time, diff, 3)); return; }

        if ((earlyLateThreshold == 220000 && result != JudgeType.KOOL) || (earlyLateThreshold < 220000 && (diff > earlyLateThreshold || diff < -earlyLateThreshold)))
        {
            int index = idx < 2 ? 0 : 1;
            fsUpdate[index] = true;

            if (diff < 0)
            {
                fsStates[index] = 0;
                bmsResult.resultData.earlyCount++;
            }
            else
            {
                fsStates[index] = 1;
                bmsResult.resultData.lateCount++;
            }
        }
        replayNoteDataList[idx].Add(new ReplayNoteData(time, diff, replayExtra));
    }

    private void HandleLongNoteTick(int idx)
    {
        currentCount++;

        double timing = noteArray[idx][noteArrayCount[idx]].tickTiming;

        if (noteArrayCount[idx] > 0) --noteArrayCount[idx];
        else
        {
            noteArray[idx][noteArrayCount[idx]].timing = 20000000000.0d;
            noteArray[idx][noteArrayCount[idx]].tickTiming = 20000000000.0d;
            noteArray[idx][noteArrayCount[idx]].failTiming = 20000000000.0d;
        }

        if (idx == 2 ? !keyInput.prevKeyState[idx] && !keyInput.prevKeyState[5] : !keyInput.prevKeyState[idx]) { currentLongNoteJudge[idx] = JudgeType.FAIL; }
        else { if (currentLongNoteJudge[idx] == JudgeType.FAIL) { currentLongNoteJudge[idx] = JudgeType.GOOD; } }

        JudgeType result = currentLongNoteJudge[idx];

        if (result <= JudgeType.MISS) 
        {
            combo = -1;
            noteBomb.isNoteBombLEffectActive[idx] = false;
        }
        else if (result >= JudgeType.COOL) 
        {
            noteBomb.noteBombNAnimationIndex[idx] = 0;
        }

        currentJudge = currentLongNoteJudge[idx];

        UpdateResult(result);

        double diff = 0.0d;
        switch (result)
        {
            case JudgeType.KOOL: diff = 0.0d; break;
            case JudgeType.COOL: diff = 350000.0d; break;
            case JudgeType.GOOD: diff = 700000.0d; break;
            case JudgeType.MISS: diff = 1400000.0d; break;
            case JudgeType.FAIL: diff = 1800000.0d; break;
        }

        replayNoteDataList[idx].Add(new ReplayNoteData(timing, diff, 2));
    }

    private void HandleReplayLongNoteTick(int idx, double diff)
    {
        JudgeType result = judge.Judge(diff);
        currentCount++;

        if (noteArrayCount[idx] > 0) --noteArrayCount[idx];
        else
        {
            noteArray[idx][noteArrayCount[idx]].timing = 20000000000.0d;
            noteArray[idx][noteArrayCount[idx]].tickTiming = 20000000000.0d;
            noteArray[idx][noteArrayCount[idx]].failTiming = 20000000000.0d;
        }

        if (result <= JudgeType.MISS) 
        { 
            combo = -1;
            noteBomb.isNoteBombLEffectActive[idx] = false;
        }
        else if (result >= JudgeType.COOL)
        {
            noteBomb.noteBombNAnimationIndex[idx] = 0;
        }

        currentJudge = result;

        UpdateResult(result);
    }

    private void PlayNotes()
    {
        for (int i = 0; i < 5; i++)
        {
            while (normalNoteHandleCount[i] > 0)
            {
                normalNoteHandleCount[i]--;
                normalNoteArrayCount[i]--;
                int tempPeek = normalNoteArrayCount[i] - notePoolMaxCount;
                if (tempPeek >= 0)
                {
                    normalNoteArray[i][normalNoteArrayCount[i]].modelTransform.localPosition = new Vector3(xPosition[i], (float)(normalNoteArray[i][tempPeek].beat * gameSpeed), 0.0f);
                    normalNoteArray[i][tempPeek].model = normalNoteArray[i][normalNoteArrayCount[i]].model;
                    normalNoteArray[i][tempPeek].modelTransform = normalNoteArray[i][normalNoteArrayCount[i]].modelTransform;
                }
                else
                {
                    normalNoteArray[i][normalNoteArrayCount[i]].model.SetActive(false);
                    ObjectPool.poolInstance.ReturnNoteInPool(i, normalNoteArray[i][normalNoteArrayCount[i]].model);
                }
            }

            while (longNoteHandleCount[i] > 0)
            {
                longNoteHandleCount[i]--;
                for (int j = 0; j < 3; j++)
                {
                    Note tempNote = longNoteArray[i][longNoteArrayCount[i] - 1];
                    longNoteArrayCount[i]--;

                    int len = longNoteArrayCount[i] - (3 * longNotePoolMaxCount);
                    if (len >= 0)
                    {
                        float yPos = (j == 1 ? (float)longNoteArray[i][len + 1].beat : (float)longNoteArray[i][len].beat) * gameSpeed;
                        tempNote.modelTransform.localPosition = new Vector3(xPosition[i], yPos, 0.0f);
                        longNoteArray[i][len].model = tempNote.model;
                        longNoteArray[i][len].modelTransform = tempNote.modelTransform;
                        if (j == 1)
                        {
                            longNoteArray[i][len].modelTransform.localScale =
                                new Vector3(1.0f, ((float)longNoteArray[i][len].beat * gameSpeed - longNoteOffset) * longNoteLength, 1.0f);
                        }
                    }
                    else
                    {
                        tempNote.model.SetActive(false);
                        ObjectPool.poolInstance.ReturnLongNoteInPool(i, (j == 2 ? 0 : j + 1), tempNote.model);
                    }
                }
            }

            while (noteArray[i][noteArrayCount[i]].failTiming < currentTicks)
            {
                HandleNote(i, currentTicks);
            }

            while (noteArray[i][noteArrayCount[i]].tickTiming <= currentTicks)
            {
                HandleLongNoteTick(i);
            }

            if (!isCurrentLongNote[i]) { continue; }

            float longNoteNextYscale = ((float)(longNoteArray[i][longNoteArrayCount[i] - 1].beat - currentBeat) * gameSpeed - longNoteOffset) * longNoteLength;
            float longNoteNextPosition = (float)currentBeat * gameSpeed;
            longNoteArray[i][longNoteArrayCount[i] - 3].modelTransform.localPosition = new Vector3(xPosition[i], longNoteNextPosition, 0.0f);
            if (longNoteNextYscale > 0.0f)
            {
                longNoteArray[i][longNoteArrayCount[i] - 2].modelTransform.localScale = new Vector3(1.0f, longNoteNextYscale, 1.0f);
            }
            else
            {
                float longNoteTopNextPosition = longNoteNextPosition + longNoteOffset;
                longNoteArray[i][longNoteArrayCount[i] - 2].modelTransform.localPosition = new Vector3(xPosition[i], longNoteTopNextPosition, 0.0f);
                longNoteArray[i][longNoteArrayCount[i] - 1].modelTransform.localPosition = new Vector3(xPosition[i], longNoteTopNextPosition, 0.0f);
            }

            if (currentTicks >= longNoteArray[i][longNoteArrayCount[i] - 1].timing)
            {
                noteBomb.isNoteBombLEffectActive[i] = false;
                isCurrentLongNote[i] = false;
                longNoteHandleCount[i]++;
            }
        }

        while (barArray[barArrayCount].timing <= currentTime)
        {
            Note bar = barArray[barArrayCount];
            int len = barArrayCount - barPoolMaxCount;
            if (barArrayCount > 0)
            {
                barArrayCount--;
            }
            else
            {
                barArray[barArrayCount].timing = 20000000000.0d;
            }
            if (len >= 0)
            {
                bar.modelTransform.localPosition = new Vector3(xPosition[2], (float)(barArray[len].beat * gameSpeed), 0.0f);
                barArray[len].model = bar.model;
                barArray[len].modelTransform = bar.modelTransform;
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
            while (noteArray[i][noteArrayCount[i]].timing <= currentTicks)
            {
                soundManager.PlayKeySound(noteArray[i][noteArrayCount[i]].keySound);
                HandleNote(i, currentTicks);
            }
        }*/
    }

    public void KeyDown(int index)
    {
        long keyDownTime = stopwatch.ElapsedTicks - resumeCountTicks;
        lock (threadLock)
        {
            if (isClear) { soundManager.PlayKeySoundEnd(currentKeySound[index]); }
            else { soundManager.PlayKeySound(currentKeySound[index]); }
            isKeyDown[index] = true;
            HandleNote(index, keyDownTime);
        }
    }

    public void KeyUp(int index)
    {
        replayNoteDataList[index].Add(new ReplayNoteData(stopwatch.ElapsedTicks - resumeCountTicks, 0, 1));
        isKeyDown[index] = true;
    }

    private void UpdateScoreGraph(float score, int index)
    {
        double under60 = height60;
        double up60 = 0.0d;
        if (score <= 600000.0d) { under60 = score * divide20000; }
        else { up60 = (score - 600000.0d) * divide6250; }
        float scoreStickHeight = (float)(under60 + up60);
        bmsResult.scoreGraphData.scoreGraphList[index] = scoreStickHeight;
        isScoreGraphUpdate = true;
    }

    private void UpdateRank(float score)
    {
        switch (score)
        {
            case float n when (n >= 0.0f && n < 550000.0f): bmsResult.resultData.rankIndex = 0; break;
            case float n when (n >= 550000.0f && n < 650000.0f): bmsResult.resultData.rankIndex = 1; break;
            case float n when (n >= 650000.0f && n < 750000.0f): bmsResult.resultData.rankIndex = 2; break;
            case float n when (n >= 750000.0f && n < 850000.0f): bmsResult.resultData.rankIndex = 3; break;
            case float n when (n >= 850000.0f && n < 900000.0f): bmsResult.resultData.rankIndex = 4; break;
            case float n when (n >= 900000.0f && n < 950000.0f): bmsResult.resultData.rankIndex = 5; break;
            case float n when (n >= 950000.0f && n < 1000000.0f): bmsResult.resultData.rankIndex = 6; break;
            case float n when (n >= 1000000.0f && n < 1025000.0f): bmsResult.resultData.rankIndex = 7; break;
            case float n when (n >= 1025000.0f && n < 1050000.0f): bmsResult.resultData.rankIndex = 8; break;
            case float n when (n >= 1050000.0f && n < 1090000.0f): bmsResult.resultData.rankIndex = 9; break;
            case float n when (n >= 1090000.0f): bmsResult.resultData.rankIndex = 10; break;
        }
        if (currentRankIndex != bmsResult.resultData.rankIndex)
        {
            currentRankIndex = bmsResult.resultData.rankIndex;
            isChangeRankImage = true;
        }
    }

    public void UpdateResult(JudgeType judge)
    {
        switch (judge)
        {
            case JudgeType.KOOL:
                bmsResult.resultData.koolCount++; 
                accuracySum += 100;
                currentScore += koolAddScore;
                gauge.hp += gauge.koolHealAmount;
                break;
            case JudgeType.COOL:
                bmsResult.resultData.coolCount++; 
                accuracySum += 70;
                currentScore += coolAddScore;
                gauge.hp += gauge.coolHealAmount;
                break;
            case JudgeType.GOOD:
                bmsResult.resultData.goodCount++; 
                accuracySum += 20;
                currentScore += goodAddScore;
                gauge.hp += gauge.goodHealAmount;
                break;
            case JudgeType.MISS:
                bmsResult.resultData.missCount++;
                gauge.hp -= gauge.missDamage;
                break;
            case JudgeType.FAIL:
                bmsResult.resultData.failCount++;
                gauge.hp -= gauge.failDamage;
                break;
        }
        isJudgementTrackerUpdate = true;

        combo++;
        if (bmsResult.resultData.maxCombo <= combo)
        {
            bmsResult.resultData.maxCombo = combo;
        }
        isGameUIUpdate = true;

        UpdateScoreGraph((float)currentScore, currentCount);
        UpdateRank((float)currentScore);

        if (gauge.hp > 1.0f) { gauge.hp = 1.0f; }
        else if (gauge.hp <= 0.0f) 
        {
            isGameOver = true;
            return;
        }

        if (currentCount >= pattern.noteCount)
        {
            isGameEnd = true;
            isPaused = true;
            bmsResult.resultData.score = currentScore + bmsResult.resultData.maxCombo;
            endCount++;
            UpdateScoreGraph((float)bmsResult.resultData.score, currentCount + 1);
            UpdateRank((float)bmsResult.resultData.score);
        }
    }

    public void GamePause(int set)
    {
        lock (threadLock)
        {
            if (isPaused) { return; }
            if (set == 0 && !isReplay)
            {
                replayNoteDataList[0].Add(new ReplayNoteData(stopwatch.ElapsedTicks - resumeCountTicks, 0, 4));
            }
            isPaused = true;
            gameUIManager.AnimationPause(true);
            stopwatch.Stop();
            soundManager.AudioPause(true);
            InputThreadAbort();
            if (bgmThread.IsAlive)
            {
                bgmThread.Abort();
            }
            if (keySoundChangeThread.IsAlive)
            {
                keySoundChangeThread.Abort();
            }
            if (isBGAVideoSupported) { videoPlayer.Pause(); }
            pauseManager.PausePanelSetting(set);
            pauseManager.Pause_SetActive(true);
            gameUIManager.InfoPanelUpdateCoroutine(false);
            gameUIManager.SetActiveInfoPanel(true);
            gameUIManager.SetNoteSpeedText();
        }
    }

    private async UniTask SongEndCheck()
    {
        isSongEndChecking = true;
        while (isSongEndChecking)
        {
            if (((!isBGAVideoSupported && bgaChangeArrayCount == 0) || (isBGAVideoSupported && !videoPlayer.isPlaying)) && bgSoundArrayCount < 0)
            {
                _ = GameEnd(true);
                break;
            }
            await UniTask.Yield();
        }
    }

    public async UniTask GameEnd(bool clear)
    {
        pauseManager.Pause_SetActive(false);
        gameUIManager.SetActiveInfoPanel(false);
        keyInput.KeyDisable();
        isPaused = true;
        isClear = clear;

        if (isClear)
        {
            float checkTime = 0.0f;
            while (soundManager.IsPlayingAudio(checkTime += Time.deltaTime))
            {
                await UniTask.Yield();
            }
        }
        else
        {
            if (bgmThread.IsAlive)
            {
                bgmThread.Abort();
            }
            if (keySoundChangeThread.IsAlive)
            {
                keySoundChangeThread.Abort();
            }
            soundManager.AudioAllStop();
        }

        for (int i = bmsResult.judgeList.Length - 1; i >= 1; i--) { bmsResult.judgeList[i] *= 0.0001d; }
        bmsResult.resultData.score = currentScore + bmsResult.resultData.maxCombo;
        bmsResult.resultData.accuracy = bmsResult.resultData.score / (1100000.0d + pattern.noteCount);

        if (isClear) { await UniTask.Delay(1000); }
        _ = LoadScene(3);
    }

    public async UniTask LoadScene(int scene)
    {
        noteBomb.NoteBombLEffectOff();
        gameUIManager.AnimationPause(false);
        pauseManager.Pause_SetActive(false);
        gameUIManager.SetActiveInfoPanel(false);
        gameUIManager.FadeIn();
        await UniTask.Delay(1000);
        ReturnAllNotes();
        ThreadDestroy();
        gameUIManager.BGATextureDestroy();
        gameUIManager.SkinTextureDestroy();
        SaveReplayData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void GameResume()
    {
        int tickOffset = isReplay ? 0 : 30000000;
        for (int i = 0; i < bgSoundArray.Length; i++)
        {
            bgSoundArray[i].timing += tickOffset;
        }
        bgmThread = new Thread(BGMPlayThread);
        if (bgSoundArrayCount >= 0)
        {
            bgmThread.Start();
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < keySoundChangeArray[i].Length; j++)
            {
                keySoundChangeArray[i][j].timing += tickOffset;
            }
        }
        keySoundChangeThread = new Thread(KeySoundChangeThread);
        keySoundChangeThread.Start();
        combo = isReplay ? combo : 0;
        inputBlockLine.localPosition = new Vector3(xPosition[2], (float)(currentBeat * gameSpeed), 0.0f);
        resumeTicks = currentTicks;
        resumeCountTicks += tickOffset;
        currentTicks = stopwatch.ElapsedTicks - resumeCountTicks;
        pauseManager.Pause_SetActive(false);
        gameUIManager.SetActiveInfoPanel(false);
        stopwatch.Start();
        _ = CountDown3();
        Thread bgmResumeThread = new Thread(ResumeBGM);
        bgmResumeThread.Start();
    }

    private async UniTask CountDown3()
    {
        isCountdown = true;
        gameUIManager.countdown.SetActiveCountdown(true);
        while (true)
        {
            long countdown = resumeTicks - currentTicks;
            if (countdown <= 0)
            {
                gameUIManager.countdown.SetActiveCountdown(false);
                gameUIManager.AnimationPause(false);
                InputThreadStart();
                if (isBGAVideoSupported) { videoPlayer.Play(); }
                isPaused = false;
                isCountdown = false;
                if (!isReplay) { comboScore.ScoreComboUpdate(0, bmsResult.resultData.maxCombo, (int)(float)currentScore); }
                break;
            }
            else
            {
                float countdownMS = countdown * 0.0000001f;
                int second = (int)countdownMS;
                gameUIManager.countdown.SetCountdownProcessivity(countdownMS - second, second);
            }
            await UniTask.Yield();
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

    private void MakeTable()
    {
        int len = pattern.noteCount + 1;
        divideTable = new double[len];
        for (int i = 1; i < len; i++) { divideTable[i] = 1.0d / i; }
    }

    private void SetDisplayDelayCorrection()
    {
        if (isReplay) { return; }
        double displayDelayCorrection = PlayerPrefs.GetInt("DisplayDelayCorrection") * 0.001d;
        for (int i = 0; i < 5; i++)
        {
            for (int j = normalNoteArrayCount[i] - 1; j >= 0; j--)
            {
                normalNoteArray[i][j].beat = pattern.GetBeatFromTiming(normalNoteArray[i][j].timing * 0.0000001d - displayDelayCorrection);
            }

            for (int j = longNoteArrayCount[i] - 1; j >= 0; j--)
            {
                if (longNoteArray[i][j].extra == 2) { continue; }
                longNoteArray[i][j].beat = pattern.GetBeatFromTiming(longNoteArray[i][j].timing * 0.0000001d - displayDelayCorrection);
            }
        }
        for (int i = barArrayCount; i >= 0; i--)
        {
            barArray[i].beat = pattern.GetBeatFromTiming(barArray[i].timing - 0.175d - displayDelayCorrection);
        }
    }

    public void ChangeDisplayDelayCorrection(int value)
    {
        if (isReplay) { return; }
        lock (threadLock)
        {
            int displayDelayCorrectionValue = PlayerPrefs.GetInt("DisplayDelayCorrection");
            displayDelayCorrectionValue += value;
            if (displayDelayCorrectionValue > 100) { userSpeed = 100; }
            else if (displayDelayCorrectionValue < -100) { displayDelayCorrectionValue = -100; }
            PlayerPrefs.SetInt("DisplayDelayCorrection", displayDelayCorrectionValue);

            SetDisplayDelayCorrection();
            RedrawNote();
            gameUIManager.DisplayDelayCorrectionTextUpdateCoroutine();
        }
    }

    public void ChangeSpeed(int value)
    {
        lock (threadLock)
        {
            userSpeed += value;
            if (userSpeed > 200) { userSpeed = 200; }
            else if (userSpeed < 10) { userSpeed = 10; }
            PlayerPrefs.SetInt("NoteSpeed", userSpeed);
            if (pauseManager.enabled) { gameUIManager.SetNoteSpeedText(); }
            else { gameUIManager.InfoPanelUpdateCoroutine(true); }
            gameSpeed = ConvertSpeed();

            RedrawNote();
            RedrawVerticalLine();
            noteParent.position = new Vector3(0.0f, (float)(-currentBeat * gameSpeed), 0.0f);
        }

        if (!isReplay) { return; }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < replayNoteArray[i].Length; j++)
            {
                if (replayNoteArray[i][j].extra != 0 && replayNoteArray[i][j].extra != 5) { continue; }
                replayNoteArray[i][j].model.transform.localPosition = new Vector3(xPosition[i], (float)(replayNoteArray[i][j].beat * gameSpeed), 0.0f);
            }
        }
    }

    private void RedrawNote()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = normalNoteArrayCount[i] - 1; j >= 0; j--)
            {
                if (normalNoteArray[i][j].model == null) { break; }

                normalNoteArray[i][j].modelTransform.localPosition = new Vector3(xPosition[i], (float)(normalNoteArray[i][j].beat * gameSpeed), 0.0f);
            }

            bool isCurrent = true;
            for (int j = longNoteArrayCount[i] - 1; j >= 0; j -= 3)
            {
                if (longNoteArray[i][j].model == null) { break; }

                for (int k = 0; k < 3; k++)
                {
                    float yPos;
                    if (k == 2)
                    {
                        if (isCurrentLongNote[i] && isCurrent) { yPos = (float)currentBeat * gameSpeed; }
                        else { yPos = (float)longNoteArray[i][j - 2].beat * gameSpeed; }
                    }
                    else { yPos = (float)longNoteArray[i][j].beat * gameSpeed; }

                    longNoteArray[i][j - k].modelTransform.localPosition = new Vector3(xPosition[i], yPos, 0.0f);

                    if (k == 1)
                    {
                        float scale;
                        if (isCurrentLongNote[i] && isCurrent) { scale = ((float)(longNoteArray[i][j].beat - currentBeat) * gameSpeed - longNoteOffset) * longNoteLength; }
                        else { scale = ((float)longNoteArray[i][j - 1].beat * gameSpeed - longNoteOffset) * longNoteLength; }
                        longNoteArray[i][j - k].modelTransform.localScale = new Vector3(1.0f, scale, 1.0f);
                    }
                }
                isCurrent = false;
            }
        }
        for (int i = barArrayCount; i >= 0; i--)
        {
            if (barArray[i].model == null) { break; }

            barArray[i].modelTransform.localPosition = new Vector3(xPosition[2], (float)(barArray[i].beat * gameSpeed), 0.0f);
        }
    }

    public void RedrawVerticalLine()
    {
        float verticalLineLength = verticalLine * userSpeed;
        for (int i = 0; i < 5; i++)
        {
            for (int j = normalNoteArrayCount[i] - 1; j >= 0; j--)
            {
                if (normalNoteArray[i][j].model == null) { break; }

                normalNoteArray[i][j].modelTransform.GetChild(0).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                normalNoteArray[i][j].modelTransform.GetChild(1).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
            }

            for (int j = longNoteArrayCount[i] - 1; j >= 0; j--)
            {
                if (longNoteArray[i][j].model == null) { break; }
                if (longNoteArray[i][j].extra == 2) { continue; }
                longNoteArray[i][j].modelTransform.GetChild(0).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                longNoteArray[i][j].modelTransform.GetChild(1).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
            }
        }
    }

    public void ChangeBGAOpacity(int value)
    {
        int opacity = PlayerPrefs.GetInt("BGAOpacity") + value;
        if (opacity > 10) { opacity = 10; }
        else if (opacity < 0) { opacity = 0; }
        PlayerPrefs.SetInt("BGAOpacity", opacity);
        if (gameUIManager.bga.gameObject.activeSelf || gameUIManager.layer.gameObject.activeSelf)
            gameUIManager.bgaOpacity.color = new Color(0, 0, 0, (10 - PlayerPrefs.GetInt("BGAOpacity")) * 0.1f);

        if (pauseManager.enabled) { gameUIManager.SetBGAOpacityText(); }
        else { gameUIManager.InfoPanelUpdateCoroutine(true); }
    }

    private void SetReplayNoteArray()
    {
        if (!isReplay) { return; }
        for (int i = 0; i < 5; i++)
        {
            replayNoteArray[i] = new ReplayNote[replayData.replayNoteList[i].Count];
            replayNoteArrayCount[i] = replayData.replayNoteList[i].Count - 1;
            for (int j = 0; j < replayNoteArray[i].Length; j++)
            {
                replayNoteArray[i][j] = new ReplayNote(replayData.replayNoteList[i][j]);
                replayNoteArray[i][j].beat = pattern.GetBeatFromTiming(replayNoteArray[i][j].timing * 0.0000001d);
            }
        }
    }

    private void SaveReplayNoteList()
    {
        if (isReplay) { return; }
        for (int i = 0; i < 5; i++)
        {
            replayData.noteList[i] = new List<AbstractNote>(pattern.lines[i].noteList.Count);
            for (int j = 0; j < pattern.lines[i].noteList.Count; j++)
            {
                replayData.noteList[i].Add(new AbstractNote(pattern.lines[i].noteList[j]));
            }
        }
    }

    private void SaveReplayData()
    {
        if (isReplay) { return; }
        replayData.randomEffector = PlayerPrefs.GetInt("RandomEffector");
        replayData.score = bmsResult.resultData.score;
        replayData.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        for (int i = 0; i < 5; i++)
        {
            replayNoteDataList[i].Reverse();
        }
        replayData.replayNoteList = replayNoteDataList;
    }

    public void ReplayNoteSetActive()
    {
        replayNoteParent.SetActive(!replayNoteParent.activeSelf);
    }

    private void InputThreadStart()
    {
        if (isReplay)
        {
            replayInputThread = new Thread(ReplayInputCheck);
            replayInputThread.Start();
        }
        else
        {
            keyInput.InputThreadStart();
        }
    }

    private void InputThreadAbort()
    {
        if (isReplay)
        {
            if (replayInputThread != null && replayInputThread.IsAlive)
            {
                replayInputThread.Abort();
            }
        }
        else
        {
            keyInput.InputThreadAbort();
        }
    }

    private void ThreadDestroy()
    {
        keyInput.KeyDisable();
        InputThreadAbort();
        if (bgmThread.IsAlive)
        {
            bgmThread.Abort();
        }
        if (keySoundChangeThread.IsAlive)
        {
            keySoundChangeThread.Abort();
        }
        soundManager.SoundAndChannelRelease();
    }

    private void OnDestroy()
    {
        ThreadDestroy();
    }
}
