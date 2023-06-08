using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleMouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField]
    private int index;
    private StartSceneManager startSceneManager = null;
    private PauseManager pauseManager = null;

    private void Awake()
    {
        startSceneManager = FindObjectOfType<StartSceneManager>();
        pauseManager = FindObjectOfType<PauseManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (startSceneManager != null)
        {
            startSceneManager.ToggleChange(index);
        }
        else if (pauseManager != null)
        {
            pauseManager.ToggleChange(index);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (startSceneManager != null)
            {
                startSceneManager.ToggleExecute();
            }
            else if (pauseManager != null)
            {
                pauseManager.ToggleExecute();
            }
        }
    }
}
