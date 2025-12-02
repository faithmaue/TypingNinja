using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public InputField player1NameField;
    public InputField player2NameField;
    public GameObject player1ReadyButton;
    public GameObject player2ReadyButton;

    public GameObject singlePlayerButton;
    public GameObject multiPlayerButton;
    public GameObject leaderboardButton;

    public string player1Name = "none";
    public string player2Name = "none";

    private int numPlayers = 0;
    private int playersReady = 0;

    void Start()
    {
        // Hide name + ready UI until a mode is chosen
        player1NameField.gameObject.SetActive(false);
        player2NameField.gameObject.SetActive(false);

        player1ReadyButton.SetActive(false);
        player2ReadyButton.SetActive(false);
    }

    public void SinglePlayerName()
    {
        numPlayers = 1;

        player1NameField.gameObject.SetActive(true);
        player2NameField.gameObject.SetActive(false);

        player1ReadyButton.SetActive(true);
        player2ReadyButton.SetActive(false);
    }

    public void MultiPlayerName()
    {
        numPlayers = 2;

        player1NameField.gameObject.SetActive(true);
        player2NameField.gameObject.SetActive(true);

        player1ReadyButton.SetActive(true);
        player2ReadyButton.SetActive(true);
    }

    public void ReadyForGame()
    {
        playersReady++;

        if (player1NameField != null)
            player1Name = player1NameField.text;

        if (player2NameField != null)
            player2Name = player2NameField.text;

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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Quit()
    {
        Application.Quit();
    }
}

