using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class SnapParts : MonoBehaviour
{
    public Transform snapPoint;
    [HideInInspector] public bool snapped = false;

    AudioSource sound;

    private void Start()
    {
        // sound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (snapped == false && other.transform.CompareTag("maleSnapZone"))
        {
            GameObject male = other.transform.parent.gameObject;

            male.transform.parent = snapPoint;
            male.transform.position = snapPoint.position;
            male.transform.rotation = snapPoint.rotation;

            var locker = male.GetComponent("RigidbodyKinematicLocker");
            if (locker != null)
            {
                Destroy(locker);  
            }

            Rigidbody rb = male.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; 
                rb.useGravity = false;  
            }

            XRGrabInteractable grabInteractable = male.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = false;
            }

            male.tag = "Untagged";

            //sound.Play();

            snapped = true;

            Transform handGrabInteraction = male.transform.Find("ISDK_HandGrabInteraction");
            if (handGrabInteraction != null)
            {
                handGrabInteraction.gameObject.SetActive(false);
            }
        }
    }
}
