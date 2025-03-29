using UnityEngine;
using System.Collections;

[System.Serializable]
public class MeleeAttackConfig
{
    public int damage = 20;
    public float attackRange = 2f;
}

public class BossController_2 : MonoBehaviour
{
    [Header("Boss Health Settings")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    private float currentMoveSpeed;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float hurtInvulnerabilityDuration = 2f;
    [SerializeField] private MeleeAttackConfig[] meleeAttacks;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    // Trạng thái boss
    private bool isDead = false;
    private bool isHurt = false;
    private bool isPlayerDetected = false;
    private bool hasReachedHalfHealth = false;

    // Bộ đếm thời gian
    private float attackTimer = 0f;
    private float hurtTimer = 0f;

    // Chỉ số tấn công
    private int nextAttackIndex = 0;

    void Awake()
    {
        // Khởi tạo giá trị ban đầu
        currentHealth = maxHealth;
        currentMoveSpeed = walkSpeed;

        // Tìm tham chiếu nếu chưa được đặt
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (cameraFollow == null)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Dừng update nếu boss đã chết
        if (isDead)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsDead", true);
            return;
        }

        // Tính khoảng cách tới player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Logic phát hiện player
        if (distanceToPlayer <= detectionRadius)
        {
            if (!isPlayerDetected)
            {
                isPlayerDetected = true;
                cameraFollow.OnBossDetection(true);
                StartCoroutine(BossDetectionSequence());
            }

            // Logic di chuyển và tấn công
            if (!isHurt)
            {
                MoveAndAttackLogic(distanceToPlayer);
            }
        }
        

        // Quản lý bộ đếm thời gian
        ManageTimers();
    }

    private void MoveAndAttackLogic(float distanceToPlayer)
    {
        // Lật mặt về phía player
        FlipTowardsPlayer();

        // Nếu chưa trong phạm vi tấn công
        if (distanceToPlayer > meleeAttacks[0].attackRange)
        {
            // Di chuyển với tốc độ và animation khác nhau
            if (!hasReachedHalfHealth)
            {
                // Trước khi dưới 50% máu: di chuyển chậm và animation walk
                currentMoveSpeed = walkSpeed;
                animator.SetBool("IsIdle", false);
                animator.SetBool("IsWalk", true);
                animator.SetBool("IsRun", false);
            }
            else
            {
                // Sau khi dưới 50% máu: di chuyển nhanh và animation run
                currentMoveSpeed = runSpeed;
                animator.SetBool("IsIdle", false);
                animator.SetBool("IsWalk", false);
                animator.SetBool("IsRun", true);
            }

            // Thực hiện di chuyển
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * currentMoveSpeed;
        }
        else
        {
            // Dừng di chuyển khi trong phạm vi tấn công
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsWalk", false);
            animator.SetBool("IsRun", false);
            animator.SetBool("IsIdle", true);

            // Thực hiện tấn công nếu cooldown đã hết
            if (attackTimer <= 0)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        // Đảm bảo có cấu hình tấn công
        if (meleeAttacks == null || meleeAttacks.Length == 0) return;

        // Tắt tất cả các animation attack
        animator.SetBool("IsAttack1", false);
        animator.SetBool("IsAttack2", false);
        animator.SetBool("IsAttack3", false);
        animator.SetBool("IsIdle", false);

        // Chọn animation và damage tấn công
        switch (nextAttackIndex)
        {
            case 0:
                animator.SetBool("IsAttack1", true);
                break;
            case 1:
                animator.SetBool("IsAttack2", true);
                break;
            case 2:
                animator.SetBool("IsAttack3", true);
                break;
        }

        // Tấn công player
        AttackPlayer(meleeAttacks[nextAttackIndex].damage);

        // Đặt lại timer và chuyển attack tiếp theo
        attackTimer = attackCooldown;
        nextAttackIndex = (nextAttackIndex + 1) % meleeAttacks.Length;
    }

    private void AttackPlayer(int damage)
    {
        // Kiểm tra khoảng cách tấn công
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= meleeAttacks[0].attackRange)
        {
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }
    }

    private void ManageTimers()
    {
        // Giảm timer tấn công
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // Quản lý trạng thái bị thương
        if (isHurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                EndHurtState();
            }
        }
    }

    private void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;

        // Xác định hướng quay
        float direction = playerTransform.position.x - transform.position.x;
        transform.localScale = new Vector3(
            Mathf.Sign(direction) * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private IEnumerator BossDetectionSequence()
    {
        animator.SetTrigger("Detected");
        yield return new WaitForSeconds(1f);
    }

    public void TakeDamage(int damage)
    {
        // Ngăn chặn sát thương trong trạng thái chết hoặc bị thương
        if (isDead || isHurt) return;

        // Kiểm tra nếu máu dưới 50%
        if (currentHealth - damage <= maxHealth / 2 && !hasReachedHalfHealth)
        {
            // Đặt máu còn lại 50%
            currentHealth = maxHealth / 2;
            hasReachedHalfHealth = true;

            // Chuyển sang trạng thái bị thương
            EnterHurtState();
            return;
        }

        // Áp dụng sát thương
        currentHealth -= damage;

        // Kiểm tra chết
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
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsRun", false);
    }

    private void EndHurtState()
    {
        isHurt = false;
        animator.ResetTrigger("Hurt");
        animator.SetBool("IsHurt", false);
        animator.SetBool("IsIdle", true);
    }

    private void Die()
    {
        isDead = true;

        animator.SetTrigger("Dead");
        animator.SetBool("IsDead", true);
        animator.SetBool("IsIdle", false);

        // Vô hiệu hóa vật lý
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

    private void OnDrawGizmosSelected()
    {
        // Vẽ bán kính phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Vẽ phạm vi tấn công
        if (meleeAttacks != null && meleeAttacks.Length > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeAttacks[0].attackRange);
        }
    }
}