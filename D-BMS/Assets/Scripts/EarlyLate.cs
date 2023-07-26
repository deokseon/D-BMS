using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EarlyLate : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private GameUIManager gameUIManager = null;
    private BMSResult bmsResult = null;

    [SerializeField]
    private Sprite[] earlyLateImageArray;
    [SerializeField]
    private SpriteRenderer[] earlyLateSprite;
    [SerializeField]
    private Animator[] earlyLateEffectAnimator;
    [SerializeField]
    private SpriteRenderer[] earlyDigitArray;
    [SerializeField]
    private SpriteRenderer[] lateDigitArray;
    [SerializeField]
    private SpriteRenderer[] koolDigitArray;
    [SerializeField]
    private SpriteRenderer[] coolDigitArray;
    [SerializeField]
    private SpriteRenderer[] goodDigitArray;
    [SerializeField]
    private GameObject judgementInfo;
    [SerializeField]
    private Transform earlyDigitParent;
    [SerializeField]
    private Transform lateDigitParent;
    [SerializeField]
    private Transform koolDigitParent;
    [SerializeField]
    private Transform coolDigitParent;
    [SerializeField]
    private Transform goodDigitParent;

    private float[] digitPositionX;

    private readonly int hashEarlyLateEffect = Animator.StringToHash("EarlyLateEffect");

    void Awake()
    {
        if (PlayerPrefs.GetInt("EarlyLate") == 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            bmsGameManager = FindObjectOfType<BMSGameManager>();
            gameUIManager = FindObjectOfType<GameUIManager>();
            bmsResult = BMSGameManager.bmsResult;

            ObjectSetting();
        }
    }

    void Update()
    {
        lock (bmsGameManager.inputHandleLock)
        {
            if (bmsGameManager.fsUpdate[0])
            {
                UpdateFSText(0, bmsGameManager.fsStates[0]);
                bmsGameManager.fsUpdate[0] = false;
            }
            if (bmsGameManager.fsUpdate[1])
            {
                UpdateFSText(1, bmsGameManager.fsStates[1]);
                bmsGameManager.fsUpdate[1] = false;
            }
            if (bmsGameManager.isEndJudgeInfoUpdate != 0)
            {
                UpdateJudgementText();
                bmsGameManager.isEndJudgeInfoUpdate = 0;
            }
        }
    }

    private void ObjectSetting()
    {
        earlyLateSprite[0].transform.localPosition = new Vector3(bmsGameManager.xPosition[1], 2.17f, 0.0f);
        earlyLateSprite[1].transform.localPosition = new Vector3(bmsGameManager.xPosition[3], 2.17f, 0.0f);
        judgementInfo.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], 3.5f, 0.0f);

        float numberSize = gameUIManager.defaultNumberArray[0].bounds.size.x * earlyDigitArray[0].transform.localScale.x;
        digitPositionX = new float[4];
        for (int i = 0; i < 4; i++)
        {
            earlyDigitArray[i].transform.localPosition = new Vector3((1.5f - i) * numberSize, 0.0f, 0.0f);
            lateDigitArray[i].transform.localPosition = new Vector3((1.5f - i) * numberSize, 0.0f, 0.0f);
            koolDigitArray[i].transform.localPosition = new Vector3((1.5f - i) * numberSize, 0.0f, 0.0f);
            coolDigitArray[i].transform.localPosition = new Vector3((1.5f - i) * numberSize, 0.0f, 0.0f);
            goodDigitArray[i].transform.localPosition = new Vector3((1.5f - i) * numberSize, 0.0f, 0.0f);
            digitPositionX[i] = -((3 - i) * numberSize * 0.5f);
        }
    }

    private void UpdateFSText(int idx, int state)
    {
        earlyLateSprite[idx].sprite = earlyLateImageArray[state];
        earlyLateEffectAnimator[idx].SetTrigger(hashEarlyLateEffect);
    }

    private void UpdateJudgementText()
    {
        DigitSet(ref earlyDigitArray, bmsGameManager.fsCount[0], earlyDigitParent);
        DigitSet(ref lateDigitArray, bmsGameManager.fsCount[1], lateDigitParent);
        DigitSet(ref koolDigitArray, bmsResult.koolCount, koolDigitParent);
        DigitSet(ref coolDigitArray, bmsResult.coolCount, coolDigitParent);
        DigitSet(ref goodDigitArray, bmsResult.goodCount, goodDigitParent);
        judgementInfo.SetActive(bmsGameManager.isEndJudgeInfoUpdate == 1 ? false : true);
    }

    private void DigitSet(ref SpriteRenderer[] digitArray, int count, Transform digitParent)
    {
        for (int i = 0; i < 4; i++)
        {
            digitArray[i].sprite = null;
        }

        int digitCount = 0;
        do
        {
            int tempValue = (int)(count * 0.1f);
            int remainder = count - (tempValue * 10);
            digitArray[digitCount++].sprite = gameUIManager.defaultNumberArray[remainder];
            count = tempValue;
        } while (count > 0);
        digitParent.localPosition = new Vector3(digitPositionX[digitCount - 1], digitParent.localPosition.y, 0.0f);
    }
}
