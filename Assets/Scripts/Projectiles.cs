using UnityEngine;
using UnityEngine.EventSystems;

public class Projectiles : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private bool hit;
    private Vector2 direction;
    private Animator anim;
    private BoxCollider2D boxCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
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
        if (collision.CompareTag("Enemy"))
        {
            BasicEnemyMap4 enemy = collision.GetComponent<BasicEnemyMap4>();
            if(enemy != null) 
            {
                enemy.TakeDamage(100);
            }
            Destroy(gameObject);
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
