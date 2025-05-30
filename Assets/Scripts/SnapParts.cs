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
    public bool isFlipped = false;
    private bool isTouchingSocket = false;
    private Coroutine hapticCoroutine = null;
    private XRBaseInteractor interactorHolding = null;

    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        interactorHolding = args.interactorObject as XRBaseInteractor;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        interactorHolding = null;
    }

    
    private IEnumerator SlideInPart(Transform male, Vector3 targetPosition, Quaternion targetRotation)
    {
        Rigidbody rb = male.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 startPos = male.position;
        Quaternion startRot = male.rotation;

        float duration = 0.3f;
        float elapsed = 0f;

        if (popSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(popSound);
        }
        // Optional vibration
        OVRInput.SetControllerVibration(0f, 0.0f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0f, 0.0f, OVRInput.Controller.LTouch);
        Invoke(nameof(StopHaptics), 0.1f);

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            male.position = Vector3.Lerp(startPos, targetPosition, t);
            male.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        male.position = targetPosition;
        male.rotation = targetRotation;

        var locker = male.GetComponent("RigidbodyKinematicLocker");
        if (locker != null) Destroy(locker);

        if (rb != null) Destroy(rb);

        XRGrabInteractable grabInteractable = male.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null) grabInteractable.enabled = false;
        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
            hapticCoroutine = null;
            StopHapticsNow();
        }

        male.tag = "Untagged";
        snapped = true;

        MoveIKTarget ikTarget = FindObjectOfType<MoveIKTarget>();
        if (ikTarget != null) ikTarget.ResetCycleTimer();

        Transform handGrabInteraction = male.transform.Find("ISDK_HandGrabInteraction");
        if (handGrabInteraction != null) handGrabInteraction.gameObject.SetActive(false);
    }

    void StopHaptics()
    {
        void StopHaptics()
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }


    }
    public void BeginSnap(Vector3 targetPos, Quaternion targetRot)
    {
        StartCoroutine(SlideInPart(this.transform, targetPos, targetRot));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!snapped)
        {
            // snapping logic
            if (other.transform.CompareTag("maleSnapZone"))
            {
                GameObject male = other.transform.parent.gameObject;
                male.transform.parent = snapPoint;

                Quaternion snapRot1 = snapPoint.rotation;
                Quaternion snapRot2 = snapPoint.rotation * Quaternion.Euler(180, 0, 0);

                float angleToRot1 = Quaternion.Angle(male.transform.rotation, snapRot1);
                float angleToRot2 = Quaternion.Angle(male.transform.rotation, snapRot2);

                Quaternion chosenRotation = (angleToRot1 <= angleToRot2) ? snapRot1 : snapRot2;
                isFlipped = (chosenRotation == snapRot2);

                Vector3 finalPosition = snapPoint.position;
                if (isFlipped)
                {
                    finalPosition += Vector3.left * 0.037f;
                }

                StartCoroutine(SlideInPart(male.transform, finalPosition, chosenRotation));
            }

            // haptic-on-touch logic
            if (other.gameObject.CompareTag("female") && !isTouchingSocket)
            {
                isTouchingSocket = true;
                if (hapticCoroutine == null) hapticCoroutine = StartCoroutine(SendHaptics());
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("female"))
        {
            isTouchingSocket = false;
            if (hapticCoroutine != null)
            {
                StopCoroutine(hapticCoroutine);
                hapticCoroutine = null;
                StopHapticsNow();
            }
        }
    }


    private IEnumerator SendHaptics()
    {
        while (true)
        {
            if (interactorHolding != null)
            {
                var controller = interactorHolding.GetComponent<ActionBasedController>();
                if (controller != null)
                {
                    controller.SendHapticImpulse(0.2f, 0.1f); // mild haptics
                }
            }
            yield return new WaitForSeconds(0.1f); // Send periodically
        }
    }

    private void StopHapticsNow()
    {
        if (interactorHolding != null)
        {
            var controller = interactorHolding.GetComponent<ActionBasedController>();
            if (controller != null)
            {
                controller.SendHapticImpulse(0, 0); // cancel
            }
        }
    }

}
