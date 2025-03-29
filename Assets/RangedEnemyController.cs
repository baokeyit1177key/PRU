using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 3f;
    

    [Header("Attack Settings")]
    [SerializeField] private float attackAnimationDuration = 0.3f;
    [SerializeField] private int bulletsPerAttack = 4;
    [SerializeField] private float bulletSpreadInterval = 0.2f;

    [Header("Projectile Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private int bulletDamage = 5;
    [SerializeField] private Vector3 bulletScale = new Vector3(1f, 1f, 1f);

    [Header("Health")]
    [SerializeField] private int maxHealth = 80;
    public int currentHealth;

    [Header("References")]
    [SerializeField] private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private bool isEnabled = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;
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
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Find firePoint if not assigned
        if (firePoint == null)
        {
            Transform foundFirePoint = transform.Find("shoot");
            if (foundFirePoint != null)
            {
                firePoint = foundFirePoint;
            }
            else
            {
                Debug.LogWarning("FirePoint not assigned and not found in children!");
            }
        }

        // Initialize animation states
        animator.SetBool("IsEnable", false);
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsIdle", false);
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Detect player
        if (distanceToPlayer <= detectionRange)
        {
            if (!isEnabled)
            {
                // First time detecting player - activate
                isEnabled = true;
                StopMovement();
                animator.SetBool("IsEnable", true);
                StartCoroutine(WaitForEnableAnimation());
            }
            else if (!animator.GetBool("IsEnable") && !isAttacking && !isMoving)
            {
                // If not in any special state, move or attack
                if (attackTimer <= 0 && distanceToPlayer <= attackRange)
                {
                    // Attack if in range and cooldown expired
                    StartCoroutine(PerformAttack());
                }
                else
                {
                    // Otherwise move
                    StartCoroutine(MoveTowardsPlayer());
                }
            }
        }

        // Update cooldown timer
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
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
        ResetAnimationStates();
        animator.SetBool("IsWalk", true);

        // Move for a random duration
        float moveDuration = Random.Range(1.5f, 2.5f);
        float moveStartTime = Time.time;

        while (Time.time - moveStartTime < moveDuration)
        {
            if (isDead || isAttacking) break;

            // Move towards player
            if (animator.GetBool("IsWalk"))
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0);
                FlipTowardsPlayer();
            }
            else
            {
                StopMovement();
            }

            yield return null;
        }

        // Stop and check if we should attack
        StopMovement();
        animator.SetBool("IsWalk", false);
        isMoving = false;

        // Check if in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            StartCoroutine(PerformAttack());
        }
        else
        {
            // Switch to idle for a moment
            animator.SetBool("IsIdle", true);
            yield return new WaitForSeconds(1f);
            animator.SetBool("IsIdle", false);
        }
    }

    private void ResetAnimationStates()
    {
        animator.ResetTrigger("IsAttack");
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsIdle", false);
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        StopMovement();
        ResetAnimationStates();

        // Face the player
        FlipTowardsPlayer();

        // Start attack animation
        animator.SetTrigger("IsAttack");

        // Wait for attack animation to complete
        yield return new WaitForSeconds(attackAnimationDuration);

        // Fire bullets after animation completes
        StartCoroutine(FireBulletSequence());

        // Enter idle state after attack
        animator.SetBool("IsIdle", true);

        // Wait a bit in idle state
        yield return new WaitForSeconds(1f);

        // Reset attack state
        animator.SetBool("IsIdle", false);
        isAttacking = false;
        attackTimer = attackCooldown;
    }

    private IEnumerator FireBulletSequence()
    {
        for (int i = 0; i < bulletsPerAttack; i++)
        {
            FireBullet();
            yield return new WaitForSeconds(bulletSpreadInterval);
        }
    }

    private void FireBullet()
    {
        if (firePoint == null || bulletPrefab == null) return;

        // Create bullet at firePoint position
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.transform.localScale = bulletScale;

        // Get direction to player (horizontal only)
        Vector2 direction = player.position - firePoint.position;
        direction.y = 0; // Make bullets fly straight horizontally
        direction = direction.normalized;

        // Set bullet properties
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f; // No gravity
            bulletRb.linearVelocity = direction * bulletSpeed;
        }

        // Setup damage component
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController == null)
        {
            bulletController = bullet.AddComponent<BulletController>();
        }
        bulletController.damage = bulletDamage;
        bulletController.shooter = gameObject;

        // Flip bullet sprite if needed
        if (direction.x < 0)
        {
            Vector3 scale = bullet.transform.localScale;
            bullet.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }

        // Auto-destroy bullet after time
        Destroy(bullet, 5f);
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

        
        ResetAnimationStates();
        animator.SetTrigger("Dead");

        
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
        transform.localScale = new Vector3(
            Mathf.Sign(direction) * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        // Show detection and attack ranges in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void FixedUpdate()
    {
        // Ensure enemy stays still when not moving
        if (!animator.GetBool("IsWalk") || isAttacking || isDead)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}