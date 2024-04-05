using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffect : MonoBehaviour
{
    private GameUIManager gameUIManager = null;

    private Animator judgeEffectAnimator;
    private SpriteRenderer judgeSpriteRenderer;
    private Sprite[,] judgeSpriteArray;
    private int currentJudge;
    private int judgeEffectIndex;
    private TimeSpan effectWaitSecond = TimeSpan.FromSeconds(1.0d / 60.0d);
    private readonly int hashJudgeEffect = Animator.StringToHash("JudgeEffect");

    private void Awake()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
        judgeEffectAnimator = GetComponent<Animator>();
        judgeSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetJudgeEffect()
    {
        Sprite[] koolSprite = gameUIManager.assetPacker.GetSprites("kool-");
        Sprite[] coolSprite = gameUIManager.assetPacker.GetSprites("cool-");
        Sprite[] goodSprite = gameUIManager.assetPacker.GetSprites("good-");
        Sprite[] missSprite = gameUIManager.assetPacker.GetSprites("miss-");
        Sprite[] failSprite = gameUIManager.assetPacker.GetSprites("fail-");
        judgeSpriteArray = new Sprite[5, 15];
        for (int i = 0; i < 15; i++)
        {
            judgeSpriteArray[4, i] = koolSprite[i % koolSprite.Length];
            judgeSpriteArray[3, i] = coolSprite[i % coolSprite.Length];
            judgeSpriteArray[2, i] = goodSprite[i % goodSprite.Length];
            judgeSpriteArray[1, i] = missSprite[i % missSprite.Length];
            judgeSpriteArray[0, i] = failSprite[i % failSprite.Length];
        }
        currentJudge = -1;
        judgeEffectIndex = 15;

        SetJudgePosition();
    }

    public void SetJudgePosition()
    {
        judgeSpriteRenderer.transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), GameUIManager.config.judgePosition, 0.0f);
    }

    public void SetJudgeEffectSprite(int judge, int index)
    {
        judgeSpriteRenderer.sprite = judgeSpriteArray[judge, index];
    }

    public void JudgeEffectSpriteAnimationStart()
    {
        _ = JudgeEffectSpriteAnimation();
    }

    public void JudgeEffectAnimationActive(JudgeType judge)
    {
        currentJudge = (int)judge - 1;
        judgeEffectIndex = 0;
        judgeEffectAnimator.SetTrigger(hashJudgeEffect);
    }

    private async UniTask JudgeEffectSpriteAnimation()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            while (judgeEffectIndex < 15)
            {
                judgeSpriteRenderer.sprite = judgeSpriteArray[currentJudge, judgeEffectIndex++];
                await UniTask.Delay(effectWaitSecond, cancellationToken: token);
            }
            judgeSpriteRenderer.sprite = null;
            await UniTask.Yield(cancellationToken: token);
        }
    }
}
