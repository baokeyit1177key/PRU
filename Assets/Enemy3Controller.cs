using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3Controller : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damage = 15;

    [Header("Attack Animation Durations")]
    [SerializeField] private float attackDuration = 0.4f; // Attack animation duration
    [SerializeField] private float idleDuration = 0.8f; // Duration to stay in idle before moving again

    [Header("Health")]
    [SerializeField] private int maxHealth = 80;
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
        animator.SetBool("IsIdle", false);
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
        ResetAllAnimationStates();
        animator.SetBool("IsWalk", true);

        float moveDuration = Random.Range(1.5f, 2.5f);
        float moveStartTime = Time.time;

        while (Time.time - moveStartTime < moveDuration)
        {
            if (isDead) break;
            if (isAttacking) break;

            if (animator.GetBool("IsWalk"))
            {
                MoveTowardPlayer();

                // Check if we're in attack range during movement
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange && cooldownTimer <= 0)
                {
                    break; // Break out of the movement loop to attack
                }
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

        // Check if we can attack
        float distanceAfterMove = Vector2.Distance(transform.position, player.position);
        if (distanceAfterMove <= attackRange && cooldownTimer <= 0)
        {
            yield return StartCoroutine(PerformAttack());
        }
        else
        {
            // Go to idle state briefly
            animator.SetBool("IsIdle", true);
            yield return new WaitForSeconds(idleDuration);
            animator.SetBool("IsIdle", false);

            // Resume movement
            if (!isDead)
            {
                StartCoroutine(MoveTowardsPlayer());
            }
        }
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

    private void ResetAllAnimationStates()
    {
        // Reset all animation triggers and bools
        animator.ResetTrigger("IsAttack");
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsIdle", false);
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        StopMovement();

        // Reset animation states before starting a new attack
        ResetAllAnimationStates();

        // Trigger attack animation
        animator.SetTrigger("IsAttack");

        // Wait for half of the attack animation before applying damage
        // This ensures damage is applied at the appropriate point in the animation
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

        // Clear previous attack state
        ResetAllAnimationStates();

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
        Debug.Log("Enemy3 taking damage: " + damage);
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Enemy3 current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy3 died, health <= 0");
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopMovement();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Clear all animation states before death
        ResetAllAnimationStates();

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