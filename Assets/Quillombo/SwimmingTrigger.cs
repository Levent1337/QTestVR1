using UnityEngine;


public class VRSwimming : MonoBehaviour
{
    [Header("References")]
    public Rigidbody targetRb;       
    public WaterProbe waterProbe;

    [Header("Buoyancy")]
    [Tooltip("Upward buoyant force in Newtons. ~mass * 9.81 for neutral float.")]
    public float buoyantForce = 100f;

    void FixedUpdate()
    {
        if (targetRb == null || waterProbe == null) return;

        if (waterProbe.isInWater)
        {
            targetRb.AddForce(Vector3.up * buoyantForce, ForceMode.Force);
        }
    }
}
