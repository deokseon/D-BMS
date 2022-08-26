using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSDrawer : MonoBehaviour
{
    public BMSPattern pattern;
    private float xPositionStart = -7.7f;

    [SerializeField]
    private GameObject[] longNotePrefab;
    [SerializeField]
    private GameObject[] longNoteEdgePrefab;
    [SerializeField]
    private Transform noteParent;

    public void DrawNotes()
    {
        float speed = BMSGameManager.gameSpeed;

        noteParent.position = Vector3.zero;

        pattern = BMSParser.instance.pattern;

        for (int i = 0; i < 5; i++)
        {
            Vector3 prev = Vector2.zero;

            float xPosition = xPositionStart + (0.57f * i);

            for (int j = pattern.lines[i].noteList.Count - 1; j >= 0; j--)
            {
                if (pattern.lines[i].noteList[j].extra == 2) { continue; }
                Note n = pattern.lines[i].noteList[j];

                Vector3 notePosition = new Vector3(xPosition, (float)(n.beat * speed), 0.0f);

                GameObject note = null;
                if (n.extra == 1)
                {
                    note = Instantiate(longNoteEdgePrefab[i % 2], noteParent);
                    note.transform.rotation = Quaternion.Euler(180.0f, 0.0f, 0.0f);

                    GameObject longNote = Instantiate(longNotePrefab[i % 2], noteParent);
                    longNote.transform.position = (notePosition + prev) * 0.5f;
                    longNote.transform.localScale = new Vector3(0.3f, ((notePosition - prev).y - 0.3f) * 1.219512f, 1.0f);
                }
                else if (j > 0 && pattern.lines[i].noteList[j - 1].extra == 2)
                {
                    note = Instantiate(longNoteEdgePrefab[i % 2], noteParent);
                }
                else
                {
                    note = ObjectPool.poolInstance.GetNoteInPool(i);
                    if (note != null) { note.SetActive(true); }
                }
                if (note != null) { note.transform.position = notePosition; }

                prev = notePosition;
                n.model = note;
            }

            for (int j = 0; j < pattern.lines[i].noteList.Count; j++)
            {
                if (pattern.lines[i].noteList[j].extra == 1)
                {
                    pattern.lines[i].noteList.RemoveAt(j);
                    j--;
                }
            }
        }

        int barLineCount = pattern.barLine.noteList.Count;
        for (int i = barLineCount - 1; i >= barLineCount - 10; i--)
        {
            if(i < 0) { break; }
            Note bar = pattern.barLine.noteList[i];
            bar.model = ObjectPool.poolInstance.GetBarInPool();
            bar.model.SetActive(true);
            bar.model.transform.position = new Vector3(0.0f, (float)(bar.beat * speed), 0.0f);
        }
    }
}
