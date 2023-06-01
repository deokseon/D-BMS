using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleMouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField]
    private int index;
    private StartSceneManager startSceneManager;

    private void Awake()
    {
        startSceneManager = FindObjectOfType<StartSceneManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        startSceneManager.ToggleChange(index);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            startSceneManager.ToggleExecute();
        }
    }
}
