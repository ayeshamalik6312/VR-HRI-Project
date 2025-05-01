using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRig : MonoBehaviour
{
    public Transform playerHead;
    public Transform leftController;
    public Transform rightController;

    public ConfigurableJoint headJoint;
    public ConfigurableJoint rightHandJoint;
    public ConfigurableJoint leftHandJoint;

    public CapsuleCollider bodyCollider;

    public float bodyHeightMin = 0.5f;
    public float bodyHeightMax = 2;

    private void FixedUpdate()
    {
        if (bodyCollider != null && playerHead != null)
        {
            bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
            bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);
        } 
        else { Debug.Log("bodyCollider or playerHead are missing."); }

        if (leftHandJoint != null && leftController != null)
        {
            leftHandJoint.targetPosition = leftController.localPosition;
            leftHandJoint.targetRotation = leftController.localRotation;
        } 
        else { Debug.Log("leftHandJoint or leftController are missing."); }

        if (rightHandJoint != null && rightController != null)
        {
            rightHandJoint.targetPosition = rightController.localPosition;
            rightHandJoint.targetRotation = rightController.localRotation;
        }
        else { Debug.Log("rightHandJoint or rightController are missing."); }

        if (headJoint != null && playerHead != null)
        {
            headJoint.targetPosition = playerHead.localPosition;
        }
        else { Debug.Log("headJoint or playerHead are missing.");  }

    }
}
