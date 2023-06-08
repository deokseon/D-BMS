using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using FMODUnity;

public class SoundManager : MonoBehaviour
{
    private readonly object threadLock = new object();
    public int isPrepared;
    private const int keySoundMaxCount = 1300;
    public int threadCount;

    private float keySoundVolume;
    private float bgmVolume;

    private FMOD.System coreSystem;
    private FMOD.ChannelGroup channelGroup;
    private FMOD.Channel endChannel;
    private FMOD.Channel keySoundChannel;
    private FMOD.Channel bgmChannel;

    public List<KeyValuePair<int, string>> pathes;
    public FMOD.Sound[] keySoundArray;

    private readonly string[] soundFileExtension = { ".ogg", ".mp3", ".wav" };

    private void Awake()
    {
        isPrepared = 0;
        threadCount = Mathf.Max(SystemInfo.processorCount - 2, 1);

        keySoundVolume = PlayerPrefs.GetFloat("KeySoundVolume") * 0.7f + 0.3f;
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume") * 0.7f + 0.3f;

        coreSystem = RuntimeManager.CoreSystem;
        coreSystem.getMasterChannelGroup(out channelGroup);
        channelGroup.setVolume(PlayerPrefs.GetFloat("MasterVolume"));

        pathes = new List<KeyValuePair<int, string>>(keySoundMaxCount);
        keySoundArray = new FMOD.Sound[keySoundMaxCount];
    }

    public void AddAudioClips()
    {
        for (int i = 0; i < threadCount; i++)
        {
            AddAudioClipsAsync(i);
        }
    }

    async private void AddAudioClipsAsync(int value)
    {
        await Task.Run(() =>
        {
            int soundFileExtensionLength = soundFileExtension.Length;
            int start = (int)(value / (double)threadCount * pathes.Count);
            int end = (int)((value + 1) / (double)threadCount * pathes.Count);
            for (int i = start; i < end; i++)
            {
                string keySoundFilePath = BMSGameManager.header.musicFolderPath + pathes[i].Value;
                for (int j = 0; j < soundFileExtensionLength; j++)
                {
                    if (!File.Exists(keySoundFilePath + soundFileExtension[j])) { continue; }
                    var keySound = new FMOD.Sound();
                    coreSystem.createSound(keySoundFilePath + soundFileExtension[j], FMOD.MODE.CREATESAMPLE | FMOD.MODE._2D, out keySound);
                    keySoundArray[pathes[i].Key] = keySound;
                    break;
                }
                lock (threadLock)
                {
                    BMSGameManager.currentLoading++;
                }
            }
            lock (threadLock)
            {
                isPrepared++;
            }
        });
    }

    public void PlayKeySound(int keyIndex)
    {
        coreSystem.playSound(keySoundArray[keyIndex], channelGroup, true, out keySoundChannel);
        keySoundChannel.setVolume(keySoundVolume);
        keySoundChannel.setPaused(false);
    }

    public void PlayKeySoundEnd(int keyIndex)
    {
        coreSystem.playSound(keySoundArray[keyIndex], channelGroup, true, out endChannel);
        endChannel.setVolume(keySoundVolume);
        endChannel.setPaused(false);
    }

    public void PlayBGM(int keyIndex)
    {
        coreSystem.playSound(keySoundArray[keyIndex], channelGroup, true, out bgmChannel);
        bgmChannel.setVolume(bgmVolume);
        bgmChannel.setPaused(false);
    }

    public bool IsPlayingAudio()
    {
        bool isKeySoundChannelPlaying, isBGMChannelPlaying;
        keySoundChannel.isPlaying(out isKeySoundChannelPlaying);
        bgmChannel.isPlaying(out isBGMChannelPlaying);
        if (isKeySoundChannelPlaying || isBGMChannelPlaying) { return true; }
        else { return false; }
    }

    public void AudioPause(bool isPause)
    {
        channelGroup.setPaused(isPause);
    }

    public void AudioAllStop()
    {
        channelGroup.stop();
    }

    public void SoundAndChannelRelease()
    {
        for (int i = pathes.Count - 1; i >= 0; i--)
        {
            keySoundArray[pathes[i].Key].release();
        }
        channelGroup.release();
    }
}
