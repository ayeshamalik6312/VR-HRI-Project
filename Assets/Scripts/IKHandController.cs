using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;

public class IKHandController : MonoBehaviour
{
    public BipedIK bipedIK; // Reference to the BipedIK component
    public Transform leftHandTarget; // Target position for the left hand
    public float blendStartTime = 2f; // Time to start blending to IK
    public float blendEndTime = 4f; // Time to stop blending and revert to animation
    public float blendDuration = 1f; // Duration of the blend to IK
    private bool isBlending = false;
    private float elapsedTime = 0f;

    void Start()
    {
        // Ensure Biped IK is set, and start with no IK influence
        if (bipedIK == null)
        {
            Debug.LogError("BipedIK not assigned!");
            return;
        }
        // Start with no IK influence
        bipedIK.solvers.leftHand.IKPositionWeight = 0f;
    }

    void Update()
    {
        // Start blending at 2 seconds
        if (!isBlending && Time.time >= blendStartTime && Time.time < blendEndTime)
        {
            StartCoroutine(BlendToIK());
        }
        // Unblend back to animation control at 4 seconds
        if (isBlending && Time.time >= blendEndTime)
        {
            StartCoroutine(UnblendFromIK());
        }
    }

    IEnumerator BlendToIK()
    {
        isBlending = true;
        elapsedTime = 0f;

        Vector3 initialHandPosition = bipedIK.solvers.leftHand.IKPosition; // Get the current hand position

        while (elapsedTime < blendDuration)
        {
            elapsedTime += Time.deltaTime;
            float blendFactor = elapsedTime / blendDuration;

            // Interpolate the left hand position towards the target
            bipedIK.solvers.leftHand.IKPosition = Vector3.Lerp(initialHandPosition, leftHandTarget.position, blendFactor);
            bipedIK.solvers.leftHand.IKPositionWeight = Mathf.Lerp(0f, 1f, blendFactor);

            yield return null;
        }

        // Ensure full IK influence at the end of the blend
        bipedIK.solvers.leftHand.IKPositionWeight = 1f;
        bipedIK.solvers.leftHand.IKPosition = leftHandTarget.position;
    }

    IEnumerator UnblendFromIK()
    {
        elapsedTime = 0f;

        Vector3 ikHandPosition = bipedIK.solvers.leftHand.IKPosition; // Get the current IK hand position

        while (elapsedTime < blendDuration)
        {
            elapsedTime += Time.deltaTime;
            float blendFactor = elapsedTime / blendDuration;

            // Interpolate the left hand position back to the animation (no IK influence)
            bipedIK.solvers.leftHand.IKPositionWeight = Mathf.Lerp(1f, 0f, blendFactor);

            yield return null;
        }

        // Ensure no IK influence at the end
        bipedIK.solvers.leftHand.IKPositionWeight = 0f;

        isBlending = false;
    }
}
