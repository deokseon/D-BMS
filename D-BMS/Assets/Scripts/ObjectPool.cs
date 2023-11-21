using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static List<Queue<GameObject>> notePool;
    private static List<List<Queue<GameObject>>> longNotePool;
    private static Queue<GameObject> barPool;

    private GameUIManager gameUIManager = null;

    public int maxNoteCount;
    public int maxLongNoteCount;
    public int maxBarCount;

    [SerializeField]
    private GameObject[] note;
    [SerializeField]
    private GameObject[] longNoteBottom;
    [SerializeField]
    private GameObject[] longNoteTop;
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

    public void SetComponent()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
    }

    public void Init()
    {
        noteParent = GameObject.Find("Notes").transform;
    }

    public void SetNoteSprite()
    {
        float noteSize = GetNoteWidth() / gameUIManager.assetPacker.GetSprite("note1").bounds.size.x;
        float longNoteTopSize = GetNoteWidth() / gameUIManager.assetPacker.GetSprite("longnotetop1").bounds.size.x;
        float longNoteBottomSize = GetNoteWidth() / gameUIManager.assetPacker.GetSprite("longnotebottom1").bounds.size.x;
        float longNoteBodySize = GetNoteWidth() / gameUIManager.assetPacker.GetSprite("longnotebody1").bounds.size.x;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < maxNoteCount; j++)
            {
                GameObject tempNoteObject = notePool[i].Dequeue();
                tempNoteObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite($"note{(i % 2) + 1}");
                tempNoteObject.transform.GetChild(2).localScale = new Vector3(noteSize, noteSize, 1.0f);
                notePool[i].Enqueue(tempNoteObject);
            }

            for (int j = 0; j < maxLongNoteCount; j++)
            {
                GameObject tempLongNoteObject = longNotePool[i][2].Dequeue();
                tempLongNoteObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite($"longnotebody{(i % 2) + 1}");
                tempLongNoteObject.transform.GetChild(2).localScale = new Vector3(longNoteBodySize, 1.0f, 1.0f);
                longNotePool[i][2].Enqueue(tempLongNoteObject);

                GameObject tempLongNoteBottomObject = longNotePool[i][0].Dequeue();
                tempLongNoteBottomObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite($"longnotebottom{(i % 2) + 1}");
                tempLongNoteBottomObject.transform.GetChild(2).localScale = new Vector3(longNoteBottomSize, longNoteBottomSize, 1.0f);
                longNotePool[i][0].Enqueue(tempLongNoteBottomObject);

                GameObject tempLongNoteTopObject = longNotePool[i][1].Dequeue();
                tempLongNoteTopObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = gameUIManager.assetPacker.GetSprite($"longnotetop{(i % 2) + 1}");
                tempLongNoteTopObject.transform.GetChild(2).localScale = new Vector3(longNoteTopSize, longNoteTopSize, 1.0f);
                longNotePool[i][1].Enqueue(tempLongNoteTopObject);
            }
        }

        Sprite barSprite = gameUIManager.assetPacker.GetSprite("barline");
        float barSize = GetLineWidth() * 5.0f / barSprite.bounds.size.x;
        for (int i = 0; i < maxBarCount; i++)
        {
            GameObject tempBarObject = barPool.Dequeue();
            tempBarObject.GetComponent<SpriteRenderer>().sprite = barSprite;
            tempBarObject.transform.localScale = new Vector3(barSize, 1.0f, 1.0f);
            barPool.Enqueue(tempBarObject);
        }
    }

    public float GetNoteWidth() { return note[0].transform.GetChild(1).localPosition.x - note[0].transform.GetChild(0).localPosition.x - gameUIManager.assetPacker.GetSprite("verticalline").bounds.size.y; }
    public float GetLineWidth() { return note[0].transform.GetChild(1).localPosition.x - note[0].transform.GetChild(0).localPosition.x + gameUIManager.assetPacker.GetSprite("verticalline").bounds.size.y; }
    public float GetOffset() { return gameUIManager.assetPacker.GetSprite("longnotebottom1").bounds.size.y * (GetNoteWidth() / gameUIManager.assetPacker.GetSprite("longnotebottom1").bounds.size.x); }
    public float GetLength() { return 1.0f / gameUIManager.assetPacker.GetSprite("longnotebody1").bounds.size.y; }

    public void SetVerticalLineSprite()
    {
        Sprite verticalLineSprite = gameUIManager.assetPacker.GetSprite("verticalline");
        Sprite longNoteBodyVerticalLineSprite = gameUIManager.assetPacker.GetSprite("longnotebodyverticalline");
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < maxNoteCount; j++)
            {
                GameObject tempNoteObject = notePool[i].Dequeue();
                tempNoteObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempNoteObject.transform.GetChild(k).GetComponent<SpriteRenderer>().sprite = verticalLineSprite;
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
                    tempLongNoteObject.transform.GetChild(k).GetComponent<SpriteRenderer>().sprite = longNoteBodyVerticalLineSprite;
                }
                tempLongNoteObject.SetActive(false);
                longNotePool[i][2].Enqueue(tempLongNoteObject);

                GameObject tempLongNoteBottomObject = longNotePool[i][0].Dequeue();
                tempLongNoteBottomObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteBottomObject.transform.GetChild(k).GetComponent<SpriteRenderer>().sprite = verticalLineSprite;
                }
                tempLongNoteBottomObject.SetActive(false);
                longNotePool[i][0].Enqueue(tempLongNoteBottomObject);

                GameObject tempLongNoteTopObject = longNotePool[i][1].Dequeue();
                tempLongNoteTopObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteTopObject.transform.GetChild(k).GetComponent<SpriteRenderer>().sprite = verticalLineSprite;
                }
                tempLongNoteTopObject.SetActive(false);
                longNotePool[i][1].Enqueue(tempLongNoteTopObject);
            }
        }
    }

    public void SetVerticalLine()
    {
        float verticalLineLength = PlayerPrefs.GetInt("NoteSpeed") * 0.018f * PlayerPrefs.GetFloat("VerticalLine");
        float normalNoteVerticalLineYPosition = gameUIManager.assetPacker.GetSprite("note1").bounds.size.y * 0.5f * (GetNoteWidth() / gameUIManager.assetPacker.GetSprite("note1").bounds.size.x);
        float longNoteBodyVerticalLineLength = gameUIManager.assetPacker.GetSprite("longnotebody1").bounds.size.y / gameUIManager.assetPacker.GetSprite("longnotebodyverticalline").bounds.size.x;
        float longNoteBottomVerticalLineYPosition = GetOffset();
                                                
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < maxNoteCount; j++)
            {
                GameObject tempNoteObject = notePool[i].Dequeue();
                tempNoteObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempNoteObject.transform.GetChild(k).localPosition = new Vector3(k == 0 ? -0.375f : 0.375f, normalNoteVerticalLineYPosition, 0.0f);
                    tempNoteObject.transform.GetChild(k).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
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
                        new Vector3(verticalLineLength == 0.0f ? 0.0f : longNoteBodyVerticalLineLength, 1.0f, 1.0f);
                }
                tempLongNoteObject.SetActive(false);
                longNotePool[i][2].Enqueue(tempLongNoteObject);

                GameObject tempLongNoteBottomObject = longNotePool[i][0].Dequeue();
                tempLongNoteBottomObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteBottomObject.transform.GetChild(k).localPosition = new Vector3(k == 0 ? -0.375f : 0.375f, longNoteBottomVerticalLineYPosition, 0.0f);
                    tempLongNoteBottomObject.transform.GetChild(k).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
                }
                tempLongNoteBottomObject.SetActive(false);
                longNotePool[i][0].Enqueue(tempLongNoteBottomObject);

                GameObject tempLongNoteTopObject = longNotePool[i][1].Dequeue();
                tempLongNoteTopObject.SetActive(true);
                for (int k = 0; k < 2; k++)
                {
                    tempLongNoteTopObject.transform.GetChild(k).localScale = new Vector3(verticalLineLength, 1.0f, 1.0f);
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
                GameObject temp = (j == 2 ? longNoteBody[i % 2] : (j == 1 ? longNoteTop[i % 2] : longNoteBottom[i % 2]));
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

    public void NoteSpriteEmpty()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < maxNoteCount; j++)
            {
                GameObject tempNoteObject = notePool[i].Dequeue();
                for (int k = 0; k < 3; k++)
                {
                    tempNoteObject.transform.GetChild(k).GetComponent<SpriteRenderer>().sprite = null;
                }
                notePool[i].Enqueue(tempNoteObject);
            }

            for (int j = 0; j < maxLongNoteCount; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    GameObject tempLongNoteTopObject = longNotePool[i][k].Dequeue();
                    for (int t = 0; t < 3; t++)
                    {
                        tempLongNoteTopObject.transform.GetChild(t).GetComponent<SpriteRenderer>().sprite = null;
                    }
                    longNotePool[i][k].Enqueue(tempLongNoteTopObject);
                }
            }
        }

        for (int i = 0; i < maxBarCount; i++)
        {
            GameObject tempBarObject = barPool.Dequeue();
            tempBarObject.GetComponent<SpriteRenderer>().sprite = null;
            barPool.Enqueue(tempBarObject);
        }
    }
}
