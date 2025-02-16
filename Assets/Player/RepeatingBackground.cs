using UnityEngine;

public class RepeatingBackground : MonoBehaviour
{
    public float scrollSpeed = 2f; // Tốc độ di chuyển của nền
    private Vector2 startPosition;
    private float repeatWidth;

    void Start()
    {
        startPosition = transform.position;
        repeatWidth = GetComponent<SpriteRenderer>().bounds.size.x; // Lấy chiều rộng ảnh nền
    }

    void Update()
    {
        float newPosition = Mathf.Repeat(Time.time * scrollSpeed, repeatWidth);
        transform.position = startPosition + Vector2.left * newPosition;
    }
}
