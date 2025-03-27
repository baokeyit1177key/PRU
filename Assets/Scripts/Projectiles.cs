using UnityEngine;
using UnityEngine.EventSystems;
public class Projectiles : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private bool hit;
    private Vector2 direction;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private int damage; // Thêm biến lưu sát thương

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Thêm phương thức để nhận sát thương từ Player
    private void Start()
    {
        // Tìm Player và lấy sát thương
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player != null)
        {
            damage = player.AttackDamage;
        }
        else
        {
            damage = 20; // Sát thương mặc định nếu không tìm thấy Player
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (hit) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);
        if (collision.CompareTag("Player") || collision.CompareTag("Projectiles"))
        {
            return; // Ignore collision with the player
        }

        // Kiểm tra va chạm với kẻ địch - cả BasicEnemyMap4 và DepstroyEnemyMap4
        if (collision.CompareTag("Enemy"))
        {
            // Kiểm tra BasicEnemyMap4
            BasicEnemyMap4 basicEnemy = collision.GetComponent<BasicEnemyMap4>();
            if (basicEnemy != null)
            {
                basicEnemy.TakeDamage(damage);
            }

            // Kiểm tra DepstroyEnemyMap4
            DepstroyEnemyMap4 depstroyEnemy = collision.GetComponent<DepstroyEnemyMap4>();
            if (depstroyEnemy != null)
            {
                depstroyEnemy.TakeDamage(damage); 
            }
            RangedEnemyController rangedEnemy = collision.GetComponent<RangedEnemyController>();
            if (rangedEnemy != null)
            {
                rangedEnemy.TakeDamage(damage);
            }
            Enemy3Controller enemy3 = collision.GetComponent<Enemy3Controller>();
            if (enemy3 != null)
            {
                enemy3.TakeDamage(damage);
            }
            EnemyMap2 enemyMap2 = collision.GetComponent<EnemyMap2>();
            if (enemyMap2 != null)
            {
                enemyMap2.TakeDamage(damage);
            }
            
            BossController_4 bossEnemy = collision.GetComponent<BossController_4>();
            if (bossEnemy != null)
            {
                bossEnemy.TakeDamage(damage);
            }
        }

        hit = true;
        boxCollider.enabled = false;
        anim.SetTrigger("hit");
        Invoke("Deactivate", 0.5f);
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
        gameObject.SetActive(false);
    }
}