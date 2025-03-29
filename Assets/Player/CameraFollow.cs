using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public float FollowSpeed = 2f;
    [SerializeField] private string playerTag = "Player";
    public Transform target;

    [Header("Boss Detection Camera Effects")]
    public float zoomedOutSize = 8f;
    public float normalSize = 5f;
    public float zoomTransitionSpeed = 2f;
    private bool isBossDetected = false;

    private Camera mainCamera;
    private Vector3 originalPosition;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Store the original camera orthographic size
        originalPosition = transform.position;
        if (mainCamera != null)
        {
            normalSize = mainCamera.orthographicSize;
        }
        FindPlayer();
    }
    void OnEnable()
    {
        FindPlayer();
    }
    void Update()
    {
        if (target == null)
        {
            FindPlayer(); // Continuously try to find player if lost
            return;
        }

        if (isBossDetected)
        {
            // Zoom out and stop following
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, zoomedOutSize, zoomTransitionSpeed * Time.deltaTime);
        }
        else
        {
            // Normal camera following
            Vector3 newPos = new Vector3(target.position.x, target.position.y, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);

            // Smoothly return to normal size
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, normalSize, zoomTransitionSpeed * Time.deltaTime);
        }
    }

    // Method to be called by BossController when player is detected
    public void OnBossDetection(bool detected)
    {
        isBossDetected = detected;
    }
    private void FindPlayer()
    {
        // Try to find player by tag if target is null
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                target = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("No player found with tag: " + playerTag);
            }
        }
    }
}