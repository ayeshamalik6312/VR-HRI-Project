using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapParts : MonoBehaviour
{
    public Transform snapPoint;
    [HideInInspector] public bool snapped = false;
    public AudioClip popSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (snapped == false && other.transform.CompareTag("maleSnapZone"))
        {
            GameObject male = other.transform.parent.gameObject;

            male.transform.parent = snapPoint;

            // Compute two possible snap rotations
            Quaternion snapRot1 = snapPoint.rotation;
            Quaternion snapRot2 = snapPoint.rotation * Quaternion.Euler(180, 0, 0); // Rotated 180 degrees around X-axis

            // Measure distance from male's current rotation to each option
            float angleToRot1 = Quaternion.Angle(male.transform.rotation, snapRot1);
            float angleToRot2 = Quaternion.Angle(male.transform.rotation, snapRot2);

            // Choose the closer rotation
            Quaternion chosenRotation = (angleToRot1 <= angleToRot2) ? snapRot1 : snapRot2;
            bool isFlipped = (chosenRotation == snapRot2);

            // Apply chosen rotation
            male.transform.rotation = chosenRotation;

            // Apply position, with offset if needed
            Vector3 newPosition = snapPoint.position;
            if (isFlipped)
            {
                // Force a true 'right' movement relative to world space
                float offsetDistance = 0.037f; // or however much you need
                newPosition += Vector3.left * offsetDistance;
            }

            male.transform.position = newPosition;

            // Clean up components
            var locker = male.GetComponent("RigidbodyKinematicLocker");
            if (locker != null)
            {
                Destroy(locker);
            }

            Rigidbody rb = male.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb); // We'll add it back later in MoveIKTarget
                Debug.Log("[SnapParts] Removed Rigidbody from key.");
            }


            XRGrabInteractable grabInteractable = male.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = false;
            }

            male.tag = "Untagged";

            snapped = true;
            if (popSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(popSound);
            }

            MoveIKTarget ikTarget = FindObjectOfType<MoveIKTarget>();
            float currentCycleTime = ikTarget != null ? ikTarget.GetCurrentCycleTime() : -1f;
            string buttonPressed = ikTarget != null ? ikTarget.ConsumeLastButtonPressed() : "None";

            ParticipantManager manager = FindObjectOfType<ParticipantManager>();
            if (manager != null)
            {
                manager.ReportSnap(isFlipped, currentCycleTime, buttonPressed);
            }


            if (ikTarget != null)
            {
                ikTarget.ResetCycleTimer();
            }

            Transform handGrabInteraction = male.transform.Find("ISDK_HandGrabInteraction");
            if (handGrabInteraction != null)
            {
                handGrabInteraction.gameObject.SetActive(false);
            }
        }
    }

}
