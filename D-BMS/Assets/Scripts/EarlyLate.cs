using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EarlyLate : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private BMSResult bmsResult = null;

    [SerializeField]
    private Sprite[] earlyLateImageArray;
    [SerializeField]
    private SpriteRenderer[] earlyLateSprite;
    [SerializeField]
    private Animator[] earlyLateEffectAnimator;
    [SerializeField]
    private TextMeshProUGUI earlyCountText;
    [SerializeField]
    private TextMeshProUGUI lateCountText;
    [SerializeField]
    private TextMeshProUGUI koolCountText;
    [SerializeField]
    private TextMeshProUGUI coolCountText;
    [SerializeField]
    private TextMeshProUGUI goodCountText;
    [SerializeField]
    private GameObject judgementInfo;

    private readonly int hashEarlyLateEffect = Animator.StringToHash("EarlyLateEffect");

    void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        bmsResult = BMSGameManager.bmsResult;

        if (PlayerPrefs.GetInt("EarlyLate") == 0)
        {
            gameObject.SetActive(false);
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

    private void UpdateFSText(int idx, int state)
    {
        earlyLateSprite[idx].sprite = earlyLateImageArray[state];
        earlyLateEffectAnimator[idx].SetTrigger(hashEarlyLateEffect);
    }

    private void UpdateJudgementText()
    {
        earlyCountText.text = bmsGameManager.fsCount[0].ToString();
        lateCountText.text = bmsGameManager.fsCount[1].ToString();
        koolCountText.text = bmsResult.koolCount.ToString();
        coolCountText.text = bmsResult.coolCount.ToString();
        goodCountText.text = bmsResult.goodCount.ToString();

        judgementInfo.SetActive(bmsGameManager.isEndJudgeInfoUpdate == 1 ? false : true);
    }
}
