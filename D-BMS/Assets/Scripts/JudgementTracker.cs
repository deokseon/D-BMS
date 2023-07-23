using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class JudgementTracker : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private BMSResult bmsResult = null;

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
    private TextMeshProUGUI frontAccuracyText;
    [SerializeField]
    private TextMeshProUGUI backAccuracyText;
    [SerializeField]
    private GameObject[] judgementTrackerObjects;

    private string[] str0000to9999Table;
    private string[] str00to100Table;

    void Awake()
    {
        if (PlayerPrefs.GetInt("JudgementTracker") == 0)
        {
            JudgementTrackerInactivate();
        }
        else
        {
            bmsGameManager = FindObjectOfType<BMSGameManager>();
            bmsResult = BMSGameManager.bmsResult;

            str0000to9999Table = new string[10000];
            for (int i = 0; i < 10000; i++) { str0000to9999Table[i] = i.ToString("D4"); }

            str00to100Table = new string[101];
            for (int i = 0; i < 100; i++) { str00to100Table[i] = i.ToString("D2"); }
            str00to100Table[100] = "100";
        }
    }

    void Update()
    {
        lock (bmsGameManager.inputHandleLock)
        {
            if (!bmsGameManager.isJudgementTrackerUpdate) return;

            koolText.text = str0000to9999Table[bmsResult.koolCount];
            coolText.text = str0000to9999Table[bmsResult.coolCount];
            goodText.text = str0000to9999Table[bmsResult.goodCount];
            missText.text = str0000to9999Table[bmsResult.missCount];
            failText.text = str0000to9999Table[bmsResult.failCount];

            int currentCount = bmsGameManager.currentCount;
            float accuracy = (float)(bmsGameManager.accuracySum * bmsGameManager.divideTable[currentCount]);
            int frontAC = (int)(accuracy);
            int backAC = (int)((accuracy - frontAC) * 100.0d);
            frontAccuracyText.text = str00to100Table[frontAC];
            backAccuracyText.text = str00to100Table[backAC];

            bmsGameManager.isJudgementTrackerUpdate = false;
        }
    }

    private void JudgementTrackerInactivate()
    {
        for (int i = 0; i < judgementTrackerObjects.Length; i++)
        {
            judgementTrackerObjects[i].SetActive(false);
        }
        koolText.gameObject.SetActive(false);
        coolText.gameObject.SetActive(false);
        goodText.gameObject.SetActive(false);
        missText.gameObject.SetActive(false);
        failText.gameObject.SetActive(false);
        frontAccuracyText.gameObject.SetActive(false);
        backAccuracyText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
