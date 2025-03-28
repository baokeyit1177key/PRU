using System.Collections;
using UnityEngine;
[System.Serializable]
public class BulletAttackConfig
{
    public GameObject bulletPrefab;
    public Vector3 bulletScale = Vector3.one;
    public int damage = 20;
}
public class BossController_4 : MonoBehaviour
{
    [Header("Boss Health Settings")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Attack Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float hurtInvulnerabilityDuration = 2f;

    [Header("Bullet Attack Configurations")]
    [SerializeField] private BulletAttackConfig[] bulletAttacks;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int[] bulletDamages = new int[] { 20, 30, 40 };
    [SerializeField] private int bulletsPerAttack = 3;
    [SerializeField] private float bulletSpreadInterval = 0.2f;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;  // Mảng các prefab enemy
    [SerializeField] private Transform enemySpawnPoint;  // Điểm spawn enemy
    [SerializeField] private int maxEnemiesPerSpawn = 3; // Số lượng enemy tối đa được sinh ra mỗi lần

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    // Internal state variables
    private bool isDead = false;
    private bool isHurt = false;
    private bool isPlayerDetected = false;
    private bool hasUsedHalfHealthProtection = false;
    private bool hasReachedHalfHealth = false;

    // Timers
    private float attackTimer = 0f;
    private float hurtTimer = 0f;
    private float halfHealthThresholdTimer = 0f;
    private float originalAttackCooldown;

    // Attack tracking
    private int nextAttackIndex = 0;
    private int[] healthThresholds;
    private int nextHealthThresholdIndex = 0;

    void Awake()
    {
        // Initialize health and attack cooldown
        currentHealth = maxHealth;
        originalAttackCooldown = attackCooldown;

        // Find references if not set in inspector
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (cameraFollow == null)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (bulletAttacks == null || bulletAttacks.Length == 0)
        {
            Debug.LogError("No bullet attack configurations set!");
        }
        healthThresholds = new int[]
        {
            Mathf.FloorToInt(maxHealth * 0.8f),
            Mathf.FloorToInt(maxHealth * 0.6f),
            Mathf.FloorToInt(maxHealth * 0.4f),
            Mathf.FloorToInt(maxHealth * 0.2f)
        };
    }

    void Update()
    {
        // Stop update if boss is dead
        if (isDead)
        {
            animator.SetBool("IsDead", true);
            return;
        }

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Player detection logic
        if (distanceToPlayer <= detectionRadius)
        {
            if (!isPlayerDetected)
            {
                isPlayerDetected = true;
                cameraFollow.OnBossDetection(true);
                StartCoroutine(BossDetectionSequence());
            }

            // Attack logic
            if (!isHurt && attackTimer <= 0)
            {
                PerformAlternatingAttack();
            }
        }
        else
        {
            // Reset when player leaves detection radius
            isPlayerDetected = false;
            ResetToIdleState();
        }

        // Manage attack and hurt timers
        ManageTimers();

        // Update animator parameters
        UpdateAnimatorParameters();
    }

    private void ManageTimers()
    {
        // Decrement attack timer
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;

            // Reset to idle when attack timer expires
            if (attackTimer <= 0)
            {
                ResetToIdleState();
            }
        }

        // Manage half health protection timer
        if (hasReachedHalfHealth)
        {
            halfHealthThresholdTimer -= Time.deltaTime;
            if (halfHealthThresholdTimer <= 0)
            {
                hasReachedHalfHealth = false;
                EndHurtState();
            }
        }

        // Manage hurt state timer
        if (isHurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                EndHurtState();
            }
        }
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetBool("IsPlayerDetected", isPlayerDetected);
        animator.SetBool("IsHurt", isHurt);
        animator.SetBool("IsDead", isDead);
    }

    private void ResetToIdleState()
    {
        animator.SetBool("IsAttack1", false);
        animator.SetBool("IsAttack2", false);
        animator.SetBool("IsAttack3", false);
        animator.SetBool("IsIdle", true);
    }

    private IEnumerator BossDetectionSequence()
    {
        animator.SetTrigger("Detected");
        yield return new WaitForSeconds(1f);
    }

    private void PerformAlternatingAttack()
    {
        // Ensure we have valid attack configurations
        if (bulletAttacks == null || bulletAttacks.Length == 0) return;

        // Ensure nextAttackIndex is within bounds
        nextAttackIndex = Mathf.Clamp(nextAttackIndex, 0, bulletAttacks.Length - 1);

        switch (nextAttackIndex)
        {
            case 0:
                PerformRangedAttack1();
                break;
            case 1:
                PerformRangedAttack2();
                break;
            case 2:
                PerformRangedAttack3();
                break;
        }

        // Cycle through attack types
        nextAttackIndex = (nextAttackIndex + 1) % bulletAttacks.Length;
    }

    private void PerformRangedAttack1()
    {
        ResetToIdleState();
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttack1", true);
        StartCoroutine(FireBulletSequence(0));
        attackTimer = attackCooldown;
        FlipTowardsPlayer();
    }

    private void PerformRangedAttack2()
    {
        ResetToIdleState();
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttack2", true);
        StartCoroutine(FireBulletSequence(1));
        attackTimer = attackCooldown;
        FlipTowardsPlayer();
    }

    private void PerformRangedAttack3()
    {
        ResetToIdleState();
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttack3", true);
        StartCoroutine(FireBulletSequence(2));
        attackTimer = attackCooldown;
        FlipTowardsPlayer();
    }

    private IEnumerator FireBulletSequence(int attackIndex)
    {
        for (int i = 0; i < bulletsPerAttack; i++)
        {
            FireBullet(attackIndex);
            yield return new WaitForSeconds(bulletSpreadInterval);
        }

        // After firing all bullets, wait and reset to idle
        yield return new WaitForSeconds(0.5f);
        ResetToIdleState();

        // Wait 3 seconds before next attack
        yield return new WaitForSeconds(3f);

        // Trigger next attack
        PerformAlternatingAttack();
    }
    private IEnumerator PerformAttackAfterIdle(int attackIndex)
    {
        // Wait a short moment to show idle state
        yield return new WaitForSeconds(0.5f);

        switch (attackIndex)
        {
            case 0:
                PerformRangedAttack1();
                break;
            case 1:
                PerformRangedAttack2();
                break;
            case 2:
                PerformRangedAttack3();
                break;
        }
    }

    private void FireBullet(int attackIndex)
    {
        // Validate input
        if (bulletAttacks == null || bulletAttacks.Length == 0 ||
            bulletAttacks[attackIndex].bulletPrefab == null ||
            firePoint == null) return;

        // Get current attack configuration
        BulletAttackConfig currentAttack = bulletAttacks[attackIndex];

        // Instantiate bullet
        GameObject bullet = Instantiate(
            currentAttack.bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        // Apply individual bullet scale
        bullet.transform.localScale = currentAttack.bulletScale;

        // Calculate direction
        Vector2 direction = playerTransform.position - firePoint.position;
        direction.y = 0;
        direction = direction.normalized;

        // Set up bullet physics
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            bulletRb.linearVelocity = direction * bulletSpeed;
        }

        // Set up bullet damage
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController == null)
        {
            bulletController = bullet.AddComponent<BulletController>();
        }
        bulletController.damage = currentAttack.damage;
        bulletController.shooter = gameObject;

        // Flip bullet sprite based on direction
        if (direction.x < 0)
        {
            Vector3 scale = bullet.transform.localScale;
            bullet.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }

        Destroy(bullet, 5f);
    }

    private void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;

        float direction = playerTransform.position.x - transform.position.x;
        transform.localScale = new Vector3(
            Mathf.Sign(direction) * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    public void TakeDamage(int damage)
    {
        // Half health protection logic
        if (hasReachedHalfHealth)
        {
            if (halfHealthThresholdTimer > 0)
            {
                Debug.Log("Boss is still invulnerable. Time remaining: " + halfHealthThresholdTimer);
                return;
            }
            else
            {
                hasReachedHalfHealth = false;
            }
        }
        if (nextHealthThresholdIndex < healthThresholds.Length && currentHealth <= healthThresholds[nextHealthThresholdIndex])
        {
            SpawnRandomEnemy();
            nextHealthThresholdIndex++; // Chuyển sang mốc tiếp theo
        }
        // Prevent damage during certain states
        if (isDead || isHurt) return;

        // Half health protection mechanism
        if (currentHealth - damage <= maxHealth / 2 && !hasUsedHalfHealthProtection)
        {
            currentHealth = maxHealth / 2;
            hasReachedHalfHealth = true;
            halfHealthThresholdTimer = 2f;
            hasUsedHalfHealthProtection = true;

            EnterHurtState();

            Debug.Log("Boss reached 50% health. Invulnerable for 2 seconds.");
            return;
        }

        // Apply damage
        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. Current health: {currentHealth}");

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void EnterHurtState()
    {
        isHurt = true;
        hurtTimer = hurtInvulnerabilityDuration;

        animator.SetTrigger("Hurt");
        animator.SetBool("IsHurt", true);

        // Reduce attack speed when hurt
        attackCooldown *= 0.66f;
    }

    private void EndHurtState()
    {
        isHurt = false;
        animator.ResetTrigger("Hurt");
        animator.SetBool("IsIdle", true);
        attackCooldown = Mathf.Max(originalAttackCooldown * 0.33f, originalAttackCooldown);


        hasReachedHalfHealth = false;
    }

    private void Die()
    {
        isDead = true;

        animator.SetTrigger("Dead");
        animator.SetBool("IsDead", true);

        // Disable physics and collider
        rb.simulated = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
    public void SpawnRandomEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || enemySpawnPoint == null) return;

        // Số lượng enemy sẽ spawn ngẫu nhiên từ 1 đến maxEnemiesPerSpawn
        int enemyCount = Random.Range(1, maxEnemiesPerSpawn + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            // Chọn ngẫu nhiên enemy prefab
            GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Thêm offset nhỏ để các enemy không spawn chồng lên nhau
            Vector3 spawnOffset = new Vector3(
                Random.Range(-1f, 1f),  // Offset x ngẫu nhiên
                0,                       // Không thay đổi theo chiều y
                0
            );

            // Sinh enemy tại điểm spawn với offset nhỏ
            Instantiate(enemyToSpawn, enemySpawnPoint.position + spawnOffset, Quaternion.identity);
        }
    }
    public void SpawnSpecificEnemy(int enemyIndex)
    {
        if (enemyPrefabs == null || enemyIndex < 0 || enemyIndex >= enemyPrefabs.Length || enemySpawnPoint == null) return;

        Instantiate(enemyPrefabs[enemyIndex], enemySpawnPoint.position, Quaternion.identity);
    }

    // Có thể gọi phương thức này trong các sự kiện animation hoặc logic boss
    private void OnEnemySpawnTrigger()
    {
        // Ví dụ: Spawn enemy khi boss bị thương hoặc ở một giai đoạn cụ thể
        SpawnRandomEnemies();
    }

    public void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || enemySpawnPoint == null) return;

        GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(enemyToSpawn, enemySpawnPoint.position, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}