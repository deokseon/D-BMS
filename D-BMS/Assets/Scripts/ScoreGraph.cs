using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreGraph : MonoBehaviour
{
    private BMSGameManager bmsGameManager = null;
    private BMSResult bmsResult = null;

    [SerializeField]
    private Sprite[] rank;
    [SerializeField]
    private Image rankImage;
    [SerializeField]
    private RectTransform scoreStick;
    [SerializeField]
    private RectTransform maxScoreStick;
    [SerializeField]
    private GameObject[] scoreGraphObjects;

    private readonly float[] yPos = { -90.0f, 19.5f, 71.5f, 151.5f, 231.5f, 271.5f, 311.5f, 351.5f, 371.5f, 391.5f, 423.5f };
    private readonly RectTransform.Axis vertical = RectTransform.Axis.Vertical;

    void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        if (bmsGameManager != null)
        {
            if (PlayerPrefs.GetInt("ScoreGraph") == 0)
            {
                ScoreGraphInactivate();
                return;
            }
            bmsResult = BMSGameManager.bmsResult;
            StartCoroutine(CheckScoreGraphUpdate());
        }

        scoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
        maxScoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
        SetScoreGraphPosition(GameUIManager.config.scoreGraphPositionOffsetX, GameUIManager.config.scoreGraphPositionOffsetY);
    }

    private IEnumerator CheckScoreGraphUpdate()
    {
        while (true)
        {
            lock (bmsGameManager.inputHandleLock)
            {
                if (bmsGameManager.isChangeRankImage)
                {
                    rankImage.sprite = rank[bmsResult.rankIndex];
                    rankImage.rectTransform.localPosition = new Vector3(-244.0f + GameUIManager.config.scoreGraphPositionOffsetX, yPos[bmsResult.rankIndex] + GameUIManager.config.scoreGraphPositionOffsetY, 0.0f);
                    bmsGameManager.isChangeRankImage = false;
                }

                if (bmsGameManager.isScoreGraphUpdate)
                {
                    int currentCount = bmsGameManager.currentCount + bmsGameManager.endCount;
                    scoreStick.SetSizeWithCurrentAnchors(vertical, bmsResult.scoreGraphArray[currentCount]);
                    maxScoreStick.SetSizeWithCurrentAnchors(vertical, bmsGameManager.maxScoreTable[currentCount]);
                    bmsGameManager.isScoreGraphUpdate = false;
                }
            }
            yield return null;
        }
    }

    private void ScoreGraphInactivate()
    {
        for (int i = 0; i < scoreGraphObjects.Length; i++)
        {
            scoreGraphObjects[i].SetActive(false);
        }
        rankImage.gameObject.SetActive(false);
        scoreStick.gameObject.SetActive(false);
        maxScoreStick.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetScoreGraphPosition(float offsetX, float offsetY)
    {
        for (int i = 0; i < scoreGraphObjects.Length; i++)
        {
            RectTransform objectRectTr = scoreGraphObjects[i].GetComponent<RectTransform>();
            objectRectTr.localPosition = new Vector3(objectRectTr.localPosition.x + offsetX, objectRectTr.localPosition.y + offsetY, 0.0f);
        }
        rankImage.rectTransform.localPosition = new Vector3(rankImage.rectTransform.localPosition.x + offsetX, rankImage.rectTransform.localPosition.y + offsetY, 0.0f);
        scoreStick.localPosition = new Vector3(scoreStick.localPosition.x + offsetX, scoreStick.localPosition.y + offsetY, 0.0f);
        maxScoreStick.localPosition = new Vector3(maxScoreStick.localPosition.x + offsetX, maxScoreStick.localPosition.y + offsetY, 0.0f);
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(gameObject.GetComponent<RectTransform>().localPosition.x + offsetX,
                                                                             gameObject.GetComponent<RectTransform>().localPosition.y + offsetY, 0.0f);
    }
}
