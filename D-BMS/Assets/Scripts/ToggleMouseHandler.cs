using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleMouseHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private Toggle toggle;
    [HideInInspector]
    public bool isClick = false;

    void Awake()
    {
        toggle = gameObject.GetComponent<Toggle>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        toggle.isOn = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isClick = true;
        }
    }
}
