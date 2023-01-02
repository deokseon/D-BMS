using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyConfig
{
    public string[] keys;

    public KeyConfig(params string[] keys)
    {
        this.keys = keys;
    }
}

public class KeySettingManager : MonoBehaviour
{
    public static KeyConfig keyConfig;
    
    void Awake()
    {
        if (keyConfig == null)
        {
            LoadKeyConfig();
        }
    }

    public bool SetKey(int index, string key)
    {
        if (keyConfig.keys[index].Equals(key)) { return false; }
        for (int i = 0; i < 5; i++)
        {
            if (index == i) { continue; }
            if (keyConfig.keys[i].Equals(key)) { return false; }
        }
        keyConfig.keys[index] = key;
        return true;
    }

    public void SaveKeyConfig()
    {
        PlayerPrefs.SetString("Key1", keyConfig.keys[0]);
        PlayerPrefs.SetString("Key2", keyConfig.keys[1]);
        PlayerPrefs.SetString("Key3", keyConfig.keys[2]);
        PlayerPrefs.SetString("Key4", keyConfig.keys[3]);
        PlayerPrefs.SetString("Key5", keyConfig.keys[4]);
        LoadKeyConfig();
    }

    public void LoadKeyConfig()
    {
        if (PlayerPrefs.GetInt("KeySet") == 1)
        {
            keyConfig = new KeyConfig(
                PlayerPrefs.GetString("Key1"),
                PlayerPrefs.GetString("Key2"),
                PlayerPrefs.GetString("Key3"),
                PlayerPrefs.GetString("Key4"),
                PlayerPrefs.GetString("Key5")
                );
        }
        else
        {
            PlayerPrefs.SetInt("KeySet", 1);
            PlayerPrefs.SetString("Key1", "A");
            PlayerPrefs.SetString("Key2", "W");
            PlayerPrefs.SetString("Key3", "J");
            PlayerPrefs.SetString("Key4", "I");
            PlayerPrefs.SetString("Key5", "L");
            LoadKeyConfig();
        }
    }
}
