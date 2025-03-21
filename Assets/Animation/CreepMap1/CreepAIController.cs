using System.Collections;
using UnityEngine;

public class CreepAIController : MonoBehaviour
{
    public float speed = 2f;      // Tốc độ di chuyển của Creep
    public int maxHealth = 2;     // Máu tối đa của Creep
    public Transform player;      // Player để creep đuổi theo
    public LayerMask obstacleLayer; // Layer chứa vật cản

    private int currentHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(CreepBehaviorLoop());
    }

    void Update()
    {
        if (!isDead && !isAttacking)
        {
            MoveToPlayer();
        }
    }

    void MoveToPlayer()
    {
        if (player == null) return;

        // Kiểm tra vật cản trước mặt
        if (Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, 1f, obstacleLayer))
        {
            AvoidObstacle();
        }
        else
        {
            // Di chuyển về phía Player
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            animator.SetBool("isWalking", true);
        }
    }

    void AvoidObstacle()
    {
        // Nếu có vật cản, đi vòng sang bên
        Vector2 newDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + newDirection, speed * Time.deltaTime);
    }

    IEnumerator CreepBehaviorLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(4f);

            // Ngừng di chuyển và tấn công
            isAttacking = true;
            animator.SetBool("isWalking", false);
            animator.SetTrigger("atk");

            yield return new WaitForSeconds(1f); // Chờ animation ATK
            isAttacking = false;

            yield return new WaitForSeconds(2f);

            // Dùng Skill
            isAttacking = true;
            animator.SetTrigger("skill");

            yield return new WaitForSeconds(1.5f); // Chờ animation Skill
            isAttacking = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Arrow") || collision.CompareTag("Skill"))
        {
            TakeDamage(1);
        }
    }

    void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(FlashEffect()); // Hiệu ứng trúng đòn

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
        isDead = true;
        animator.SetTrigger("death");
        StopAllCoroutines();
        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f); // Chờ animation chết
        Destroy(gameObject);
    }
}
