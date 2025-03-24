using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{
    /* https://www.youtube.com/watch?v=D9qQb3EvU8c&t=0s&ab_channel=ValemTutorials */

    //public InputActionProperty grabInputSource; 
    public OVRInput.Axis1D button;
    public float radius = 0.1f;
    public LayerMask grabLayer;
    public GameObject referenceGO;

    FixedJoint fixedJoint;
    bool isGrabbing = false;

    private void Start()
    {
        Rigidbody rb;
        if ((rb = referenceGO.GetComponent<Rigidbody>()) == null)
        {
            rb = referenceGO.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        //bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;
        float input = OVRInput.Get(button, OVRInput.Controller.Touch);
        bool isGrabButtonPressed = false;
        if (input > 0.8)
            isGrabButtonPressed = true;
            
        Debug.Log(isGrabButtonPressed);

        if (isGrabButtonPressed && !isGrabbing)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(referenceGO.transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                isGrabbing = true;

                Rigidbody nearbyRb = nearbyColliders[0].attachedRigidbody;

                fixedJoint = referenceGO.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;

                if (nearbyRb)
                {
                    fixedJoint.connectedBody = nearbyRb;
                    fixedJoint.connectedAnchor = nearbyRb.transform.InverseTransformPoint(referenceGO.transform.position); //turn transform.position into local
                }
                else
                {
                    fixedJoint.connectedAnchor = referenceGO.transform.position;
                }
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            isGrabbing = false;

            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
            }
        }
    }
}
