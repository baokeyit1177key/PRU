using UnityEngine;
using System.Collections;

public class BossController_4 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float walkDuration = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float baseAttackDamage = 20f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Phase Settings")]
    [SerializeField] private float phaseChangeHealthThreshold = 0.5f;
    private bool isSecondPhase = false;

    [Header("References")]
    [SerializeField] private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    [Header("State Variables")]
    public bool IsIdle = true;
    public bool IsAttack1 = false;
    public bool IsAttack2 = false;
    public bool IsAttack3 = false;
    public bool IsWalk = false;
    public bool IsProtect = false;
    public bool IsHurt = false;
    public bool IsDead = false;

    private bool isMoving = false;
    private float attackTimer = 0f;
    private float walkTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Initialize animation states
        ResetAnimationStates();
    }

    void Update()
    {
        if (IsDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Update attack cooldown
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // Player detection and actions
        if (distanceToPlayer <= detectionRange)
        {
            if (!isMoving && attackTimer <= 0)
            {
                if (distanceToPlayer <= detectionRange)
                {
                    StartCoroutine(MoveTowardsPlayer());
                }
            }
        }
    }

    private void ResetAnimationStates()
    {
        IsIdle = true;
        IsAttack1 = false;
        IsAttack2 = false;
        IsAttack3 = false;
        IsWalk = false;
        IsProtect = false;
        IsHurt = false;
    }

    private IEnumerator MoveTowardsPlayer()
    {
        isMoving = true;
        ResetAnimationStates();
        IsWalk = true;

        walkTimer = walkDuration;

        while (walkTimer > 0)
        {
            if (IsDead) break;

            // Move towards player
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            FlipTowardsPlayer();

            walkTimer -= Time.deltaTime;
            yield return null;
        }

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        IsWalk = false;
        isMoving = false;

        // Perform attack if in range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange && attackTimer <= 0)
        {
            StartCoroutine(PerformAttack());
        }
        else
        {
            // Idle state
            IsIdle = true;
            yield return new WaitForSeconds(1f);
            IsIdle = false;
        }
    }

    private IEnumerator PerformAttack()
    {
        ResetAnimationStates();

        // Face the player
        FlipTowardsPlayer();

        // Determine attack based on phase
        if (!isSecondPhase)
        {
            IsAttack1 = true;
        }
        else
        {
            int attackChoice = Random.Range(0, 3);
            switch (attackChoice)
            {
                case 0: IsAttack2 = true; break;
                case 1: IsAttack3 = true; break;
                case 2: IsProtect = true; break;
            }
        }

        // Wait for attack animation
        yield return new WaitForSeconds(0.5f);

        // Reset attack state
        ResetAnimationStates();
        attackTimer = attackCooldown;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || IsProtect) return;

        currentHealth -= damage;

        if (currentHealth <= maxHealth * phaseChangeHealthThreshold && !isSecondPhase)
        {
            // Transition to second phase
            isSecondPhase = true;
            IsHurt = true;
            baseAttackDamage *= 1.5f;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        ResetAnimationStates();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;
        transform.localScale = new Vector3(
            Mathf.Sign(direction) * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    void FixedUpdate()
    {
        // Ensure enemy stays still when not moving
        if (!IsWalk || IsDead)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}