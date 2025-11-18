using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public InputField player1NameField;
    public InputField player2NameField;
    public GameObject player1ReadyButton;
    public GameObject player2ReadyButton;

    public GameObject singlePlayerButton;
    public GameObject multiPlayerButton;
    public GameObject skinsButton;
    public GameObject leaderboardButton;
    public string player1Name = "none";
    public string player2Name = "none";

    
    private string[] skins = { "Blue", "Black", "Red" };
    private int index = 0;
    private int numPlayers = 0;
    private int playersReady = 0;

    // No static here; and don't initialize from DataManager at field level
    private SaveData save;
    private string skin;
    private List<PlayerRecord> history;

    void Start()
    {
        // Get the save data AFTER Unity is running
        save = DataManager.Data;
        skin = save.selectedSkin;
        history = save.history;

        player1ReadyButton.SetActive(false);
        player2ReadyButton.SetActive(false);

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] == save.selectedSkin)
                index = i;
        }
    }

    public void NextSkin()
    {
        index = (index + 1) % skins.Length;
        save.selectedSkin = skins[index];
        DataManager.Save();
    }

    public void PrevSkin()
    {
        index = (index - 1 + skins.Length) % skins.Length;
        save.selectedSkin = skins[index];
        DataManager.Save();
    }

    public void SinglePlayerName()
    {
        numPlayers = 1;
        player1ReadyButton.SetActive(true);
        player2ReadyButton.SetActive(false);
    }

    public void MultiPlayerName()
    {
        numPlayers = 2;
        player1ReadyButton.SetActive(true);
        player2ReadyButton.SetActive(true);
    }

    public void ReadyForGame()
    {
        playersReady++;
        if (player1NameField != null)
        {
            player1Name = player1NameField.text;
        }
        if (player2NameField != null)
        {
            player2Name = player2NameField.text;
        }
       // Debug.Log("Player 1 name: " + player1Name);

        if (playersReady == numPlayers)
        {
            Debug.Log("game started");
            StartGame();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameplayScreen");
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

