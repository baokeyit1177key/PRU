using UnityEngine;
using System.Collections;

public class CreepAIController : MonoBehaviour
{
    public float speed = 2f;
    public int maxHealth = 2;
    public Transform player;
    public LayerMask obstacleLayer;
    public float attackRange = 1.5f;
    public float stopAttackRange = 3f;

    private int currentHealth;
    private int hitCount = 0;  // Đếm số lần trúng Arrow
    private const int maxHits = 3; // Giới hạn số lần trúng trước khi hủy (3 lần)
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            MoveToPlayer();
        }
        else if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(AttackPlayer());
        }

        FlipTowardsPlayer();
    }

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

    void MoveToPlayer()
    {
        if (player == null) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, 1f, obstacleLayer);
        if (hit.collider != null)
        {
            AvoidObstacle();
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            animator.SetBool("isWalking", true);
        }
    }

    void AvoidObstacle()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 perpendicularDirection = Vector2.Perpendicular(directionToPlayer).normalized;
        Vector2 newDirection = (Random.value > 0.5f) ? perpendicularDirection : -perpendicularDirection;

        transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + newDirection, speed * Time.deltaTime);
    }

    IEnumerator AttackPlayer()
    {
        animator.SetBool("isWalking", false);
        animator.SetTrigger("atk");

        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(2f);

        animator.SetTrigger("skill");
        yield return new WaitForSeconds(1.5f);
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
        animator.SetTrigger("skill");
        yield return new WaitForSeconds(1.5f);
    }

    void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("death");
        StopAllCoroutines();
        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject); // Hủy quái sau khi chết
    }
}
