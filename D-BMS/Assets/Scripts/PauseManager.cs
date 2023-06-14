﻿using System.Collections;
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
    private ToggleGroup toggleGroup;
    [SerializeField]
    private Image panelImage;
    [SerializeField]
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI[] toggleText;
    private int currentIndex;
    private PauseManager pauseManager;
    private BMSGameManager bmsGameManager;

    void Awake()
    {
        pauseManager = gameObject.GetComponent<PauseManager>();
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        currentIndex = 0;

        toggleText = new TextMeshProUGUI[toggleArray.Length];

        for (int i = toggleArray.Length - 1; i >= 0; i--)
        {
            toggleText[i] = toggleArray[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            AddToggleListener(toggleArray[i], toggleText[i]);
        }

        ToggleChange(0);
        toggleGroup.allowSwitchOff = false;
        pauseManager.Pause_SetActive(false);
    }

    public void PausePanelSetting(int set)
    {
        if (set == 0)
        {
            titleText.text = "PAUSE";
            titleText.color = new Color(215.0f / 255.0f, 215.0f / 255.0f, 215.0f / 255.0f);
            toggleText[0].text = "RESUME";
            panelImage.color = new Color(0.0f, 244.0f / 255.0f, 255.0f / 255.0f, 200.0f / 255.0f);
        }
        else
        {
            titleText.text = "GAME OVER";
            titleText.color = new Color(170.0f / 255.0f, 0.0f, 0.0f);
            toggleText[0].text = "RESULT";
            panelImage.color = new Color(255.0f / 255.0f, 0.0f, 0.0f, 200.0f / 255.0f);
        }
    }

    public void Pause_SetActive(bool isActive)
    {
        pauseCanvas.enabled = isActive;
        pauseManager.enabled = isActive;
        ToggleChange(0);
    }

    private void AddToggleListener(Toggle toggle, TextMeshProUGUI toggleText)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                toggleText.color = Color.black;
                toggleText.fontSize = 35;
            }
            else
            {
                toggleText.color = Color.white;
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