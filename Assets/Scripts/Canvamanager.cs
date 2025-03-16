using UnityEngine;

public class Canvamanager : MonoBehaviour
{


    void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
}
