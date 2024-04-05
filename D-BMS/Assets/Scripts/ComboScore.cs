using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboScore : MonoBehaviour
{
    private GameUIManager gameUIManager = null;

    [SerializeField]
    private GameObject comboTitle;
    [SerializeField]
    private Transform comboParentTransform;
    private Sprite[] comboNumberArray;
    [SerializeField]
    private SpriteRenderer[] comboDigitArray;
    [SerializeField]
    private Animator comboTitleAnimator;
    [SerializeField]
    private Animator comboTitleBounceAnimator;
    [SerializeField]
    private Animator comboBounceAnimator;
    [SerializeField]
    private Animator[] comboAnimatorArray;
    private float[] comboPositionX;

    [HideInInspector]
    public Sprite[] defaultNumberArray;
    [SerializeField]
    private SpriteRenderer[] scoreDigitArray;
    [SerializeField]
    private SpriteRenderer[] maxcomboDigitArray;

    private readonly int hashComboTitle = Animator.StringToHash("ComboTitle");
    private readonly int hashComboTitleBounce = Animator.StringToHash("ComboTitleBounce");
    private readonly int hashComboBounce = Animator.StringToHash("ComboBounce");
    private readonly int hashCombo = Animator.StringToHash("Combo");

    private void Awake()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
    }

    public void ComboScoreCustomSetting()
    {
        comboTitle.SetActive(true);
        comboParentTransform.localPosition = new Vector3(comboPositionX[2], GameUIManager.config.comboPosition, 0.0f);
        comboDigitArray[0].sprite = comboNumberArray[3];
        comboDigitArray[1].sprite = comboNumberArray[6];
        comboDigitArray[2].sprite = comboNumberArray[9];
    }

    public void SetNumberSpriteArray()
    {
        comboNumberArray = gameUIManager.assetPacker.GetSprites("combo-");
        defaultNumberArray = gameUIManager.assetPacker.GetSprites("default-");
    }

    public void SetCombo()
    {
        comboTitle.GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite("text-combo");

        comboParentTransform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), GameUIManager.config.comboPosition, 0.0f);
        float comboNumberSize = comboNumberArray[0].bounds.size.x * comboDigitArray[0].transform.localScale.x;
        comboPositionX = new float[5];
        for (int i = 0; i < 5; i++)
        {
            comboDigitArray[i].transform.localPosition = new Vector3((2 - i) * comboNumberSize, 0.0f, 0.0f);
            comboPositionX[i] = gameUIManager.GetLinePositionX(2) - ((4 - i) * comboNumberSize * 0.5f);
        }

        float comboTitleYPos = 0.085f + GameUIManager.config.comboPosition + comboNumberArray[0].bounds.size.y * comboDigitArray[0].transform.localScale.y * 0.5f +
                                comboTitle.GetComponent<SpriteRenderer>().sprite.bounds.size.y * comboTitle.transform.localScale.y * 0.5f;
        comboTitle.transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), comboTitleYPos, 0.0f);
    }

    public void SetScore()
    {
        GameObject scoreTitle = GameObject.Find("Score_Title");
        scoreTitle.GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite("text-score");
        scoreTitle.transform.localPosition = new Vector3(GameUIManager.config.scoreImagePositionX, GameUIManager.config.scoreImagePositionY, 0.0f);

        GameObject.Find("ScoreParent").transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), 0.0f, 0.0f);

        for (int i = 0; i < 7; i++)
        {
            scoreDigitArray[i].transform.localPosition = new Vector3(GameUIManager.config.scoreDigitPositionX - i * defaultNumberArray[0].bounds.size.x * scoreDigitArray[i].transform.localScale.x, GameUIManager.config.scoreDigitPositionY, 0.0f);
        }
    }

    public void SetMaxcombo()
    {
        GameObject maxcomboTitle = GameObject.Find("Maxcombo_Title");
        maxcomboTitle.GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite("text-maxcombo");
        maxcomboTitle.transform.localPosition = new Vector3(GameUIManager.config.maxcomboImagePositionX, GameUIManager.config.maxcomboImagePositionY, 0.0f);

        GameObject.Find("MaxcomboParent").transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), 0.0f, 0.0f);

        for (int i = 0; i < 5; i++)
        {
            maxcomboDigitArray[i].transform.localPosition = new Vector3(GameUIManager.config.maxcomboDigitPositionX - i * defaultNumberArray[0].bounds.size.x * maxcomboDigitArray[i].transform.localScale.x, GameUIManager.config.maxcomboDigitPositionY, 0.0f);
        }
    }

    public void ScoreComboUpdate(int combo, int maxcombo, int score)
    {
        if (combo != 0)
        {
            if (!comboTitle.activeSelf)
            {
                comboTitle.SetActive(true);
            }
            int digitCount = 0;
            while (combo > 0)
            {
                int tempValue = (int)(combo * 0.1f);
                int remainder = combo - (tempValue * 10);
                comboDigitArray[digitCount].sprite = comboNumberArray[remainder];
                comboAnimatorArray[digitCount++].SetTrigger(hashCombo);
                combo = tempValue;
            }
            comboTitleAnimator.SetTrigger(hashComboTitle);
            comboTitleBounceAnimator.SetTrigger(hashComboTitleBounce);
            comboBounceAnimator.SetTrigger(hashComboBounce);
            comboParentTransform.localPosition = new Vector3(comboPositionX[digitCount - 1], GameUIManager.config.comboPosition, 0.0f);
            while (digitCount < 5)
            {
                comboDigitArray[digitCount++].sprite = null;
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                comboDigitArray[i].sprite = null;
            }
            comboTitle.SetActive(false);
        }

        for (int i = 0; i < 5; i++)
        {
            int tempValue = (int)(maxcombo * 0.1f);
            int remainder = maxcombo - (tempValue * 10);
            maxcomboDigitArray[i].sprite = defaultNumberArray[remainder];
            maxcombo = tempValue;
        }

        for (int i = 0; i < 7; i++)
        {
            int tempValue = (int)(score * 0.1f);
            int remainder = score - (tempValue * 10);
            scoreDigitArray[i].sprite = defaultNumberArray[remainder];
            score = tempValue;
        }
    }
}
