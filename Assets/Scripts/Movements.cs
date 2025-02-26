using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private static float speed = 10;
    [SerializeField] private LayerMask groundLayer;
    public Rigidbody2D myRigid;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private float horizontalInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private void Awake()
    {
        myRigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        myRigid.linearVelocity = new Vector2(horizontalInput * speed, myRigid.linearVelocity.y);
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(1.2934f, 1.2367f, 1f);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1.2934f, 1.2367f, 1f);
        }
        if (Input.GetKey(KeyCode.Space) && isGrounded())
        {
            Jump();
        }
        animator.SetBool("walk", horizontalInput != 0);
        animator.SetBool("grounded", isGrounded());

    }
    private void Jump()
    {
        myRigid.linearVelocity = new Vector2(myRigid.linearVelocity.x, speed);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
    }
    private bool isGrounded()
    {
        RaycastHit2D raycast = Physics2D.BoxCast(boxCollider.bounds.center,boxCollider.bounds.size,0,Vector2.down,0.1f, groundLayer);
        return raycast.collider != null;
    }
    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded();
    }
}
