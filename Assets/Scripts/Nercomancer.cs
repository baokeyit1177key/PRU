using System.Collections;
using UnityEngine;

public class Nercomancer : BasicEnemyMap4
{
    public GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireVFXPrefab; // Assign a fire visual effect in the inspector
    [SerializeField] private int firePillarDamage = 10; // Damage amount for the fire pillar attack
    [SerializeField] private float firePillarRadius = 1f; // Radius of the fire pillar effect (centered on player)
    [SerializeField] private float firePillarDuration = 2f;
    [SerializeField] private int damgecountdown = 3;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    protected override void PerformAttack()
    {
        StartCoroutine(ShootWithDelay(0.5f));
        damgecountdown--;
        if(damgecountdown <= 0)
        {
            animator.SetTrigger("castSpell");
            StartCoroutine(PerformFirePillarAttack(0.3f));
            damgecountdown = 3;
        }
    }
    IEnumerator ShootWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 playerDirection = (player.transform.position - firePoint.position).normalized;
            GameObject projectileObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            projectileObj.transform.localScale = new Vector3(5f, 5f, 5f);
            Debug.DrawLine(firePoint.position, firePoint.position + (Vector3)(playerDirection * 5), Color.red, 3f);
            EnemyProjectiles projectile = projectileObj.GetComponent<EnemyProjectiles>();

            if (projectile != null)
            {
                // Set the reference to this enemy
                projectile.enemy = this;

                // Set the direction for the projectile to travel
                projectile.SetDirection(playerDirection);
            }
        }
    }

    IEnumerator PerformFirePillarAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
       
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Create the fire pillar effect at the player's position
            GameObject fireEffect = Instantiate(fireVFXPrefab, player.transform.position, Quaternion.identity);
            fireEffect.SetActive(true);
            
            float diameter = firePillarRadius * 2;
            fireEffect.transform.localScale = new Vector3(diameter, diameter, 1f);
            Animator pillarAnimator = fireEffect.GetComponent<Animator>();
            if (pillarAnimator != null)
            {
                pillarAnimator.SetTrigger("play");
            }
            yield return new WaitForSeconds(0.5f);

            // Check if player is still in the fire area before dealing damage
            if (Vector2.Distance(player.transform.position, fireEffect.transform.position) <= firePillarRadius)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(firePillarDamage);
                }
            }


            StartCoroutine(DealFireDamageOverTime(player.transform.position, fireEffect));

            
            Destroy(fireEffect, firePillarDuration);
        }
    }

    private IEnumerator DealFireDamageOverTime(Vector3 firePosition, GameObject fireEffect)
    {
        float elapsedTime = 0f;
        float tickInterval = 0.5f; 
        int tickDamage = 2; 

        while (fireEffect != null && elapsedTime < firePillarDuration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsedTime += tickInterval;
            if (fireEffect == null) break;
            // Check if any players are still in the radius
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(firePosition, firePillarRadius);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    PlayerController playerController = hitCollider.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.TakeDamage(tickDamage);
                    }
                }
            }
        }
        if (fireEffect != null)
        {
            Destroy(fireEffect);
        }
    }

    // Add this to visualize the fire radius in the editor (optional)
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); 
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.transform.position, firePillarRadius);
        }
    }
}
