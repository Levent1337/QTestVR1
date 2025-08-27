using UnityEngine;
using UnityEngine.InputSystem;


public class SimpleSwimming : MonoBehaviour
{

    [Header("Buoyancy Settings")]
    public float floatForce = 15f;
    public float Drag = 2.0f;
    public float angularDamping = 1f;
    public float maxSpeed = 10f;

    [Header("References)")]
    public BoxCollider waterCollider;

    public Rigidbody rb;
    public Transform leftHand;
    public Transform rightHand;
    public Rigidbody leftHandRb;
    public Rigidbody rightHandRb;

    public InputActionReference leftSwimAction;
    public InputActionReference rightSwimAction;
    [Header("Propulsion Tuning")]
    public float thrustPerMS = 3.0f;     // acceleration per m/s of relative hand speed
    public float maxThrust = 50f;        // clamp accel
    public float minHandSpeed = 0.2f;    // ignore micro jitter
    public bool horizontalOnly = false;  // zero vertical thrust if desired




    private void OnEnable()
    {
        if (leftSwimAction) leftSwimAction.action.Enable();
        if (rightSwimAction) rightSwimAction.action.Enable();
    }
    private void OnDisable()
    {
        if (leftSwimAction) leftSwimAction.action.Disable();
        if (rightSwimAction) rightSwimAction.action.Disable();
    }
    private void FixedUpdate()
    {
        ApplyDirectionalDrag();
        ApplyBuoyancy();
        ClampVelocity();
       
         Debug.Log($"Left={leftSwimAction.action.ReadValue<float>():F2} Right={rightSwimAction.action.ReadValue<float>():F2}");

        if (leftSwimAction && leftSwimAction.action.IsPressed())
        {
            HandleHandSwim(leftHand, leftHandRb);
        }
        if (rightSwimAction && rightSwimAction.action.IsPressed())
        {
            HandleHandSwim(rightHand, rightHandRb);
        }

    }

    private void ApplyDirectionalDrag()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        localVelocity.x *= (1 - Drag * Time.fixedDeltaTime);
        localVelocity.z *= (1 - Drag * Time.fixedDeltaTime);
        rb.linearVelocity = transform.TransformDirection(localVelocity);
    }

    private void ApplyBuoyancy()
    {
        ApplyBuoyancyAtPoint();
        
        ApplyAngularDamping();
    }

    private void ApplyBuoyancyAtPoint()
    {
        if (rb == null || waterCollider == null) return;

        if (RigidbodyInsideWater(rb, out float depth))
        {
            // Buoyancy force (applies upward lift)
            Vector3 uplift = Vector3.up * floatForce * depth;
            rb.AddForce(uplift, ForceMode.Force); ;

            // Vertical damping (slows vertical velocity)
            Vector3 velocity = rb.linearVelocity; // center of mass velocity
            Vector3 verticalDamping = -Vector3.up * velocity.y * Drag;
            rb.AddForce(verticalDamping, ForceMode.Force);
        }
    }


    private void ApplyAngularDamping()
    {
        Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);
        localAngularVel.x *= (1 - Drag * Time.fixedDeltaTime);
        localAngularVel.z *= (1 - Drag * Time.fixedDeltaTime);
        localAngularVel.y *= (1 - Drag * Time.fixedDeltaTime);
        rb.angularVelocity = transform.TransformDirection(localAngularVel);
    }

    private bool RigidbodyInsideWater(Rigidbody rb, out float depth)
    {
        depth = 0f;
        if (rb == null || waterCollider == null) return false;

        // Use the rigidbody's center of mass position in world space
        Vector3 com = rb.worldCenterOfMass;

        if (!waterCollider.bounds.Contains(com))
            return false;

        float waterTop = waterCollider.bounds.max.y;
        depth = Mathf.Clamp(waterTop - com.y, 0, waterCollider.bounds.size.y);

        return depth > 0.01f;
    }

    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    public void ApplyPaddleForce(Vector3 force, Vector3 applicationPoint)
    {
        rb.AddForceAtPosition(force, applicationPoint, ForceMode.Force);
    }

    private void HandleHandSwim(Transform hand, Rigidbody handRb)
    {
        if (hand == null || handRb == null || rb == null) return;

        // Hand velocity in world
        Vector3 handVel = handRb.linearVelocity;

        // Body velocity at the hand's world position (what the water "sees" moving with the body)
        Vector3 bodyVelAtHand = rb.GetPointVelocity(hand.position);

        // Relative velocity = hand motion through the water, not just being carried by the body
        Vector3 relVel = handVel - bodyVelAtHand;

        // Optional: only use horizontal component
        if (horizontalOnly) relVel.y = 0f;

        // Deadzone
        if (relVel.sqrMagnitude < minHandSpeed * minHandSpeed) return;

        // Thrust opposite the stroke direction
        Vector3 accel = -relVel * thrustPerMS;

        // Clamp
        if (accel.sqrMagnitude > maxThrust * maxThrust)
            accel = accel.normalized * maxThrust;

        // Apply as acceleration so mass doesn’t change the feel
        rb.AddForce(accel, ForceMode.Acceleration);
    }

}
