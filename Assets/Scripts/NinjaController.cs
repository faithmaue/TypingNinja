using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class NinjaController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Frames per second for both idle and swing animations")]
    public float fps = 10f;

    private SpriteRenderer sr;
    private readonly List<Sprite> idleFrames = new();
    private readonly List<Sprite> swingFrames = new();

    private bool isSwinging = false;
    private string currentSkin = "";

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Start as level 1 skin (Blue)
        UpdateSkin(1);
        StartCoroutine(IdleLoop());
    }

    /// <summary>
    /// Called by GameManager whenever the effective level changes.
    /// </summary>
    public void UpdateSkin(int level)
    {
        string newSkin = GetSkinForLevel(level);

        if (newSkin == currentSkin)
            return;  // already using this skin

        currentSkin = newSkin;
        LoadFrames();

        Debug.Log("Ninja skin changed to: " + newSkin);
    }

    private string GetSkinForLevel(int level)
    {
        if (level >= 1 && level <= 3)
            return "Blue";
        if (level >= 4 && level <= 6)
            return "Black";
        if (level >= 7 && level <= 9)
            return "Red";

        // if you go beyond level 9, just stay Red
        return "Red";
    }

    private void LoadFrames()
    {
        idleFrames.Clear();
        swingFrames.Clear();

        // NOTE: folder path is 'ninjas', NOT 'Ninjas'
        Sprite[] idle = Resources.LoadAll<Sprite>($"Ninjas/{currentSkin}/Idle");
        Sprite[] swing = Resources.LoadAll<Sprite>($"Ninjas/{currentSkin}/Swing");

        foreach (var f in idle)
            idleFrames.Add(f);
        foreach (var f in swing)
            swingFrames.Add(f);

        if (idleFrames.Count == 0)
        {
            Debug.LogError($"No idle frames found for skin '{currentSkin}'. " +
                           $"Check Resources/Ninjas/{currentSkin}/Idle.");
        }
        else
        {
            // Ensure we immediately show something instead of being blank
            sr.sprite = idleFrames[0];
        }

        if (swingFrames.Count == 0)
        {
            Debug.LogWarning($"No swing frames found for skin '{currentSkin}'. " +
                             $"Check Resources/Ninjas/{currentSkin}/Swing.");
        }
    }

    private IEnumerator IdleLoop()
    {
        int i = 0;

        while (true)
        {
            if (!isSwinging && idleFrames.Count > 0)
            {
                sr.sprite = idleFrames[i % idleFrames.Count];
                i++;
            }

            yield return new WaitForSeconds(1f / fps);
        }
    }

    /// <summary>
    /// Called by GameManager when the player correctly finishes a word and presses Enter.
    /// </summary>
    public void Swing()
    {
        if (!isSwinging && swingFrames.Count > 0)
            StartCoroutine(SwingAnimation());
    }

    private IEnumerator SwingAnimation()
    {
        isSwinging = true;

        foreach (var frame in swingFrames)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(1f / fps);
        }

        isSwinging = false;
    }
}

