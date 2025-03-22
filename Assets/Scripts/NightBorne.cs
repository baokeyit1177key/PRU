using UnityEngine;

public class NightBorne : BasicEnemyMap4
{
    protected override void PerformAttack()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // Loop through all colliders in range
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Check if the collider belongs to a player or another damageable entity
            if (hitCollider.CompareTag("Player"))
            {
                // Get the player controller and deal damage
                PlayerController player = hitCollider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
