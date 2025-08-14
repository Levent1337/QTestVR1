using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsRig : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;
    public CapsuleCollider bodyCollider;
    public Transform LeftController;
    public Transform RightController;
    public ConfigurableJoint HeadJoint;
    public ConfigurableJoint LeftHandJoint;
    public ConfigurableJoint RightHandJoint;

    [Header("Dimentions")]
    public float bodyHeightMin = 0.5f;
    public float bodyHeightMax = 2f;



    void FixedUpdate()
    {
        bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height/2, playerHead.localPosition.z);

        LeftHandJoint.targetPosition  = LeftController.localPosition;
        LeftHandJoint.targetRotation  = LeftController.localRotation;

        RightHandJoint.targetPosition = RightController.localPosition;
        RightHandJoint.targetRotation = RightController.localRotation;

        HeadJoint.targetPosition = playerHead.localPosition;
    }


}
