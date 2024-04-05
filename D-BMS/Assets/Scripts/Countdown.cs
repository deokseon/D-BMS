using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    private GameUIManager gameUIManager = null;

    private Sprite[] digitArray;
    [SerializeField]
    private SpriteRenderer countdownBar;
    [SerializeField]
    private SpriteRenderer countdownTime;

    private void Awake()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
    }

    public void SetCountdownObject()
    {
        SetCountdownMaterial();
        SetCountdownPosition();
        digitArray = gameUIManager.assetPacker.GetSprites("combo-");
    }

    private void SetCountdownMaterial()
    {
        gameObject.SetActive(true);
        Sprite countDownCircle = gameUIManager.assetPacker.GetSprite("countdowncircle");
        countdownBar.sprite = countDownCircle;
        Rect spriteRect = countDownCircle.textureRect;

        Vector4 spriteData = new Vector4(
            spriteRect.x / countDownCircle.texture.width,
            spriteRect.y / countDownCircle.texture.height,
            spriteRect.width / countDownCircle.texture.width,
            spriteRect.height / countDownCircle.texture.height
            );

        countdownBar.material.SetVector("_SpriteData", spriteData);
        gameObject.SetActive(false);
    }

    private void SetCountdownPosition()
    {
        transform.localPosition = new Vector3(gameUIManager.GetLinePositionX(2), 2.2f, 0.0f);
    }

    public void SetActiveCountdown(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetCountdownProcessivity(float amount, int second)
    {
        countdownBar.material.SetInt("_Arc2", (int)((1.0f - amount) * 360.0f));
        countdownTime.sprite = digitArray[second == 3 ? second : second + 1];
    }
}
