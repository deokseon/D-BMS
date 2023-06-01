using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseClick : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private SongSelectUIManager selectUIManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (this.gameObject.name[0] == 'N')
            {
                selectUIManager.NoteSpeedClick(1);
            }
            else if (this.gameObject.name[0] == 'R')
            {
                selectUIManager.RandomEffectorClick(1);
            }
            else
            {
                selectUIManager.SortByClick(1);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (this.gameObject.name[0] == 'N')
            {
                selectUIManager.NoteSpeedClick(-1);
            }
            else if (this.gameObject.name[0] == 'R')
            {
                selectUIManager.RandomEffectorClick(-1);
            }
            else
            {
                selectUIManager.SortByClick(-1);
            }
        }
    }
}
