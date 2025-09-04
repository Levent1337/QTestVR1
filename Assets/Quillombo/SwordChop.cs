using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwordChop : MonoBehaviour
{
    [Header("Filtering")]
    public LayerMask plantLayer;              // Assign your Plant layer

    [Header("Cut Settings")]
    public float maxBreakDistance = 0.4f;     // Ignore joints farther than this from the contact
    public float plantCutCooldownSeconds = 0.5f; // <-- per-plant cooldown

    [Header("Debug")]
    public bool drawDebug = false;

    // Plants currently cooling down
    private readonly HashSet<Transform> _cooldownPlants = new HashSet<Transform>();

    void OnCollisionEnter(Collision collision) => TryBreakAtCollision(collision);
    void OnCollisionStay(Collision collision) => TryBreakAtCollision(collision);

    void TryBreakAtCollision(Collision collision)
    {
        // Layer filter
        if ((plantLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        // Find plant root
        var plantRoot = GetPlantRoot(collision.gameObject);
        if (plantRoot == null) return;

        // Respect cooldown for this plant
        if (_cooldownPlants.Contains(plantRoot)) return;

        // Find the single best joint across all contacts
        Vector3 bestJointPos = Vector3.zero;
        ConfigurableJoint bestJoint = null;
        float bestSqr = (maxBreakDistance > 0f) ? maxBreakDistance * maxBreakDistance : float.PositiveInfinity;

        var joints = plantRoot.GetComponentsInChildren<ConfigurableJoint>(false);
        if (joints.Length == 0) return;

        foreach (var contact in collision.contacts)
        {
            Vector3 cp = contact.point;

            foreach (var j in joints)
            {
                Vector3 a = j.transform.TransformPoint(j.anchor);
                Vector3 b = (j.connectedBody != null)
                    ? j.connectedBody.transform.TransformPoint(j.connectedAnchor)
                    : a;
                Vector3 mid = (a + b) * 0.5f;

                float sqr = (mid - cp).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    bestJoint = j;
                    bestJointPos = mid;
                }
            }
        }

        if (bestJoint == null) return;

        if (drawDebug)
        {
            Debug.DrawLine(collision.GetContact(0).point, bestJointPos, Color.red, 1f);
            DebugDrawCross(bestJointPos, 0.05f, 1f);
        }

        // Break exactly one joint
        Destroy(bestJoint);

        // Start cooldown for this plant
        StartCoroutine(PlantCooldown(plantRoot));
    }

    IEnumerator PlantCooldown(Transform plantRoot)
    {
        if (plantRoot == null) yield break;
        _cooldownPlants.Add(plantRoot);
        yield return new WaitForSeconds(plantCutCooldownSeconds);
        _cooldownPlants.Remove(plantRoot);
    }

    Transform GetPlantRoot(GameObject plantPart)
    {
        var marker = plantPart.GetComponentInParent<PlantMarker>();
        return marker ? marker.transform : null;
    }

    void DebugDrawCross(Vector3 p, float r, float time)
    {
        Debug.DrawLine(p + Vector3.right * r, p - Vector3.right * r, Color.yellow, time);
        Debug.DrawLine(p + Vector3.up * r, p - Vector3.up * r, Color.yellow, time);
        Debug.DrawLine(p + Vector3.forward * r, p - Vector3.forward * r, Color.yellow, time);
    }
}
