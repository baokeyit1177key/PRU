using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform skillSpawnPoint; // Điểm xuất hiện skill

    public Rigidbody2D myRigid;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private float horizontalInput;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Di chuyển nhân vật
        horizontalInput = Input.GetAxis("Horizontal");
        myRigid.linearVelocity = new Vector2(horizontalInput * speed, myRigid.linearVelocity.y);

        // Đổi hướng nhân vật và cập nhật vị trí skillSpawnPoint
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(1.2934f, 1.2367f, 1f);
            skillSpawnPoint.localPosition = new Vector3(1.5f, 0, 0); // Đưa SkillSpawnPoint ra trước mặt
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1.2934f, 1.2367f, 1f);
            skillSpawnPoint.localPosition = new Vector3(-1.5f, 0, 0); // Đổi hướng SkillSpawnPoint
        }

        // Nhảy
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        animator.SetBool("walk", horizontalInput != 0);
        animator.SetBool("grounded", IsGrounded());
    }

    private void Jump()
    {
        myRigid.linearVelocity = new Vector2(myRigid.linearVelocity.x, speed);
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycast = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycast.collider != null;
    }

    public bool CanAttack()
    {
        return IsGrounded();
    }
}
