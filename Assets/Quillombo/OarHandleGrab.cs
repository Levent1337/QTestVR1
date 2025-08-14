using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class OarHandleGrab : MonoBehaviour
{
    [Header("Assign the oar shaft Rigidbody")]
    public Rigidbody oarRigidbody;

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Disable kinematic so physics can move it
        if (oarRigidbody != null)
            oarRigidbody.isKinematic = false;

        // Move the oar’s Rigidbody to the hand
        transform.SetParent(oarRigidbody.transform, true);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (oarRigidbody != null)
            oarRigidbody.isKinematic = true;

        // Detach
        transform.SetParent(null, true);
    }
}
