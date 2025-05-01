using UnityEngine;

public class GripAnimationController : MonoBehaviour
{
    public Animator leftArmAnimator;
    public Animator rightArmAnimator;

    public OVRInput.Controller leftController = OVRInput.Controller.LTouch;
    public OVRInput.Controller rightController = OVRInput.Controller.RTouch;

    public Transform leftHandAnchor; 
    public Transform rightHandAnchor; 

    private string grabLayerName = "grab";

    private int leftGrabLayerIndex;
    private int rightGrabLayerIndex;

    public Transform objectToGrab;

    public Transform grabPoint;

    void Start()
    {
        leftGrabLayerIndex = leftArmAnimator.GetLayerIndex(grabLayerName);
        rightGrabLayerIndex = rightArmAnimator.GetLayerIndex(grabLayerName);
    }

    void Update()
    {
        float leftGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, leftController);
        float rightGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, rightController);

        leftArmAnimator.SetLayerWeight(leftGrabLayerIndex, leftGrip);
        rightArmAnimator.SetLayerWeight(rightGrabLayerIndex, rightGrip);

        if (leftGrip > 0.5f)
        {
            PickUpObject(leftHandAnchor);
        }

        if (rightGrip > 0.5f)
        {
            PickUpObject(rightHandAnchor);
        }
    }

    void PickUpObject(Transform handAnchor)
    {
        if (grabPoint != null)
        {
            objectToGrab.SetParent(handAnchor);

            objectToGrab.position = handAnchor.position;
            objectToGrab.rotation = handAnchor.rotation;
        }
    }
}
