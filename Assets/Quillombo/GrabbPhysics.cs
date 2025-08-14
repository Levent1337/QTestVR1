using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{
    public InputActionReference grabInputSource;
    public float radius = 0.1f;
    public LayerMask grabLayer;

    private FixedJoint fixedJoint;
    private bool isGrabbing;
    private InputAction grabAction;

    void OnEnable()
    {
        grabAction = grabInputSource != null ? grabInputSource.action : null;
        
    }

    void OnDisable()
    {
        grabAction?.Disable();
        if (fixedJoint) Destroy(fixedJoint);
        isGrabbing = false;
    }

    void FixedUpdate()
    {
        if (grabAction == null) return;

        // If your action is a Button, IsPressed() is perfect:
        bool isGrabButtonPressed = grabAction.IsPressed();
        // or: bool isGrabButtonPressed = grabAction.ReadValue<float>() > 0.1f;

        if (isGrabButtonPressed && !isGrabbing)
        {
            var cols = Physics.OverlapSphere(transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);
            if (cols.Length > 0)
            {
                var rb = cols[0].attachedRigidbody;

                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;

                if (rb)
                {
                    fixedJoint.connectedBody = rb;
                    fixedJoint.connectedAnchor = rb.transform.InverseTransformPoint(transform.position);
                }
                else
                {
                    fixedJoint.connectedBody = null;              // stick to world
                    fixedJoint.connectedAnchor = transform.position;
                }

                isGrabbing = true;
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            isGrabbing = false;
            if (fixedJoint) Destroy(fixedJoint);
        }
    }
}
