using System.Collections;
using UnityEngine;

public class BossMovementController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float jumpDistance = 18f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float jumpDuration = 3f;
    [SerializeField] private AnimationCurve jumpCurve;
    [Header("Proximity Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float minTimeBetweenJumps = 30f;
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isJumping = false;
    private float lastJumpTime;
    private void Start()
    {
        jumpCurve = new AnimationCurve(
           new Keyframe(0f, 0f, 0, 0),           // Start at ground
           new Keyframe(0.3f, 1f, 2, 2),         // Quick rise to peak
           new Keyframe(0.7f, 1f, -2, -2),       // Slight pause at peak
           new Keyframe(1f, 0f, -3, -3)          // Smooth descent
       );
        startPosition = transform.position;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        lastJumpTime = -minTimeBetweenJumps;
    }
    private void Update()
    {

        if (!isJumping && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Check if player is within detection radius and enough time has passed since last jump
            if (distanceToPlayer <= detectionRadius &&
                Time.time >= lastJumpTime + minTimeBetweenJumps)
            {
                JumpToOtherSide();
            }
        }
    }
    public void JumpToOtherSide()
    {
        if (isJumping) return;

        // Determine target position (current + or - jump distance)
        targetPosition = new Vector3(
            transform.position.x + (transform.localScale.x > 0 ? -jumpDistance : jumpDistance),
            transform.position.y,
            transform.position.z
        );

        StartCoroutine(SmoothJumpCoroutine());
    }

    private IEnumerator SmoothJumpCoroutine()
    {
        isJumping = true;
        lastJumpTime = Time.time;
        // Trigger jump animation
        animator.SetTrigger("Jump");

        float elapsedTime = 0f;

        Vector3 startPos = transform.position;

        while (elapsedTime < jumpDuration)
        {
            // Calculate horizontal movement
            float horizontalProgress = elapsedTime / jumpDuration;
            Vector3 currentHorizontalPos = Vector3.Lerp(startPos, targetPosition, horizontalProgress);

            // Calculate vertical movement using jump curve
            float verticalProgress = jumpCurve.Evaluate(horizontalProgress);
            Vector3 verticalOffset = Vector3.up * verticalProgress * jumpHeight;

            // Combine horizontal and vertical movement
            transform.position = currentHorizontalPos + verticalOffset;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exactly at target
        transform.position = targetPosition;

        // Flip local scale to face opposite direction
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;

        // Animation back to idle
        animator.SetTrigger("Land");

        isJumping = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
