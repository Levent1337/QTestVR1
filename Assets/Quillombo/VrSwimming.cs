using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class VRSwimmingRoot : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty swimAction;         // trigger/grip action (InputActionReference)

    [Header("References (child body)")]
    public Rigidbody bodyRB;                       // main body RB living on a CHILD
    public Transform leftHand;
    public Transform rightHand;

    [Header("Water Filter")]
    public string waterTag = "Water";              // tag on water trigger volumes; leave empty to accept any trigger
    public bool requireTrigger = true;             // water colliders must be triggers

    [Header("Buoyancy & Damping")]
    public float buoyancyAccel = 0.75f;            // passive upward accel (m/s^2)
    public float waterLinearDamping = 4f;          // Unity 6: Rigidbody.linearDamping
    public float waterAngularDamping = 2f;         // Unity 6: Rigidbody.angularDamping

    [Header("Stroke Propulsion")]
    public float strokeAccelPerMS = 2.25f;         // accel per 1 m/s of hand speed
    public float maxSwimSpeed = 3.5f;              // 0 = uncapped
    [Range(0f, 0.95f)] public float handVelSmoothing = 0.2f;

    [Header("Interop")]
    public bool disableStickLocomotion = true;
    public Behaviour movementComponent;            // drag your PhysicsMovement here (optional)

    [Header("Debug")]
    public bool debugLogs = true;

    // state
    bool inWater;
    int overlapCount;                              // supports overlapping volumes
    Vector3 prevL, prevR, velL, velR;
    float origLinDamp, origAngDamp;

    void OnEnable()
    {
        var act = swimAction.action;
        if (act != null && !act.enabled) act.Enable();
    }

    void Start()
    {
        if (bodyRB == null)
        {
            Debug.LogError("[VRSwimmingRoot] bodyRB is not assigned. Drag your CHILD Rigidbody here.");
            enabled = false;
            return;
        }

        // cache damping
        origLinDamp = bodyRB.linearDamping;
        origAngDamp = bodyRB.angularDamping;

        if (leftHand != null) prevL = leftHand.position;
        if (rightHand != null) prevR = rightHand.position;

        // make sure at least one collider exists under the body RB (compound collider)
        if (bodyRB.GetComponentInChildren<Collider>(true) == null)
        {
            Debug.LogWarning("[VRSwimmingRoot] No Collider found in bodyRB's hierarchy. " +
                             "Move your body collider under the same child that has the Rigidbody.");
        }

        if (debugLogs)
            Debug.Log($"[VRSwimmingRoot] Ready on '{name}'. Using child RB '{bodyRB.name}'.");
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (leftHand != null)
        {
            Vector3 inst = (leftHand.position - prevL) / Mathf.Max(dt, 1e-5f);
            velL = Vector3.Lerp(inst, velL, handVelSmoothing);
            prevL = leftHand.position;
        }
        if (rightHand != null)
        {
            Vector3 inst = (rightHand.position - prevR) / Mathf.Max(dt, 1e-5f);
            velR = Vector3.Lerp(inst, velR, handVelSmoothing);
            prevR = rightHand.position;
        }

        if (!inWater) return;

        // 1) Passive buoyancy (mass-agnostic)
        bodyRB.AddForce(Vector3.up * buoyancyAccel, ForceMode.Acceleration);

        // 2) Strokes while button held
        var act = swimAction.action;
        if (act != null && act.IsPressed())
        {
            Vector3 swimAccel = -velL - velR; // push water back -> go forward
            bodyRB.AddForce(swimAccel * strokeAccelPerMS, ForceMode.Acceleration);
        }

        // 3) Optional speed cap
        if (maxSwimSpeed > 0f)
        {
            Vector3 v = bodyRB.linearVelocity;
            float m = v.magnitude;
            if (m > maxSwimSpeed)
                bodyRB.linearVelocity = v * (maxSwimSpeed / m);
        }
    }

    // -------- Called by SwimTriggerRelay on the child RB --------
    public void RelayTriggerEnter(Collider other, Component sender)
    {
        if (!PassesWaterFilter(other))
        {
            if (debugLogs) Debug.Log($"[VRSwimmingRoot] IGNORE enter '{other.name}' ({sender.name}).");
            return;
        }
        overlapCount++;
        if (!inWater) EnterWater();
        if (debugLogs) Debug.Log($"[VRSwimmingRoot] ENTER water via '{other.name}' ({sender.name}). overlaps={overlapCount}");
    }

    public void RelayTriggerExit(Collider other, Component sender)
    {
        if (!PassesWaterFilter(other))
        {
            if (debugLogs) Debug.Log($"[VRSwimmingRoot] IGNORE exit '{other.name}' ({sender.name}).");
            return;
        }
        overlapCount = Mathf.Max(0, overlapCount - 1);
        if (debugLogs) Debug.Log($"[VRSwimmingRoot] EXIT water via '{other.name}' ({sender.name}). overlaps={overlapCount}");
        if (inWater && overlapCount == 0) ExitWater();
    }

    // -------- enter/exit & helpers --------
    bool PassesWaterFilter(Collider other)
    {
        if (requireTrigger && !other.isTrigger) return false;
        if (!string.IsNullOrEmpty(waterTag) && !other.CompareTag(waterTag)) return false;
        return true;
    }

    void EnterWater()
    {
        inWater = true;
        origLinDamp = bodyRB.linearDamping;
        origAngDamp = bodyRB.angularDamping;
        bodyRB.linearDamping = waterLinearDamping;
        bodyRB.angularDamping = waterAngularDamping;
        if (disableStickLocomotion && movementComponent != null) movementComponent.enabled = false;

        if (debugLogs)
            Debug.Log($"[VRSwimmingRoot] >>> ENTERED water. linDamp={bodyRB.linearDamping:F2}, angDamp={bodyRB.angularDamping:F2}");
    }

    void ExitWater()
    {
        inWater = false;
        bodyRB.linearDamping = origLinDamp;
        bodyRB.angularDamping = origAngDamp;
        if (disableStickLocomotion && movementComponent != null) movementComponent.enabled = true;

        if (debugLogs)
            Debug.Log($"[VRSwimmingRoot] <<< EXITED water. linDamp={bodyRB.linearDamping:F2}, angDamp={bodyRB.angularDamping:F2}");
    }
}
