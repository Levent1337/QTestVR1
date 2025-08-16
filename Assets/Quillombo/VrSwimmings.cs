using UnityEngine;

public class VrSwimmings : MonoBehaviour
{
    [Header("References (children)")]
    [SerializeField] private Rigidbody xrOriginRb;   // RB on XR Origin
    [SerializeField] private WaterProbe waterProbe;  // probe with isInWater

    [Header("Buoyancy (constant)")]
    [Tooltip("Constant upward force in Newtons (scaled by wetness).")]
    public float upwardForce = 100f;

    [Header("State smoothing (hysteresis)")]
    [Tooltip("Seconds to fade IN when entering water (smaller = snappier).")]
    public float enterBlendTime = 0.20f;
    [Tooltip("Seconds to fade OUT when exiting water (bigger = stickier).")]
    public float exitBlendTime = 0.40f;

    [Header("Damping & limits")]
    [Tooltip("Vertical damping per kg (N·s/m per kg). 8–15 is a good range.")]
    public float verticalDampingPerKg = 12f;
    [Tooltip("Extra damping right at the surface (applied when 0<wetness<1).")]
    public float surfaceDampingBoost = 2.0f;   // boost factor on top of base damping
    [Tooltip("Optional max upward speed (m/s). 0 = no clamp.")]
    public float maxRiseSpeed = 1.5f;

    [Header("Drag")]
    [Tooltip("Linear drag while fully in water.")]
    public float waterDrag = 2f;
    [Tooltip("Linear drag while not in water.")]
    public float airDrag = 0f;

    // Smoothed water occupancy in [0..1]
    float wetness = 0f;
    float baseDampingNPerMS; // mass-scaled damping coefficient (N·s/m)

    void Awake()
    {
        if (!xrOriginRb) xrOriginRb = GetComponentInChildren<Rigidbody>(true);
        if (!waterProbe) waterProbe = GetComponentInChildren<WaterProbe>(true);

        if (xrOriginRb)
            baseDampingNPerMS = xrOriginRb.mass * Mathf.Max(0f, verticalDampingPerKg);
    }

    void FixedUpdate()
    {
        if (!xrOriginRb || !waterProbe) return;
        if (xrOriginRb.isKinematic) return;

        // --- 1) Smooth the 'in water' state into a continuous wetness 0..1 (hysteresis) ---
        bool rawInWater = waterProbe.isInWater;
        float target = rawInWater ? 1f : 0f;
        float tau = rawInWater ? Mathf.Max(0.0001f, enterBlendTime) : Mathf.Max(0.0001f, exitBlendTime);
        // MoveTowards for predictable, frame-rate independent easing
        wetness = Mathf.MoveTowards(wetness, target, Time.fixedDeltaTime / tau);

        // --- 2) Blend drag based on wetness (prevents sharp transitions) ---
        xrOriginRb.linearDamping = Mathf.Lerp(airDrag, waterDrag, wetness);

        if (wetness <= 0f) return; // fully out of water, nothing to do

        // --- 3) Apply constant upward force scaled by wetness ---
        xrOriginRb.WakeUp();
        xrOriginRb.AddForce(Vector3.up * (upwardForce * wetness), ForceMode.Force);

        // --- 4) linearVelocity-proportional damping (extra near surface) ---
        float vy = Vector3.Dot(xrOriginRb.linearVelocity, Vector3.up);
        // Surface boost peaks at wetness=0.5 and fades to 0 at 0 or 1
        float surfaceBlend = wetness * (1f - wetness) * 4f; // 0..1 bell curve
        float damping = baseDampingNPerMS * (1f + surfaceDampingBoost * surfaceBlend);
        float dampForce = -damping * vy; // oppose vertical linearVelocity
        xrOriginRb.AddForce(Vector3.up * dampForce, ForceMode.Force);

        // --- 5) Optional rise speed clamp ---
        if (maxRiseSpeed > 0f && vy > maxRiseSpeed)
        {
            var v = xrOriginRb.linearVelocity;
            v.y = maxRiseSpeed;
            xrOriginRb.linearVelocity = v;
        }
    }
}
