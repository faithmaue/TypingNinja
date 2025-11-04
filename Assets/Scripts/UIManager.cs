using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TMP_Text skinText;
    private SaveData saveData;
    public GameObject player1NameField;
    public GameObject player2NameField;
    public GameObject player1ReadyButton;
    public GameObject player2ReadyButton;
    private string[] skins = { "Blue", "Green", "Red" };
    private int index = 0;
    private int numPlayers = 0;
    private int playersReady = 0;
     

    void Start()
    {
        player1NameField.SetActive(false);
        player2NameField.SetActive(false);
        player1ReadyButton.SetActive(false);
        player2ReadyButton.SetActive(false);
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

    public void SinglePlayerName()
    {
        numPlayers = 1;
        player1NameField.SetActive(true);
        player1ReadyButton.SetActive(true);
    }
    public void MultiPlayerName()
    {
        numPlayers = 2;
        player1NameField.SetActive(true);
        player2NameField.SetActive(true);
        player1ReadyButton.SetActive(true);
        player2ReadyButton.SetActive(true);
    }

    public void ReadyForGame()
    {
        playersReady++;
        if (playersReady == numPlayers)
        {
            StartGame();
        }
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
