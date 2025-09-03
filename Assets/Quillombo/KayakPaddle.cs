using UnityEngine;

public class KayakPaddle : MonoBehaviour
{
    [Header("References")]
    public BoatController boat;
    public Transform paddleTip;
    public BoxCollider waterCollider;

    [Header("Force Settings")]
    public float paddleForceMultiplier = 15f;
    public float minMovementThreshold = 0.05f;
    public float sidewaysFactor = 0.2f;

    [Header("Smoothing")]
    public float velocitySmoothing = 10f;

    private Vector3 lastTipPos;
    private Vector3 smoothedVelocity;

    private void Start()
    {
        if (paddleTip != null)
            lastTipPos = paddleTip.position;
    }

    private void Update()
    {
        if (paddleTip == null || boat == null || waterCollider == null)
            return;

        // Calculate raw velocity
        Vector3 velocity = (paddleTip.position - lastTipPos) / Time.deltaTime;
        lastTipPos = paddleTip.position;

        // Smooth sudden changes
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, Time.deltaTime * velocitySmoothing);

        // Only apply if paddle is in water and moving enough
        if (waterCollider.bounds.Contains(paddleTip.position) && smoothedVelocity.magnitude > minMovementThreshold)
        {
            // Convert velocity to boat local space
            Vector3 localVelocity = boat.transform.InverseTransformDirection(smoothedVelocity);

            // Adjust for boat forward = local X axis
            float forwardComponent = -localVelocity.x;                   // pushing along X
            float sidewaysComponent = -localVelocity.z * sidewaysFactor; // reduced sideways effect

            // Build force in local space
            Vector3 forceLocal = new Vector3(forwardComponent, 0, sidewaysComponent);

            // Convert back to world space
            Vector3 force = boat.transform.TransformDirection(forceLocal) * paddleForceMultiplier;

            boat.ApplyPaddleForce(force, paddleTip.position);
        }
    }
}
