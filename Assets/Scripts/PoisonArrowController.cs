using System.Collections;
using UnityEngine;

public class PoisonArrowController : MonoBehaviour
{
    private int poisonDamage;
    private float poisonDuration;

    public void Initialize(int damage, float duration)
    {
        poisonDamage = damage;
        poisonDuration = duration;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if hit player
        PlayerController playerHealth = collision.gameObject.GetComponent<PlayerController>();
        if (playerHealth != null)
        {
            // Apply poison damage over time
            StartCoroutine(ApplyPoisonDamage(playerHealth));
        }

        // Destroy arrow
        Destroy(gameObject);
    }

    private IEnumerator ApplyPoisonDamage(PlayerController player)
    {
        float elapsedTime = 0f;

        while (elapsedTime < poisonDuration)
        {
            player.TakeDamage(poisonDamage);
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
    }
}
