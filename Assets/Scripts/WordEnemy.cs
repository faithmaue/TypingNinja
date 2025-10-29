using UnityEngine;
using TMPro;

public class WordEnemy : MonoBehaviour
{
    public TMP_Text wordText;
    private string word;
    private bool isBoss;

    public string Word => word;
    public bool IsBoss => isBoss;

    public void Init(string word, bool boss)
    {
        this.word = word;
        this.isBoss = boss;
        wordText.text = word;
        wordText.color = boss ? Color.red : Color.white;
    }

    public void MoveDown(float baseSpeed, float speedGrowth, int score)
    {
        float speed = baseSpeed * Time.deltaTime * (1 + speedGrowth * score / 600f);
        transform.Translate(Vector3.down * speed);
    }

    public void HighlightMatch(int prefixLength)
    {
        string prefix = word.Substring(0, prefixLength);
        wordText.text = $"<color=#00FF80>{prefix}</color>{word.Substring(prefixLength)}";
    }
}
