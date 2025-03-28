using System.Collections;
using UnityEngine;
using static PlayerController;

public class PoisonArrowController : MonoBehaviour
{
    private int poisonDamage;
    private float poisonDuration;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidbody;
    public void Initialize(int damage, float duration)
    {
        poisonDamage = damage;
        poisonDuration = duration;
    }
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
        float elapsedTime = 0f;

        while (elapsedTime < poisonDuration)
        {
            player.TakeDamage(poisonDamage, DamageType.Poison);
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;
        }
    }
}
