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

    private const int channelCount = 300;

    private float keySoundVolume;
    private float bgmVolume;

    private AudioClip bgSoundClip;
    private List<AudioSource> bgSoundAudioList;
    private int bgSoundAudioListCount;
    private AudioClip keySoundClip;
    private List<AudioSource> keySoundAudioList;
    private int keySoundAudioListCount;

    private List<AudioSource> playingAudioList;

    private int maxBGSoundIndex;
    private int maxKeySoundIndex;
    private int currentBGSoundIndex;
    private int currentKeySoundIndex;

    private void Awake()
    {
        keySoundVolume = PlayerPrefs.GetFloat("KeySoundVolume") * 0.7f + 0.3f;
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume") * 0.7f + 0.3f;

        pathes = new List<KeyValuePair<int, string>>();
        clips = new Dictionary<int, AudioClip>();

        if (soundFileExtensions == null)
            soundFileExtensions = new string[] { ".ogg", ".wav", ".mp3" };

        currentBGSoundIndex = 0;
        currentKeySoundIndex = 0;

        bgSoundAudioList = new List<AudioSource>(channelCount);
        keySoundAudioList = new List<AudioSource>(channelCount);
        playingAudioList = new List<AudioSource>(channelCount);
        for (int i = 0; i < channelCount; i++)
        {
            bgSoundAudioList.Add(gameObject.AddComponent<AudioSource>());
            bgSoundAudioList[i].loop = false;
            bgSoundAudioList[i].playOnAwake = false;
            bgSoundAudioList[i].dopplerLevel = 0.0f;
            bgSoundAudioList[i].reverbZoneMix = 0.0f;

            keySoundAudioList.Add(gameObject.AddComponent<AudioSource>());
            keySoundAudioList[i].loop = false;
            keySoundAudioList[i].playOnAwake = false;
            keySoundAudioList[i].dopplerLevel = 0.0f;
            keySoundAudioList[i].reverbZoneMix = 0.0f;
        }
        bgSoundAudioListCount = bgSoundAudioList.Count;
        keySoundAudioListCount = keySoundAudioList.Count;
        maxBGSoundIndex = bgSoundAudioListCount - 2;
        maxKeySoundIndex = keySoundAudioListCount - 2;
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

            ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = false;
            ((DownloadHandlerAudioClip)uwr.downloadHandler).compressed = false;
            yield return uwr.SendWebRequest();

            if (uwr.downloadHandler.data.Length != 0)
            {
                AudioClip ac = DownloadHandlerAudioClip.GetContent(uwr);
                ac.LoadAudioData();
                clips.Add(pathes[i].Key, ac);
            }
            BMSGameManager.currentLoading++;
            //Debug.Log(pathes[i].Key);
        }

        isPrepared = true;
    }

    public void PlayKeySound(int key)
    {
        if (key == 0 || !clips.TryGetValue(key, out keySoundClip)) { return; }
        //Debug.Log(key);

        for (int i = keySoundAudioListCount; i > 0; i--)
        {
            currentKeySoundIndex = currentKeySoundIndex > maxKeySoundIndex ? 0 : currentKeySoundIndex + 1;
            if (!keySoundAudioList[currentKeySoundIndex].isPlaying)
            {
                keySoundAudioList[currentKeySoundIndex].PlayOneShot(keySoundClip, keySoundVolume);
                return;
            }
        }
    }

    public void PlayBGSound(int key)
    {
        if (key == 0 || !clips.TryGetValue(key, out bgSoundClip)) { return; }

        for (int i = bgSoundAudioListCount; i > 0; i--)
        {
            currentBGSoundIndex = currentBGSoundIndex > maxBGSoundIndex ? 0 : currentBGSoundIndex + 1;
            if (!bgSoundAudioList[currentBGSoundIndex].isPlaying)
            {
                bgSoundAudioList[currentBGSoundIndex].PlayOneShot(bgSoundClip, bgmVolume);
                return;
            }
        }
    }

    public void DividePlayingAudio()
    {
        for (int i = channelCount - 1; i >= 0; i--)
        {
            if (keySoundAudioList[i].isPlaying)
            {
                playingAudioList.Add(keySoundAudioList[i]);
                keySoundAudioList.RemoveAt(i);
                keySoundAudioListCount--;
                maxKeySoundIndex--;
            }
            if (bgSoundAudioList[i].isPlaying)
            {
                playingAudioList.Add(bgSoundAudioList[i]);
                bgSoundAudioList.RemoveAt(i);
                bgSoundAudioListCount--;
                maxBGSoundIndex--;
            }
        }
    }

    public bool IsPlayingAudioClip()
    {
        int audioListCount = playingAudioList.Count;
        for (int i = 0; i < audioListCount; i++)
        {
            if (playingAudioList[i].isPlaying) { return true; }
        }
        return false;
    }

    public void AudioAllStop()
    {
        for (int i = 0; i < keySoundAudioListCount; i++) { keySoundAudioList[i].Stop(); }
        for (int i = 0; i < bgSoundAudioListCount; i++) { bgSoundAudioList[i].Stop(); }
    }
}
