using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class WaterProbe : MonoBehaviour
{
    [Tooltip("If set, only these layers count as water. Leave empty to use 'Water' tag.")]
    public LayerMask waterLayers;

    public bool isInWater { get; private set; }
    private readonly HashSet<Collider> overlaps = new HashSet<Collider>();

    bool IsWater(Collider other)
    {
        if (waterLayers.value != 0)
            return (waterLayers.value & (1 << other.gameObject.layer)) != 0;
        return other.CompareTag("Water");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsWater(other)) return;
        overlaps.Add(other);
        isInWater = true;
        Debug.Log($"{name} entered water volume {other.name}");
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsWater(other)) return;
        overlaps.Add(other); // covers starting inside
        isInWater = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsWater(other)) return;
        overlaps.Remove(other);
        isInWater = overlaps.Count > 0;
    }

    void OnDisable()
    {
        overlaps.Clear();
        isInWater = false;
    }
}
