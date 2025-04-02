using UnityEngine;
using System.Collections;

public class ForemanTutorialController : MonoBehaviour
{
    public Animator foremanAnimator;
    public Animator doorAnimator;

    public AudioSource tutorialAudio;
    public AudioSource tutorial2Audio;
    public AudioSource doorAudio;

    public Transform foremanLeftHand;
    public Transform foremanRightHand;
    public GameObject goodStructure;
    public GameObject badStructure;
    public GameObject overlayObject; // Holographic overlay


    public GameObject holographicOverlay; // AR Layer GameObject

    public Vector3 goodStructurePositionOffset;
    public Vector3 goodStructureRotationOffset;
    public Vector3 badStructurePositionOffset;
    public Vector3 badStructureRotationOffset;
    public Vector3 badStructureSwitchPositionOffset;
    public Vector3 badStructureSwitchRotationOffset;
    public Vector3 badStructureSwitchOffsetPosition;
    public Vector3 badStructureSwitchOffsetRotation;

    public Transform goodPieceTargetTransform;
    public Transform badPieceTargetTransform;

    private bool tutorialRunning = false;

    public enum ConditionType { Baseline, AugPrompt, Continuous }

    public void StartTutorialAndCondition(ConditionType condition)
    {
        if (tutorialRunning) return;

        ApplyAugmentation(condition);
        StartCoroutine(CharacterSequence());
    }

    private void ApplyAugmentation(ConditionType condition)
    {
        if (holographicOverlay == null)
        {
            Debug.LogWarning("Holographic Overlay GameObject not assigned.");
            return;
        }

        switch (condition)
        {
            case ConditionType.Baseline:
                holographicOverlay.SetActive(false);
                break;

            case ConditionType.AugPrompt:
                holographicOverlay.SetActive(true);
                holographicOverlay.tag = "AR";
                break;

            case ConditionType.Continuous:
                holographicOverlay.SetActive(true);
                holographicOverlay.tag = "ARContinuous";
                break;
        }
    }

    public IEnumerator CharacterSequence()
    {
        tutorialRunning = true;

        yield return new WaitForSeconds(5f);

        foremanAnimator.SetTrigger("StartWalk");
        doorAnimator.SetTrigger("OpenDoor");
        PlayAudioSegment(doorAudio, 0f, 2f);
        yield return new WaitForSeconds(2f);

        foremanAnimator.SetTrigger("Turn");
        yield return new WaitForSeconds(1f);

        foremanAnimator.SetTrigger("StartTutorial");
        tutorialAudio.Play();

        float animationFPS = 30f;

        yield return WaitForFrame(494f, animationFPS, foremanAnimator);

        badStructure.transform.SetParent(foremanLeftHand);
        goodStructure.transform.SetParent(foremanRightHand);

        badStructure.transform.localPosition = badStructurePositionOffset;
        badStructure.transform.localRotation = Quaternion.Euler(badStructureRotationOffset);
        goodStructure.transform.localPosition = goodStructurePositionOffset;
        goodStructure.transform.localRotation = Quaternion.Euler(goodStructureRotationOffset);

        yield return WaitForFrame(560f, animationFPS, foremanAnimator);
        goodStructure.transform.SetParent(null);
        yield return StartCoroutine(TransitionPieceToTarget(
            goodStructure,
            goodPieceTargetTransform.position,
            goodPieceTargetTransform.rotation,
            10f / animationFPS));

        yield return WaitForFrame(580f, animationFPS, foremanAnimator);
        badStructure.transform.SetParent(foremanRightHand, true);

        Vector3 startLocalPos = badStructure.transform.localPosition;
        Quaternion startLocalRot = badStructure.transform.localRotation;

        Vector3 targetLocalPos = badStructureSwitchPositionOffset;
        Quaternion targetLocalRot = Quaternion.Euler(badStructureSwitchRotationOffset);

        float lerpDuration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            badStructure.transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);
            badStructure.transform.localRotation = Quaternion.Lerp(startLocalRot, targetLocalRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        badStructure.transform.localPosition = targetLocalPos;
        badStructure.transform.localRotation = targetLocalRot;

        yield return WaitForFrame(640f, animationFPS, foremanAnimator);
        yield return StartCoroutine(TransitionBadPiece(
            badStructure,
            badStructureSwitchPositionOffset,
            Quaternion.Euler(badStructureSwitchRotationOffset),
            badStructureSwitchOffsetPosition,
            Quaternion.Euler(badStructureSwitchOffsetRotation),
            25f / animationFPS));

        yield return WaitForFrame(770f, animationFPS, foremanAnimator);
        badStructure.transform.SetParent(null);
        yield return StartCoroutine(TransitionPieceToTarget(
            badStructure,
            badPieceTargetTransform.position,
            badPieceTargetTransform.rotation,
            10f / animationFPS));

        yield return WaitForFrame(1077f, animationFPS, foremanAnimator);

        MoveIKTarget moveIKTarget = FindObjectOfType<MoveIKTarget>();
        if (moveIKTarget != null)
        {
            moveIKTarget.move = true;
            overlayObject.tag = "Untagged";

            yield return new WaitUntil(() => moveIKTarget.state == MoveIKTarget.State.Drop);
            yield return new WaitUntil(() => moveIKTarget.state == MoveIKTarget.State.Move);

            tutorial2Audio.Play();
            foremanAnimator.Play("foremantut2", 0, 0f);
            yield return new WaitForSeconds(16.33f);

            foremanAnimator.Play("right turn reverse", 0, 0f);
            doorAnimator.Play("door-open 0", 0, 0f);
            PlayAudioSegment(doorAudio, 0f, 2f);
            yield return new WaitForSeconds(1f);
            foremanAnimator.Play("standard walk", 0, 0f);
            yield return new WaitForSeconds(2f);
            PlayAudioSegment(doorAudio, 0f, 4f);
        }

        foremanAnimator.Play("Empty", 0, 0f);
        tutorialRunning = false;
    }

    private IEnumerator TransitionPieceToTarget(GameObject piece, Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = piece.transform.position;
        Quaternion startRotation = piece.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            piece.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            piece.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        piece.transform.position = targetPosition;
        piece.transform.rotation = targetRotation;
    }

    private IEnumerator TransitionBadPiece(GameObject piece, Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            piece.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            piece.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        piece.transform.localPosition = endPos;
        piece.transform.localRotation = endRot;
    }

    private IEnumerator StopAudioAfterDuration(AudioSource audioSource, float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
    }

    private IEnumerator WaitForFrame(float targetFrame, float animationFPS, Animator animator)
    {
        while (true)
        {
            AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
            float currentFrame = animationState.normalizedTime * animationState.length * animationFPS;

            if (currentFrame >= targetFrame)
                yield break;

            yield return null;
        }
    }

    private void PlayAudioSegment(AudioSource audioSource, float startTime, float endTime)
    {
        audioSource.time = startTime;
        audioSource.Play();
        float duration = endTime - startTime;
        StartCoroutine(StopAudioAfterDuration(audioSource, duration));
    }
}
