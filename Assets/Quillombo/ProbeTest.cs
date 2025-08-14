using UnityEngine;
using UnityEngine.InputSystem;

public class XRInputProbe : MonoBehaviour
{
    [Header("Assign these from XRI asset")]
    public InputActionReference moveAction; // XRI Left / Thumbstick
    public InputActionReference turnAction; // XRI Right / Thumbstick

    Vector2 lastMove, lastTurn;
    float logCooldown;

    void Awake()
    {
        Debug.Log("[PROBE] Connected InputSystem devices:");
        foreach (var d in InputSystem.devices)
            Debug.Log($"[PROBE]  - {d.displayName} ({d.layout}) | {d.path}");
    }

    void OnEnable()
    {
        moveAction?.action?.Enable();
        turnAction?.action?.Enable();

        LogBindings("MOVE", moveAction?.action);
        LogBindings("TURN", turnAction?.action);
    }

    void OnDisable()
    {
        moveAction?.action?.Disable();
        turnAction?.action?.Disable();
    }

    void Update()
    {
        var m = moveAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
        var t = turnAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;

        // Throttle logs and only print when values change a bit
        logCooldown -= Time.unscaledDeltaTime;
        if (logCooldown <= 0f &&
            ((m - lastMove).sqrMagnitude > 0.0004f || (t - lastTurn).sqrMagnitude > 0.0004f))
        {
            Debug.Log($"[PROBE] Move:{m} | Turn:{t} | " +
                      $"M enabled:{moveAction.action.enabled} phase:{moveAction.action.phase} | " +
                      $"T enabled:{turnAction.action.enabled} phase:{turnAction.action.phase}");
            lastMove = m;
            lastTurn = t;
            logCooldown = 0.25f;
        }
    }

    static void LogBindings(string label, InputAction a)
    {
        if (a == null) { Debug.LogWarning($"[PROBE] {label} action is NULL (not assigned)."); return; }

        Debug.Log($"[PROBE] {label} action '{a.name}' in map '{a.actionMap?.name}' " +
                  $"type:{a.type} expected:{a.expectedControlType} enabled:{a.enabled}");

        if (a.controls.Count == 0)
        {
            Debug.LogWarning($"[PROBE] {label} has NO matched controls. Check binding path or control scheme.");
        }
        else
        {
            foreach (var c in a.controls)
                Debug.Log($"[PROBE] {label} bound to: {c.displayName} | {c.device.layout} | {c.path}");
        }

        // PlayerInput info (non-fatal if none exists)
#if UNITY_2023_1_OR_NEWER
        var pi = Object.FindFirstObjectByType<PlayerInput>();
#else
        var pi = Object.FindObjectOfType<PlayerInput>();
#endif
        if (pi != null)
            Debug.Log($"[PROBE] PlayerInput scheme: '{pi.currentControlScheme}'. Devices: {string.Join(", ", pi.devices)}");
    }
}
