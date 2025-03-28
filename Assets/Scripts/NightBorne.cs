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
                    player.TakeDamage(attackDamage);
                }
            }
        }
    }
    protected override IEnumerator HandleDeath()
    {
        Debug.Log(gameObject.name + " has died!");
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Explode();
        
    }
    private void Explode()
    {
        animator.SetTrigger("Explode");
        float explosionRadius = 4f;
        int explosionDamage = 50;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Player") || collider.CompareTag("Enemy"))
            {
                BasicEnemyMap4 enemy = collider.GetComponent<BasicEnemyMap4>();
                if (enemy != null && enemy != this)
                {
                    enemy.TakeDamage(explosionDamage); // Damage other enemies
                }

                PlayerController player = collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(explosionDamage); // Damage player
                }

                Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 knockback = (collider.transform.position - transform.position).normalized * 5f;
                    rb.AddForce(knockback, ForceMode2D.Impulse);
                }
            }
        }
        Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
