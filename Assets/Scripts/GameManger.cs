using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
// using System;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float baseFallSpeed = 60f;
    public float spawnInterval = 1.25f;
    public float spawnGrowth = 0.12f;
    public float speedGrowth = 0.06f;
    public int miniBossInterval = 100;
    public int megaBossInterval = 200;
    public int levelPoints = 300;
    public int skinUnlockEveryLevels = 3;

    [Header("References")]
    public WordEnemy enemyPrefab;
    public Transform enemyParent;
    public NinjaController ninja;
    public TMP_Text scoreText;
    public TMP_Text inputBufferText;
    public TMP_Text playerNameText;

    [Header("Backgrounds")]
    public SpriteRenderer backgroundRenderer;
    public List<Sprite> backgrounds;

    [Header("Sounds")]
    public AudioSource sfxSource;
    public AudioClip swooshClip;
    public AudioClip hitClip;
    public AudioClip bossClip;
    public int levelNum = 0;

    private float spawnTimer;
    private float elapsed;
    private int score;
    private string inputBuffer = "";
    private List<WordEnemy> enemies = new();
    private List<string> normalWords = new();
    private List<string> miniBossWords = new();
    private List<string> megaBossWords = new();
    private int prevLevel = 0;
    private int bgIndex = 0;
    private bool fading = false;
    private float fadeProgress = 0f;

    void Start()
    {
        LoadDictionaries();
        ResetGame();
    }

    void LoadDictionaries()
    {
        // Use TextAsset in Resources folder (dictionary.txt)
        TextAsset dict1 = Resources.Load<TextAsset>("dictionary1");
        TextAsset dict2 = Resources.Load<TextAsset>("dictionary2");
        TextAsset dict3 = Resources.Load<TextAsset>("dictionary3");
        if (dict1 != null)
        {
            string[] lines = dict1.text.Split('\n');
            foreach (string w in lines)
            {
                string word = w.Trim().ToLower();
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
                megaBossWords.Add(word);
            }
        }
        else
        {
            megaBossWords.AddRange(new string[] { "intermittenly", "revolutionary", "unquestionable", "misunderstood", "disproportionate"});
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

        UpdateUI();
    }

    void Update()
    {
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

        foreach (WordEnemy e in enemies)
        {
            if (e != null)
                e.MoveDown(baseFallSpeed, speedGrowth, score);
        }

        UpdateBackground();
        CheckGameOver();
    }

    void CheckGameOver()
    {
        foreach (WordEnemy e in enemies)
        {
            if (e != null && e.transform.position.y < -5f)
            {
                Debug.Log("Game Over!");
                // Add GameOver UI trigger here
            }
        }
    }

    void SpawnEnemy()
    {
        string word = normalWords[Random.Range(0, normalWords.Count)];
        Vector3 pos = new(Random.Range(-7f, 7f), 6f, 0);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, false); //, false);
        enemies.Add(newEnemy);
    }

    void SpawnMiniBoss()
    {
        string word = miniBossWords[Random.Range(0, miniBossWords.Count)];
        Vector3 pos = new(Random.Range(-7f, 7f), 6.5f, 0);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, true); //, true);
        enemies.Add(newEnemy);
        sfxSource.PlayOneShot(bossClip);
    }

    void SpawnMegaBoss()
    {
        string word = megaBossWords[Random.Range(0, megaBossWords.Count)];
        Vector3 pos = new(Random.Range(-7f, 7f), 6.5f, 0);
        WordEnemy newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);
        newEnemy.Init(word, true);
        enemies.Add(newEnemy);
        sfxSource.PlayOneShot(bossClip);
    }

    void UpdateBackground()
    {
        int level = score / levelPoints;
        if (level != prevLevel)
        {
            prevLevel = level;
            if (level > 0 && level % skinUnlockEveryLevels == 0)
            {
                OnLevelComplete(level);
            }
        }

        int group = level % (backgrounds.Count + 1);
        if (group != bgIndex && !fading)
        {
            bgIndex = group;
            backgroundRenderer.sprite = backgrounds[bgIndex];
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
        if (key == "Backspace")
        {
            if (inputBuffer.Length > 0)
                inputBuffer = inputBuffer[..^1];
            inputBufferText.text = inputBuffer;
            return;
        }

        inputBuffer += key.ToLower();
        inputBufferText.text = inputBuffer;

        foreach (WordEnemy e in enemies)
        {
            if (e.Word.StartsWith(inputBuffer))
            {
                e.HighlightMatch(inputBuffer.Length);
                if (inputBuffer == e.Word)
                {
                    score += 10 + e.Word.Length * 2; //+ (e.GetBoss ? 50 : 0);
                    ninja.PlayAnimation();
                    sfxSource.PlayOneShot(hitClip);
                    Destroy(e.gameObject);
                    enemies.Remove(e);
                    inputBuffer = "";
                    inputBufferText.text = "";
                    UpdateUI();
                    break;
                }
                return;
            }
        }

        // no matches
        inputBuffer = "";
        inputBufferText.text = "";
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
    }

}
