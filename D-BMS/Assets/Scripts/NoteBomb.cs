using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteBomb : MonoBehaviour
{
    private GameUIManager gameUIManager = null;

    [SerializeField]
    private SpriteRenderer[] noteBombNArray;
    [SerializeField]
    private SpriteRenderer[] noteBombLArray;
    private Sprite[] noteBombNSpriteArray;
    private Sprite[] noteBombLSpriteArray;
    [HideInInspector]
    public bool[] isNoteBombLEffectActive;
    [HideInInspector]
    public int[] noteBombNAnimationIndex;

    private TimeSpan effectWaitSecond = TimeSpan.FromSeconds(1.0d / 60.0d);

    private void Awake()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
    }

    public void SetNoteBomb()
    {
        noteBombNSpriteArray = gameUIManager.assetPacker.GetSprites("notebombN-");
        noteBombLSpriteArray = gameUIManager.assetPacker.GetSprites("notebombL-");

        noteBombNAnimationIndex = new int[5];
        isNoteBombLEffectActive = new bool[5] { false, false, false, false, false };
        for (int i = 0; i < 5; i++)
        {
            _ = NoteBombNEffect(i);
            _ = NoteBombLEffect(i);
        }

        SetNoteBombScale();
        SetNoteBombPosition();
    }

    public void SetNoteBombScale()
    {
        for (int i = 0; i < 5; i++)
        {
            noteBombNArray[i].transform.localScale = new Vector3(GameUIManager.config.noteBombNScale, GameUIManager.config.noteBombNScale, 1.0f);
            noteBombLArray[i].transform.localScale = new Vector3(GameUIManager.config.noteBombLScale, GameUIManager.config.noteBombLScale, 1.0f);
        }
    }

    public void SetNoteBombPosition()
    {
        float yPos = GameObject.Find("JudgeLine1").GetComponent<SpriteRenderer>().sprite.bounds.size.y *
                     GameObject.Find("JudgeLine1").transform.localScale.y * 0.5f + (PlayerPrefs.GetInt("JudgeLine") == 0 ? GameUIManager.config.judgeLinePosition : GameUIManager.config.judgeLinePosition - 0.24f);
        for (int i = 0; i < 5; i++)
        {
            noteBombNArray[i].transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(i), yPos, 0.0f);
            noteBombLArray[i].transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(i), yPos, 0.0f);
        }
    }

    public void NoteBombLEffectOff()
    {
        for (int i = 0; i < 5; i++)
        {
            isNoteBombLEffectActive[i] = false;
        }
    }

    private async UniTask NoteBombNEffect(int line)
    {
        var token = this.GetCancellationTokenOnDestroy();
        int noteBombNSpriteArrayLength = noteBombNSpriteArray.Length;
        noteBombNAnimationIndex[line] = noteBombNSpriteArrayLength;
        while (true)
        {
            while (noteBombNAnimationIndex[line] < noteBombNSpriteArrayLength)
            {
                noteBombNArray[line].sprite = noteBombNSpriteArray[noteBombNAnimationIndex[line]++];
                await UniTask.Delay(effectWaitSecond, cancellationToken: token);
            }
            noteBombNArray[line].sprite = null;
            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask NoteBombLEffect(int line)
    {
        var token = this.GetCancellationTokenOnDestroy();
        int noteBombLSpriteArrayLength = noteBombLSpriteArray.Length - 1;
        int currentEffectIndex = 0;
        while (true)
        {
            while (isNoteBombLEffectActive[line])
            {
                noteBombLArray[line].sprite = noteBombLSpriteArray[currentEffectIndex];
                currentEffectIndex = currentEffectIndex == noteBombLSpriteArrayLength ? 0 : currentEffectIndex + 1;
                await UniTask.Delay(effectWaitSecond, cancellationToken: token);
            }
            noteBombLArray[line].sprite = null;
            currentEffectIndex = 0;
            await UniTask.Yield(cancellationToken: token);
        }
    }
}
