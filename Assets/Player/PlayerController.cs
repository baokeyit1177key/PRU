using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public GameObject bulletPrefab;
    public int maxHealth = 100;
    public int damage = 10;
    public HealthBar healthBar;
    public int currentHealth;
    private Rigidbody2D rb;
    [SerializeField] private Transform firePoint;
    private int jumpCount = 2; // Nhảy tối đa 2 lần
    [SerializeField] private float attackCooldown;
    private float cooldownTimer = 0f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    public LayerMask groundLayer;
    private bool isDead = false;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpriteRenderer playerSprite;
    private bool isInLava = false;
    [SerializeField] private int lavaDamagePerSecond = 5;
    private float lavaDamageAccumulator = 0f;
    public int statPoints;
    // Thêm vào class PlayerController
    [SerializeField] private int attackDamage = 20; // Sát thương cơ bản, có thể điều chỉnh trong Inspector
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float invincibilityFlashInterval = 0.1f;
    private bool isInvincible = false;
    public int AttackDamage { get { return attackDamage; } set { attackDamage = value; } }
    [SerializeField] private List<DamageType> bypassInvincibilityTypes;
    private static PlayerController instance;
    public enum DamageType
    {
        Normal,
        Burn,
        Poison,
        Bleed,
        Shock
        // Add more as needed
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Main Player initialized: " + gameObject.name);// Keep the Player across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate Player
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        rb = GetComponent<Rigidbody2D>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            firePoint = fp.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = new Vector3(1f, 0f, 0f);
        }

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            groundCheck = gc.transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0f, -1f, 0f);
        }      
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cooldownTimer >= attackCooldown)
        {
            Shoot();
            animator.SetTrigger("attack");
            cooldownTimer = 0f;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            gameManager.GamePauseMenu();
        } 
        if(Input.GetKey(KeyCode.Z))
        {           
            gameManager.CompleteMap();
        }

        CheckGround();

        if (groundCheck != null)
        {
            groundCheck.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        }

        if (isInLava)
        {
            ApplyLavaDamage();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the boss
        if (collision.gameObject.CompareTag("Boss"))
        {
            // Destroy the boss
            Destroy(collision.gameObject);

            // Open the upgrade menu
            gameManager.CompleteMap();
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find SpawnPoint in the new scene
        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;  
            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }
        else
        {
            Debug.LogWarning("SpawnPoint not found in the new scene!");
        }
    }
    private IEnumerator InvincibilityCoroutine()
    {
        // Set invincibility flag
        isInvincible = true;

        // Track elapsed time
        float elapsedTime = 0f;

        // Visual feedback during invincibility
        while (elapsedTime < invincibilityDuration)
        {

            playerSprite.enabled = !playerSprite.enabled;

            // Wait for flash interval
            yield return new WaitForSeconds(invincibilityFlashInterval);

            // Increment elapsed time
            elapsedTime += invincibilityFlashInterval;
        }

        // Ensure sprite is visible at end of invincibility
        playerSprite.enabled = true;

        // Reset invincibility
        isInvincible = false;
    }
    public void TakeDamage(int damage, DamageType damageType = DamageType.Normal)
    {
        // Check if damage type bypasses invincibility
        bool canTakeDamage = bypassInvincibilityTypes.Contains(damageType) || !isInvincible;

        if (canTakeDamage)
        {
            // Reduce health
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth);

            // Check if player dies
            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            // Start invincibility only for non-bypassing damage types
            if (!bypassInvincibilityTypes.Contains(damageType))
            {
                StartCoroutine(InvincibilityCoroutine());
            }
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 3; i++) // Nháy 3 lần
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("death");
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        gameManager.GameOverMenu();
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount--;
        }
    }

    void CheckGround()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = 2;
        }
    }

    void Aim()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    private bool isGrounded()
    {
        RaycastHit2D raycast = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycast.collider != null;
    }

    void Shoot()
    {
        StartCoroutine(ShootWithDelay(0.2f));
    }

    IEnumerator ShootWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 shootDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        projectile.GetComponent<Projectiles>().SetDirection(shootDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Trap" || collision.gameObject.name == "Trap (1)" || collision.gameObject.name == "Trap (2)")
        {
            TakeDamage(currentHealth);
        }

        if (collision.gameObject.CompareTag("Lava"))
        {
            isInLava = true;
        }
    }

    private void ApplyLavaDamage()
    {
        lavaDamageAccumulator += Time.deltaTime;

        if (lavaDamageAccumulator >= 0.2f)
        {
            // Gây 1 damage
            TakeDamage(1, DamageType.Burn);

            // Trừ thời gian tích lũy đi 0.2 giây
            lavaDamageAccumulator -= 0.2f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Lava"))
        {
            isInLava = false;
        }
    }
}
