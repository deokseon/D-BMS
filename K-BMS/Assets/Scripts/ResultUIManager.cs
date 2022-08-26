﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using B83.Image.BMP;

public class ResultUIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI totalnotesText;
    [SerializeField]
    private TextMeshProUGUI koolText;
    [SerializeField]
    private TextMeshProUGUI coolText;
    [SerializeField]
    private TextMeshProUGUI goodText;
    [SerializeField]
    private TextMeshProUGUI missText;
    [SerializeField]
    private TextMeshProUGUI failText;
    [SerializeField]
    private TextMeshProUGUI accuracyText;
    [SerializeField]
    private TextMeshProUGUI maxComboText;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI noteSpeedText;
    [SerializeField]
    private TextMeshProUGUI randomEffectorText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private Image rankImage;
    [SerializeField]
    private Sprite[] rankImageArray;
    [SerializeField]
    private GameObject playLamp;
    [SerializeField]
    private GameObject clearLamp;
    [SerializeField]
    private GameObject nomissLamp;
    [SerializeField]
    private GameObject allcoolLamp;
    [SerializeField]
    private Transform dotParent;
    [SerializeField]
    private GameObject dot;
    [SerializeField]
    private TextMeshProUGUI averageInputTimingText;
    [SerializeField]
    private RawImage banner;
    [SerializeField]
    private Texture noBannerTexture;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI artistText;
    [SerializeField]
    private TextMeshProUGUI bpmText;

    private BMPLoader loader;

    public void Awake()
    {
        Resources.UnloadUnusedAssets();

        loader = new BMPLoader(); 

        DrawStatisticsResult();
        DrawJudgeGraph();
        DrawSongInfo();
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void DrawStatisticsResult()
    {
        totalnotesText.text = BMSGameManager.bmsResult.noteCount.ToString();
        koolText.text = BMSGameManager.bmsResult.koolCount.ToString();
        coolText.text = BMSGameManager.bmsResult.coolCount.ToString();
        goodText.text = BMSGameManager.bmsResult.goodCount.ToString();
        missText.text = BMSGameManager.bmsResult.missCount.ToString();
        failText.text = BMSGameManager.bmsResult.failCount.ToString();
        accuracyText.text = BMSGameManager.bmsResult.accuracy.ToString("P");
        maxComboText.text = BMSGameManager.bmsResult.maxCombo.ToString();
        scoreText.text = ((int)BMSGameManager.bmsResult.score).ToString();
        noteSpeedText.text = BMSGameManager.userSpeed.ToString();
        randomEffectorText.text = BMSGameManager.randomEffector.ToString();
        levelText.text = BMSGameManager.header.level.ToString();

        int idx = 0;
        int score = ((int)BMSGameManager.bmsResult.score);
        if (score >= 1090000) { idx = 0; }  // S+++
        else if (score >= 1050000 && score < 1090000) { idx = 1; }  // S++
        else if (score >= 1025000 && score < 1050000) { idx = 2; }  // S+
        else if (score >= 1000000 && score < 1025000) { idx = 3; }  // S
        else if (score >= 950000 && score < 1000000) { idx = 4; }   // A+
        else if (score >= 900000 && score < 950000) { idx = 5; }    // A
        else if (score >= 850000 && score < 900000) { idx = 6; }    // B
        else if (score >= 750000 && score < 850000) { idx = 7; }    // C
        else if (score >= 650000 && score < 750000) { idx = 8; }    // D
        else if (score >= 550000 && score < 650000) { idx = 9; }    // E
        else { idx = 10; }  // F
        rankImage.sprite = rankImageArray[idx];

        if (!BMSGameManager.isClear) 
        { 
            playLamp.SetActive(true);
        }
        else if (BMSGameManager.bmsResult.missCount > 0 || 
                 BMSGameManager.bmsResult.failCount > 0) 
        { 
            clearLamp.SetActive(true);
        }
        else if (BMSGameManager.bmsResult.goodCount > 0)
        { 
            nomissLamp.SetActive(true);
        }
        else 
        { 
            allcoolLamp.SetActive(true);
        }
    }

    private void DrawJudgeGraph()
    {
        int len = BMSGameManager.bmsResult.judgeList.Count;
        double divideNoteCount = 1.0d / BMSGameManager.bmsResult.noteCount;
        double total = 0;
        for (int i = 0; i < len; i++)
        {
            double y = BMSGameManager.bmsResult.judgeList[i].Value;
            total += y;
            if (Utility.Dabs(y) > 115) { continue; }
            double x = (BMSGameManager.bmsResult.judgeList[i].Key * divideNoteCount * 600) - 300;

            GameObject tempDot = Instantiate(dot, dotParent);
            tempDot.transform.localPosition = new Vector3((float)x, (float)y * 2, 0.0f);
        }
        averageInputTimingText.text = $"{(int)(total * divideNoteCount)} MS";
    }

    private void DrawSongInfo()
    {
        if (string.IsNullOrEmpty(BMSGameManager.header.bannerPath)) { banner.texture = noBannerTexture; }
        else
        {
            Texture tex = null;
            tex = (BMSGameManager.header.bannerPath.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase) ?
                    loader.LoadBMP(BMSGameManager.header.bannerPath).ToTexture2D() : Resources.Load<Texture>(BMSGameManager.header.bannerPath));

            banner.texture = (tex != null ? tex : noBannerTexture);
        }

        titleText.text = BMSGameManager.header.title;
        artistText.text = BMSGameManager.header.artist;
        if (BMSGameManager.header.minBPM == BMSGameManager.header.maxBPM) { bpmText.text = "BPM: " + BMSGameManager.header.bpm.ToString(); }
        else { bpmText.text = "BPM: " + BMSGameManager.header.minBPM.ToString() + " ~ " + BMSGameManager.header.maxBPM.ToString(); }
    }
}
