using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class IKManager : MonoBehaviour
{
    // Root of armature
    public Joint root;

    // End effector
    public Joint end;

    public GameObject target;

    public float threshold = 0.05f;
    public float rate = 5;
    public int stepsPerFrame = 24;

    float CalculateSlope(Joint joint)
    {
        float deltaTheta = 0.01f;
        float distance1 = GetDistance(end.transform.position, target.transform.position);

        joint.RotateJoint(deltaTheta);

        float distance2 = GetDistance(end.transform.position, target.transform.position);

        joint.RotateJoint(-deltaTheta);
        
        return (distance2 - distance1) / deltaTheta;
    }

    private void Update()
    {
        for (int i = 0; i < stepsPerFrame; i++)
        {
            if (GetDistance(end.transform.position, target.transform.position) > threshold)
            {
                Joint current = root;
                while (current != null)
                {
                    float slope = CalculateSlope(current);
                    current.RotateJoint(-slope * rate);
                    current = current.GetChild();
                }
            }
        }
        
    }

    float GetDistance(Vector3 p1, Vector3 p2)
    {
        return Vector3.Distance(p1, p2);
    }
}
