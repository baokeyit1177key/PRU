using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2Controller : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damage = 15;

    [Header("Attack Animation Durations")]
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float idleDuration = 0.8f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 80;
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
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && !isEnabled)
        {
            isEnabled = true;
            StartCoroutine(MoveTowardsPlayer());
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private IEnumerator MoveTowardsPlayer()
    {
        isMoving = true;
        animator.SetBool("IsWalk", true);

        while (!isDead && !isAttacking)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && cooldownTimer <= 0)
            {
                yield return StartCoroutine(PerformAttack());
            }
            else
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
                FlipTowardsPlayer();
            }
            yield return null;
        }
        rb.velocity = Vector2.zero;
        isMoving = false;
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("IsAttack");

        yield return new WaitForSeconds(attackDuration * 0.5f);

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(attackDuration * 0.5f);
        isAttacking = false;
        cooldownTimer = attackCooldown;
        StartCoroutine(MoveTowardsPlayer());
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
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        animator.SetTrigger("IsDead");
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;
        float direction = player.position.x - transform.position.x;
        transform.localScale = new Vector3(Mathf.Sign(direction) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void FixedUpdate()
    {
        if (!isMoving || isAttacking || isDead)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
