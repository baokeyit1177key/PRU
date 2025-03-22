using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepstroyEnemyMap4 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damage = 10;

    [Header("Attack Animation Durations")]
    [SerializeField] private float attack1Duration = 0.35f; // Attack1 animation duration
    [SerializeField] private float attack2Duration = 0.20f; // Attack2 animation duration
    [SerializeField] private float idleDuration = 1f; // Duration to stay in idle before moving again

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("References")]
    [SerializeField] private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private bool isEnabled = false;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private bool isDead = false;
    private bool isMoving = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        animator.SetBool("IsEnable", false);
        animator.SetBool("IsWalk", false);
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (!isEnabled)
            {
                isEnabled = true;
                StopMovement();
                animator.SetBool("IsEnable", true);
                StartCoroutine(WaitForEnableAnimation());
            }
            else if (!animator.GetBool("IsEnable") && !isAttacking && !isMoving)
            {
                StartCoroutine(MoveTowardsPlayer());
            }
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator WaitForEnableAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetBool("IsEnable", false);
        StopMovement();
    }

    private IEnumerator MoveTowardsPlayer()
    {
        isMoving = true;
        animator.SetBool("IsWalk", true);

        float moveDuration = Random.Range(2f, 3f);
        float moveStartTime = Time.time;

        while (Time.time - moveStartTime < moveDuration)
        {
            if (isDead) break;
            if (isAttacking) break;

            if (animator.GetBool("IsWalk"))
            {
                MoveTowardPlayer();
            }
            else
            {
                StopMovement();
            }

            yield return null;
        }

        StopMovement();
        animator.SetBool("IsWalk", false);
        isMoving = false;

        // Check distance to decide attack type
        yield return StartCoroutine(PerformAttack());
    }

    private void MoveTowardPlayer()
    {
        if (!animator.GetBool("IsWalk"))
        {
            StopMovement();
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0);
        FlipTowardsPlayer();
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        StopMovement();

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float attackDuration;

        if (distanceToPlayer <= attackRange)
        {
            // Close attack (AttackPhysical)
            animator.SetTrigger("IsAttack2");
            attackDuration = attack2Duration;
        }
        else
        {
            // Range attack (New Animation)
            animator.SetTrigger("IsAttack1");
            attackDuration = attack1Duration;
        }

        // Wait for exact attack animation duration
        yield return new WaitForSeconds(attackDuration);

        // Apply damage if in range
        if (distanceToPlayer <= attackRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }

        // Switch to idle state
        animator.SetBool("IsIdle", true);

        // Stay in idle for the specified duration
        yield return new WaitForSeconds(idleDuration);

        // Turn off idle state
        animator.SetBool("IsIdle", false);

        // Reset attack state and cooldown
        isAttacking = false;
        cooldownTimer = attackCooldown;

        // Start moving towards player again
        if (!isDead)
        {
            StartCoroutine(MoveTowardsPlayer());
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopMovement();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        animator.SetBool("IsWalk", false);
        animator.SetTrigger("IsDead");
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 0.5f);
        Destroy(gameObject);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;
        float direction = player.position.x - transform.position.x;
        transform.localScale = new Vector3(Mathf.Sign(direction) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void FixedUpdate()
    {
        // Ensure enemy stays still when not in walk state
        if (!animator.GetBool("IsWalk") || isAttacking || isDead)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}