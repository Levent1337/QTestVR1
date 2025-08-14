using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimTriggerRelay : MonoBehaviour
{
    [Tooltip("VRSwimmingRoot on the ROOT object.")]
    public VRSwimmingRoot receiver;

    // This object (or its children) must have your NON-trigger body collider.
    // Water volumes must be TRIGGERS (with tag 'Water' unless you cleared the tag check).

    void OnTriggerEnter(Collider other)
    {
        if (receiver) receiver.RelayTriggerEnter(other, this);
    }

    void OnTriggerExit(Collider other)
    {
        if (receiver) receiver.RelayTriggerExit(other, this);
    }
}
