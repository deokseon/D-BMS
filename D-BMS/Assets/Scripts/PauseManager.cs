using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [SerializeField]
    private Canvas pauseCanvas;
    [SerializeField]
    private Toggle[] toggleArray;
    [SerializeField]
    private Image[] checkMarkArray;
    [SerializeField]
    private TextMeshProUGUI[] toggleText;
    [SerializeField]
    private Image panelImage;
    [SerializeField]
    private TextMeshProUGUI titleText;
    private int currentIndex;
    private PauseManager pauseManager;
    private BMSGameManager bmsGameManager;

    void Awake()
    {
        pauseManager = gameObject.GetComponent<PauseManager>();
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        currentIndex = 0;

        for (int i = toggleArray.Length - 1; i >= 0; i--)
        {
            AddToggleListener(toggleArray[i], i, toggleText[i]);
        }

        pauseManager.Pause_SetActive(false);
    }

    public void PausePanelSetting(int set)
    {
        if (set == 0)
        {
            titleText.text = "PAUSE";
            titleText.color = new Color32(215, 215, 215, 255);
            toggleText[0].text = "RESUME";
            panelImage.color = new Color32(0, 255, 255, 200);
            for (int i = checkMarkArray.Length - 1; i >= 0; i--)
            {
                checkMarkArray[i].color = new Color32(0, 255, 255, 255);
            }
        }
        else
        {
            titleText.text = "GAME OVER";
            titleText.color = new Color32(170, 0, 0, 255);
            toggleText[0].text = "RESULT";
            panelImage.color = new Color32(255, 0, 0, 200);
            for (int i = checkMarkArray.Length - 1; i >= 0; i--)
            {
                checkMarkArray[i].color = new Color32(255, 0, 0, 255);
            }
        }
    }

    public void Pause_SetActive(bool isActive)
    {
        pauseCanvas.enabled = isActive;
        pauseManager.enabled = isActive;
        ToggleChange(2);
        ToggleChange(0);
    }

    private void AddToggleListener(Toggle toggle, int index, TextMeshProUGUI toggleText)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                currentIndex = index;
                toggleText.color = checkMarkArray[0].color;
                toggleText.fontSize = 35;
                if (toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick)
                {
                    toggle.gameObject.GetComponent<ToggleMouseHandler>().isClick = false;
                    ToggleExecute();
                }
            }
            else
            {
                toggleText.color = new Color32(215, 215, 215, 255);
                toggleText.fontSize = 30;
            }
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleExecute();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ToggleChange(currentIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ToggleChange(currentIndex - 1);
        }
    }

    public void ToggleChange(int index)
    {
        currentIndex = (index + toggleArray.Length) % toggleArray.Length;
        toggleArray[currentIndex].isOn = true;
    }

    public void ToggleExecute()
    {
        if (currentIndex == 0)
        {
            if (toggleText[0].text.CompareTo("RESUME") == 0)
            {
                bmsGameManager.GameResume();
            }
            else
            {
                StartCoroutine(bmsGameManager.GameEnd(false));
            }
        }
        else if (currentIndex == 1)
        {
            StartCoroutine(bmsGameManager.GameRestart());
        }
        else
        {
            StartCoroutine(bmsGameManager.CoLoadScene(1));
        }
    }
}
