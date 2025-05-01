using UnityEngine;

public class CopyHandRotation : MonoBehaviour
{
    public Transform inputHand;  // The hand whose joint rotations you want to copy (e.g., left hand)
    public Transform targetHand; // The hand you want to rotate (e.g., right hand)

    void Update()
    {
        // Assuming the structure of the hand is identical (same number of children and hierarchy)
        CopyJointRotations(inputHand, targetHand);
    }

    void CopyJointRotations(Transform inputJoint, Transform targetJoint)
    {
        // Copy the local rotation from the input joint to the target joint
        targetJoint.localRotation = inputJoint.localRotation;

        // Recursively copy rotations for each child joint
        for (int i = 0; i < inputJoint.childCount; i++)
        {
            CopyJointRotations(inputJoint.GetChild(i), targetJoint.GetChild(i));
        }
    }
}
