using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleMouseHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private Toggle toggle;
    [SerializeField]
    private bool usePointerEnter = true;
    [SerializeField]
    private bool usePointerDown = true;
    [HideInInspector]
    public bool isClick = false;

    void Awake()
    {
        toggle = gameObject.GetComponent<Toggle>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!usePointerEnter) { return; }
        toggle.isOn = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!usePointerDown) { return; }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isClick = true;
        }
    }
}
