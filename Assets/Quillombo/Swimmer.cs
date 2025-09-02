using UnityEngine;
using UnityEngine.InputSystem;

public class Swimmer : MonoBehaviour
{
    public float swimForce = 7;
    public float dragForce = 5;
    public float minForce;
    public float minTimeBetweenStrokes;

    public InputActionReference leftControllerSwimReference;
    public InputActionReference leftControllerSwimVelocity;
    public InputActionReference rightControllerSwimReference;
    public InputActionReference rightControllerSwimVelocity;

    public Rigidbody _rigidbody;
    float _cooldownTimer;
    public Collider Water;
    public bool isInWater;

    public Transform trackingReference; // assign your Camera (HMD) or XR Origin head

    private void FixedUpdate()
    {
        _cooldownTimer += Time.fixedDeltaTime;

        if (_cooldownTimer > minTimeBetweenStrokes
            && leftControllerSwimReference.action.IsPressed()
            && rightControllerSwimReference.action.IsPressed())
        {
            Debug.Log("Swim Pressed");

            var leftHanVelocity = leftControllerSwimVelocity.action.ReadValue<Vector3>();
            var rightHanVelocity = rightControllerSwimVelocity.action.ReadValue<Vector3>();

            // Sum the hand velocities in world space (flip sign if it feels backward)
            Vector3 worldStroke = -(leftHanVelocity + rightHanVelocity); // try removing '-' if needed

            // Make the stroke relative to the player's view and keep it horizontal
            Vector3 headLocal = trackingReference != null
                ? trackingReference.InverseTransformDirection(worldStroke)
                : worldStroke;
            headLocal.y = 0f;

            // Convert back to world and apply
            Vector3 worldVelocity = trackingReference != null
                ? trackingReference.TransformDirection(headLocal)
                : new Vector3(headLocal.x, 0f, headLocal.z);

            if (worldVelocity.sqrMagnitude > minForce * minForce)
            {
                Debug.Log("force applied");
                _rigidbody.AddForce(worldVelocity * swimForce, ForceMode.VelocityChange);
                _cooldownTimer = 0f;
            }
        }

        // (Optional) simple damping so you don't coast forever.
        // If you prefer your original behavior, you can keep it as you had.
        if (_rigidbody.angularVelocity.sqrMagnitude > 0.01f)
        {
            _rigidbody.AddForce(-_rigidbody.linearVelocity * dragForce, ForceMode.Acceleration);
        }
    }
}
