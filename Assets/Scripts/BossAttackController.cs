using UnityEngine;
using System.Collections;

public class BossAttackController : MonoBehaviour
{
    [Header("Attack Configuration")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Beam Attack Settings")]
    [SerializeField] private GameObject beamSpritePrefab;
    [SerializeField] private float beamDuration = 0.2f;
    [SerializeField] private float beamLength = 20f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int beamDamage = 20;

    [Header("Poison Arrow Settings")]
    [SerializeField] private GameObject poisonArrowPrefab;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private int poisonDamage = 3;
    [SerializeField] private float poisonDuration = 3f;

    [Header("Animation References")]
    [SerializeField] private Animator animator;

    [Header("Attack Parameters")]
    [SerializeField] private float minTimeBetweenAttacks = 2f;
    [SerializeField] private float maxTimeBetweenAttacks = 4f;

    // Attack types
    public enum AttackType
    {
        BeamAttack,
        PoisonArrowAttack
    }

    private float lastAttackTime;
    private bool isAttacking = false;

    private void Start()
    {
        lastAttackTime = -minTimeBetweenAttacks; // Allow immediate first attack
    }

    private void Update()
    {
        // Check if enough time has passed since last attack
        if (!isAttacking && Time.time >= lastAttackTime + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks))
        {
            PerformRandomAttack();
        }
    }

    private void PerformRandomAttack()
    {
        // Randomly choose between attack types
        AttackType selectedAttack = (AttackType)Random.Range(0, System.Enum.GetValues(typeof(AttackType)).Length);

        switch (selectedAttack)
        {
            case AttackType.BeamAttack:
                StartCoroutine(PerformBeamAttack());
                break;
            case AttackType.PoisonArrowAttack:
                StartCoroutine(PerformPoisonArrowAttack());
                break;
        }
    }

    private IEnumerator PerformBeamAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Trigger beam attack animation
        animator.SetTrigger("BeamAttack");

        // Calculate beam direction and origin
        Vector2 beamDirection = DetermineBeamDirection();
        Vector2 beamOrigin = (Vector2)projectileSpawnPoint.position;

        // Spawn beam sprite
        GameObject beamObject = Instantiate(beamSpritePrefab, beamOrigin, Quaternion.identity);
        beamObject.SetActive(true);
        // Rotate beam to match direction
        float angle = beamDirection.x > 0 ? 0 : 180f;
        beamObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Stretch beam to correct length
        beamObject.transform.localScale = new Vector3(beamLength, beamObject.transform.localScale.y, 1);

        // Perform raycast for damage
        RaycastHit2D hit = Physics2D.Raycast(beamOrigin, beamDirection, beamLength, playerLayer);

        // Damage player if hit
        if (hit.collider != null)
        {
            PlayerController playerHealth = hit.collider.GetComponent<PlayerController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(beamDamage);
            }
        }

        // Wait for beam duration
        yield return new WaitForSeconds(beamDuration);

        // Destroy beam sprite
        Destroy(beamObject);

        isAttacking = false;
    }

    private IEnumerator PerformPoisonArrowAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Trigger poison arrow attack animation
        animator.SetTrigger("PoisonArrowAttack");

        // Wait for animation wind-up
        yield return new WaitForSeconds(0.5f);

        // Spawn arrow
        GameObject arrow = Instantiate(poisonArrowPrefab, projectileSpawnPoint.position, Quaternion.identity);
        arrow.SetActive(true);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        // Calculate arrow direction
        Vector2 arrowDirection = DetermineProjectileDirection();
        arrowRb.linearVelocity = arrowDirection * arrowSpeed;

        // Add poison arrow behavior
        PoisonArrowController arrowController = arrow.AddComponent<PoisonArrowController>();
        arrowController.Initialize(poisonDamage, poisonDuration);

        // Wait for attack to complete
        yield return new WaitForSeconds(1f);

        isAttacking = false;
    }

    private Vector2 DetermineBeamDirection()
    {
        // Determine beam direction based on boss facing
        return transform.localScale.x > 0 ? Vector2.left : Vector2.right;
    }

    private Vector2 DetermineProjectileDirection()
    {
        // Find player or default direction
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            // Direction from boss to player
            return (player.position - projectileSpawnPoint.position).normalized;
        }

        // Default direction based on boss facing
        return transform.localScale.x > 0 ? Vector2.left : Vector2.right;
    }

    // Public method to manually trigger an attack type
    public void ForceAttack(AttackType attackType)
    {
        if (!isAttacking)
        {
            switch (attackType)
            {
                case AttackType.BeamAttack:
                    StartCoroutine(PerformBeamAttack());
                    break;
                case AttackType.PoisonArrowAttack:
                    StartCoroutine(PerformPoisonArrowAttack());
                    break;
            }
        }
    }
}