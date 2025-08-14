using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{

    public InputAction grabInputSource;
        public float radius = 0.1f;
    public LayerMask grabLayer;
    private FixedJoint fixedJoint;
    private bool isGrabbing;


    private void FixedUpdate()
    {
        bool isGrabButtonPressed = grabInputSource.ReadValue<float>() > 0.1f;

        if(isGrabButtonPressed && !isGrabbing)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position,radius,grabLayer,QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidbody = nearbyColliders[0].attachedRigidbody;

                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;
                if (nearbyRigidbody)
                {
                    fixedJoint.connectedBody = nearbyRigidbody;
                    fixedJoint.connectedAnchor = nearbyRigidbody.transform.InverseTransformPoint(transform.position); 
                    
                }
                else
                {
                    fixedJoint.connectedAnchor = transform.position;
                }

                isGrabbing = true; 

            }
        }
        else if(!isGrabbing && isGrabbing)
        {
            isGrabbing = false;

            if(fixedJoint)
            {
                Destroy(fixedJoint);
            }
        }
    }
}
