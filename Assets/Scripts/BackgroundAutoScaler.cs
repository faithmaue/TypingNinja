using UnityEngine;

[ExecuteAlways]
public class BackgroundAutoScaler : MonoBehaviour
{
    void Update()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Sprite world size
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        // Screen world size (orthographic camera)
        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        // Scale so sprite fully covers the screen
        transform.localScale = new Vector3(
            worldWidth / spriteWidth,
            worldHeight / spriteHeight,
            1f
        );
    }
}
