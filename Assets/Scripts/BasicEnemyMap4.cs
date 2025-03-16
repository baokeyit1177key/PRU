using UnityEngine;

public abstract class BasicEnemyMap4 : MonoBehaviour
{
    [SerializeField] protected float enemyMoveSpeed = 1f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected bool isRanged = false;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 2f;

    protected PlayerController player; 
    protected bool playerDetected = false;
    protected float cooldownTimer = 0f;
    protected bool canAttack = true;

    protected virtual void Start()
    {
        player = FindAnyObjectByType<PlayerController>(); // Phương thức mới thay thế
    }

    protected virtual void Update()
    {
        DetectPlayer();

        if (playerDetected)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            if (isRanged)
            {
                if (distanceToPlayer < attackRange - 1)
                {
                    MoveAwayFromPlayer();
                }
                else if (distanceToPlayer > attackRange + 1)
                {
                    MoveToPlayer();
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
                }
                else
                {
                    AttackPlayer();
                }
            }

            FlipEnemy();
        }

        HandleAttackCooldown();
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
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                enemyMoveSpeed * Time.deltaTime
            );
        }
    }

    protected virtual void MoveAwayFromPlayer()
    {
        if (player != null)
        {
            Vector2 moveDirection = transform.position - player.transform.position;
            transform.position += (Vector3)moveDirection.normalized * enemyMoveSpeed * Time.deltaTime;
        }
    }

    protected void FlipEnemy()
    {
        if (player != null)
        {
            transform.localScale = new Vector3(
                player.transform.position.x < transform.position.x ? -1 : 1,
                1,
                1
            );
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
            PerformAttack();

            canAttack = false;
            cooldownTimer = 0f;
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