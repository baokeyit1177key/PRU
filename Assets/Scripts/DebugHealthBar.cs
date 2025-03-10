using UnityEngine;

public class DebugHealthBar : MonoBehaviour
{
    private void OnDisable()
    {
        Debug.Log($"[Debug] {gameObject.name} was disabled!", gameObject);
    }

    private void OnDestroy()
    {
        Debug.Log($"[Debug] {gameObject.name} was destroyed!", gameObject);
    }
}
