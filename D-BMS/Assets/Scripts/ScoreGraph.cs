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
    private Transform rankImageTransform;

    void Awake()
    {
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        bmsResult = BMSGameManager.bmsResult;
        rankImageTransform = rankImage.transform;

        scoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);
        maxScoreStick.SetSizeWithCurrentAnchors(vertical, 0.0f);

        if (PlayerPrefs.GetInt("ScoreGraph") == 0)
        {
            ScoreGraphInactivate();
        }
    }

    void Update()
    {
        lock (bmsGameManager.inputHandleLock)
        {
            if (bmsGameManager.isChangeRankImage)
            {
                rankImage.sprite = rank[bmsResult.rankIndex];
                rankImageTransform.localPosition = new Vector3(-244.0f, yPos[bmsResult.rankIndex], 0.0f);
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
}
