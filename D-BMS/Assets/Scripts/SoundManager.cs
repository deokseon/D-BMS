using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : MonoBehaviour
{
    public bool isPrepared { get; set; } = false;
    public GameObject keySoundObject;
    public BMSMultiChannelAudioSource src;
    public List<KeyValuePair<int, string>> pathes { get; set; }
    public Dictionary<int, AudioClip> clips { get; set; }


    private static string[] soundFileExtensions;

    private void Awake()
    {
        pathes = new List<KeyValuePair<int, string>>();
        clips = new Dictionary<int, AudioClip>();

        if (soundFileExtensions == null)
            soundFileExtensions = new string[] { ".ogg", ".wav", ".mp3" };
    }

    public void AddAudioClips()
    {
        StartCoroutine(CAddAudioClips());
    }

    private IEnumerator CAddAudioClips()
    {
        int extensionFailCount;
        string musicFolderPath = BMSGameManager.header.musicFolderPath;
        int len = pathes.Count;
        for (int i = 0; i < len; i++)
        {
            UnityWebRequest uwr = null;
            extensionFailCount = 0;
            AudioType type = AudioType.OGGVORBIS;
            do
            {
                if (File.Exists(musicFolderPath + pathes[i].Value + soundFileExtensions[extensionFailCount])) break;
                extensionFailCount++;
            } while (extensionFailCount < 2);

            if (soundFileExtensions[extensionFailCount].CompareTo(".wav") == 0) { type = AudioType.WAV; }
            else if (soundFileExtensions[extensionFailCount].CompareTo(".mp3") == 0) { type = AudioType.MPEG; }

            uwr = UnityWebRequestMultimedia.GetAudioClip(
                @"file:\\" + musicFolderPath +
                UnityWebRequest.EscapeURL(pathes[i].Value + soundFileExtensions[extensionFailCount]).Replace('+', ' '), type);
            yield return uwr.SendWebRequest();

            if (uwr.downloadHandler.data.Length != 0)
            {
                AudioClip ac = DownloadHandlerAudioClip.GetContent(uwr);
                ac.LoadAudioData();
                clips.Add(pathes[i].Key, ac);
            }
        }

        isPrepared = true;
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
