using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : MonoBehaviour
{
    public bool isPrepared { get; set; } = false;
    public List<KeyValuePair<int, string>> pathes { get; set; }
    public Dictionary<int, AudioClip> clips { get; set; }


    private static string[] soundFileExtensions;

    private int channelLength = 127;

    private AudioClip bgSoundClip;
    private AudioSource[] bgSoundAudioArray;
    private AudioClip keySoundClip;
    private AudioSource[] keySoundAudioArray;

    private int currentBGSoundIndex;
    private int currentKeySoundIndex;

    private void Awake()
    {
        pathes = new List<KeyValuePair<int, string>>();
        clips = new Dictionary<int, AudioClip>();

        if (soundFileExtensions == null)
            soundFileExtensions = new string[] { ".ogg", ".wav", ".mp3" };

        currentBGSoundIndex = 0;
        currentKeySoundIndex = 0;

        bgSoundAudioArray = new AudioSource[channelLength];
        keySoundAudioArray = new AudioSource[channelLength];

        for (int i = 0; i < channelLength; i++)
        {
            bgSoundAudioArray[i] = gameObject.AddComponent<AudioSource>();
            bgSoundAudioArray[i].loop = false;
            bgSoundAudioArray[i].playOnAwake = false;
            bgSoundAudioArray[i].dopplerLevel = 0.0f;
            bgSoundAudioArray[i].reverbZoneMix = 0.0f;

            keySoundAudioArray[i] = gameObject.AddComponent<AudioSource>();
            keySoundAudioArray[i].loop = false;
            keySoundAudioArray[i].playOnAwake = false;
            keySoundAudioArray[i].dopplerLevel = 0.0f;
            keySoundAudioArray[i].reverbZoneMix = 0.0f;
        }
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
            BMSGameManager.currentLoading++;
        }

        isPrepared = true;
    }

    public void PlayKeySound(int key, float volume = 1.0f)
    {
        if (key == 0 || !clips.TryGetValue(key, out keySoundClip)) { return; }
        while (true)
        {
            currentKeySoundIndex = (currentKeySoundIndex + 1) % 127;

            if (!keySoundAudioArray[currentKeySoundIndex].isPlaying)
            {
                keySoundAudioArray[currentKeySoundIndex].PlayOneShot(keySoundClip, volume);
                break;
            }
        }
    }

    public void PlayBGSound(int key, float volume = 0.8f)
    {
        if (key == 0 || !clips.TryGetValue(key, out bgSoundClip)) { return; }
        while (true)
        {
            currentBGSoundIndex = (currentBGSoundIndex + 1) % 127;

            if (!bgSoundAudioArray[currentBGSoundIndex].isPlaying)
            {
                bgSoundAudioArray[currentBGSoundIndex].PlayOneShot(bgSoundClip, volume);
                break;
            }
        }
    }

    public void AudioAllStop()
    {
        for (int i = 0; i < channelLength; i++)
        {
            bgSoundAudioArray[i].Stop();
            keySoundAudioArray[i].Stop();
        }
    }
}
