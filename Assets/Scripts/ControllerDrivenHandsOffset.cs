using UnityEngine;
using System.Collections;

public class ControllerDrivenHandsOffset : MonoBehaviour
{
    // First object and its target position/rotation
    public GameObject object1;
    public Vector3 position1;
    public Vector3 rotation1;

    // Second object and its target position/rotation
    public GameObject object2;
    public Vector3 position2;
    public Vector3 rotation2;

    void Start()
    {
        // Start coroutine to apply offsets after 2 frames
        StartCoroutine(ApplyOffsetsAfterFrames(2));
    }

    IEnumerator ApplyOffsetsAfterFrames(int frameDelay)
    {
        // Wait for the specified number of frames
        for (int i = 0; i < frameDelay; i++)
        {
            yield return null; // Waits one frame
        }

        // Apply position and rotation to object 1
        if (object1 != null)
        {
            object1.transform.position = position1;
            object1.transform.rotation = Quaternion.Euler(rotation1);
        }

        // Apply position and rotation to object 2
        if (object2 != null)
        {
            object2.transform.position = position2;
            object2.transform.rotation = Quaternion.Euler(rotation2);
        }
    }
}
