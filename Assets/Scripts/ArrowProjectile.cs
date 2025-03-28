using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private BossAttackController parentAttack;
    private LayerMask groundLayer;
    private LayerMask playerLayer;
    private bool hasHit = false;

    public void Initialize(BossAttackController parent, LayerMask ground, LayerMask player)
    {
        parentAttack = parent;
        groundLayer = ground;
        playerLayer = player;
        Destroy(gameObject, 5f); // Destroy arrow after 5 seconds if no collision
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Check ground hit
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            HandleGroundHit();
        }

        // Check player hit
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            HandlePlayerHit(collision.gameObject);
        }
    }

    private void HandleGroundHit()
    {
        hasHit = true;

        // Trigger ground hit animation
        parentAttack.animator.SetTrigger("ArrowGroundHit");

        // Damage nearby players
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            transform.position,
            1f,
            parentAttack.playerLayer
        );

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerController playerHealth = playerCollider.GetComponent<PlayerController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(parentAttack.groundHitDamage);
            }
        }

        Destroy(gameObject);
    }

    private void HandlePlayerHit(GameObject playerObject)
    {
        hasHit = true;

        // Damage player for deflecting
        PlayerController playerHealth = playerObject.GetComponent<PlayerController>();
        if (playerHealth != null)
        {
            Debug.Log(gameObject.name);
            playerHealth.TakeDamage(parentAttack.playerDeflectDamage);
        }

        // Destroy arrow
        Destroy(gameObject);
    }
}

