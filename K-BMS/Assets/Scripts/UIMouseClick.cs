using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private SongSelectUIManager selectUIManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (this.gameObject.name[0] == 'N')
            {
                selectUIManager.NoteSpeedClick(0.1f);
            }
            else
            {
                selectUIManager.RandomEffectorClick(1);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (this.gameObject.name[0] == 'N')
            {
                selectUIManager.NoteSpeedClick(-0.1f);
            }
            else
            {
                selectUIManager.RandomEffectorClick(-1);
            }
        }
    }
}
