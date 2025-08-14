using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class VRSwimming : MonoBehaviour
{
    [Header("XR Controller References")]
    public Transform leftController;
    public Transform rightController;

    [Header("Grip Input Actions")]
    public InputActionProperty leftGripAction;   // Assign LeftGrip from Input Actions
    public InputActionProperty rightGripAction;  // Assign RightGrip from Input Actions

    [Header("Swimming Settings")]
    public float swimForce = 4f;
    public float drag = 2f;
    public bool requireGrip = true;

    private Rigidbody rb;
    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1;

        if (leftController != null)
            lastLeftPos = leftController.position;
        if (rightController != null)
            lastRightPos = rightController.position;

        // Enable input actions so we can read them
        if (leftGripAction.action != null) leftGripAction.action.Enable();
        if (rightGripAction.action != null) rightGripAction.action.Enable();
    }

    void FixedUpdate()
    {
        if (leftController == null || rightController == null) return;

        Vector3 leftDelta = leftController.position - lastLeftPos;
        Vector3 rightDelta = rightController.position - lastRightPos;

        // Check grip input
        bool swimLeft = !requireGrip || (leftGripAction.action != null && leftGripAction.action.IsPressed());
        bool swimRight = !requireGrip || (rightGripAction.action != null && rightGripAction.action.IsPressed());

        if (swimLeft)
            rb.AddForce(-leftDelta * swimForce, ForceMode.Acceleration);

        if (swimRight)
            rb.AddForce(-rightDelta * swimForce, ForceMode.Acceleration);

        lastLeftPos = leftController.position;
        lastRightPos = rightController.position;
    }
}
