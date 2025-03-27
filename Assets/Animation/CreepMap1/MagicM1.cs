using UnityEngine;
using System.Collections;

public class MagicM1 : MonoBehaviour
{
    public float speed = 2f;
    public int maxHealth = 2;
    public Transform player;
    public LayerMask obstacleLayer;
    public float attackRange = 1.5f;
    public float stopAttackRange = 3f;
    public float attackCloseRange = 4f; // Tầm tấn công gần, dưới 4 pixel
    public float detectionRange = 5f; // Tầm phát hiện, quái sẽ bắt đầu di chuyển khi nhân vật vào trong phạm vi này

    private int currentHealth;
    private int hitCount = 0;  // Đếm số lần trúng Arrow
    private const int maxHits = 3; // Giới hạn số lần trúng trước khi hủy
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isAttacking = false; // Kiểm tra xem quái có đang tấn công không

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(MoveWithDelay()); // Bắt đầu coroutine để di chuyển từng bước một cách chậm
        StartCoroutine(AttackRoutine()); // Bắt đầu tấn công định kỳ mỗi 3 giây
    }

    void Update()
    {
        if (isDead) return;

        // Kiểm tra khoảng cách với người chơi và quay mặt về phía người chơi
        FlipTowardsPlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Nếu người chơi nằm trong phạm vi phát hiện, quái bắt đầu di chuyển và tấn công
        if (distanceToPlayer <= detectionRange)
        {
            // Nếu quái ở gần đủ để tấn công
            if (distanceToPlayer <= attackCloseRange && !isAttacking)
            {
                StopCoroutine(MoveWithDelay()); // Dừng di chuyển khi trong phạm vi tấn công
                StartCoroutine(AttackPlayer()); // Tấn công ngay lập tức
            }
            else if (distanceToPlayer > attackCloseRange && !isAttacking)
            {
                StartCoroutine(MoveWithDelay()); // Tiếp tục di chuyển khi ra khỏi phạm vi tấn công
            }
        }
        else
        {
            // Nếu người chơi rời khỏi tầm phát hiện, quái dừng lại và quay về trạng thái Idle
            StopCoroutine(MoveWithDelay()); // Dừng di chuyển
            animator.SetBool("isWalking", false); // Dừng animation đi bộ
            animator.SetTrigger("isIdle"); // Quay về trạng thái Idle
        }
    }

    // Quay mặt quái về phía người chơi
    void FlipTowardsPlayer()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;
        if (direction > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // Coroutine để di chuyển quái với khoảng cách ngắn mỗi 2 giây
    IEnumerator MoveWithDelay()
    {
        while (!isDead)
        {
            MoveToPlayer(); // Di chuyển quái một bước ngắn
            yield return new WaitForSeconds(2f); // Dừng 2 giây trước khi di chuyển lần tiếp theo
        }
    }

    // Di chuyển quái về phía người chơi nhưng chỉ đi được một đoạn ngắn
    void MoveToPlayer()
    {
        if (player == null) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, 1f, obstacleLayer);
        if (hit.collider != null)
        {
            AvoidObstacle(); // Nếu gặp chướng ngại vật, tránh
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * 0.5f * Time.deltaTime); // Di chuyển một đoạn ngắn
            animator.SetBool("isWalking", true); // Bật animation đi bộ
        }
    }

    // Tránh chướng ngại vật nếu có
    void AvoidObstacle()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 perpendicularDirection = Vector2.Perpendicular(directionToPlayer).normalized;
        Vector2 newDirection = (Random.value > 0.5f) ? perpendicularDirection : -perpendicularDirection;

        transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + newDirection, speed * 0.5f * Time.deltaTime); // Di chuyển tránh chướng ngại vật
    }

    // Tấn công người chơi
    IEnumerator AttackPlayer()
    {
        isAttacking = true; // Đánh dấu quái đang tấn công
        animator.SetBool("isWalking", false); // Dừng trạng thái walking
        animator.SetTrigger("isAttacking"); // Chuyển sang trạng thái tấn công

        // Thực hiện hành động tấn công mà không sử dụng animation khác ngoài "isAttacking"
        yield return new WaitForSeconds(1f); // Thực hiện hành động tấn công
        isAttacking = false; // Hủy trạng thái tấn công sau khi thực hiện

        // Quay lại trạng thái Idle sau khi tấn công
        animator.SetTrigger("isIdle"); // Quái quay về Idle

        // Chờ 2 giây trước khi có thể tấn công lại
        yield return new WaitForSeconds(2f);
    }

    // Coroutine thực hiện tấn công định kỳ mỗi 3 giây
    IEnumerator AttackRoutine()
    {
        while (!isDead)
        {
            // Kiểm tra khoảng cách và tấn công nếu cần thiết
            if (Vector2.Distance(transform.position, player.position) <= attackRange && !isAttacking)
            {
                StartCoroutine(AttackPlayer());
            }

            yield return new WaitForSeconds(3f); // Tấn công lại sau mỗi 3 giây
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Arrow"))
        {
            hitCount++;
            Debug.Log("Quái bị trúng Arrow: " + hitCount + " lần");
            TakeDamage(1);

            // Nếu quái bị trúng đủ 3 lần, tự hủy
            if (hitCount >= maxHits)
            {
                Debug.Log("Quái trúng 3 lần, tự hủy!");
                Die();
            }

            Destroy(collision.gameObject); // Xóa mũi tên sau khi va chạm
        }
        else if (collision.CompareTag("Player"))
        {
            StartCoroutine(UseSkillOnCollision());
        }
    }

    IEnumerator UseSkillOnCollision()
    {
        animator.SetTrigger("isAttacking"); // Chuyển sang trạng thái tấn công khi va chạm với người chơi
        yield return new WaitForSeconds(1.5f);
    }

    void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Thực hiện hiệu ứng nhấp nháy màu đỏ khi bị tấn công
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Flash hiệu ứng nhấp nháy đỏ nhẹ
    IEnumerator FlashEffect()
    {
        spriteRenderer.color = Color.red; // Đổi màu thành đỏ
        yield return new WaitForSeconds(0.1f); // Hiệu ứng đỏ trong 0.1 giây
        spriteRenderer.color = Color.white; // Đổi lại màu ban đầu
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("isDead"); // Chuyển sang trạng thái chết
        StopAllCoroutines();
        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject); // Hủy quái sau khi chết
    }
}
