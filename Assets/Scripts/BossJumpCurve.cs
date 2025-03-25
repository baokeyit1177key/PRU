using UnityEngine;

public class BossJumpCurveSetup : MonoBehaviour
{
    [Header("Jump Animation Curve Presets")]
    public AnimationCurve SmoothParabolicJump;
    public AnimationCurve SharpJump;
    public AnimationCurve GentleJump;

    private void Awake()
    {
        // Smooth Parabolic Jump (Recommended for Most Cases)
        SmoothParabolicJump = new AnimationCurve(
            new Keyframe(0f, 0f, 0, 0),           // Start at ground
            new Keyframe(0.3f, 1f, 2, 2),         // Quick rise to peak
            new Keyframe(0.7f, 1f, -2, -2),       // Slight pause at peak
            new Keyframe(1f, 0f, -3, -3)          // Smooth descent
        );

        // Sharp, Quick Jump
        SharpJump = new AnimationCurve(
            new Keyframe(0f, 0f, 0, 4),           // Rapid initial ascent
            new Keyframe(0.2f, 1f, 0, 0),         // Quick peak
            new Keyframe(1f, 0f, -4, -4)          // Steep descent
        );

        // Gentle, Slow Jump
        GentleJump = new AnimationCurve(
            new Keyframe(0f, 0f, 0, 1),           // Gradual rise
            new Keyframe(0.5f, 1f, 1, 1),         // Soft peak
            new Keyframe(1f, 0f, -1, -1)          // Gradual descent
        );

        // Optional: Make curves smooth
        SmoothParabolicJump.preWrapMode = WrapMode.ClampForever;
        SmoothParabolicJump.postWrapMode = WrapMode.ClampForever;
    }

    // Visualization method for understanding curve in Inspector
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        DrawAnimationCurveGizmo(SmoothParabolicJump, Vector3.zero, 5f, 2f);
    }

    // Helper method to visualize curve in Scene view
    private void DrawAnimationCurveGizmo(AnimationCurve curve, Vector3 origin, float width, float height)
    {
        if (curve == null) return;

        Vector3 lastPos = origin;
        for (float t = 0; t <= 1f; t += 0.05f)
        {
            Vector3 currentPos = origin + new Vector3(
                t * width,
                curve.Evaluate(t) * height,
                0
            );

            Gizmos.DrawLine(lastPos, currentPos);
            lastPos = currentPos;
        }
    }
}