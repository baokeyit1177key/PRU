using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public GameObject bulletPrefab;

    private Rigidbody2D rb;
    private Transform firePoint;
    private int jumpCount = 2; // Nhảy tối đa 2 lần

    public Transform groundCheck;  // Điểm kiểm tra mặt đất
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; // Layer của Ground

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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
        Move();
        Aim();
        Attack();
        CheckGround(); // Kiểm tra mặt đất

        // Luôn đặt groundCheck ngay dưới chân nhân vật
        if (groundCheck != null)
        {
            groundCheck.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        }
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
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ~0); // ~0 chọn tất cả layer

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

    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
