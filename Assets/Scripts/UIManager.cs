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

    
    private string[] skins = { "Blue", "Green", "Red" };
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
<<<<<<< Updated upstream
=======
        PlayerPrefs.SetInt("isMultiplayer", 0);

        player1NameField.gameObject.SetActive(true);
        player2NameField.gameObject.SetActive(false);

>>>>>>> Stashed changes
        player1ReadyButton.SetActive(true);
        player2ReadyButton.SetActive(false);
    }

    public void MultiPlayerName()
    {
        numPlayers = 2;
<<<<<<< Updated upstream
=======
        PlayerPrefs.SetInt("isMultiplayer", 1);

        player1NameField.gameObject.SetActive(true);
        player2NameField.gameObject.SetActive(true);

>>>>>>> Stashed changes
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

        if (playersReady == numPlayers)
        {
            // store multiplayer flag previously set by SinglePlayerName() / MultiPlayerName()
            // ensure names and starting player are stored for the gameplay scene
            PlayerPrefs.SetString("player1Name", player1Name);
            PlayerPrefs.SetString("player2Name", player2Name);
            PlayerPrefs.SetInt("currentPlayer", 1); // start with player 1

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

