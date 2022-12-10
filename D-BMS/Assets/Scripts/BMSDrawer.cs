using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSDrawer : MonoBehaviour
{
    public BMSPattern pattern;
    private float xPositionStart = -7.7f;

    [SerializeField]
    private Transform noteParent;

    public void DrawNotes()
    {
        float speed = BMSGameManager.gameSpeed;

        noteParent.position = Vector3.zero;

        pattern = BMSParser.instance.pattern;

        for (int i = 0; i < 5; i++)
        {
            float xPosition = xPositionStart + (0.57f * i);

            for (int j = pattern.normalNote[i].Count - 1; j >= 0; j--)
            {
                GameObject note = ObjectPool.poolInstance.GetNoteInPool(i);

                if (note == null) { break; }

                note.SetActive(true);
                note.transform.position = new Vector3(xPosition, (float)(pattern.normalNote[i][j].beat * speed), 0.0f);

                pattern.normalNote[i][j].model = note;
                pattern.normalNote[i][j].modelTransform = note.transform;
            }

            for (int j = pattern.longNote[i].Count - 1; j >= 0; j -= 3)
            {
                for (int k = 0; k < 3; k++)
                {
                    GameObject tempLongNote = ObjectPool.poolInstance.GetLongNoteInPool(i, k);

                    if (tempLongNote == null) { j = -1; break; }

                    tempLongNote.SetActive(true);
                    tempLongNote.transform.position = new Vector3(xPosition, (float)(pattern.longNote[i][j - ((k + 2) % 3)].beat * speed), 0.0f);
                    pattern.longNote[i][j - ((k + 2) % 3)].model = tempLongNote;
                    pattern.longNote[i][j - ((k + 2) % 3)].modelTransform = tempLongNote.transform;
                    if (k == 2)
                    {
                        pattern.longNote[i][j - ((k + 2) % 3)].model.transform.localScale =
                            new Vector3(0.3f, ((float)(pattern.longNote[i][j].beat - pattern.longNote[i][j - 2].beat) * speed - 0.3f) * 1.21577f, 1.0f);
                    }
                }
            }
        }

        int barLineCount = pattern.barLine.noteList.Count;
        int loopCount = barLineCount - ObjectPool.poolInstance.maxBarCount;
        for (int i = barLineCount - 1; i >= loopCount; i--)
        {
            if(i < 0) { break; }
            Note bar = pattern.barLine.noteList[i];
            bar.model = ObjectPool.poolInstance.GetBarInPool();
            bar.modelTransform = bar.model.transform;
            bar.model.SetActive(true);
            bar.model.transform.position = new Vector3(-6.56f, (float)(bar.beat * speed) - 0.285f, 0.0f);
        }
    }
}
