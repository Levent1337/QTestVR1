using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class WaterTrigger : MonoBehaviour
{
    [Tooltip("Only objects on this layer can trigger swimming.")]
    public string playerLayerName = "Player";

    private int playerLayer;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            var swimming = other.GetComponent<VRSwimming>();
            var cc = other.GetComponent<CharacterController>();
            var rb = other.GetComponent<Rigidbody>();
            var locomotion = other.GetComponentInChildren<XRBodyTransformer>();

            if (swimming && cc && rb)
            {
                cc.enabled = false;
                rb.isKinematic = false;
                swimming.enabled = true;

                if (locomotion) locomotion.enabled = false; // disable walking updates
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            var swimming = other.GetComponent<VRSwimming>();
            var cc = other.GetComponent<CharacterController>();
            var rb = other.GetComponent<Rigidbody>();
            var locomotion = other.GetComponentInChildren<XRBodyTransformer>();

            if (swimming && cc && rb)
            {
                swimming.enabled = false;
                rb.isKinematic = true;
                cc.enabled = true;

                if (locomotion) locomotion.enabled = true; // re-enable walking
            }
        }
    }
}
