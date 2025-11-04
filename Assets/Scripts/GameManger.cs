using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float baseFallSpeed = 60f;
    public float spawnInterval = 1.25f;
    public float spawnGrowth = 0.12f;
    public float speedGrowth = 0.06f;
    public int bossInterval = 200;
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

    private float spawnTimer;
    private float elapsed;
    private int score;
    private string inputBuffer = "";
    private List<WordEnemy> enemies = new();
    private List<string> normalWords = new();
    private List<string> bossWords = new();
    private int prevLevel = 0;
    private int bgIndex = 0;
    private bool fading = false;
    private float fadeProgress = 0f;

    void Start()
    {
        LoadDictionary();
        ResetGame();
    }

    void LoadDictionary()
    {
        // Use TextAsset in Resources folder (dictionary.txt)
        TextAsset dict = Resources.Load<TextAsset>("dictionary");
        if (dict != null)
        {
            string[] lines = dict.text.Split('\n');
            foreach (string w in lines)
            {
                string word = w.Trim().ToLower();
                if (word.Length > 0 && word.Length < 8) normalWords.Add(word);
                else if (word.Length >= 8) bossWords.Add(word);
            }
        }
        else
        {
            normalWords.AddRange(new string[] {"ninja","shadow","katana","dojo","honor","stealth"});
            bossWords.AddRange(new string[] {"lightning","warrior","dragon","phantom"});
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
            if (score > 0 && score % bossInterval == 0)
                SpawnBoss();
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
        newEnemy.Init(word, false);
        enemies.Add(newEnemy);
    }

    void SpawnBoss()
    {
        string word = bossWords[Random.Range(0, bossWords.Count)];
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
                DataManager.UnlockNextSkin();
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
                    score += 10 + e.Word.Length * 2 + (e.IsBoss ? 50 : 0);
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
