using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        // Get all characters typed this frame
        foreach (char c in Input.inputString)
        {
            // Handle Backspace separately
            if (c == '\b')
            {
                gameManager.OnKeyPress("Backspace");
            }
            // Ignore Enter/Return
            else if (c == '\n' || c == '\r')
            {
                // do nothing for now
            }
            else
            {
                // Normal character
                gameManager.OnKeyPress(c.ToString());
            }
        }
    }
}

