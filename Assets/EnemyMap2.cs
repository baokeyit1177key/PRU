using System.Collections;
using UnityEngine;

public class EnemyMap2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damage = 15;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 80;
    [SerializeField] private float shieldDuration = 1f;

    [Header("Animation Durations")]
    [SerializeField] private float attackDuration = 0.4f;

    private int currentHealth;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isShielding = false;
    private float cooldownTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Kiểm tra trong tầm phát hiện
        if (distanceToPlayer <= detectionRange)
        {
            // Trong tầm đánh thì attack
            if (distanceToPlayer <= attackRange && !isAttacking && cooldownTimer <= 0)
            {
                StartCoroutine(PerformAttack());
            }
            // Trong tầm phát hiện thì di chuyển
            else if (!isAttacking && !isShielding)
            {
                MoveTowardPlayer();
            }
        }
        else
        {
            // Ngoài tầm phát hiện thì quay về Idle
            StopMovement();
            animator.SetBool("isWalk", false);
        }

        // Giảm cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void MoveTowardPlayer()
    {
        animator.SetBool("isWalk", true);
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Flip hướng
        transform.localScale = new Vector3(
            Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        StopMovement();

        animator.SetBool("isWalk", false);
        animator.SetBool("isAttack", true);

        // Wait for half of the attack animation before applying damage
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // Apply damage if player is still in range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }

        // Wait for the rest of the attack animation
        yield return new WaitForSeconds(attackDuration * 0.5f);

        animator.SetBool("isAttack", false);
        isAttacking = false;
        cooldownTimer = attackCooldown;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || isShielding) return;

        currentHealth -= damageAmount;

        // Khi máu dưới 50% thì chuyển shield
        if (currentHealth <= maxHealth / 2 && !isShielding)
        {
            StartCoroutine(PerformShield());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator PerformShield()
    {
        isShielding = true;
        StopMovement();

        animator.SetBool("isWalk", false);
        animator.SetBool("isAttack", false);
        animator.SetBool("isShield", true);

        yield return new WaitForSeconds(shieldDuration);

        animator.SetBool("isShield", false);
        isShielding = false;
    }

    private void Die()
    {
        isDead = true;
        StopMovement();

        animator.SetBool("isWalk", false);
        animator.SetBool("isAttack", false);
        animator.SetBool("isShield", false);
        animator.SetBool("isDead", true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}