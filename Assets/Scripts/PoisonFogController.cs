using System.Collections;
using UnityEngine;

public class PoisonFogController : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private int poisonDamage = 5;

    private float radius = 2f;
    private CircleCollider2D fogCollider;

    private void Awake()
    {
        fogCollider = GetComponent<CircleCollider2D>();
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        if (fogCollider != null)
        {
            fogCollider.radius = radius;
        }
    }

    private void Start()
    {
        // Set collider radius
        if (fogCollider != null)
        {
            fogCollider.radius = radius;
        }

        // Start damage and self-destruction
        StartCoroutine(PoisonEffectCoroutine());
    }

    private IEnumerator PoisonEffectCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Damage players in the fog
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D hitCollider in hitColliders)
            {
                PlayerController playerHealth = hitCollider.GetComponent<PlayerController>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(poisonDamage);
                }
            }

            // Wait before next damage tick
            yield return new WaitForSeconds(damageInterval);
            elapsedTime += damageInterval;
        }

        // Destroy fog effect
        Destroy(gameObject);
    }
}
