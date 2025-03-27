using UnityEngine;
using System.Collections;


public class BossController_2 : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isHurt = false;

    [Header("Movement Settings")]
    public float normalMoveSpeed = 3f;
    public float hurtPhaseMoveSpeed = 5f;
    private float currentMoveSpeed;

    [Header("Detection Settings")]
    public float detectionRadius = 10f;
    private Transform player;

    [Header("Targeting Settings")]
    public float walkDuration = 2f;
    public float walkCooldown = 3f;
    private bool isWalking = false;
    private Vector2 walkDirection;

    [Header("Attack Settings")]
    public float normalAttackCooldown = 2f;
    public float hurtPhaseAttackCooldown = 1f;
    private float currentAttackCooldown;
    public float normalDamage = 10f;
    public float hurtPhaseDamage = 20f;
    private float currentDamage;

    [Header("Phase Settings")]
    private bool isInHurtPhase = false;
    private const float HURT_PHASE_THRESHOLD = 0.5f;

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        currentMoveSpeed = normalMoveSpeed;
        currentDamage = normalDamage;
        currentAttackCooldown = normalAttackCooldown;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(TargetPlayerRoutine());
    }

    void FixedUpdate()
    {
        if (isWalking && player != null)
        {
            // Move towards player
            rb.linearVelocity = walkDirection * currentMoveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    IEnumerator TargetPlayerRoutine()
    {
        while (true)
        {
            // Wait for cooldown
            yield return new WaitForSeconds(walkCooldown);

            // Check if player is within detection radius
            if (player != null &&
                Vector2.Distance(transform.position, player.position) <= detectionRadius)
            {
                // Start walking towards player
                StartWalkingTowardsPlayer();

                // Walk for specified duration
                yield return new WaitForSeconds(walkDuration);

                // Stop walking
                StopWalking();
            }
        }
    }

    void StartWalkingTowardsPlayer()
    {
        if (player == null) return;

        isWalking = true;
        walkDirection = (player.position - transform.position).normalized;

        // Update animator
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", true);
    }

    void StopWalking()
    {
        isWalking = false;
        rb.linearVelocity = Vector2.zero;

        // Update animator
        animator.SetBool("isWalk", false);
        animator.SetBool("isIdle", true);
    }

    void CheckHealthPhase()
    {
        if (!isInHurtPhase && currentHealth <= maxHealth * HURT_PHASE_THRESHOLD)
        {
            EnterHurtPhase();
        }
    }

    void EnterHurtPhase()
    {
        isInHurtPhase = true;
        isHurt = true;
        currentMoveSpeed = hurtPhaseMoveSpeed;
        currentDamage = hurtPhaseDamage;
        currentAttackCooldown = hurtPhaseAttackCooldown;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentHealth <= maxHealth * HURT_PHASE_THRESHOLD)
        {
            EnterHurtPhase();
        }
    }

    void Die()
    {
        animator.SetBool("isDead", true);
        StopAllCoroutines();
        enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize detection radius in scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}