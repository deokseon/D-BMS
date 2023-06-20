using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class JudgementTracker : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private GameUIManager gameUIManager = null;
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

    void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
        bmsResult = BMSGameManager.bmsResult;

        if (PlayerPrefs.GetInt("JudgementTracker") == 0)
        {
            JudgementTrackerInactivate();
        }
    }

    void Update()
    {
        lock (bmsGameManager.inputHandleLock)
        {
            if (!bmsGameManager.isJudgementTrackerUpdate) return;

            koolText.text = gameUIManager.str0000to9999Table[bmsResult.koolCount];
            coolText.text = gameUIManager.str0000to9999Table[bmsResult.coolCount];
            goodText.text = gameUIManager.str0000to9999Table[bmsResult.goodCount];
            missText.text = gameUIManager.str0000to9999Table[bmsResult.missCount];
            failText.text = gameUIManager.str0000to9999Table[bmsResult.failCount];

            int currentCount = bmsGameManager.currentCount;
            float accuracy = (float)(bmsGameManager.accuracySum * bmsGameManager.divideTable[currentCount]);
            int frontAC = (int)(accuracy);
            int backAC = (int)((accuracy - frontAC) * 100.0d);
            frontAccuracyText.text = gameUIManager.str00to100Table[frontAC];
            backAccuracyText.text = gameUIManager.str00to100Table[backAC];

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
