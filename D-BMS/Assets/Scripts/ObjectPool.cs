using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static List<Queue<GameObject>> notePool;
    private static Queue<GameObject> barPool;
    public int maxNoteCount;
    public int maxBarCount;
    [SerializeField]
    private GameObject[] note;
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
        if (barPool == null) { barPool = new Queue<GameObject>(); }

        notePoolParent = this.transform;

        CreateNotePool();
    }

    public void Init()
    {
        noteParent = GameObject.Find("Notes").transform;
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
