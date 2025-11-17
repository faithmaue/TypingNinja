using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PlayerRecord
{
    public string playerName;
    public int wordsTyped;
    public int mistakes;
    public float accuracy;
    public float wpm;
    public string skinUsed;
    public string timestamp;
}

[System.Serializable]
public class SaveData
{
    public string selectedSkin = "Blue"; 
    public List<string> unlocked = new List<string>() { "Blue" };

    public List<PlayerRecord> history = new List<PlayerRecord>();
}



public static class DataManager
{
    private static string path = Application.persistentDataPath + "/save.json";
    private static SaveData data;

    // -----------------------------
    // Load Save File
    // -----------------------------
    public static SaveData Load()
    {
        if (data != null) return data;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            data = new SaveData();
        }

        return data;
    }

    public static SaveData Data
    {
        get
        {
            Load();     // ensures data is instantiated
            return data;
        }
    }


    // -----------------------------
    // Save Save File
    // -----------------------------
    public static void Save()
    {
        if (data == null)
            data = new SaveData();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Saved game data to: " + path);
    }

    // -----------------------------
    // Record Player Performance
    // -----------------------------
    public static void RecordPlayerStats(string playerName, int wordsTyped, int mistakes, float wpm, string skinUsed)
    {
        Load();

        float accuracy = (wordsTyped == 0)
            ? 0
            : ((float)(wordsTyped - mistakes) / wordsTyped) * 100f;

        PlayerRecord record = new PlayerRecord
        {
            playerName = playerName,
            wordsTyped = wordsTyped,
            mistakes = mistakes,
            accuracy = accuracy,
            wpm = wpm,
            skinUsed = skinUsed,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        data.history.Add(record);
        Save();

        Debug.Log($"Recorded stats for {playerName}: WPM={wpm}, Mistakes={mistakes}, Accuracy={accuracy}%");
    }


    public static void UnlockSkin(string skinName)
    {
        Load();

        if (!data.unlocked.Contains(skinName))
        {
            data.unlocked.Add(skinName);
            Save();
            Debug.Log("Unlocked skin: " + skinName);
        }
    }

    public static void SelectSkin(string skinName)
    {
        Load();

        if (data.unlocked.Contains(skinName))
        {
            data.selectedSkin = skinName;
            Save();
            Debug.Log("Selected skin: " + skinName);
        }
        else
        {
            Debug.LogWarning("Tried to select a locked skin: " + skinName);
        }
    }
    
}
