using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public GameObject bulletPrefab;
    private int maxHealth = 100;
    public HealthBar healthBar;
    private int currentHealth;
    private Rigidbody2D rb;
    [SerializeField] private Transform firePoint;
    private int jumpCount = 2; // Nhảy tối đa 2 lần
    [SerializeField] private float attackCooldown; // Cooldown time in seconds
    private float cooldownTimer = 0f;
    public Transform groundCheck;  // Điểm kiểm tra mặt đất
    public float groundCheckRadius = 0.2f;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private float horizontalInput;
    public LayerMask groundLayer; // Layer của Ground
    private bool isDead = false;
    [SerializeField] private GameManager gameManager;
    private bool isInLava = false;
    [SerializeField] private int lavaDamagePerSecond = 5;
    private float lavaDamageAccumulator = 0f;
    // Thêm vào class PlayerController
    [SerializeField] private int attackDamage = 20; // Sát thương cơ bản, có thể điều chỉnh trong Inspector
    public int AttackDamage { get { return attackDamage; } set { attackDamage = value; } }

    // Thêm biến để theo dõi chạm vào "C1M1"
    private bool isTouchingC1M1 = false;
    private float c1m1DamageAccumulator = 0f;
    [SerializeField] private float c1m1DamageInterval = 1f; // Thời gian giữa các lần trừ máu (giây)
    [SerializeField] private int c1m1DamageAmount = 20; // Số máu bị trừ mỗi lần

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Tạo FirePoint nếu chưa có
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            firePoint = fp.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = new Vector3(1f, 0f, 0f);
        }

        // Tạo GroundCheck nếu chưa có
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            groundCheck = gc.transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0f, -1f, 0f); // Đặt dưới chân Player
        }
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime; // Update cooldown timer

        if (Input.GetMouseButtonDown(0) && cooldownTimer >= attackCooldown) // Check cooldown before shooting
        {
            Shoot();
            animator.SetTrigger("attack");
            cooldownTimer = 0f; // Reset cooldown
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            gameManager.GamePauseMenu();
        }
        CheckGround(); // Kiểm tra mặt đất

        // Luôn đặt groundCheck ngay dưới chân nhân vật
        if (groundCheck != null)
        {
            groundCheck.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        }
        if (isInLava)
        {
            ApplyLavaDamage();
        }

        // Nếu đang chạm vào "C1M1", trừ máu liên tục
        if (isTouchingC1M1)
        {
            c1m1DamageAccumulator += Time.deltaTime;

            if (c1m1DamageAccumulator >= c1m1DamageInterval)
            {
                TakeDamage(c1m1DamageAmount);
                c1m1DamageAccumulator = 0f; // Reset thời gian tích lũy
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return; // Đảm bảo chỉ chạy 1 lần
        isDead = true;

        animator.SetTrigger("death"); // Chạy animation chết
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

        // Nhảy 2 lần
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount--; // Giảm số lần nhảy mỗi lần nhấn Space
        }
    }

    void CheckGround()
    {
        // Kiểm tra xem GroundCheck có chạm bất kỳ vật thể nào không
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = 2; // Nếu chạm bất kỳ vật thể nào, reset số lần nhảy về 2
        }
    }

    void Aim()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = (mousePos - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
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

    // OnCollisionEnter2D xử lý va chạm vật lý
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("C1M1"))
        {
            isTouchingC1M1 = true; // Khi va chạm với quái "C1M1", bắt đầu trừ máu liên tục
        }

        if (collision.gameObject.CompareTag("Lava"))
        {
            isInLava = true; // Nếu vào lava, bắt đầu trừ máu từ lava
        }
    }

    // Khi không còn chạm vào "C1M1", dừng trừ máu
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("C1M1"))
        {
            isTouchingC1M1 = false; // Ngừng trừ máu khi không còn va chạm với "C1M1"
        }

        if (collision.gameObject.CompareTag("Lava"))
        {
            isInLava = false; // Ngừng trừ máu khi ra khỏi lava
        }
    }

    private void ApplyLavaDamage()
    {
        // Cộng dồn thời gian
        lavaDamageAccumulator += Time.deltaTime;

        // Nếu đã tích lũy đủ 0.2 giây (1/5 giây)
        if (lavaDamageAccumulator >= 0.2f)
        {
            // Gây 1 damage
            TakeDamage(1);

            // Trừ thời gian tích lũy đi 0.2 giây
            lavaDamageAccumulator -= 0.2f;
        }
    }
}
