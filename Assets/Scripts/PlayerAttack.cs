using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint; // Replaces arrowPoint
    [SerializeField] private GameObject bulletPrefab; // Replaces arrows array

    private Animator anim;
    private Movement movement;
    private float CooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && CooldownTimer > attackCooldown && movement.canAttack())
            Attack();

        CooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        anim.SetTrigger("attack");
        CooldownTimer = 0;

    }
}
