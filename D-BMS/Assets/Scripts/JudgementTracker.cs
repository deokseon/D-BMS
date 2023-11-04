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
    private TextMeshProUGUI dotText;
    [SerializeField]
    private TextMeshProUGUI percentText;
    [SerializeField]
    private GameObject[] judgementTrackerObjects;

    private string[] str0to9999Table;
    private string[] str00to100Table;

    void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        if (bmsGameManager != null)
        {
            if (PlayerPrefs.GetInt("JudgementTracker") == 0)
            {
                JudgementTrackerInactivate();
                return;
            }

            bmsResult = BMSGameManager.bmsResult;
            str0to9999Table = new string[10000];
            for (int i = 0; i < 10000; i++) { str0to9999Table[i] = i.ToString(); }

            str00to100Table = new string[101];
            for (int i = 0; i < 100; i++) { str00to100Table[i] = i.ToString("D2"); }
            str00to100Table[100] = "100";

            StartCoroutine(CheckJudgementUpdate());
        }
        AccuracyTextPositionSet();
        SetJudgementTrackerPosition(GameUIManager.config.judgementTrackerPositionOffsetX, GameUIManager.config.judgementTrackerPositionOffsetY);
    }

    private IEnumerator CheckJudgementUpdate()
    {
        while (true)
        {
            lock (bmsGameManager.inputHandleLock)
            {
                if (bmsGameManager.isJudgementTrackerUpdate)
                {
                    koolText.text = str0to9999Table[bmsResult.resultData.koolCount];
                    coolText.text = str0to9999Table[bmsResult.resultData.coolCount];
                    goodText.text = str0to9999Table[bmsResult.resultData.goodCount];
                    missText.text = str0to9999Table[bmsResult.resultData.missCount];
                    failText.text = str0to9999Table[bmsResult.resultData.failCount];

                    int currentCount = bmsGameManager.currentCount;
                    float accuracy = (float)(bmsGameManager.accuracySum * bmsGameManager.divideTable[currentCount]);
                    int frontAC = (int)accuracy;
                    int backAC = (int)((accuracy - frontAC) * 100.0d);
                    frontAccuracyText.text = str00to100Table[frontAC];
                    backAccuracyText.text = str00to100Table[backAC];

                    bmsGameManager.isJudgementTrackerUpdate = false;
                }
            }
            yield return null;
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

    private void AccuracyTextPositionSet()
    {
        backAccuracyText.rectTransform.localPosition = new Vector3(percentText.rectTransform.localPosition.x - percentText.preferredWidth,
                                                                   percentText.rectTransform.localPosition.y, 0.0f);
        dotText.rectTransform.localPosition = new Vector3(backAccuracyText.rectTransform.localPosition.x - backAccuracyText.preferredWidth,
                                                                   percentText.rectTransform.localPosition.y, 0.0f);
        frontAccuracyText.rectTransform.localPosition = new Vector3(dotText.rectTransform.localPosition.x - dotText.preferredWidth,
                                                                   percentText.rectTransform.localPosition.y, 0.0f);
    }

    public void SetJudgementTrackerPosition(float offsetX, float offsetY)
    {
        for (int i = 0; i < judgementTrackerObjects.Length; i++)
        {
            RectTransform objectRectTr = judgementTrackerObjects[i].GetComponent<RectTransform>();
            objectRectTr.localPosition = new Vector3(objectRectTr.localPosition.x + offsetX, objectRectTr.localPosition.y + offsetY, 0.0f);
        }
        koolText.rectTransform.localPosition = new Vector3(koolText.rectTransform.localPosition.x + offsetX,
                                                           koolText.rectTransform.localPosition.y + offsetY, 0.0f);
        coolText.rectTransform.localPosition = new Vector3(coolText.rectTransform.localPosition.x + offsetX,
                                                           coolText.rectTransform.localPosition.y + offsetY, 0.0f);
        goodText.rectTransform.localPosition = new Vector3(goodText.rectTransform.localPosition.x + offsetX,
                                                           goodText.rectTransform.localPosition.y + offsetY, 0.0f);
        missText.rectTransform.localPosition = new Vector3(missText.rectTransform.localPosition.x + offsetX,
                                                           missText.rectTransform.localPosition.y + offsetY, 0.0f);
        failText.rectTransform.localPosition = new Vector3(failText.rectTransform.localPosition.x + offsetX,
                                                           failText.rectTransform.localPosition.y + offsetY, 0.0f);
        frontAccuracyText.rectTransform.localPosition = new Vector3(frontAccuracyText.rectTransform.localPosition.x + offsetX,
                                                                    frontAccuracyText.rectTransform.localPosition.y + offsetY, 0.0f);
        backAccuracyText.rectTransform.localPosition = new Vector3(backAccuracyText.rectTransform.localPosition.x + offsetX,
                                                                   backAccuracyText.rectTransform.localPosition.y + offsetY, 0.0f);
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(gameObject.GetComponent<RectTransform>().localPosition.x + offsetX,
                                                                             gameObject.GetComponent<RectTransform>().localPosition.y + offsetY, 0.0f);
    }
}
