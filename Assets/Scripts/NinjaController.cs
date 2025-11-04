using UnityEngine;
using System.Collections;

public class NinjaController : MonoBehaviour
{
    [Header("Sprite Renderer Target")]
    public SpriteRenderer targetRenderer;

    [Header("Blue Animation Frames (Levels 1-3)")]
    public Sprite[] blueFrames;

    [Header("Red Animation Frames (Levels 4-6)")]
    public Sprite[] redFrames;

    [Header("Green Animation Frames (Levels 7-9)")]
    public Sprite[] greenFrames;

    [Header("Settings")]
    public float frameRate = 0.2f;
    public int currentLevel = 1;

    private bool isAnimating = false;

    public void PlayAnimation()
    {
        if (!isAnimating)
            StartCoroutine(AnimateForLevel());
    }

    private IEnumerator AnimateForLevel()
    {
        isAnimating = true;

        Sprite[] chosenFrames = GetFramesForLevel(currentLevel);

        if (chosenFrames == null || chosenFrames.Length == 0)
        {
            Debug.LogWarning("No frames assigned for this level range!");
            yield break;
        }

        // Loop through frames
        for (int i = 0; i < chosenFrames.Length; i++)
        {
            targetRenderer.sprite = chosenFrames[i];
            yield return new WaitForSeconds(frameRate);
        }

        isAnimating = false;
    }

    private Sprite[] GetFramesForLevel(int level)
    {
        if (level >= 1 && level <= 3)
            return blueFrames;
        else if (level >= 4 && level <= 6)
            return redFrames;
        else if (level >= 7 && level <= 9)
            return greenFrames;

        return null;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayAnimation();
        }
    }

}
