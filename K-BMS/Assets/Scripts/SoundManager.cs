using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject keySoundObject;
    public BMSMultiChannelAudioSource src;
    public List<KeyValuePair<int, string>> pathes { get; set; }
    public Dictionary<int, AudioClip> clips { get; set; }


    private void Awake()
    {
        pathes = new List<KeyValuePair<int, string>>();
        clips = new Dictionary<int, AudioClip>();
    }

    public void AddAudioClips()
    {
        int len = pathes.Count;
        for (int i = 0; i < len; i++)
        {
            AudioClip clip = Resources.Load<AudioClip>($"{BMSGameManager.header.musicFolderPath}{pathes[i].Value}");

            clips.Add(pathes[i].Key, clip);
        }
    }

    public void DeleteAudioClips()
    {
        clips = null;
    }

    public void PlayKeySound(int key, float volume = 1.0f)
    {
        if (key == 0) { return; }
        if (clips.ContainsKey(key)) { src.PlayKeySoundOneShot(clips[key], volume); }
    }

    public void PlayBGSound(int key, float volume = 0.8f)
    {
        if (key == 0) { return; }
        if (clips.ContainsKey(key)) { src.PlayBGSoundOneShot(clips[key], volume); }
    }
}
