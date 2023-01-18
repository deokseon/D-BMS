using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField]
    private Animator fadeAnimator;
    [SerializeField]
    private Image fadeImage;
    private static bool isFirst = true;
    private bool isStart;
    private bool isEnd;

    private void Awake()
    {
        isStart = false;
        isEnd = false;

        if (isFirst)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(false);
        }
        else
        {
            isStart = true;
            isEnd = true;
            StartCoroutine(CoFadeOut());
        }
    }
    private IEnumerator CoFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(1.0f);
        fadeImage.gameObject.SetActive(false);
        isStart = false;
        isEnd = false;
    }

    public void StartButtonClick()
    {
        if (isStart || isEnd) { return; }
        isStart = true;
        isFirst = false;
        StartCoroutine(CoLoadSelectScene());
    }

    private IEnumerator CoLoadSelectScene()
    {
        fadeAnimator.gameObject.SetActive(true);
        fadeAnimator.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void ExitButtonClick()
    {
        if (isEnd || isStart) { return; }
        isEnd = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitButtonClick();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            StartButtonClick();
        }
    }
}
