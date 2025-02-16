using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Nhân vật để camera theo dõi
    public float smoothSpeed = 0.125f;
    public Vector3 offset;  // Độ lệch giữa camera và nhân vật

    void LateUpdate()
    {
        if (player == null) return;

        // Lấy vị trí mới dựa theo vị trí nhân vật
        Vector3 targetPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
