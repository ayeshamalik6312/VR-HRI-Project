using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    public enum JointType
    {
        Vertical,
        Horizontal
    }

    public Joint child;
    public JointType jointType;
    
    public Joint GetChild()
    {
        return child;
    }

    public void RotateJoint(float angle)
    {
        switch (jointType)
        {
            case JointType.Vertical:
                RotateJointY(angle);
                break;
            case JointType.Horizontal:
                RotateJointX(angle);
                break;
            default:
                Debug.Log("No joint type associated");
                break;
        }
    }

    void RotateJointY(float angle)
    {
        transform.Rotate(Vector3.up * angle);
    }

    void RotateJointX(float angle)
    {
        transform.Rotate(Vector3.right * angle);
    }
}
