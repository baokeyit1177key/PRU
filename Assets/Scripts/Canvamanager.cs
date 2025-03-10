using UnityEngine;

public class Canvamanager : MonoBehaviour
{
    private static bool canvasExists = false;  // To ensure only one Canvas stays in the scene

    void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
}
