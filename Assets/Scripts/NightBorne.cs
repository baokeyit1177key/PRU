using System.Collections;
using UnityEngine;

public class NightBorne : BasicEnemyMap3
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    protected override void PerformAttack()
    {
        StartCoroutine(DelayedDamage());
    }
    private IEnumerator DelayedDamage()
    {
        yield return new WaitForSeconds(0.48f); 

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerController player = hitCollider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage, PlayerController.DamageType.Normal);
                }
            }
        }
    }
    protected override IEnumerator HandleDeath()
    {
        Debug.Log(gameObject.name + " has died!");
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        StartCoroutine(Explode());
        
    }
    private IEnumerator Explode()
    {
        animator.SetTrigger("Explode");
        float explosionRadius = 8f;
        float minExplosionRadius = 0f; // Set minimum radius to 0 to ensure close-range damage
        int explosionDamage = 50;

        yield return new WaitForSeconds(0.4f);

        // Get ALL colliders in the explosion radius with no minimum distance
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        Debug.Log($"Found {hitColliders.Length} objects in explosion radius");

        foreach (Collider2D collider in hitColliders)
        {
            // Skip self
            if (collider.gameObject == gameObject) continue;

            float distanceToTarget = Vector2.Distance(transform.position, collider.transform.position);

            // Check if within explosion range (both minimum and maximum)
            if (distanceToTarget <= explosionRadius && distanceToTarget >= minExplosionRadius)
            {
                Debug.Log($"Hit object: {collider.gameObject.name} at distance {distanceToTarget}");

                if (collider.CompareTag("Player") || collider.CompareTag("Enemy"))
                {
                    // Optional: Calculate damage based on distance (more damage when closer)
                   float damageMultiplier = 1 - (distanceToTarget / explosionRadius);
                   int calculatedDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);

                    BasicEnemyMap3 enemy = collider.GetComponent<BasicEnemyMap3>();
                    if (enemy != null && enemy != this)
                    {
                        enemy.TakeDamage(calculatedDamage);
                    }

                    PlayerController player = collider.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        Debug.Log($"Player at distance {distanceToTarget} taking damage: {calculatedDamage}");

                        // Try with DamageType parameter first if your PlayerController uses it
                        try
                        {
                            player.TakeDamage(calculatedDamage, PlayerController.DamageType.Normal);
                        }
                        catch (System.Exception)
                        {
                            // Fall back to single parameter version
                            player.TakeDamage(calculatedDamage);
                        }
                    }

                    // Apply knockback in direction away from explosion center
                    Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        // Calculate knockback direction and force
                        Vector2 knockbackDirection = (collider.transform.position - transform.position).normalized;
                        float knockbackForce = 5f;

                        // Apply more knockback when closer to explosion center (optional)
                        // float knockbackMultiplier = 1 - (distanceToTarget / explosionRadius) + 0.2f;
                        // knockbackForce *= knockbackMultiplier;

                        // Apply the force
                        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                        Debug.Log($"Applied knockback to {collider.gameObject.name}: {knockbackDirection * knockbackForce}");
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
