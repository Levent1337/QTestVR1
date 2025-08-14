using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FixedPaddle : MonoBehaviour
{
    [Header("References")]
    public XRGrabInteractable grabInteractable;
    public Transform bladeTip;       // assign the tip of the paddle
    public Rigidbody boatRb;         // assign the boat's Rigidbody
    public float waterLevel = 0f;    // y-height of water plane

    [Header("Forces")]
    public float strokeForce = 200f;
    public float dragForce = 50f;

    private bool isGrabbed = false;
    private Vector3 lastBladePos;

    void Start()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void FixedUpdate()
    {
        if (!isGrabbed || boatRb == null) return;

        Vector3 bladeVelocity = (bladeTip.position - lastBladePos) / Time.fixedDeltaTime;
        lastBladePos = bladeTip.position;

        if (bladeTip.position.y < waterLevel)
        {
            // Apply propulsion opposite to paddle motion
            boatRb.AddForceAtPosition(-bladeVelocity * strokeForce, bladeTip.position);

            // Add drag when blade is in water but barely moving
            if (bladeVelocity.magnitude < 0.05f)
            {
                Vector3 localBoatVel = boatRb.GetPointVelocity(bladeTip.position);
                boatRb.AddForceAtPosition(-localBoatVel * dragForce, bladeTip.position);
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        lastBladePos = bladeTip.position;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }
}
