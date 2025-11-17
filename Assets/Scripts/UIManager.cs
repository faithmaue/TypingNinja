using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]

public class UIManager : MonoBehaviour
{
    public GameObject player1NameField;
    public GameObject player2NameField;
    public GameObject player1ReadyButton;
    public GameObject player2ReadyButton;

    public GameObject singlePlayerButton;
    public GameObject multiPlayerButton;
    public GameObject skinsButton;
    public GameObject leaderboardButton;
    
    private string[] skins = { "Blue", "Green", "Red" };
    private int index = 0;
    private int numPlayers = 0;
    private int playersReady = 0;
    private static SaveData save = DataManager.Data;
    string skin = save.selectedSkin;
    List<PlayerRecord> history = save.history;

     

    void Start()
    {
        player1NameField.SetActive(false);
        player2NameField.SetActive(false);
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
        player1NameField.SetActive(true);
        player1ReadyButton.SetActive(true);
        player2NameField.SetActive(false);
        player2ReadyButton.SetActive(false);
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
            Debug.Log("game started");
            StartGame();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("LevelsScreen");
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
