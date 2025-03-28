using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDeathHandlerMap3 : MonoBehaviour
{
    [SerializeField] private GameObject boss; // Kéo GameObject của Boss vào đây từ Inspector
    [SerializeField] private string nextScene = "Map3"; // Tên scene cần chuyển sang, ở đây là "Map3"

    private bool isTransitioning = false; // Biến kiểm soát việc chuyển cảnh tránh load nhiều lần

    void Update()
    {
        // Nếu boss đã bị Destroy và chưa chuyển cảnh
        if (boss == null && !isTransitioning)
        {
            isTransitioning = true;
            Debug.Log("Boss đã bị tiêu diệt! Đang chuyển sang " + nextScene);
            SceneManager.LoadScene(nextScene);
        }
    }
}
