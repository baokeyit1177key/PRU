using UnityEngine;

public class EnemyProjectiles : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private bool hit;
    private Vector2 direction;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidbody;
    public BasicEnemyMap4 enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Thêm phương thức để nhận sát thương từ Player
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (hit && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) Destroy(gameObject);
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);
        if (collision.CompareTag("Enemy") || collision.CompareTag("Projectiles"))
        {
            return; // Ignore collision with the player
        }
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(enemy.attackDamage);
                hit = true;
                anim.SetTrigger("hit");
                rigidbody.linearVelocity = Vector2.zero;
            }
        }

        hit = true;
        boxCollider.enabled = false;
    }

    public void SetDirection(Vector2 _direction)
    {
        direction = _direction.normalized; // Ensure direction is normalized
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;
        // Rotate the arrow to face the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Deactivate()
    {
        Destroy(gameObject);
    }
}
