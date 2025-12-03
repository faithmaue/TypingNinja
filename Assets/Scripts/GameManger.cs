using UnityEngine;
using System.Collections.Generic;
using TMPro;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
using Unity.Burst.CompilerServices;
=======
using UnityEngine.SceneManagement;
>>>>>>> Stashed changes
=======
using UnityEngine.SceneManagement;
>>>>>>> Stashed changes

public class GameManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float baseFallSpeed = 2f;
    public float spawnInterval = 1.25f;
    public float spawnGrowth = 0.12f;
    public float speedGrowth = 0.06f;
    public int miniBossInterval = 100;
    public int megaBossInterval = 200;
    public int levelPoints = 300;
    public int skinUnlockEveryLevels = 3;

<<<<<<< Updated upstream
=======
    [Header("UI")]
    public GameObject gameOverPanel;
    //public GameObject levelCompletePanel;

    [Header("Panel Buttons (TMP Texts)")]
    public TMP_Text gameOverButtonText;        // Game Over panel button text
    //public TMP_Text levelCompleteButtonText;   // Next Level panel button text
    public TMP_Text gameOverTitleText;         // "Game Over" / "Player 1 Failed"
    public TMP_Text nextLevelTitleText;        // "Level Complete!" / "Player 1 Complete"
    public TMP_Text playerOneScoreLabel;
    public TMP_Text playerTwoScoreLabel;


>>>>>>> Stashed changes
    [Header("References")]
    public WordEnemy enemyPrefab;
    public Transform enemyParent;
    public NinjaController ninja;
    public TMP_Text scoreText;
    public TMP_Text inputBufferText;
    public TMP_Text playerNameText;
<<<<<<< Updated upstream

=======
    public TMP_Text timerText;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

    [Header("Backgrounds")]
    public SpriteRenderer backgroundRenderer;
    public List<Sprite> backgrounds = new();

    [Header("Sounds")]
    public AudioSource sfxSource;
    public AudioClip swooshClip;
    public AudioClip hitClip;
    public AudioClip bossClip;
    public int levelNum = 0;

    private float spawnTimer;
    private float elapsed;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    private int score;
=======
    private int score; // active player's current score (keeps UI/spawn logic simple)
    private bool isGameOver = false;
    private float levelTimer = 30f;
>>>>>>> Stashed changes
=======
    private int score; // active player's current score (keeps UI/spawn logic simple)
    private bool isGameOver = false;
    private float levelTimer = 30f;
>>>>>>> Stashed changes
    private string inputBuffer = "";
    private List<WordEnemy> enemies = new();
    private List<string> normalWords = new();
    private List<string> miniBossWords = new();
    private List<string> megaBossWords = new();
    private int prevLevel = 0;
    private int bgIndex = 0;
    private bool fading = false;
    private float fadeProgress = 0f;
    private UIManager playerUI;
    private string player1Name;
    private string player2Name;

    [Header("Multiplayer")]
    public bool isMultiplayer = false;
    private int currentPlayer = 1; // 1 or 2
    private int player1Score = 0;
    private int player2Score = 0;
    private int maxPlayers = 2;
    private int player1Finished = 0;
    private int player2Finished = 0;


    void Start()
    {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        playerUI = GetComponent<UIManager>();
        if(playerUI != null)
=======
=======
>>>>>>> Stashed changes
        // Persistent settings
        if (PlayerPrefs.HasKey("levelNum"))
            levelNum = PlayerPrefs.GetInt("levelNum");

        if (PlayerPrefs.HasKey("baseFallSpeed"))
            baseFallSpeed = PlayerPrefs.GetFloat("baseFallSpeed");

        if (PlayerPrefs.HasKey("bgIndex"))
            bgIndex = PlayerPrefs.GetInt("bgIndex");

        // Apply background
        if (backgrounds.Count > 0 && backgroundRenderer != null)
            backgroundRenderer.sprite = backgrounds[Mathf.Clamp(bgIndex, 0, backgrounds.Count - 1)];

        // Multiplayer flags set by UIManager prior to loading gameplay
        isMultiplayer = PlayerPrefs.GetInt("isMultiplayer", 0) == 1;
        currentPlayer = PlayerPrefs.GetInt("currentPlayer", 1); // will be 1 first

        // Load saved scores (if any)
        player1Score = PlayerPrefs.GetInt("player1Score", 0);
        player2Score = PlayerPrefs.GetInt("player2Score", 0);

        player1Finished = PlayerPrefs.GetInt("player1Finished", 0);
        player2Finished = PlayerPrefs.GetInt("player2Finished", 0);

        // Names: prefer UIManager (if persisted), otherwise PlayerPrefs
        playerUI = FindObjectOfType<UIManager>();
        if (playerUI != null)
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        {
            player1Name = playerUI.player1Name;
            player2Name = playerUI.player2Name;
            PlayerPrefs.SetString("player1Name", player1Name);
            PlayerPrefs.SetString("player2Name", player2Name);
        }
        else
        {
            player1Name = PlayerPrefs.GetString("player1Name", "Player 1");
            player2Name = PlayerPrefs.GetString("player2Name", "Player 2");
<<<<<<< Updated upstream
        }
        Debug.Log("Player 1 name: " + player1Name);

        if (PlayerPrefs.HasKey("currentPlayer"))
            currentPlayer = PlayerPrefs.GetInt("currentPlayer");
        else
        {   
            currentPlayer = 1;
            PlayerPrefs.SetInt("currentPlayer", currentPlayer);
            PlayerPrefs.Save();
        }

=======
        }

        if (PlayerPrefs.HasKey("currentPlayer"))
            currentPlayer = PlayerPrefs.GetInt("currentPlayer");
        else
        {   
            currentPlayer = 1;
            PlayerPrefs.SetInt("currentPlayer", currentPlayer);
            PlayerPrefs.Save();
        }

>>>>>>> Stashed changes
        if (levelNum == 0 && currentPlayer == 1)
        {
            Debug.Log("resetting point vals");
            PlayerPrefs.SetInt("player1Score", 0);
            PlayerPrefs.SetInt("player2Score", 0);
        }

        if (currentPlayer == 1)
            playerNameText.text = player1Name;
        else
            playerNameText.text = player2Name;

        // Active player's score -> local score used by UI/spawn logic
        score = (currentPlayer == 1) ? player1Score : player2Score;

        // Set UI to show the active player's name
        playerNameText.text = (currentPlayer == 1) ? player1Name : player2Name;

        LoadDictionaries();
        ResetGame();
    }

    void LoadDictionaries()
    {
        TextAsset dict1 = Resources.Load<TextAsset>("dictionary1");
        TextAsset dict2 = Resources.Load<TextAsset>("dictionary2");
        TextAsset dict3 = Resources.Load<TextAsset>("dictionary3");

        if (dict1 != null)
        {
            string[] lines = dict1.text.Split('\n');
            foreach (string w in lines)
            {
                string word = w.Trim().ToLower();
                if (!string.IsNullOrEmpty(word))
                    normalWords.Add(word);
            }
        }
        else
        {
            normalWords.AddRange(new string[] { "ninja", "shadow", "katana", "dojo", "honor", "stealth" });
        }

        if (dict2 != null)
        {
            string[] lines = dict2.text.Split('\n');
            foreach (string w in lines)
            {
                string word = w.Trim().ToLower();
                if (!string.IsNullOrEmpty(word))
                    miniBossWords.Add(word);
            }
        }
        else
        {
            miniBossWords.AddRange(new string[] { "lightning", "warriors", "dragons", "phantoms" });
        }

        if (dict3 != null)
        {
            string[] lines = dict3.text.Split('\n');
            foreach (string w in lines)
            {
                string word = w.Trim().ToLower();
                if (!string.IsNullOrEmpty(word))
                    megaBossWords.Add(word);
            }
        }
        else
        {
            megaBossWords.AddRange(new string[] { "intermittenly", "revolutionary", "unquestionable", "misunderstood", "disproportionate" });
        }
    }

    void ResetGame()
    {
        // Remove existing enemies
        foreach (Transform t in enemyParent)
            Destroy(t.gameObject);

        enemies.Clear();
        spawnTimer = 0f;
        elapsed = 0f;

        // Each player's turn begins fresh — ensure score is the player's saved score (usually 0)
        score = (currentPlayer == 1) ? player1Score : player2Score;

        inputBuffer = "";
        prevLevel = 0;
        // keep bgIndex as loaded
        fading = false;
        fadeProgress = 0f;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
        levelTimer = 30f;
>>>>>>> Stashed changes
=======
        levelTimer = 30f;
>>>>>>> Stashed changes

        UpdateUI();
    }

    void Update()
    {
<<<<<<< Updated upstream
=======
        if (isGameOver)
            return;

        // Timer
        levelTimer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(levelTimer);

        if (levelTimer <= 0f)
        {
            HandleLevelComplete();
            return;
        }

        // Spawn logic
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        float dt = Time.deltaTime;
        spawnTimer += dt;
        elapsed += dt;

        float dynamicInterval = Mathf.Max(0.35f, spawnInterval * (1 - spawnGrowth * Mathf.Min(1f, elapsed / 60f)));
        if (spawnTimer >= dynamicInterval)
        {
            spawnTimer = 0;
            if (score > 0 && score % miniBossInterval == 0 && (levelNum - 1) % 3 == 1)
                SpawnMiniBoss();
            else if (score > 0 && score % megaBossInterval == 0 && (levelNum - 1) % 3 == 2)
                SpawnMegaBoss();
            else
                SpawnEnemy();
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
        // Movement
>>>>>>> Stashed changes
=======
        // Movement
>>>>>>> Stashed changes
        foreach (WordEnemy e in enemies)
        {
            if (e != null);
                e.MoveDown(baseFallSpeed, speedGrowth, score);
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        UpdateBackground();
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        CheckGameOver();
    }

    void CheckGameOver()
    {
        foreach (WordEnemy e in enemies)
        {
            if (e != null && e.transform.position.y < -5f)
            {
<<<<<<< Updated upstream
                Debug.Log("Game Over!");
                // Add GameOver UI trigger here
=======
                HandleGameOver();
                break;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            }
        }
    }

    void SpawnEnemy()
    {
        string word = normalWords[Random.Range(0, normalWords.Count)];
        Vector3 pos = new Vector3(Random.Range(-7f, 7f), 5.5f, 0f);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, false);
        enemies.Add(newEnemy);
    }

    void SpawnMiniBoss()
    {
        string word = miniBossWords[Random.Range(0, miniBossWords.Count)];
        Vector3 pos = new(Random.Range(-7f, 7f), 6.5f, 0);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, true);
        enemies.Add(newEnemy);
        if (bossClip != null) sfxSource.PlayOneShot(bossClip);
    }

    void SpawnMegaBoss()
    {
        string word = megaBossWords[Random.Range(0, megaBossWords.Count)];
        Vector3 pos = new(Random.Range(-7f, 7f), 6.5f, 0);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, true);
        enemies.Add(newEnemy);
        if (bossClip != null) sfxSource.PlayOneShot(bossClip);
    }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
    void UpdateBackground()
    {
        if (backgroundRenderer == null || backgrounds.Count == 0)
            return;

        int level = score / levelPoints;
        if (level != prevLevel)
        {
            prevLevel = level;
            if (level > 0 && level % skinUnlockEveryLevels == 0)
            {
                OnLevelComplete(level);
            }
        }

        int group = level % backgrounds.Count;
        if (group != bgIndex && !fading)
        {
            bgIndex = group;
            backgroundRenderer.sprite = backgrounds[bgIndex];    // ⬅ CHANGED
            if (swooshClip != null)
                sfxSource.PlayOneShot(swooshClip);
        }
    }

=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    public static void OnLevelComplete(int level)
    {
        if (level == 4) DataManager.UnlockSkin("Black");
        if (level == 7) DataManager.UnlockSkin("Red");
    }

    public void OnKeyPress(string key)
    {
        if (isGameOver) return;

        if (key == "Backspace")
        {
            if (inputBuffer.Length > 0)
                inputBuffer = inputBuffer[..^1];
            inputBufferText.text = inputBuffer;
            return;
        }

        inputBuffer += key.ToLower();
        inputBufferText.text = inputBuffer;

        // If any enemy starts with the buffer, highlight the match (and check for complete word)
        for (int i = 0; i < enemies.Count; i++)
        {
            WordEnemy e = enemies[i];
            if (e == null) continue;

            if (e.Word.StartsWith(inputBuffer))
            {
                e.HighlightMatch(inputBuffer.Length);

                if (inputBuffer == e.Word)
                {
                    // correct full word
                    int award = 10 + e.Word.Length * 2;
                    AddScore(award);

                    ninja.PlayAnimation();
                    if (hitClip != null) sfxSource.PlayOneShot(hitClip);

                    Destroy(e.gameObject);
                    enemies.RemoveAt(i);

                    inputBuffer = "";
                    inputBufferText.text = "";
                    UpdateUI();
                }

                return; // we had at least a partial match so do not apply penalty
            }
        }

        // No matches -> penalty
        AddScore(-1);

        // clear buffer so player restarts typing
        inputBuffer = "";
        inputBufferText.text = "";
    }

    public void AddScore(int amount)
    {
        if (currentPlayer == 1)
        {
            player1Score += amount;
            player1Score = Mathf.Max(0, player1Score);
            score = player1Score;
            PlayerPrefs.SetInt("player1Score", player1Score);
        }
        else
        {
            player2Score += amount;
            player2Score = Mathf.Max(0, player2Score);
            score = player2Score;
            PlayerPrefs.SetInt("player2Score", player2Score);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
    }
<<<<<<< Updated upstream
=======

    void HandleGameOver()
    {
        Debug.Log(currentPlayer + "failed");
        PlayerPrefs.SetInt("player1Score", player1Score);
        PlayerPrefs.SetInt("player2Score", player2Score);
        PlayerPrefs.SetInt("currentPlayer", currentPlayer);
        PlayerPrefs.Save();

        if (isGameOver) return;
        isGameOver = true;

        DisableGameplayUI();
        gameOverPanel.SetActive(true);

        // Disable enemy movement
        foreach (WordEnemy e in enemies)
            if (e != null) e.enabled = false;

        // MULTIPLAYER LOGIC
        if (isMultiplayer)
        {
            if (currentPlayer == 1)
            {
                score = 0;
                inputBuffer = "";
                levelTimer = 30f;
                ResetEnemies();

                playerNameText.text = player2Name;

                // Update UI text
                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player1Name} Failed!";

                gameOverButtonText.text = "Player 2 Turn";
                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;

                gameOverPanel.SetActive(true);
                return;
            }
            else
            {
                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();

                gameOverPanel.SetActive(true);
                MultiLevelComplete();
                //return;
            }
        }
        else
        {
            // SINGLE PLAYER NORMAL BEHAVIOR
            if (gameOverTitleText != null)
                gameOverTitleText.text = "Level Failed!";

            gameOverButtonText.text = "Retry";
            gameOverPanel.SetActive(true);
        }
    }

    void ResetEnemies()
    {
        foreach (Transform t in enemyParent)
            Destroy(t.gameObject);

        enemies.Clear();
    }

    void DisableGameplayUI()
    {
        CanvasGroup cg = FindObjectOfType<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    public void RetryLevel()
    {
        if (isMultiplayer)
        {
            if (currentPlayer == 1)
            {
                // Switch player BEFORE showing the panel
                currentPlayer = 2;
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();

                // Reset the next player's turn state
                score = player2Score = 0;
                inputBuffer = "";
                levelTimer = 30f;
                ResetEnemies();

                playerNameText.text = player2Name;

                gameOverButtonText.text = "Player 2 Turn";

                gameOverPanel.SetActive(true);
                SceneManager.LoadScene("GameplayScreen");
            }
            else
            {
                if (player1Finished == 1 && player2Finished == 1)
                {
                    if (levelNum == 8)
                    {
                        BackToMenu();
                    }
                    else
                    {
                        NextLevel();
                    }
                }
                else
                {
                    BackToMenu();
                }
            }
        }
        else
        {
            if (gameOverTitleText.text.Contains("Failed!")) 
            {
                SceneManager.LoadScene("GameplayScreen");
            }
            else
            {
                if (levelNum == 8)
                {
                    BackToMenu();
                }
                else
                {
                    NextLevel();
                }
            }
        }
    }

    void HandleLevelComplete()
    {
        Debug.Log(currentPlayer + " completed");
        PlayerPrefs.SetInt("player1Score", player1Score);
        PlayerPrefs.SetInt("player2Score", player2Score);
        PlayerPrefs.SetInt("currentPlayer", currentPlayer);
        PlayerPrefs.Save();

        if (isGameOver) return;
        isGameOver = true;

        DisableGameplayUI();
        gameOverPanel.SetActive(true);

        // MULTIPLAYER LOGIC
        if (isMultiplayer)
        {
            if (currentPlayer == 1)
            {
                player1Finished = 1;
                PlayerPrefs.SetInt("player1Finished", player1Finished);
                PlayerPrefs.Save();

                score = 0;
                levelTimer = 30f;
                ResetEnemies();

                playerNameText.text = player2Name;

                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player1Name} Completed!";

                gameOverButtonText.text = "Player 2 Turn";
                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;
                gameOverPanel.SetActive(true);
                return;
            }
            else
            {
                player2Finished = 1;
                PlayerPrefs.SetInt("player2Finished", player2Finished);
                PlayerPrefs.Save();

                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player1Name} Completed!";

                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();

                gameOverPanel.SetActive(true);
                
                MultiLevelComplete();
            }
        }
        else
        {
            // Single Player
            if (gameOverTitleText != null)
                gameOverTitleText.text = "Level Complete!";
                
            gameOverButtonText.text = "Next Level";
            gameOverPanel.SetActive(true);
        }
    }

    public void MultiLevelComplete()
    {
        Debug.Log("p1 done: " + player1Finished + "; p2 done: " + player2Finished);
        if (player1Finished == 1 && player2Finished == 1)
        {
            Debug.Log("final - both completed");

            if (levelNum == 8)
            {
                gameOverButtonText.text = "Finish";
                if (player1Score > player2Score)
                {
                    gameOverTitleText.text = $"{player1Name} Wins!";
                }
                else if (player1Score < player2Score)
                {
                    gameOverTitleText.text = $"{player2Name} Wins!";
                }
                else
                {
                    gameOverTitleText.text = "It's a tie!";
                }
            }
            else
            {
                gameOverButtonText.text = "Next Level";
            }
        }
        else if (player1Finished == 1 && player2Finished == 0)
        {
            Debug.Log("final - p1 completed");
            gameOverButtonText.text = "Finish";
            gameOverTitleText.text = $"{player1Name} Wins!";
        }
        else if (player1Finished == 0 && player2Finished == 1)
        {
            Debug.Log("final - p2 completed");
            gameOverButtonText.text = "Finish";
            gameOverTitleText.text = $"{player2Name} Wins!";
        }
        else
        {
            Debug.Log("final - none completed");
            gameOverButtonText.text = "Finish";
            if (player1Score > player2Score)
            {
                gameOverTitleText.text = $"{player1Name} Wins!";
            }
            else if (player1Score < player2Score)
            {
                gameOverTitleText.text = $"{player2Name} Wins!";
            }
            else
            {
                gameOverTitleText.text = "It's a tie!";
            }
        }

    }

    public void NextLevel()
    {
        if (isMultiplayer && currentPlayer == 2)
        {
            currentPlayer = 1;
        }

        levelNum++;
        baseFallSpeed += 0.2f;
        PlayerPrefs.SetInt("levelNum", levelNum);
        PlayerPrefs.SetFloat("baseFallSpeed", baseFallSpeed);
        PlayerPrefs.SetInt("bgIndex", levelNum % backgrounds.Count);

        PlayerPrefs.SetInt("player1Finished", 0);
        PlayerPrefs.SetInt("player2Finished", 0);
        PlayerPrefs.SetInt("currentPlayer", currentPlayer);
        PlayerPrefs.SetInt("player1Score", player1Score);
        PlayerPrefs.SetInt("player2Score", player2Score);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameplayScreen");
    }

    public void BackToMenu()
    {
        // clear multiplayer state when returning to menu
        PlayerPrefs.SetInt("isMultiplayer", 0);
        PlayerPrefs.SetInt("currentPlayer", 1);
        PlayerPrefs.SetInt("player1Score", 0);
        PlayerPrefs.SetInt("player2Score", 0);
        PlayerPrefs.SetInt("player1Finished", 0);
        PlayerPrefs.SetInt("player2Finished", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MenuScreen");
    }

    public void Quit()
    {
        Application.Quit();
    }
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
}
