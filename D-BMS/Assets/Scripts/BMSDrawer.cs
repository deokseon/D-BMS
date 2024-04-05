using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSDrawer : MonoBehaviour
{
    [SerializeField]
    private GameObject replayNoteObject;

    public void DrawNotes(bool isRestart)
    {
        BMSPattern pattern = BMSParser.instance.pattern;
        BMSGameManager bmsGameManager = GetComponent<BMSGameManager>();

        float speed = bmsGameManager.ConvertSpeed();
        float offset = ObjectPool.poolInstance.GetOffset();
        float longNoteLen = ObjectPool.poolInstance.GetLength();

        for (int i = 0; i < 5; i++)
        {
            for (int j = pattern.normalNote[i].Count - 1; j >= 0; j--)
            {
                GameObject note = ObjectPool.poolInstance.GetNoteInPool(i);

                if (note == null) { break; }

                note.SetActive(true);
                note.transform.localPosition = new Vector3(bmsGameManager.xPosition[i], (float)(pattern.normalNote[i][j].beat * speed), 0.0f);

                pattern.normalNote[i][j].model = note;
                pattern.normalNote[i][j].modelTransform = note.GetComponent<Transform>();
            }

            for (int j = pattern.longNote[i].Count - 1; j >= 0; j -= 3)
            {
                for (int k = 0; k < 3; k++)
                {
                    GameObject tempLongNote = ObjectPool.poolInstance.GetLongNoteInPool(i, k);

                    if (tempLongNote == null) { j = -1; break; }

                    tempLongNote.SetActive(true);
                    float yPos = (k == 2 ? (float)pattern.longNote[i][j].beat : (float)pattern.longNote[i][j - ((k + 2) % 3)].beat) * speed;
                    pattern.longNote[i][j - ((k + 2) % 3)].model = tempLongNote;
                    pattern.longNote[i][j - ((k + 2) % 3)].modelTransform = tempLongNote.GetComponent<Transform>();
                    if (k == 2)
                    {
                        pattern.longNote[i][j - ((k + 2) % 3)].model.transform.localScale =
                            new Vector3(1.0f, ((float)pattern.longNote[i][j - 1].beat * speed - offset) * longNoteLen, 1.0f);
                    }
                    pattern.longNote[i][j - ((k + 2) % 3)].modelTransform.localPosition = new Vector3(bmsGameManager.xPosition[i], yPos, 0.0f);
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
            bar.modelTransform = bar.model.GetComponent<Transform>();
            bar.model.SetActive(true);
            bar.model.transform.localPosition = new Vector3(bmsGameManager.xPosition[2], (float)(bar.beat * speed), 0.0f);
        }

        if (!BMSGameManager.isReplay || isRestart) { return; }

        Transform replayNoteParent = new GameObject("ReplayNoteParent").transform;
        replayNoteParent.SetParent(GameObject.Find("Notes").transform);
        replayNoteParent.localPosition = Vector3.zero;

        Sprite replayNoteSprite = FindObjectOfType<GameUIManager>().assetPacker.GetSprite("ReplayNote");
        float replayNoteSize = ObjectPool.poolInstance.GetNoteWidth() / replayNoteSprite.bounds.size.x;

        for (int i = 0; i < 5; i++)
        {
            for (int j = bmsGameManager.replayNoteArray[i].Length - 1; j >= 0; j--)
            {
                if (bmsGameManager.replayNoteArray[i][j].extra != 0 && bmsGameManager.replayNoteArray[i][j].extra != 5) { continue; }

                GameObject note = Instantiate(replayNoteObject, replayNoteParent);
                SpriteRenderer noteSpriteRenderer = note.GetComponent<SpriteRenderer>();
                noteSpriteRenderer.sprite = replayNoteSprite;
                noteSpriteRenderer.color = GetReplayNoteColor(bmsGameManager.replayNoteArray[i][j].diff, (bmsGameManager.replayNoteArray[i][j].extra == 5 && bmsGameManager.earlyLateThreshold == 220000) ? 550000 : bmsGameManager.earlyLateThreshold);
                note.transform.localPosition = new Vector3(bmsGameManager.xPosition[i], (float)(bmsGameManager.replayNoteArray[i][j].beat * speed), 0.0f);
                note.transform.localScale = new Vector3(replayNoteSize, replayNoteSize, 1.0f);
                bmsGameManager.replayNoteArray[i][j].model = note;
            }
        }
    }

    private Color GetReplayNoteColor(double diff, int threshold)
    {
        if (diff < -1750000.0d)
        {
            return Color.white;
        }

        return (diff <= threshold && diff >= -threshold) ? Color.green : (diff > 0 ? Color.red : Color.cyan);
    }
}
