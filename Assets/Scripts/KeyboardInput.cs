using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        // Handle non-character keys first
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            gameManager.OnKeyPress("Enter");
            return;
        }

        // Existing logic for characters typed this frame
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                // Backspace
                gameManager.OnKeyPress("Backspace");
            }
            else if (c == '\n' || c == '\r')
            {
                // Enter (handled above), so ignore here
            }
            else
            {
                // Normal character
                gameManager.OnKeyPress(c.ToString());
            }
        }
    }
}


