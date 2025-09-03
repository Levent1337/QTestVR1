using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Boat Settings")]
    public float forwardDrag = 0.2f;
    public float sidewaysDrag = 2.0f;
    public float angularDamping = 1f;     // consider renaming to 'angularDrag' (see Awake)
    public float maxSpeed = 10f;

    [Header("Buoyancy Settings")]
    public float floatForce = 15f;
    public float verticalDampingFactor = 0.5f;
    public float rollDampingFactor = 1.0f;
    public float pitchDampingFactor = 0.5f;
    public float yawDampingFactor = 0.2f;

    public Transform northPoint, southPoint, eastPoint, westPoint;

    [Tooltip("The water volume (must be a BoxCollider with tag 'Water')")]
    public BoxCollider waterCollider;

    // NEW: tune how much rotation a paddle stroke adds
    [Header("Paddle Coupling")]
    [Range(0f, 1f)] public float paddleTorqueScale = 0.25f; // lower = less rotation
    public float paddleForceScale = 1.0f;                   // keep linear push strength

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        // If this was intended as Unity's angular drag, use angularDrag (Rigidbody doesn't have 'angularDamping')
        rb.angularDamping = angularDamping;

        // Optional: cap spin so yaw can't explode
        rb.maxAngularVelocity = 3.0f;
    }

    private void FixedUpdate()
    {
        ApplyDirectionalDrag();
        ApplyBuoyancy();
        ClampVelocity();
    }

    private void ApplyDirectionalDrag()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        localVelocity.x *= (1 - sidewaysDrag * Time.fixedDeltaTime);
        localVelocity.z *= (1 - forwardDrag * Time.fixedDeltaTime);
        rb.linearVelocity = transform.TransformDirection(localVelocity);
    }

    private void ApplyBuoyancy()
    {
        ApplyBuoyancyAtPoint(northPoint);
        ApplyBuoyancyAtPoint(southPoint);
        ApplyBuoyancyAtPoint(eastPoint);
        ApplyBuoyancyAtPoint(westPoint);
        ApplyAngularDamping();
    }

    private void ApplyBuoyancyAtPoint(Transform point)
    {
        if (point == null || waterCollider == null) return;
        if (PointInsideWater(point.position, out float depth))
        {
            Vector3 uplift = Vector3.up * floatForce * depth;
            rb.AddForceAtPosition(uplift, point.position, ForceMode.Force);

            Vector3 pointVelocity = rb.GetPointVelocity(point.position);
            Vector3 verticalDamping = -Vector3.up * pointVelocity.y * verticalDampingFactor;
            rb.AddForceAtPosition(verticalDamping, point.position, ForceMode.Force);
        }
    }

    private void ApplyAngularDamping()
    {
        Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);
        localAngularVel.x *= (1 - rollDampingFactor * Time.fixedDeltaTime);
        localAngularVel.z *= (1 - pitchDampingFactor * Time.fixedDeltaTime);
        localAngularVel.y *= (1 - yawDampingFactor * Time.fixedDeltaTime);
        rb.angularVelocity = transform.TransformDirection(localAngularVel);
    }

    private bool PointInsideWater(Vector3 point, out float depth)
    {
        depth = 0f;
        if (!waterCollider.bounds.Contains(point))
            return false;
        float waterTop = waterCollider.bounds.max.y;
        depth = Mathf.Clamp(waterTop - point.y, 0, waterCollider.bounds.size.y);
        return depth > 0.01f;
    }

    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    public void ApplyPaddleForce(Vector3 force, Vector3 applicationPoint)
    {
        // Split into linear + scaled torque instead of AddForceAtPosition (which always injects full torque)
        rb.AddForce(force * paddleForceScale, ForceMode.Force);

        Vector3 r = applicationPoint - rb.worldCenterOfMass;
        Vector3 torque = Vector3.Cross(r, force);
        rb.AddTorque(torque * paddleTorqueScale, ForceMode.Force);
    }

}
