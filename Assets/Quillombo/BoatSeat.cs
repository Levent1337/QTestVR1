using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class BoatSnapInteractable : XRBaseInteractable
{
    [Header("Seat Settings")]
    public XROrigin xrOrigin;
    public Transform seatPosition;

    private bool isSeated = false;
    private CharacterController characterController;
    private ContinuousMoveProvider moveProvider;
    private ContinuousTurnProvider turnProvider;

    private Vector3 headsetOffset;   // local offset of camera inside XR Origin

    protected override void Awake()
    {
        base.Awake();
        if (xrOrigin != null)
        {
            characterController = xrOrigin.GetComponent<CharacterController>();
            moveProvider = xrOrigin.GetComponentInChildren<ContinuousMoveProvider>();
            turnProvider = xrOrigin.GetComponentInChildren<ContinuousTurnProvider>();
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (!isSeated)
            SitInBoat();
        else
            LeaveBoat();
    }

    private void SitInBoat()
    {
        if (xrOrigin == null || seatPosition == null) return;

        // store the headset offset so we can preserve head position
        headsetOffset = xrOrigin.CameraInOriginSpacePos;

        // teleport XR Origin to seat
        xrOrigin.MoveCameraToWorldLocation(seatPosition.position + seatPosition.rotation * headsetOffset);
        xrOrigin.transform.rotation = Quaternion.Euler(0, seatPosition.rotation.eulerAngles.y, 0);

        // disable locomotion
        if (characterController != null) characterController.enabled = false;
        if (moveProvider != null) moveProvider.enabled = false;
        if (turnProvider != null) turnProvider.enabled = false;

        isSeated = true;
    }

    private void LeaveBoat()
    {
        // re-enable locomotion
        if (characterController != null) characterController.enabled = true;
        if (moveProvider != null) moveProvider.enabled = true;
        if (turnProvider != null) turnProvider.enabled = true;

        isSeated = false;
    }

    void LateUpdate()
    {
        if (isSeated && xrOrigin != null && seatPosition != null)
        {
            // update XR Origin position each frame without full parenting
            xrOrigin.MoveCameraToWorldLocation(seatPosition.position + seatPosition.rotation * headsetOffset);

            // only match boat's yaw, ignore roll/pitch for comfort
            Vector3 euler = seatPosition.rotation.eulerAngles;
            xrOrigin.transform.rotation = Quaternion.Euler(0, euler.y, 0);
        }
    }
}
