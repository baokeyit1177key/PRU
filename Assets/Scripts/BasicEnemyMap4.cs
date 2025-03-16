using UnityEngine;

public abstract class BasicEnemyMap4 : MonoBehaviour
{
    [SerializeField] protected float enemyMoveSpeed = 1f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected bool isRanged = false;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected int maxHealth = 100;   
    protected int enemyHealth;                        
    [SerializeField] protected int attackDamage = 10;
    protected PlayerController player;
    [SerializeField] private LayerMask groundLayer;
    protected bool playerDetected = false;
    protected float cooldownTimer = 0f;
    protected bool canAttack = true;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Vector3 originalScale;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    protected virtual void Start()
    {
        player = FindAnyObjectByType<PlayerController>(); // Phương thức mới thay thế
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        enemyHealth = maxHealth;
        originalScale = transform.localScale;

    }

    protected virtual void Update()
    {
        DetectPlayer();

        if (playerDetected)
        {
            FlipEnemy();
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            animator.SetFloat("playerDistance", distanceToPlayer);
            if (isRanged)
            {
                if (distanceToPlayer < attackRange - 1)
                {
                    MoveAwayFromPlayer();
                }
                else if (distanceToPlayer > attackRange + 1)
                {
                    MoveToPlayer();
                    animator.SetBool("IsRunning", true);
                }
                else
                {
                    AttackPlayer();
                }
            }
            else
            {
                if (distanceToPlayer > attackRange)
                {
                    MoveToPlayer();
                    animator.SetBool("IsRunning", true);
                }
                else
                {
                    AttackPlayer();
                }
            }         
        }
       else animator.SetBool("IsRunning", true);
        HandleAttackCooldown();
    }
    private bool isGrounded()
    {
        RaycastHit2D raycast = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycast.collider != null;
    }
    protected void DetectPlayer()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            playerDetected = distance <= detectionRange;
        }
    }

    protected virtual void MoveToPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * enemyMoveSpeed, rb.linearVelocity.y);
            
        }
    }

    protected virtual void MoveAwayFromPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (transform.position - player.transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * enemyMoveSpeed, rb.linearVelocity.y);
        }
    }

    protected void FlipEnemy()
    {
        if (player != null)
        {
            float moveDirection = player.transform.position.x - transform.position.x;

            // Flip only if the enemy is actually moving left or right
            if (Mathf.Abs(moveDirection) > 0.1f)
            {
                transform.localScale = new Vector3(
            (moveDirection < 0 ? 1 : -1) * originalScale.x,
            originalScale.y,
            originalScale.z
            );
            }
        }
    }

    protected void HandleAttackCooldown()
    {
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= attackCooldown)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }
    }

    protected virtual void AttackPlayer()
    {
        if (canAttack)
        {
            animator.SetTrigger("Attack");
            canAttack = false;
            cooldownTimer = 0f;
        }
    }
    protected virtual void DealDamage()
    {
        if (player != null)
        {
            player.TakeDamage(attackDamage); // Make sure PlayerController has a TakeDamage method
        }
    }
    public void TakeDamage(int damage)
    {
        enemyHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage! HP: " + enemyHealth);

        if (enemyHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            DealDamage();
        }
    }
    protected abstract void PerformAttack();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}