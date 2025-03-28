using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject boss; // Kéo GameObject Boss vào đây trong Inspector
    [SerializeField] private string nextScene = "Map2"; // Tên map cần chuyển

    private bool isTransitioning = false; // Để tránh load nhiều lần

    void Update()
    {
        if (boss == null && !isTransitioning) // Kiểm tra boss đã bị Destroy chưa
        {
            isTransitioning = true;
            Debug.Log("Boss đã bị tiêu diệt! Đang chuyển sang " + nextScene);
            SceneManager.LoadScene(nextScene);
        }
    }
}
