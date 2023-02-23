using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static List<Queue<GameObject>> notePool;
    private static List<List<Queue<GameObject>>> longNotePool;
    private static Queue<GameObject> barPool;

    public int maxNoteCount;
    public int maxLongNoteCount;
    public int maxBarCount;

    [SerializeField]
    private GameObject[] note;
    [SerializeField]
    private GameObject[] longNoteEdgeBottom;
    [SerializeField]
    private GameObject[] longNoteEdgeTop;
    [SerializeField]
    private GameObject[] longNoteBody;
    [SerializeField]
    private GameObject barObject;

    private Transform noteParent;
    private Transform notePoolParent;

    public static ObjectPool poolInstance = null;

    private void Awake()
    {
        if (poolInstance == null) { poolInstance = this; }
        else if (poolInstance != this) { Destroy(this.gameObject); }

        DontDestroyOnLoad(this.gameObject);

        if (notePool == null)
        {
            notePool = new List<Queue<GameObject>>();
            for (int i = 0; i < 5; i++) { notePool.Add(new Queue<GameObject>()); }
        }
        if (longNotePool == null)
        {
            longNotePool = new List<List<Queue<GameObject>>>();
            for (int i = 0; i < 5; i++) { 
                longNotePool.Add(new List<Queue<GameObject>>());
                for (int j = 0; j < 3; j++) { longNotePool[i].Add(new Queue<GameObject>()); }
            }
        }
        if (barPool == null) { barPool = new Queue<GameObject>(); }

        notePoolParent = this.transform;

        CreateNotePool();
    }

    public void Init()
    {
        noteParent = GameObject.Find("Notes").transform;
        SetVerticalLine();
    }

    public float GetOffset() { return longNoteEdgeBottom[0].GetComponent<SpriteRenderer>().sprite.bounds.size.y * longNoteEdgeBottom[0].transform.localScale.y; }
    public float GetLength() { return 1.0f / longNoteBody[0].GetComponent<SpriteRenderer>().sprite.bounds.size.y; }

    private float GetLontNoteBodyVerticalLineLength()
    {
        GameObject temp = longNotePool[0][2].Dequeue();
        temp.SetActive(true);
        float len = longNoteBody[0].GetComponent<SpriteRenderer>().sprite.bounds.size.y /
                    temp.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        temp.SetActive(false);
        longNotePool[0][2].Enqueue(temp);
        return len;
    }

    private void SetVerticalLine()
    {
        float verticalLineLength = PlayerPrefs.GetInt("NoteSpeed") * 0.1f * PlayerPrefs.GetFloat("VerticalLine");
        float normalNoteVerticalLineYPosition = note[0].GetComponent<SpriteRenderer>().sprite.bounds.size.y * 0.5f;
        float longNoteBodyVerticalLineLength = GetLontNoteBodyVerticalLineLength();
        float longNoteBottomVerticalLineYPosition = longNoteEdgeBottom[0].GetComponent<SpriteRenderer>().sprite.bounds.size.y;
                                                
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < maxNoteCount; j++)
            {
                GameObject tempNoteObject = notePool[i].Dequeue();
                tempNoteObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempNoteObject.transform.GetChild(k).localPosition = new Vector3(k == 0 ? -1.25f : 1.25f, normalNoteVerticalLineYPosition, 0.0f);
                    tempNoteObject.transform.GetChild(k).localScale = new Vector3(verticalLineLength, 0.7f, 1.0f);
                }
                tempNoteObject.SetActive(false);
                notePool[i].Enqueue(tempNoteObject);
            }

            for (int j = 0; j < maxLongNoteCount; j++)
            {
                GameObject tempLongNoteObject = longNotePool[i][2].Dequeue();
                tempLongNoteObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteObject.transform.GetChild(k).localScale = 
                        new Vector3(verticalLineLength == 0.0f ? 0.0f : longNoteBodyVerticalLineLength, 0.7f, 1.0f);
                }
                tempLongNoteObject.SetActive(false);
                longNotePool[i][2].Enqueue(tempLongNoteObject);

                GameObject tempLongNoteBottomObject = longNotePool[i][0].Dequeue();
                tempLongNoteBottomObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteBottomObject.transform.GetChild(k).localPosition = new Vector3(k == 0 ? -1.25f : 1.25f, longNoteBottomVerticalLineYPosition, 0.0f);
                    tempLongNoteBottomObject.transform.GetChild(k).localScale = new Vector3(verticalLineLength, 0.7f, 1.0f);
                }
                tempLongNoteBottomObject.SetActive(false);
                longNotePool[i][0].Enqueue(tempLongNoteBottomObject);

                GameObject tempLongNoteTopObject = longNotePool[i][1].Dequeue();
                tempLongNoteTopObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteTopObject.transform.GetChild(k).localScale = new Vector3(verticalLineLength, 0.7f, 1.0f);
                }
                tempLongNoteTopObject.SetActive(false);
                longNotePool[i][1].Enqueue(tempLongNoteTopObject);
            }
        }
    }

    private void CreateNotePool()
    {
        for (int i = 0; i < 5; i++)
        {
            while (notePool[i].Count < maxNoteCount)
            {
                GameObject tempNote = Instantiate(note[i % 2], notePoolParent);
                tempNote.SetActive(false);
                notePool[i].Enqueue(tempNote);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject temp = (j == 2 ? longNoteBody[i % 2] : (j == 1 ? longNoteEdgeTop[i % 2] : longNoteEdgeBottom[i % 2]));
                while (longNotePool[i][j].Count < maxLongNoteCount)
                {
                    GameObject tempObject = Instantiate(temp, notePoolParent);
                    tempObject.SetActive(false);
                    longNotePool[i][j].Enqueue(tempObject);
                }
            }
        }
        while (barPool.Count < maxBarCount)
        {
            GameObject tempBar = Instantiate(barObject, notePoolParent);
            tempBar.SetActive(false);
            barPool.Enqueue(tempBar);
        }
    }

    public GameObject GetNoteInPool(int idx)
    {
        if (notePool[idx].Count > 0)
        {
            notePool[idx].Peek().transform.SetParent(noteParent);
            return notePool[idx].Dequeue();
        }
        else { return null; }
    }
    public void ReturnNoteInPool(int idx, GameObject note)
    {
        note.transform.SetParent(notePoolParent);
        notePool[idx].Enqueue(note);
    }

    public GameObject GetLongNoteInPool(int idx, int extra)
    {
        if (longNotePool[idx][extra].Count > 0)
        {
            longNotePool[idx][extra].Peek().transform.SetParent(noteParent);
            return longNotePool[idx][extra].Dequeue();
        }
        else { return null; }
    }
    public void ReturnLongNoteInPool(int idx, int extra, GameObject longNote)
    {
        longNote.transform.SetParent(notePoolParent);
        longNotePool[idx][extra].Enqueue(longNote);
    }

    public GameObject GetBarInPool()
    {
        if (barPool.Count > 0)
        {
            barPool.Peek().transform.SetParent(noteParent);
            return barPool.Dequeue();
        }
        else { return null; }
    }

    public void ReturnBarInPool(GameObject bar)
    {
        bar.transform.SetParent(notePoolParent);
        barPool.Enqueue(bar);
    }

    public void DestroyLongNote()
    {
        int len = noteParent.childCount;
        for (int i = 0; i < len; i++)
        {
            if (noteParent.GetChild(i).gameObject.activeSelf == true)
            {
                Destroy(noteParent.GetChild(i).gameObject);
            }
        }
    }
}
