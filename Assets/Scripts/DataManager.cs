using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public string selectedSkin = "Blue";
    public string[] unlocked = { "Blue" };
}

public static class DataManager
{
    private static string path = Application.persistentDataPath + "/save.json";
    private static SaveData data;

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

    public static void Save()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static void UnlockNextSkin()
    {
        Load();
        string[] all = { "Blue", "Green", "Red" };
        foreach (string s in all)
        {
            bool unlocked = System.Array.Exists(data.unlocked, u => u == s);
            if (!unlocked)
            {
                System.Array.Resize(ref data.unlocked, data.unlocked.Length + 1);
                data.unlocked[^1] = s;
                Save();
                Debug.Log("Unlocked skin: " + s);
                return;
            }
        }
    }
}
