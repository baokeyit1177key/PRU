using UnityEngine;

public class MushroomMob : BasicEnemyMap4
{
    protected override void PerformAttack()
    {
        // Find all colliders within the attack range
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
            //else if (hitCollider.CompareTag("Damageable")) // Optional: for other entities that can take damage
            //{
            //    // Get other damageable component and deal damage
            //    IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            //    if (damageable != null)
            //    {
            //        damageable.TakeDamage(damageAmount);
            //    }
            //}
        }
    }
}
