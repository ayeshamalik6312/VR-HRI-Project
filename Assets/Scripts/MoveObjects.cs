using UnityEngine;

public class MoveObjects : MonoBehaviour
{
    // Define the two objects
    public GameObject leftHandObject;
    public GameObject rightHandObject;

    // Define the transform positions (x, y, z) for both objects
    public Vector3 leftHandPosition;
    public Vector3 rightHandPosition;

    void Start()
    {
        // Move the left hand object to the specified position
        if (leftHandObject != null)
        {
            leftHandObject.transform.position = leftHandPosition;
        }

        // Move the right hand object to the specified position
        if (rightHandObject != null)
        {
            rightHandObject.transform.position = rightHandPosition;
        }
    }
}
