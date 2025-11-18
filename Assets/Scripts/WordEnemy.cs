using UnityEngine;
using TMPro;

public class WordEnemy : MonoBehaviour
{
    public TMP_Text wordText;
    private string word;
    private bool wordLevel;

    public string Word => word;
    public bool WordLevel => wordLevel;


    public void Init(string word, bool wordDiff)
    {
        this.word = word;
        this.wordLevel = wordDiff;
        wordText.text = word;
        wordText.color = Color.white; //boss ? Color.red : Color.white;
    }

    public void MoveDown(float baseSpeed, float speedGrowth, int score)
    {
        // For now: constant speed, ignore speedGrowth and score
        float speed = baseSpeed;

        // Move straight down in world space, frame-rate independent
        transform.position += Vector3.down * speed * Time.deltaTime;
    }


    public void HighlightMatch(int prefixLength)
    {
        string prefix = word.Substring(0, prefixLength);
        wordText.text = $"<color=#00FF80>{prefix}</color>{word.Substring(prefixLength)}";
    }
}
