using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float baseFallSpeed = 1.5f;
    public float spawnInterval = 2.5f;
    public float spawnGrowth = 0.12f;
    public float speedGrowth = 0.06f;
    public int miniBossInterval = 50;
    public int megaBossInterval = 100;
    public int levelPoints = 300;
    public int skinUnlockEveryLevels = 3;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;


    [Header("References")]
    public WordEnemy enemyPrefab;
    public Transform enemyParent;
    public NinjaController ninja;
    public TMP_Text scoreText;
    public TMP_Text inputBufferText;
    public TMP_Text playerNameText;
    public TMP_Text timerText;



    [Header("Backgrounds")]
    public SpriteRenderer backgroundRenderer;         // ⬅ CHANGED
    public List<Sprite> backgrounds = new();          // ⬅ CHANGED

    [Header("Sounds")]
    public AudioSource sfxSource;
    public AudioClip swooshClip;
    public AudioClip hitClip;
    public AudioClip bossClip;
    public int levelNum = 0;

    private float spawnTimer;
    private float elapsed;
    private int score;
    private bool isGameOver = false;
    private float levelTimer = 60f;
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

    private int streak = 0;
    private const int streakGoal = 3;
    private const int streakBonus = 50;
    private const int mistypePenalty = -10;
    
    [Header("UI Popups")]
    public TMP_Text streakPopup;
    void Start()
    {
        // Load previous level and ball speed values if they exist
        if (PlayerPrefs.HasKey("levelNum"))
            levelNum = PlayerPrefs.GetInt("levelNum");

        if (PlayerPrefs.HasKey("baseFallSpeed"))
            baseFallSpeed = PlayerPrefs.GetFloat("baseFallSpeed");

        if (PlayerPrefs.HasKey("bgIndex"))
            bgIndex = PlayerPrefs.GetInt("bgIndex");

        // Apply background
        if (backgrounds.Count > 0 && backgroundRenderer != null)
            backgroundRenderer.sprite = backgrounds[bgIndex];

        playerUI = FindObjectOfType<UIManager>();
        if(playerUI != null)
        {
            player1Name = playerUI.player1Name;
            player2Name = playerUI.player2Name;
        }

        playerNameText.text = player1Name;

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
        foreach (Transform t in enemyParent)
            Destroy(t.gameObject);

        enemies.Clear();
        spawnTimer = 0f;
        elapsed = 0f;
        score = 0;
        inputBuffer = "";
        prevLevel = 0;
        bgIndex = 0;
        fading = false;
        fadeProgress = 0f;
        levelTimer = 60f;


        UpdateUI();
    }

    void Update()
    {
        if (isGameOver)
            return;

        // --- Timer logic ---
        levelTimer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(levelTimer);

        if (levelTimer <= 0f)
        {
            HandleLevelComplete();
            return;
        }

        // --- Spawn logic ---
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

        // --- Movement of enemies ---
        foreach (WordEnemy e in enemies)
        {
            if (e != null)
                e.MoveDown(baseFallSpeed, speedGrowth, score);
        }

        // --- Background + fail check ---
        UpdateBackground(); ///////////////// COMMENT OUT
        CheckGameOver();
    }


    void CheckGameOver()
    {
        foreach (WordEnemy e in enemies)
        {
            if (e != null && e.transform.position.y < -5f)
            {
                HandleGameOver();
                break;  // don’t check further once we know it’s over
            }
        }
    }


    void SpawnEnemy()
    {
        string word = normalWords[Random.Range(0, normalWords.Count)];

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
        if (bossClip != null)
            sfxSource.PlayOneShot(bossClip);
    }

    void SpawnMegaBoss()
    {
        string word = megaBossWords[Random.Range(0, megaBossWords.Count)];
        Vector3 pos = new(Random.Range(-7f, 7f), 6.5f, 0);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, true);
        enemies.Add(newEnemy);
        if (bossClip != null)
            sfxSource.PlayOneShot(bossClip);
    }

    void UpdateBackground()
    {
        // Level starts at 1
        int level = score / levelPoints + 1;

        // Update ninja skin based on level
        if (ninja != null)
            ninja.UpdateSkin(level);

        if (level != prevLevel)
        {
            prevLevel = level;
            // If you still want to trigger skin unlock tracking, you can do:
            // OnLevelComplete(level);
        }

        if (backgrounds == null || backgrounds.Count == 0 || backgroundRenderer == null)
            return;

        int group = (level - 1) % backgrounds.Count;

        if (group != bgIndex && !fading)
        {
            bgIndex = group;
            backgroundRenderer.sprite = backgrounds[bgIndex];

            if (sfxSource != null && swooshClip != null)
                sfxSource.PlayOneShot(swooshClip);
        }
    }


    public static void OnLevelComplete(int level)
    {
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
                    // Score
                    score += 10 + e.Word.Length * 2;

                    // Ninja attack
                    if (ninja != null)
                        ninja.Swing();

                    // Hit SFX
                    if (hitClip != null && sfxSource != null)
                        sfxSource.PlayOneShot(hitClip);

                    // Remove the enemy
                    Destroy(e.gameObject);
                    enemies.RemoveAt(i);

                    // Reset input
                    inputBuffer = "";
                    inputBufferText.text = "";
                    UpdateUI();
                    return;
                }
            }

            // Enter pressed but no word matched → clear input
            inputBuffer = "";
            inputBufferText.text = "";
            return;
        }

        // Normal character input
        key = key.ToLowerInvariant();
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
    }

    void ShowStreakPopup()
{
   streakPopup.text = "STREAK BONUS!";
   streakPopup.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
   streakPopup.gameObject.SetActive(true);
}

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
    }

    void HandleGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over!");

        // Stop all movement/spawn logic
        // (Update() early-return already prevents new spawns)
        foreach (WordEnemy e in enemies)
        {
            if (e != null)
                e.enabled = false;  // if needed; mostly cosmetic
        }

        // Show Game Over UI
        gameOverPanel.SetActive(true);
    }

    public void RetryLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScreen");
    }


    void HandleLevelComplete()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Level Complete!");

        // Increase difficulty for next level
        //baseFallSpeed += 0.2f;
        //levelNum++;

        // Show Level Complete UI
        levelCompletePanel.SetActive(true);
    }

    public void NextLevel()
    {
        levelNum++;             // Increase the level
        baseFallSpeed += 0.2f;  // Increase difficulty

        // Store these values so they persist after the scene reload
        PlayerPrefs.SetInt("levelNum", levelNum);
        PlayerPrefs.SetFloat("baseFallSpeed", baseFallSpeed);

        // Change background index
        PlayerPrefs.SetInt("bgIndex", levelNum % backgrounds.Count);

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScreen");
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScreen");
    }

    public void Quit()
    {
        Application.Quit();
    }

}
