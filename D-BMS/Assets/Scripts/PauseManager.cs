using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Networking;

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
    private Texture[] backgroundTexture;
    private int currentIndex;
    private PauseManager pauseManager;
    private BMSGameManager bmsGameManager;

    void Awake()
    {
        pauseManager = gameObject.GetComponent<PauseManager>();
        bmsGameManager = FindObjectOfType<BMSGameManager>();
        currentIndex = 0;

        backgroundTexture = new Texture[2];

        for (int i = toggleArray.Length - 1; i >= 0; i--)
        {
            AddToggleListener(toggleArray[i], i, toggleText[i]);
        }

        SetBackground(0, "pause-bg");
        SetBackground(1, "fail-bg");

        pauseManager.Pause_SetActive(false);
    }

    private void SetBackground(int index, string file)
    {
        string filePath = $@"{Directory.GetParent(Application.dataPath)}\Skin\Background\{file}";
        if (File.Exists(filePath + ".jpg"))
        {
            StartCoroutine(LoadBG(index, filePath + ".jpg"));
        }
        else if (File.Exists(filePath + ".png"))
        {
            StartCoroutine(LoadBG(index, filePath + ".png"));
        }
    }

    private IEnumerator LoadBG(int index, string path)
    {
        string imagePath = $@"file:\\{path}";

        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath);
        yield return uwr.SendWebRequest();

        backgroundTexture[index] = (uwr.downloadHandler as DownloadHandlerTexture).texture;
    }

    public void PausePanelSetting(int set)
    {
        if (set == 0)
        {
            toggleText[0].text = "RESUME";
            gameObject.GetComponent<RawImage>().texture = backgroundTexture[set];
            for (int i = checkMarkArray.Length - 1; i >= 0; i--)
            {
                checkMarkArray[i].color = new Color32(0, 255, 255, 255);
            }
        }
        else
        {
            toggleText[0].text = "RESULT";
            gameObject.GetComponent<RawImage>().texture = backgroundTexture[set];
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

    private void ToggleChange(int index)
    {
        currentIndex = (index + toggleArray.Length) % toggleArray.Length;
        toggleArray[currentIndex].isOn = true;
    }

    private void ToggleExecute()
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
