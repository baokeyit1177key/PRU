using System.Collections;
using UnityEngine;
using static PlayerController;

public class PoisonArrowController : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidbody;
    private bool isPoisoned = false;
    [SerializeField] private int poisonDamage = 3;
    [SerializeField] private float poisonDuration = 5f;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {          
            if (collision.CompareTag("Enemy") || collision.CompareTag("Projectiles"))
            {
                return; // Ignore collision with the player
            }
            if (collision.CompareTag("Player"))
            {
            Debug.Log("Hit: " + collision.gameObject.name);
            PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    StartCoroutine(ApplyPoisonDamage(player));
                    rigidbody.linearVelocity = Vector2.zero;
                    Destroy(gameObject);
                }
            }
            boxCollider.enabled = false;
    }

    private IEnumerator ApplyPoisonDamage(PlayerController player)
    {
        if (isPoisoned) yield break;

        isPoisoned = true;
        float elapsedTime = 0f;
        int tickCount = 0;

        Debug.Log($"Starting poison effect on player. Duration: {poisonDuration}s, Damage per tick: {poisonDamage}");

        // Apply initial poison damage
        player.TakeDamage(poisonDamage, DamageType.Poison);
        tickCount++;
        Debug.Log($"Poison tick {tickCount}: Applied {poisonDamage} damage");

        // Continue applying damage over time
        while (elapsedTime < poisonDuration)
        {
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;

            // Check if player still exists and is valid
            if (player == null)
            {
                Debug.Log("Poison effect stopped: Player reference is null");
                break;
            }

            // Apply poison damage
            player.TakeDamage(poisonDamage, DamageType.Poison);
            tickCount++;
            Debug.Log($"Poison tick {tickCount}: Applied {poisonDamage} damage at time {elapsedTime}s");
        }

        Debug.Log($"Poison effect completed after {tickCount} ticks and {elapsedTime}s");
        isPoisoned = false;
    }
}
