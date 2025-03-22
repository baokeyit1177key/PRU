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
    [SerializeField] private float attack1Duration = 0.35f; // Attack1 animation duration (now melee)
    [SerializeField] private float attack2Duration = 0.20f; // Attack2 animation duration (now ranged)
    [SerializeField] private float idleDuration = 1f; // Duration to stay in idle before moving again

    [Header("Projectile Settings")]
    [SerializeField] private Transform firePoint; // Add this in the inspector
    [SerializeField] private GameObject bulletPrefab; // Add this in the inspector
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 8;
    [SerializeField] private Vector3 bulletScale = new Vector3(1f, 1f, 1f); // Kích thước cho viên đạn

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
    private string currentAttackType = "none"; // Track current attack type

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

        // If firePoint is not set, we'll try to find it
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

    private void ResetAllAnimationStates()
    {
        // Reset all animation triggers and bools
        animator.ResetTrigger("IsAttack1");
        animator.ResetTrigger("IsAttack2");
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsIdle", false);
        currentAttackType = "none";
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        StopMovement();

        // Reset animation states before starting a new attack
        ResetAllAnimationStates();

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Close attack (melee)
            currentAttackType = "melee";
            animator.SetTrigger("IsAttack1");

            // Wait for exact attack animation duration
            yield return new WaitForSeconds(attack1Duration);

            // Apply damage if in range
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damage);
                }
            }
        }
        else
        {
            // Range attack
            currentAttackType = "ranged";
            animator.SetTrigger("IsAttack2");

            // Wait for exact attack animation duration
            yield return new WaitForSeconds(attack2Duration);

            // Shoot a bullet
            ShootBullet();
        }

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
        currentAttackType = "none";

        // Start moving towards player again
        if (!isDead)
        {
            StartCoroutine(MoveTowardsPlayer());
        }
    }

    private void ShootBullet()
    {
        if (firePoint == null || bulletPrefab == null) return;

        // Create the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Điều chỉnh kích thước của viên đạn
        bullet.transform.localScale = bulletScale;

        // Get direction to player - Chỉ lấy hướng theo trục X, giữ Y = 0 để đạn bay thẳng
        Vector2 direction = player.position - firePoint.position;
        direction.y = 0; // Đặt y = 0 để đạn bay ngang
        direction = direction.normalized;

        // Set bullet velocity
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            // Vô hiệu hóa trọng lực cho viên đạn
            bulletRb.gravityScale = 0f;
            bulletRb.linearVelocity = direction * bulletSpeed;
        }

        // Set up bullet damage component or add it if it doesn't exist
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController == null)
        {
            bulletController = bullet.AddComponent<BulletController>();
        }

        bulletController.damage = bulletDamage;
        bulletController.shooter = gameObject;

        // Lật hướng viên đạn theo hướng di chuyển nếu cần
        if (direction.x < 0)
        {
            Vector3 scale = bullet.transform.localScale;
            bullet.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }

        // Destroy bullet after some time if it doesn't hit anything
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

// Add this script to your bullet prefab
public class BulletController : MonoBehaviour
{
    public int damage = 8;
    public GameObject shooter; // Reference to who shot this bullet (to prevent self-damage)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit the player
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }

            // Destroy the bullet after hitting
            Destroy(gameObject);
        }
        // You can add more collision checks here (like for walls, etc.)
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}