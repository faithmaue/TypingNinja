using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float baseFallSpeed = 1.5f; // Default fall speed
    public float spawnInterval = 2.5f; // seconds btwn words spawning
    public float spawnGrowth = 0.12f; // ???
    public float speedGrowth = 0.06f; // ??? 
    public int miniBossInterval = 50; // Points to get til mini boss word (OLD VERSION)
    public int megaBossInterval = 100; // Points to get til mega boss word (OLD VERSION)
    //public int levelPoints = 300; // What is this used for??
    public int skinUnlockEveryLevels = 3; // Levels in between each skin

    [Header("UI")]
    public GameObject gameOverPanel; // Panel shown after player death/ level completion

    [Header("Panel Buttons (TMP Texts)")]
    public TMP_Text gameOverButtonText;        // Game Over panel button text
    public TMP_Text gameOverTitleText;         // Status of game ("Game Over" / "Player 1 Failed")
    public TMP_Text playerOneScoreLabel;       // Score shown between levels (Player 1)
    public TMP_Text playerTwoScoreLabel;       // Score shown between levels (Player 2)

    [Header("References")]
    public WordEnemy enemyPrefab; // Prefab relating to spawning enemy words
    public Transform enemyParent; // Relating to all enemy words in game
    public NinjaController ninja; // Controls ninja sprite
    public TMP_Text scoreText; // Score label shown during game play
    public TMP_Text inputBufferText; // Text shown at bottom of screen (what the player types)
    public TMP_Text playerNameText; // Current player's name shown during game play
    public TMP_Text timerText; // Timer label shown during game play

    [Header("Backgrounds")]
    public SpriteRenderer backgroundRenderer;         // ⬅ CHANGED
    public List<Sprite> backgrounds = new();          // ⬅ CHANGED

    [Header("Sounds")]
    public AudioSource sfxSource; // Sound source game object
    public AudioClip swooshClip; // swoosh/swing sound 
    public AudioClip hitClip; // hit sound
    public AudioClip bossClip; // boss word sound
    public int levelNum = 0; // Curr level (0-8 instead of 1-9)
    private float spawnTimer; // Timer instantiation
    private float elapsed; // Amount of time elapsed
    private int score; // active player's current score (keeps UI/spawn logic simple)
    private bool isGameOver = false; // Tracks if level has been failed/ completed
    private float levelTimer = 60f; // total time for level
    private string inputBuffer = ""; // input text from player
    private List<WordEnemy> enemies = new(); // list of enemies on game screen
    private List<string> normalWords = new(); // Dictionary 1 words
    private List<string> miniBossWords = new(); // Dictionary 2 words
    private List<string> megaBossWords = new(); // Dictionary 3 words
    private int prevLevel = 0; // previous level???
    private int bgIndex = 0; // index of current background in list
    private bool fading = false; // ???
    private float fadeProgress = 0f; // ???
    private UIManager playerUI; // keeps track of player names (don't know why it's needed)
    private string player1Name; // Player 1 name
    private string player2Name; // Player 2 name

    [Header("Multiplayer")]
    public bool isMultiplayer = false; // Keeps track of if game is multiplayer
    private int currentPlayer = 1; // 1 or 2
    private int player1Score = 0; // First player score
    private int player2Score = 0; // Second player score
    private int player1Finished = 0; // Keeps track of if player 1 finished the current level
    private int player2Finished = 0; // Keeps track of if player 2 finished the current level

    private int streak = 0;
    private const int streakGoal = 5;
    private const int streakBonus = 50;
    private const int mistypePenalty = -10;
    public bool showInputBuffer = true;
    public GameObject disableBufferTextButton;
    public GameObject enableBufferTextButton;

    [Header("UI Popups")]
    public TMP_Text streakPopup;

    void Start()
    {
        //gameOverPanel.SetActive(false);

        const float DefaultBaseFallSpeed = 1.2f;
        const int DefaultLevelNum = 1;

        // Did we explicitly come here from NextLevel()?
        bool continueFromLevel = PlayerPrefs.GetInt("ContinueFromLevel", 0) == 1;

        if (continueFromLevel)
        {
            levelNum = PlayerPrefs.GetInt("levelNum", DefaultLevelNum);
            baseFallSpeed = PlayerPrefs.GetFloat("baseFallSpeed", DefaultBaseFallSpeed);

            PlayerPrefs.DeleteKey("ContinueFromLevel");
        }
        else
        {
            levelNum = DefaultLevelNum;
            baseFallSpeed = DefaultBaseFallSpeed;

            PlayerPrefs.DeleteKey("levelNum");
            PlayerPrefs.DeleteKey("baseFallSpeed");
            PlayerPrefs.DeleteKey("bgIndex");   // optional cleanup
        }

        Debug.Log("level: " + levelNum);

        // Determine background index from level number
        if (backgrounds != null && backgrounds.Count > 0)
        {
            bgIndex = Mathf.Clamp(levelNum - 1, 0, backgrounds.Count - 1);
        }

        // Apply background
        if (backgrounds.Count > 0 && backgroundRenderer != null)
        {
            int clampedIndex = Mathf.Clamp(bgIndex, 0, backgrounds.Count - 1);
            backgroundRenderer.sprite = backgrounds[clampedIndex];
        }

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

        // Make sure ninja skin matches the current level
        if (ninja != null)
            ninja.UpdateSkin(levelNum);

        // Multiplayer flags set by UIManager prior to loading gameplay
        isMultiplayer = PlayerPrefs.GetInt("isMultiplayer", 0) == 1;
        currentPlayer = PlayerPrefs.GetInt("currentPlayer", 1); // will be 1 first

        // Load saved scores
        player1Score = PlayerPrefs.GetInt("player1Score", 0);
        player2Score = PlayerPrefs.GetInt("player2Score", 0);

        // Load saved level completion
        player1Finished = PlayerPrefs.GetInt("player1Finished", 0);
        player2Finished = PlayerPrefs.GetInt("player2Finished", 0);

        // Names (Go with entered names, if not use "Player 1" and "Player 2")
        playerUI = FindObjectOfType<UIManager>();
        if (playerUI != null)
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
        }

        // Retrieves current player
        if (PlayerPrefs.HasKey("currentPlayer"))
            currentPlayer = PlayerPrefs.GetInt("currentPlayer");
        else
        {   
            currentPlayer = 1;
            PlayerPrefs.SetInt("currentPlayer", currentPlayer);
            PlayerPrefs.Save();
        }

        // Resets score if game was start over from beginning
        if (levelNum == 0 && currentPlayer == 1)
        {
            PlayerPrefs.SetInt("player1Score", 0);
            PlayerPrefs.SetInt("player2Score", 0);
        }

        // Modifies player name label to appropriate name
        if (currentPlayer == 1)
            playerNameText.text = player1Name;
        else
            playerNameText.text = player2Name;

        // Active player's score -> local score used by UI/spawn logic
        score = (currentPlayer == 1) ? player1Score : player2Score;

        // Set UI to show the active player's name
        playerNameText.text = (currentPlayer == 1) ? player1Name : player2Name;

        PlayerPrefs.SetInt("ShowInputBuffer", 1);
        inputBufferText.gameObject.SetActive(true);
        enableBufferTextButton.SetActive(false);

        LoadDictionaries();
        ResetGame();
    }

    void LoadDictionaries()
    {
        TextAsset dict1 = Resources.Load<TextAsset>("dictionary1");
        TextAsset dict2 = Resources.Load<TextAsset>("dictionary2");
        TextAsset dict3 = Resources.Load<TextAsset>("dictionary3");

        // Loads all words in dictionary 1 (if not use manually entered words)
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
        // Loads all words in dictionary 2 (if not use manually entered words)
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
        // Loads all words in dictionary 3 (if not use manually entered words)
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

    private List<string> GetCurrentDictionary()
    {
        // Levels 1–3  → dictionary1 (normalWords)
        if (levelNum >= 1 && levelNum <= 3)
            return normalWords;

        // Levels 4–6  → dictionary2 (miniBossWords)
        if (levelNum >= 4 && levelNum <= 6)
            return miniBossWords;

        // Levels 7–9+ → dictionary3 (megaBossWords)
        return megaBossWords;
    }

    void ResetGame()
    {
        // Remove existing enemies
        foreach (Transform t in enemyParent)
            Destroy(t.gameObject);

        enemies.Clear(); 
        spawnTimer = 0f; // Reset timer used to spawn words
        elapsed = 0f; // Reset elapsed time
        streak = 0;

        // Each player's turn begins fresh — ensure score is the player's saved score
        score = (currentPlayer == 1) ? player1Score : player2Score;

        inputBuffer = ""; // Reset user input
        prevLevel = levelNum;   // not super important now, but fine
        fading = false;
        fadeProgress = 0f;
        levelTimer = 60f;
        scoreText.text = "Score: " + score; // UpdateUI()


        if(timerText != null) {
            timerText.text = "Time: " +  Mathf.CeilToInt(levelTimer);
        }


    }

    void Update()
    {
        if (isGameOver)
            return;

        // Update Timer
        levelTimer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(levelTimer);

        // If time runs out before player dies, level is completed
        if (levelTimer <= 0f)
        {
            HandleLevelComplete();
            return;
        }

        // Spawn words
        float dt = Time.deltaTime;
        spawnTimer += dt;
        elapsed += dt;

        float dynamicInterval = Mathf.Max(0.35f, spawnInterval * (1 - spawnGrowth * Mathf.Min(1f, elapsed / 60f)));
        if (spawnTimer >= dynamicInterval)
        {
            spawnTimer = 0;
            if (levelNum >= 7 && levelNum <= 9)
                SpawnMiniBoss();
            else if (levelNum >= 4 && levelNum <= 6)
                SpawnMegaBoss();
            else
                SpawnEnemy();
        }

        // Movement
        foreach (WordEnemy e in enemies)
        {
            if (e != null)
                e.MoveDown(baseFallSpeed, speedGrowth, score);
        }
        // --- Background + fail check ---
        // UpdateBackground(); ///////////////// COMMENT OUT
        CheckGameOver();
    }

    void CheckGameOver()
    {
        // Check if any word on game board has reached the bottom of the screen, if so it is game over
        foreach (WordEnemy e in enemies)
        {
            if (e != null && e.transform.position.y < -5f)
            {
                HandleGameOver();
                break;
            }
        }
    }

    void SpawnEnemy()
    {
        List<string> dict = GetCurrentDictionary();
        if (dict == null || dict.Count == 0)
        {
            Debug.LogWarning("Active dictionary is empty – cannot spawn enemy word.");
            return;
        }

        string word = dict[Random.Range(0, dict.Count)];

        // Choose a random X within view, and a Y a bit above the top
        float x = Random.Range(-7f, 7f);   // you can tweak these later
        float y = 5.5f;                    // slightly above the visible area

        Vector3 pos = new Vector3(x, y, 0f);

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


    public static void OnLevelComplete(int level)
    {
        // Unlock black skin after 3 levels and red skin after 6
        if (level == 4) 
            DataManager.UnlockSkin("Black");
        if (level == 7) 
            DataManager.UnlockSkin("Red");
    }

    public void OnKeyPress(string key)
    {
        // Handle backspace
        if (key == "Backspace")
        {
            if (inputBuffer.Length > 0)
                inputBuffer = inputBuffer[..^1];

            inputBufferText.text = inputBuffer;
            return;
        }

        // Handle Enter (submit the current buffer)
        if (key == "Enter")
        {
            if (string.IsNullOrEmpty(inputBuffer))
                return;

            // Look for an exact match with the current buffer
            for (int i = 0; i < enemies.Count; i++)
            {
                WordEnemy e = enemies[i];
                if (e == null) continue;

                if (string.Equals(inputBuffer, e.Word, System.StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 hitPos = e.transform.position;
                    // Let the ninja move & slash at the word
                    ninja.SlashAt(hitPos, () =>
                    {
                        // Play SFX
                        if (hitClip != null && sfxSource != null)
                            sfxSource.PlayOneShot(hitClip);

                        // Remove enemy safely
                        if (e != null)
                        {
                            Destroy(e.gameObject);
                            enemies.Remove(e);   // safer than RemoveAt(i) in a callback
                        }

                        streak++;
                        if (streak == streakGoal)
                        {
                            ShowStreakPopup();
                            score += streakBonus;
                            streak = 0;
                        }

                        // ✅ Add score ONCE
                        score += 10 + e.Word.Length * 2;

                        // ✅ Reset input ONCE
                        inputBuffer = "";
                        inputBufferText.text = "";

                        scoreText.text = "Score: " + score; // UpdateUI()
                    });

                    // Don’t touch score or input here; the callback will handle it
                    return;
                }
            }

            // Enter pressed but no word matched → clear input
            inputBuffer = "";
            inputBufferText.text = "";
            streak = 0;
            score += mistypePenalty;
            return;
        }

        // Normal character input
        key = key.ToLowerInvariant();
        if (key == "x" || key == "v" || key == "z" || key == "q")
        {
            score += 5;
            scoreText.text = "Score: " + score; // update UI
        }

        inputBuffer += key;
        inputBufferText.text = inputBuffer;

        // Highlight prefix matches only
        for (int i = 0; i < enemies.Count; i++)
        {
            WordEnemy e = enemies[i];
            if (e == null) continue;

            if (e.Word.StartsWith(inputBuffer, System.StringComparison.OrdinalIgnoreCase))
            {
                e.HighlightMatch(inputBuffer.Length);
                return;
            }
        }

        // No enemy word starts with this buffer → reset
        inputBuffer = "";
        inputBufferText.text = "";
        streak = 0;
    }

    public void SetShowInputBuffer()
    {
        PlayerPrefs.SetInt("ShowInputBuffer", 1);
        inputBufferText.gameObject.SetActive(true);
        enableBufferTextButton.SetActive(false);
        disableBufferTextButton.SetActive(true);
    }
    public void SetDisableInputBuffer()
    {
        PlayerPrefs.SetInt("ShowInputBuffer", 0);
        inputBufferText.gameObject.SetActive(false);
        disableBufferTextButton.SetActive(false);
        enableBufferTextButton.SetActive(true);
    }


    void ShowStreakPopup()
    {
        streakPopup.text = "STREAK BONUS!";
        streakPopup.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        StartCoroutine(StreakPopupRoutine());
    }

    private IEnumerator StreakPopupRoutine()
    {
        streakPopup.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        streakPopup.gameObject.SetActive(false);
    }

    void HandleGameOver() // If player dies/ doesn't complete level
    {
        // Update player score and who just played
        Debug.Log(currentPlayer + " failed");
        PlayerPrefs.SetInt("player1Score", player1Score);
        PlayerPrefs.SetInt("player2Score", player2Score);
        PlayerPrefs.SetInt("currentPlayer", currentPlayer);
        PlayerPrefs.Save();

        if (isGameOver) return;
        isGameOver = true;

        DisableGameplayUI(); // Disable elements used during game play
        gameOverPanel.SetActive(true); // Activate panel for in between levels

        // Disable enemy movement
        foreach (WordEnemy e in enemies)
            if (e != null) e.enabled = false;

        // MULTIPLAYER LOGIC
        if (isMultiplayer)
        {
            if (currentPlayer == 1) // Player one just failed this level; go to player 2
            {
                player1Score = score;
                PlayerPrefs.SetInt("player1Score", player1Score);
                PlayerPrefs.SetInt("player2Score", player2Score);
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();
                // Reset score, input text, timer, and enemies
                /*score = player2Score;
                inputBuffer = "";
                levelTimer = 60f;
                ResetEnemies();

                playerNameText.text = player2Name; // Update player name label*/

                // Update panel text
                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player1Name} Failed!";

                gameOverButtonText.text = $"{player2Name}'s Turn";
                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;

                gameOverPanel.SetActive(true); // Make panel visible/ able to interact with
                return;
            }
            else // If player two just failed this level, current level is complete
            {
                player2Score = score;
                PlayerPrefs.SetInt("player1Score", player1Score);
                PlayerPrefs.SetInt("player2Score", player2Score);
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();
                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player2Name} Failed!";
                // Update panel text
                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();

                gameOverPanel.SetActive(true); // Make panel visible/ able to interact with
                MultiLevelComplete(); 
            }
        }
        else // Single player
        {
            // Update panel text
            if (gameOverTitleText != null)
                gameOverTitleText.text = "Level Failed!";

            gameOverButtonText.text = "Retry";
            gameOverPanel.SetActive(true); // Make panel visible/ able to interact with
        }
    }

    void ResetEnemies()
    {
        // Clear all enemies in list
        foreach (Transform t in enemyParent)
            Destroy(t.gameObject);

        enemies.Clear();
    }

    void DisableGameplayUI()
    {
        // Disable any game element in the game UI
        CanvasGroup cg = FindObjectOfType<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    public void RetryLevel()
    {
        Debug.Log("retry level called");
        if (isMultiplayer)
        {
            if (currentPlayer == 1) // Player 1 just finished their turn
            {
                // Switch player
                currentPlayer = 2;
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.SetInt("ContinueFromLevel", 1);
                PlayerPrefs.Save();

                // Reset the next player's turn state
                score = player2Score;
                inputBuffer = "";
                levelTimer = 60f;
                ResetEnemies();

                playerNameText.text = player2Name; // Update player name label

                //gameOverButtonText.text = $"{player2Name}'s Turn";

                //gameOverPanel.SetActive(true); // Make panel visible/ able to interact with
                SceneManager.LoadScene("GameplayScreen"); // Load same level for player 2
            }
            else // If player two just finished their turn
            {
                if (player1Finished == 1 && player2Finished == 1) // If both players passed
                {
                    if (levelNum == 8) // If on the last level, go back to main menu
                    {
                        BackToMenu();
                    }
                    else // If not on last level, go to next level (with increased difficulty)
                    {
                        NextLevel();
                    }
                }
                else // If one or both players failed the level, go back to main menu
                {
                    BackToMenu();
                }
            }
        }
        else // Single player
        {
            if (gameOverTitleText.text.Contains("Failed!")) // If player failed the level, replay same level
            {
                // Clear level progress so retry starts this level fresh
                PlayerPrefs.DeleteKey("ContinueFromLevel");
                PlayerPrefs.DeleteKey("levelNum");
                PlayerPrefs.DeleteKey("baseFallSpeed");
                PlayerPrefs.DeleteKey("bgIndex");

                SceneManager.LoadScene("GameplayScreen");
            }
            else // If player completed the level
            {
                if (levelNum == 8) // If at last level, go back to main menu
                {
                    BackToMenu();
                }
                else // If not at last level, go to next level (with increased difficulty)
                {
                    NextLevel();
                }
            }
        }
    }

    void HandleLevelComplete() // Player successfully completes level
    {
        // Save player scores and current player
        Debug.Log(currentPlayer + " completed");
        PlayerPrefs.SetInt("player1Score", player1Score);
        PlayerPrefs.SetInt("player2Score", player2Score);
        PlayerPrefs.SetInt("currentPlayer", currentPlayer);
        PlayerPrefs.Save();

        if (isGameOver) return;
        isGameOver = true;

        DisableGameplayUI(); // Disable elements used to play game
        gameOverPanel.SetActive(true); // Activate the game over screen

        // MULTIPLAYER LOGIC
        if (isMultiplayer)
        {
            if (currentPlayer == 1) // If player one successfully completed the level, player two takes turn
            {
                player1Score = score;
                PlayerPrefs.SetInt("player1Score", player1Score);
                PlayerPrefs.SetInt("player2Score", player2Score);
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();
                // Update variable to indicate player one finished the level
                player1Finished = 1;
                PlayerPrefs.SetInt("player1Finished", player1Finished);
                PlayerPrefs.Save();

                // Reset the next player's turn state
                /*score = player2Score;
                inputBuffer = "";
                levelTimer = 60f;
                ResetEnemies();

                playerNameText.text = player2Name; // Update player label to player 2*/

                // Update panel text
                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player1Name} Completed!";

                gameOverButtonText.text = $"{player2Name}'s Turn";
                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;
                gameOverPanel.SetActive(true); // Activate the game over screen
                return;
            }
            else // Second player just successfully completed the level
            {
                player2Score = score;
                PlayerPrefs.SetInt("player1Score", player1Score);
                PlayerPrefs.SetInt("player2Score", player2Score);
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();
                // Update variable to indicate player one finished the level
                player2Finished = 1;
                PlayerPrefs.SetInt("player2Finished", player2Finished);
                PlayerPrefs.Save();

                // Update panel text
                if (gameOverTitleText != null)
                    gameOverTitleText.text = $"{player1Name} Completed!";

                playerOneScoreLabel.text = $"{player1Name}: " + player1Score;
                playerTwoScoreLabel.text = $"{player2Name}: " + player2Score;
                PlayerPrefs.SetInt("currentPlayer", currentPlayer);
                PlayerPrefs.Save();

                gameOverPanel.SetActive(true); // Activate the game over screen
                
                MultiLevelComplete();
            }
        }
        else // Single Player
        {
            // Update panel text
            if (gameOverTitleText != null)
                gameOverTitleText.text = "Level Complete!";
                
            gameOverButtonText.text = "Next Level";
            gameOverPanel.SetActive(true); // Activate the game over screen
        }
    }

    public void MultiLevelComplete() // Both players took turn on level
    {
        if (player1Finished == 1 && player2Finished == 1) // If both players finished the level
        {
            if (levelNum == 8) // If at final level calculate and display winner
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
            else // If not at final level players will go to next level
            {
                gameOverButtonText.text = "Next Level";
            }
        }
        else if (player1Finished == 1 && player2Finished == 0) // If only player one finished level, end game (player 1 wins)
        {
            gameOverButtonText.text = "Finish";
            gameOverTitleText.text = $"{player1Name} Wins!";
        }
        else if (player1Finished == 0 && player2Finished == 1)  // If only player two finished level, end game (player 2 wins)
        {
            gameOverButtonText.text = "Finish";
            gameOverTitleText.text = $"{player2Name} Wins!";
        }
        else // If neither player finished the level, calculate and display winner
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

    }

    public void NextLevel()
    {
        if (isMultiplayer && currentPlayer == 2) // If in multiplayer and both player completed the level, reset current player
        {
            currentPlayer = 1;
        }

        // Reset game play values
        levelNum++;
        baseFallSpeed += 0.2f;
        PlayerPrefs.SetInt("ContinueFromLevel", 1);
        PlayerPrefs.SetInt("levelNum", levelNum);
        PlayerPrefs.SetFloat("baseFallSpeed", baseFallSpeed);

        // Reset player specific values
        PlayerPrefs.SetInt("player1Finished", 0);
        PlayerPrefs.SetInt("player2Finished", 0);
        PlayerPrefs.SetInt("currentPlayer", currentPlayer);
        PlayerPrefs.SetInt("player1Score", player1Score);
        PlayerPrefs.SetInt("player2Score", player2Score);
        PlayerPrefs.Save();

        // Load new level scene with updated values
        SceneManager.LoadScene("GameplayScreen");
    }

    public void BackToMenu()
    {
        PlayerPrefs.SetInt("ShowInputBuffer", 0);
        // New game from the menu should always be level 1 / speed 1.5 / first background
        PlayerPrefs.DeleteKey("ContinueFromLevel");
        PlayerPrefs.DeleteKey("levelNum");
        PlayerPrefs.DeleteKey("baseFallSpeed");
        PlayerPrefs.DeleteKey("bgIndex");

        // Clear multiplayer state
        PlayerPrefs.SetInt("isMultiplayer", 0);
        PlayerPrefs.SetInt("currentPlayer", 1);
        PlayerPrefs.SetInt("player1Score", 0);
        PlayerPrefs.SetInt("player2Score", 0);
        PlayerPrefs.SetInt("player1Finished", 0);
        PlayerPrefs.SetInt("player2Finished", 0);
        PlayerPrefs.Save();
        
        // Return to menu screen
        SceneManager.LoadScene("MenuScreen");
    }

    public void Quit()
    {
        PlayerPrefs.SetInt("ShowInputBuffer", 0);
        #if UNITY_EDITOR
        // Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Quit in a build
        Application.Quit();
        #endif
    }
}
