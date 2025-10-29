using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TMP_Text skinText;
    private SaveData saveData;
    private string[] skins = { "Blue", "Green", "Red" };
    private int index = 0;

    void Start()
    {
        saveData = DataManager.Load();
        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] == saveData.selectedSkin)
                index = i;
        }
        UpdateSkinLabel();
    }

    public void NextSkin()
    {
        index = (index + 1) % skins.Length;
        saveData.selectedSkin = skins[index];
        DataManager.Save();
        UpdateSkinLabel();
    }

    public void PrevSkin()
    {
        index = (index - 1 + skins.Length) % skins.Length;
        saveData.selectedSkin = skins[index];
        DataManager.Save();
        UpdateSkinLabel();
    }

    void UpdateSkinLabel()
    {
        skinText.text = "Selected Skin: " + saveData.selectedSkin;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
