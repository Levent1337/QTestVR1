using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerTest : MonoBehaviour
{
    [SerializeField] private InputActionReference leftControllerSwimReference;
    [SerializeField] private InputActionReference rightControllerSwimReference;

    private void OnEnable()
    {
        if (leftControllerSwimReference != null) leftControllerSwimReference.action.Enable();
        if (rightControllerSwimReference != null) rightControllerSwimReference.action.Enable();
    }

    private void OnDisable()
    {
        if (leftControllerSwimReference != null) leftControllerSwimReference.action.Disable();
        if (rightControllerSwimReference != null) rightControllerSwimReference.action.Disable();
    }

    private void Update()
    {
        if (leftControllerSwimReference == null || rightControllerSwimReference == null)
            return;

        // Requires the actions to be Button-type or have a Press interaction
        if (leftControllerSwimReference.action.IsPressed() ||
            rightControllerSwimReference.action.IsPressed())
        {
            Debug.Log("trigger pressed");
        }
    }
}
