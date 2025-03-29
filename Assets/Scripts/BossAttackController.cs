using UnityEngine;
using System.Collections;
using static PlayerController;

public class BossAttackController : MonoBehaviour
{
    [Header("Attack Configuration")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform beamSpawnPoint;
    [SerializeField] private float detectionRange = 15f; // Add this: detection range for the boss
    [SerializeField] private bool drawDebugRange = true; // Add this: to visualize the range in editor

    [Header("Beam Attack Settings")]
    [SerializeField] private GameObject beamSpritePrefab;
    [SerializeField] private float beamDuration = 0.6f;
    [SerializeField] private float beamLength = 15f;
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] private int beamDamage = 20;

    [Header("Poison Arrow Settings")]
    [SerializeField] private GameObject poisonArrowPrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private int poisonDamage = 3;
    [SerializeField] private float poisonDuration = 5f;

    [Header("Animation References")]
    [SerializeField] public Animator animator;

    [Header("Attack Parameters")]
    [SerializeField] private float minTimeBetweenAttacks = 5f;
    [SerializeField] private float maxTimeBetweenAttacks = 10f;
    [Header("Damage Parameters")]
    [SerializeField] public int groundHitDamage = 20;
    [SerializeField] public int playerDeflectDamage = 10;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float jumpDuration = 0.8f;
    private Vector3 originalPosition;
    [SerializeField] private PlayerController player;
    // Attack types
    public enum AttackType
    {
        BeamAttack,
        PoisonArrowAttack,
        JumpAttack
    }

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isJumping = false;
    private void Start()
    {
        lastAttackTime = -minTimeBetweenAttacks; // Allow immediate first attack
    }

    private void Update()
    {
        // Check if enough time has passed since last attack
        if (!isAttacking && IsPlayerInRange() && !IsInJumpAnimation() && Time.time >= lastAttackTime + Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks))
        {
            PerformRandomAttack();
        }
    }

    private void PerformRandomAttack()
    {
        if (!IsPlayerInRange()) return;
        // Randomly choose between attack types
        AttackType selectedAttack = (AttackType)Random.Range(0, System.Enum.GetValues(typeof(AttackType)).Length);

        switch (selectedAttack)
        {
            case AttackType.BeamAttack:
                StartCoroutine(PerformBeamAttack(0.75f));
                break;
            case AttackType.PoisonArrowAttack:
                StartCoroutine(PerformPoisonArrowAttack());
                break;
            case AttackType.JumpAttack:
                PerformJumpArrowAttack();
                break;
        }
    }
    private bool IsPlayerInRange()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
            }
            else
            {
                return false; // No player found
            }
        }

        // Check if player is within detection range
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        return distanceToPlayer <= detectionRange;
    }

    // Add this to visualize the detection range in the editor
    private void OnDrawGizmos()
    {
        if (drawDebugRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
    private IEnumerator PerformBeamAttack(float delay)
    {
        if (!IsPlayerInRange())
        {
            isAttacking = false;
            yield break;
        }
        isAttacking = true;
        lastAttackTime = Time.time;       
        // Trigger beam attack animation
        animator.SetTrigger("BeamAttack");
        yield return new WaitForSeconds(delay);
        // Calculate beam direction and origin
        Vector2 beamDirection = DetermineBeamDirection();
        Vector2 beamOrigin = (Vector2)beamSpawnPoint.position;

        // Spawn beam sprite
        GameObject beamObject = Instantiate(beamSpritePrefab, beamOrigin, Quaternion.identity);
        beamObject.SetActive(true);
        // Rotate beam to match direction
        float angle = beamDirection.x > 0 ? 0 : 180f;
        beamObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Stretch beam to correct length
        beamObject.transform.localScale = new Vector3(beamLength, beamObject.transform.localScale.y, 1);

        // Perform raycast for damage
        RaycastHit2D[] hits = Physics2D.RaycastAll(beamOrigin, beamDirection, beamLength, playerLayer);

        // Debug logging
        Debug.Log($"Beam Attack: Origin {beamOrigin}, Direction {beamDirection}, Length {beamLength}");
        Debug.Log($"Number of hits: {hits.Length}");

        // Damage players if hit
        foreach (RaycastHit2D hit in hits)
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");

            // Multiple ways to detect player

            if (player != null)
            {
                Debug.Log($"Beam hit player {hit.collider.gameObject.name} at {hit.point}, dealing {beamDamage} damage");
                player.TakeDamage(beamDamage, DamageType.Normal);
            }
            else
            {
                Debug.Log($"No PlayerController found on {hit.collider.gameObject.name}");
            }
        }

        // Wait for beam duration
        yield return new WaitForSeconds(beamDuration);

        // Destroy beam sprite
        Destroy(beamObject);

        isAttacking = false;
    }
    private void PerformJumpArrowAttack()
    {
        if (isJumping || IsPlayerInRange() ) return;


        StartCoroutine(JumpAndShootRoutine());

    }
    private void ShootArrows()
    {
        GameObject arrow = Instantiate(arrowPrefab, projectileSpawnPoint.position, Quaternion.identity);
        arrow.SetActive(true);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        // Set arrow velocity downwards
        arrowRb.linearVelocity = Vector2.down * arrowSpeed;

        // Add arrow collision detection
        ArrowProjectile arrowScript = arrow.AddComponent<ArrowProjectile>();
        arrowScript.Initialize(this, groundLayer, playerLayer);
    }
    private IEnumerator JumpAndShootRoutine()
    {
        isJumping = true;
        animator.SetTrigger("Jump");

        // Store original position
        originalPosition = transform.position;

        // Jump up
        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration / 2)
        {
            float t = elapsedTime / (jumpDuration / 2);
            transform.position = Vector3.Lerp(originalPosition, originalPosition + Vector3.up * jumpHeight, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.42f);
        // Shoot arrows
        ShootArrows();

        // Fall down
        elapsedTime = 0f;
        while (elapsedTime < jumpDuration / 2)
        {
            float t = elapsedTime / (jumpDuration / 2);
            transform.position = Vector3.Lerp(originalPosition + Vector3.up * jumpHeight, originalPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset position and state
        transform.position = originalPosition;
        animator.SetTrigger("Land");
        isJumping = false;
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
        Vector3 newScale = arrow.transform.localScale;
        newScale.x *= -1;
        arrow.transform.localScale = newScale;
        arrow.SetActive(true);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        // Calculate arrow direction
        Vector2 arrowDirection = DetermineProjectileDirection();
        arrowRb.linearVelocity = arrowDirection * arrowSpeed;

        // Add poison arrow behavior
        PoisonArrowController arrowController = arrow.AddComponent<PoisonArrowController>();
        Destroy(arrow, 3f);
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
        if (!isAttacking && !IsInJumpAnimation())
        {
            switch (attackType)
            {
                case AttackType.BeamAttack:
                    StartCoroutine(PerformBeamAttack(0.75f));
                    break;
                case AttackType.PoisonArrowAttack:
                    StartCoroutine(PerformPoisonArrowAttack());
                    break;
                case AttackType.JumpAttack:
                    PerformJumpArrowAttack();
                    break;
            }
        }
    }
    protected virtual void DealDamage(int damage)
    {
        if (player != null)
        {
            player.TakeDamage(damage); // Make sure PlayerController has a TakeDamage method
        }
    }
    private bool IsInJumpAnimation()
    {
        
        return animator.GetCurrentAnimatorStateInfo(0).IsName("j_up");
    }
}