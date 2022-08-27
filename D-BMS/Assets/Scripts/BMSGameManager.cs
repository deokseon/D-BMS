using System.Collections;
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
    public double scroll;

    [SerializeField]
    private BMSDrawer bmsDrawer;
    [SerializeField]
    private GameUIManager gameUIManager;
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField]
    private KeyInput keyInput;
    [SerializeField]
    private Transform noteParent;
    [SerializeField]
    private Animator fadeinAnimator;

    private double currentBeat = 0;
    private double currentScrollTime = 0;
    private double currentTime = 0;
    private double accuracySum = 0;
    private int currentCount = 0;

    private double divide60 = 1.0d / 60.0d;
    private double divide44100 = 1.0d / 44100.0d;

    public static BMSHeader header;
    private Gauge gauge;
    private JudgeManager judge;
    private BMSPattern pattern;
    private SoundManager soundManager;
    private WaitForSeconds wait3Sec;
    private double currentBPM;
    private int combo = 0;
    private bool isBGAVideoSupported = false;

    public int[] currentNote;

    private AudioSource timeSampleAudio;

    private double koolAddScore;
    private double coolAddScore;
    private double goodAddScore;
    private double currentScore;

    [SerializeField]
    private GameObject[] longNotePress;

    private JudgeType currentLongNoteJudge;
    private bool[] currentButtonPressed = { false, false, false, false, false };

    private int notePoolMaxCount;
    private int barPoolMaxCount;

    public void CalulateSpeed()
    {
        gameSpeed = (userSpeed * 120.3f / (float)header.bpm);
    }

    private IEnumerator PreLoad()
    {
        ObjectPool.poolInstance.Init();

        BMSParser.instance.Parse();
        pattern = BMSParser.instance.pattern;

        pattern.GetBeatsAndTimings();
        for (int i = 0; i < 5; i++)
        {
            if (pattern.lines[i].noteList.Count > 0) { currentNote[i] = pattern.lines[i].noteList.Peek.keySound; }
        }

        gauge = new Gauge();
        gameUIManager.UpdateScore(bmsResult, 0.0f, 0.0d, 0.0d, 0.0d);

        gameUIManager.LoadImages();
        soundManager.AddAudioClips();

        CalulateSpeed();
        bmsDrawer.DrawNotes();

        keyInput.KeySetting();
        
        currentBPM = pattern.bpms.Peek.bpm;
        pattern.bpms.RemoveLast();
        gameUIManager.UpdateBPMText(currentBPM);

        bmsResult.noteCount = pattern.noteCount;
        bmsResult.judgeList = new List<KeyValuePair<int, double>>(bmsResult.noteCount);

        koolAddScore = 1100000.0d / pattern.noteCount;
        coolAddScore = koolAddScore * 0.7d;
        goodAddScore = koolAddScore * 0.2d;

        if (videoPlayer.isActiveAndEnabled)
        {
            for (int i = pattern.bgaChanges.Count - 1; i > -1; i--)
            {
                if (!pattern.bgaChanges[i].isPic)
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
                isBGAVideoSupported = !errorFlag;
            }
        }
        yield return new WaitUntil(() => gameUIManager.isPrepared);
        yield return new WaitUntil(() => soundManager.isPrepared);

        currentTime += (judgeAdjValue * 0.001f);

        gameUIManager.bga.texture = videoPlayer.texture;
        gameUIManager.bga.color = Color.white;

        Time.fixedDeltaTime = 0.001f;

        StartCoroutine(SongEndCheck());
        StartCoroutine(TimerStart());

        System.GC.Collect();

        isPaused = false;
    }

    private void Awake()
    {
        isPaused = true;
        isClear = true;
        pattern = BMSParser.instance.pattern;
        judge = JudgeManager.instance;
        bmsResult = new BMSResult();
        soundManager = GetComponent<SoundManager>();

        header = BMSFileSystem.selectedHeader;
        BMSFileSystem.selectedHeader = null;

        gameUIManager.UpdateInfoText();

        currentNote = new int[5] { 0, 0, 0, 0, 0 };

        currentScore = 0.0d;

        notePoolMaxCount = ObjectPool.poolInstance.maxNoteCount;
        barPoolMaxCount = ObjectPool.poolInstance.maxBarCount;

        timeSampleAudio = GetComponent<AudioSource>();

        wait3Sec = new WaitForSeconds(3.0f);

        gameUIManager.bga.color = new Color(1, 1, 1, 0);

        StartCoroutine(PreLoad());
    }

    private void Update()
    {
        if (isPaused) { return; }

        while (pattern.bgaChanges.Count > 0 && 
            pattern.bgaChanges.Peek.timing - ((!pattern.bgaChanges.Peek.isPic) ? 0.4 : 0) <= currentTime)
        {
            if (isBGAVideoSupported && !pattern.bgaChanges.Peek.isPic) { videoPlayer.Play(); }
            else { gameUIManager.ChangeBGA(pattern.bgaChanges.Peek.key); }
            pattern.bgaChanges.RemoveLast();
        }

        double frameTime = timeSampleAudio.timeSamples * divide44100 - currentTime;
        PlayNotes();
        currentTime += frameTime;

        double avg = currentBPM * frameTime;

        BPM next = null;
        bool flag = false;

        if (pattern.bpms.Count > 0)
        {
            BPM bpm = pattern.bpms.Peek;
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
            pattern.bpms.RemoveLast();

            next = null;
            if (pattern.bpms.Count > 0) { next = pattern.bpms.Peek; }
        }

        if (flag && prevTime <= currentScrollTime + frameTime)
        {
            avg += currentBPM * (currentScrollTime + frameTime - prevTime);
        }

        avg *= divide60;
        currentBeat += avg;
        currentScrollTime += frameTime;
        scroll += avg * gameSpeed;
        noteParent.transform.position = new Vector3(0.0f, (float)-scroll, 0.0f);
    }

    private void HandleNote(Line l, int idx)
    {
        Note n = l.noteList.Peek;
        currentCount++;
        l.noteList.RemoveLast();

        int len = l.noteList.Count;
        if (len > 0) { currentNote[idx] = l.noteList.Peek.keySound; }

        bool nextCheck = true;
        if ((len > 0 && l.noteList.Peek.extra != 2) || (len == 0))
        {
            int k = (len > 0 ? FindIndex(l, len) : -1);
            if (k != -1)
            {
                int tempPeek = len - (notePoolMaxCount + k);
                l.noteList[tempPeek].model = n.model;
                l.noteList[tempPeek].model.transform.localPosition =
                    new Vector3(-7.7f + 0.57f * idx, (float)(l.noteList[tempPeek].beat * gameSpeed), 0.0f);
            }
            else 
            { 
                n.model.SetActive(false);
                ObjectPool.poolInstance.ReturnNoteInPool(idx, n.model);
            }
            n.model = null;
            nextCheck = false;
        }

        JudgeType result = judge.Judge(n, currentTime);
        if (nextCheck)
        { 
            currentLongNoteJudge = result;
            if (currentLongNoteJudge == JudgeType.COOL) { currentLongNoteJudge = JudgeType.KOOL; }
            longNotePress[idx].SetActive(true);
        }

        if (result <= JudgeType.MISS) { combo = -1; }
        gameUIManager.TextUpdate(++combo, result, idx);

        if (result != JudgeType.FAIL) { gameUIManager.UpdateFSText((float)(currentTime - n.timing) * 1000, idx, currentCount); }

        UpdateResult(result);
    }

    private void HandleLongNoteTick(Line l, int idx)
    {
        currentCount++;
        l.noteList.RemoveLast();

        int len = l.noteList.Count;
        if (len > 0) { currentNote[idx] = l.noteList.Peek.keySound; }

        if (!currentButtonPressed[idx]) { currentLongNoteJudge = JudgeType.FAIL; }
        else { if (currentLongNoteJudge == JudgeType.FAIL) { currentLongNoteJudge = JudgeType.GOOD; } }

        JudgeType result = currentLongNoteJudge;

        if (result <= JudgeType.MISS) { combo = -1; }
        gameUIManager.TextUpdate(++combo, result, idx);

        UpdateResult(result);

        if (len > 0) { if (l.noteList.Peek.extra != 2) { longNotePress[idx].SetActive(false); } }
    }

    private void PlayNotes()
    {
        while (pattern.bgSounds.Count > 0 && pattern.bgSounds.Peek.timing <= currentTime)  // 배경음 재생
        {
            soundManager.PlayBGSound(pattern.bgSounds.Peek.keySound);
            pattern.bgSounds.RemoveLast();
        }

        for (int i = 0; i < 5; i++)  // 롱노트 틱 검사
        {
            Line l = pattern.lines[i];
            while (l.noteList.Count > 0 && l.noteList.Peek.extra == 2 && l.noteList.Peek.timing <= currentTime)
            {
                HandleLongNoteTick(l, i);
            }
        }

        for (int i = 0; i < 5; i++)  // 놓친 노트 검사
        {
            Line l = pattern.lines[i];
            while (l.noteList.Count > 0 && judge.Judge(l.noteList.Peek, currentTime) == JudgeType.FAIL)
            {
                HandleNote(l, i);
            }
        }

        while (pattern.barLine.noteList.Count > 0 && pattern.barLine.noteList.Peek.timing + 0.175f <= currentTime)
        {
            Note bar = pattern.barLine.noteList.Peek;
            pattern.barLine.noteList.RemoveLast();
            int len = pattern.barLine.noteList.Count - barPoolMaxCount;
            if (len >= 0)
            {
                pattern.barLine.noteList[len].model = bar.model;
                pattern.barLine.noteList[len].model.transform.localPosition =
                    new Vector3(0.0f, (float)(pattern.barLine.noteList[len].beat * gameSpeed), 0.0f);
            }
            else
            {
                bar.model.SetActive(false);
                ObjectPool.poolInstance.ReturnBarInPool(bar.model);
            }
            bar.model = null;
        }

        // auto
        for (int i = 0; i < 5; i++)
        {
            currentButtonPressed[i] = true;
            Line l = pattern.lines[i];
            while (l.noteList.Count > 0 && l.noteList.Peek.extra != 2 && l.noteList.Peek.timing <= currentTime)
            {
                soundManager.PlayKeySound(l.noteList.Peek.keySound);
                HandleNote(l, i);
            }
        }
    }

    public void KeyDown(int index)
    {
        currentButtonPressed[index] = true;

        if (pattern.lines[index].noteList.Count <= 0 || pattern.lines[index].noteList.Peek.extra == 2 ||
            judge.Judge(pattern.lines[index].noteList.Peek, currentTime) == JudgeType.IGNORE) { return; }

        HandleNote(pattern.lines[index], index);
    }

    public void KeyUp(int index)
    {
        currentButtonPressed[index] = false;
    }

    public void UpdateResult(JudgeType judge)
    {
        float currentHP = gauge.HP;
        switch (judge)
        {
            case JudgeType.KOOL:
                bmsResult.koolCount++; 
                accuracySum += 1.0d;
                currentScore += koolAddScore;
                gauge.HP += gauge.koolHealAmount;
                break;
            case JudgeType.COOL:
                bmsResult.coolCount++; 
                accuracySum += 0.7d;
                currentScore += coolAddScore;
                gauge.HP += gauge.coolHealAmount;
                break;
            case JudgeType.GOOD:
                bmsResult.goodCount++; 
                accuracySum += 0.2d;
                currentScore += goodAddScore;
                gauge.HP += gauge.goodHealAmount;
                break;
            case JudgeType.MISS:
                bmsResult.missCount++;
                gauge.HP -= gauge.missDamage;
                break;
            default:
                bmsResult.failCount++;
                gauge.HP -= gauge.failDamage;
                break;
        }

        gameUIManager.UpdateScore(bmsResult, (gauge.HP - currentHP) * (float)divide60,
            accuracySum / currentCount, currentScore, currentCount * koolAddScore);

        if (currentCount >= pattern.noteCount)
            gameUIManager.UpdateSongEndText(bmsResult.koolCount, bmsResult.coolCount, bmsResult.goodCount);
    }

    private int FindIndex(Line l, int len)
    {
        int k = 0;
        for (int i = 1; i <= notePoolMaxCount + k; i++)
        {
            if (len - i < 0) { return -1; }
            if ((len - i - 1 >= 0 && l.noteList[len - i - 1].extra == 2) || l.noteList[len - i].extra == 2) { k++; }
        }
        return k;
    }

    private IEnumerator TimerStart()
    {
        while (isPaused) { yield return null; }

        timeSampleAudio.Play();
    }

    private IEnumerator SongEndCheck()
    {
        while (true)
        {
            if (currentCount >= pattern.noteCount && 
                ((!isBGAVideoSupported && pattern.bgaChanges.Count == 0) || 
                (isBGAVideoSupported && !videoPlayer.isPlaying)))
            {
                Time.fixedDeltaTime = 0.02f;

                soundManager.DeleteAudioClips();
                gameUIManager.DeleteImages();

                keyInput.KeyDisable();

                bmsResult.accuracy = accuracySum / pattern.noteCount;
                bmsResult.score = currentScore;

                gameUIManager.SaveJudgeList(bmsResult);

                yield return wait3Sec;
                fadeinAnimator.SetTrigger("FadeIn");
                yield return new WaitForSeconds(1.0f);
                UnityEngine.SceneManagement.SceneManager.LoadScene(2);
                yield break;
            }
            yield return wait3Sec;
        }
    }
}
