using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EarlyLate : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private GameUIManager gameUIManager = null;
    private BMSResult bmsResult = null;

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

    private float[] digitPositionX;

    private readonly int hashEarlyLateEffect = Animator.StringToHash("EarlyLateEffect");

    void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();

        if (bmsGameManager != null)
        {
            if (PlayerPrefs.GetInt("EarlyLate") == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            bmsResult = BMSGameManager.bmsResult;

            _ = CheckEarlyLate(0);
            _ = CheckEarlyLate(1);
            _ = CheckEndInfoUpdate();
            _ = WaitSetting();
        }
    }

    private async UniTask CheckEarlyLate(int index)
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            lock (bmsGameManager.threadLock)
            {
                if (bmsGameManager.fsUpdate[index])
                {
                    UpdateELText(index, bmsGameManager.fsStates[index]);
                    bmsGameManager.fsUpdate[index] = false;
                }
            }
            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask CheckEndInfoUpdate()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            await UniTask.WaitUntil(() => bmsGameManager.isEndJudgeInfoUpdate != 0, cancellationToken: token);
            lock (bmsGameManager.threadLock)
            {
                if (bmsGameManager.isEndJudgeInfoUpdate != 0)
                {
                    UpdateJudgementText();
                    bmsGameManager.isEndJudgeInfoUpdate = 0;
                }
            }
        }
    }

    private async UniTask WaitSetting()
    {
        await UniTask.WaitUntil(() => gameUIManager.isPrepared == gameUIManager.taskCount + 1);

        ObjectSetting();
    }

    public void ObjectSetting()
    {
        earlyLateImageArray = new Sprite[2];
        earlyLateImageArray[0] = gameUIManager.assetPacker.GetSprite("early");
        earlyLateImageArray[1] = gameUIManager.assetPacker.GetSprite("late");

        judgementInfo.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = earlyLateImageArray[0];
        judgementInfo.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sprite = earlyLateImageArray[1];
        judgementInfo.transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite("kool-0");
        judgementInfo.transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite("cool-0");
        judgementInfo.transform.GetChild(4).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite("good-0");

        SetEarlyLatePosition();

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

    public void CustomSetting()
    {
        for (int i = 0; i < 2; i++)
        {
            earlyLateSprite[i].sprite = earlyLateImageArray[i];
        }
    }

    private void UpdateELText(int idx, int state)
    {
        earlyLateSprite[idx].sprite = earlyLateImageArray[state];
        earlyLateEffectAnimator[idx].SetTrigger(hashEarlyLateEffect);
    }

    private void UpdateJudgementText()
    {
        judgementInfo.SetActive(true);
        DigitSet(ref earlyDigitArray, bmsResult.resultData.earlyCount, GameObject.Find("EarlyDigitParent").transform);
        DigitSet(ref lateDigitArray, bmsResult.resultData.lateCount, GameObject.Find("LateDigitParent").transform);
        DigitSet(ref koolDigitArray, bmsResult.resultData.koolCount, GameObject.Find("KoolDigitParent").transform);
        DigitSet(ref coolDigitArray, bmsResult.resultData.coolCount, GameObject.Find("CoolDigitParent").transform);
        DigitSet(ref goodDigitArray, bmsResult.resultData.goodCount, GameObject.Find("GoodDigitParent").transform);
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

    public void SetEarlyLatePosition()
    {
        earlyLateSprite[0].transform.localPosition = new Vector3((gameUIManager.GetXPosition(0) + gameUIManager.GetXPosition(1)) * 0.5f, GameUIManager.config.earlyLatePosition, 0.0f);
        earlyLateSprite[1].transform.localPosition = new Vector3((gameUIManager.GetXPosition(3) + gameUIManager.GetXPosition(4)) * 0.5f, GameUIManager.config.earlyLatePosition, 0.0f);
        judgementInfo.transform.localPosition = new Vector3(gameUIManager.GetXPosition(2), 3.5f, 0.0f);
    }

    public float GetMinEarlyLatePosition()
    {
        return -0.24f + earlyLateImageArray[0].bounds.size.y * earlyLateSprite[0].transform.localScale.y * 0.5f;
    }

    public float GetMaxEarlyLatePosition()
    {
        return 7.5f - earlyLateImageArray[0].bounds.size.y * earlyLateSprite[0].transform.localScale.y * 0.5f;
    }
}
