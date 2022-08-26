using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSMultiChannelAudioSource : MonoBehaviour
{
    [SerializeField] private int channelLength;
    public int capacity { get; set; }

    private AudioSource[] bgSoundAudioArray;
    private AudioSource[] keySoundAudioArray;

    private int currentBGSoundIndex;
    private int currentKeySoundIndex;

    private void Awake()
    {
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

    public void PlayBGSoundOneShot(AudioClip clip, float volume)
    {
        while (true)
        {
            currentBGSoundIndex = (currentBGSoundIndex + 1) % 127;

            if (!bgSoundAudioArray[currentBGSoundIndex].isPlaying)
            {
                bgSoundAudioArray[currentBGSoundIndex].PlayOneShot(clip, volume);
                break;
            }
        }
    }

    public void PlayKeySoundOneShot(AudioClip clip, float volume)
    {
        while (true)
        {
            currentKeySoundIndex = (currentKeySoundIndex + 1) % 127;

            if (!keySoundAudioArray[currentKeySoundIndex].isPlaying)
            {
                keySoundAudioArray[currentKeySoundIndex].PlayOneShot(clip, volume);
                break;
            }
        }
    }
}
