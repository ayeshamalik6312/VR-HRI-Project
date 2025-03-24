using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundKey : MonoBehaviour
{

    public Animator humanAnimator;               
    public Animator robotAnimator;               
    public Transform robotPickupPoint;           
    public Transform humanHand;                 
    public Transform robotHand;                  
    public Transform fallTargetPosition; 

    public GameObject key1;                   
    public GameObject key2;                 
    public GameObject key3;
    public GameObject socket1;
    public GameObject socket2;
    public GameObject socket3;


    private float[] human1Frames = { 122f, 188f, 508f, 881f, 1090, 100 };
    private float[] human2Frames = { 122f, 188f, 508f, 881f, 1090, 100 };
    private float[] human3Frames = { 110f, 188f, 576f, 1070f, 1280, 100};

    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    public Vector3 lookPositionOffset;
    public Vector3 lookRotationOffset;
    public Vector3 connectPositionOffset;
    public Vector3 connectRotationOffset;
    public Vector3 robotConnectPositionOffset;
    public Vector3 robotConnectRotationOffset;


    private float[] robotFrames = { 200f, 300f, 500f };

    private bool hasGrabbed = false;
    private bool hasConnected = false;
    private bool hasRobotGrabbed = false;
    private bool hasDropped = false;






    private GameObject currentKey;
    private GameObject currentSocket;

    private float[] currentFrames;            
    private Vector3 initialKeyPosition;          
    private Quaternion initialKeyRotation;       
    private Vector3 initialKeyPosition1;         
    private Quaternion initialKeyRotation1;     
    private Vector3 initialKeyPosition2;        
    private Quaternion initialKeyRotation2;       
    private Vector3 initialKeyPosition3;        
    private Quaternion initialKeyRotation3;

    private Vector3 initialSocketPosition;
    private Quaternion initialSocketRotation;
    private Vector3 initialSocketPosition1;
    private Quaternion initialSocketRotation1;
    private Vector3 initialSocketPosition2;
    private Quaternion initialSocketRotation2;
    private Vector3 initialSocketPosition3;
    private Quaternion initialSocketRotation3;

    void Start()
    {
        initialKeyPosition1 = key1.transform.position;
        initialKeyRotation1 = key1.transform.rotation;
        initialKeyPosition2 = key2.transform.position;
        initialKeyRotation2 = key2.transform.rotation;
        initialKeyPosition3 = key3.transform.position;
        initialKeyRotation3 = key3.transform.rotation;

        initialSocketPosition1 = socket1.transform.position;
        initialSocketRotation1 = socket1.transform.rotation;
        initialSocketPosition2 = socket2.transform.position;
        initialSocketRotation2 = socket2.transform.rotation;
        initialSocketPosition3 = socket3.transform.position;
        initialSocketRotation3 = socket3.transform.rotation;

    }

    void Update()
    {
        string currentAnimation = GetCurrentAnimationName();

        if (currentAnimation == "human-1")
        {
            currentKey = key1;
            currentFrames = human1Frames;
            initialKeyPosition = initialKeyPosition1;
            initialKeyRotation = initialKeyRotation1;


            currentSocket = socket1;
            initialSocketPosition = initialSocketPosition1;
            initialSocketRotation = initialSocketRotation1;

        }
        else if (currentAnimation == "human-2")
        {
            currentKey = key2;
            currentFrames = human2Frames;
            initialKeyPosition = initialKeyPosition2;
            initialKeyRotation = initialKeyRotation2;


            currentSocket = socket2;
            initialSocketPosition = initialSocketPosition2;
            initialSocketRotation = initialSocketRotation2;


        }
        else if (currentAnimation == "human-3")
        {
            currentKey = key3;
            currentFrames = human3Frames;
            initialKeyPosition = initialKeyPosition3;
            initialKeyRotation = initialKeyRotation3;


            currentSocket = socket3;
            initialSocketPosition = initialSocketPosition3;
            initialSocketRotation = initialSocketRotation3;

        }

        float currentFrame = GetCurrentFrame(humanAnimator);
        float event1 = (Mathf.Abs(currentFrame - currentFrames[0]));
        float event2 = (Mathf.Abs(currentFrame - currentFrames[5]));
        float event3 = (Mathf.Abs(currentFrame - currentFrames[2]));
        float event4 = (Mathf.Abs(currentFrame - currentFrames[3]));



        // grab event 
        if (Mathf.Abs(currentFrame - currentFrames[0]) < 0.8f && !hasGrabbed)
        {
            StartCoroutine(ParentToHumanHand());
            hasGrabbed = false; 
        } 
        // robot grab event 
        if (Mathf.Abs(currentFrame - currentFrames[5]) < 1f && !hasRobotGrabbed)
        {
            ParentToRobot();
            hasRobotGrabbed = false;

        }

        // connect event 
        if (Mathf.Abs(currentFrame - currentFrames[2]) < 0.8f && !hasConnected)
        {
            StartCoroutine(ReparentToRobot());
            hasConnected = false;

        }

        // drop event 
        if (Mathf.Abs(currentFrame - currentFrames[3]) < 0.8f && !hasDropped)
        {
            hasDropped = true;
            currentSocket.transform.SetParent(null);
            Rigidbody rb = currentSocket.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentSocket.AddComponent<Rigidbody>();

            }

            rb.isKinematic = false;
            rb.useGravity = true;
            hasDropped = false;

        }


        // reset event 
        if (Mathf.Abs(currentFrame - currentFrames[4]) < 0.8f)
        {
            currentKey.transform.SetParent(null);
            currentSocket.transform.SetParent(null);
            currentKey.transform.position = initialKeyPosition;
            currentKey.transform.rotation = initialKeyRotation;
            currentSocket.transform.position = initialSocketPosition;
            currentSocket.transform.rotation = initialSocketRotation;

            Rigidbody rb = currentSocket.GetComponent<Rigidbody>();
            if (rb == null)
            {
            }
            else
            {
                Destroy(rb);
            }
            hasGrabbed = false;
            hasRobotGrabbed = false;
            hasConnected = false;
            hasDropped = false;
        }
    }

    private void ParentToRobot()
    {
        hasRobotGrabbed = true;
        currentSocket.transform.SetParent(robotPickupPoint);
        hasRobotGrabbed = false;
    }

    private IEnumerator ParentToHumanHand()
    {
        hasGrabbed = true;
        Vector3 startPos = currentKey.transform.position;
        Quaternion startRot = currentKey.transform.rotation;

        Vector3 targetWorldPos = humanHand.TransformPoint(grabPositionOffset);
        Quaternion targetWorldRot = humanHand.rotation * Quaternion.Euler(grabRotationOffset);

        float elapsedTime = 0f;
        float lerpDuration = 0.3f;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            currentKey.transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
            currentKey.transform.rotation = Quaternion.Lerp(startRot, targetWorldRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentKey.transform.SetParent(humanHand, false); 
        currentKey.transform.localPosition = grabPositionOffset;
        currentKey.transform.localRotation = Quaternion.Euler(grabRotationOffset);

        hasGrabbed = false;


    }
    private IEnumerator ReparentToRobot()
    {
        hasConnected = true;
        Vector3 startPos = currentKey.transform.position;
        Quaternion startRot = currentKey.transform.rotation;

        Vector3 targetWorldPos = currentSocket.transform.TransformPoint(connectPositionOffset);
        Quaternion targetWorldRot = currentSocket.transform.rotation * Quaternion.Euler(connectRotationOffset);

        float elapsedTime = 0f;
        float lerpDuration = 0.4f;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            currentKey.transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
            currentKey.transform.rotation = Quaternion.Lerp(startRot, targetWorldRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentKey.transform.SetParent(currentSocket.transform, false); 
        currentKey.transform.localPosition = connectPositionOffset;
        currentKey.transform.localRotation = Quaternion.Euler(connectRotationOffset);

        hasConnected = false;


    }
    private string GetCurrentAnimationName()
    {
        AnimatorStateInfo animationState = humanAnimator.GetCurrentAnimatorStateInfo(0);
        if (animationState.IsName("human-1")) return "human-1";
        if (animationState.IsName("human-2")) return "human-2";
        if (animationState.IsName("human-3")) return "human-3";
        return "";
    }

    private float GetCurrentFrame(Animator animator)
    {
        AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
        return animationState.normalizedTime * animationState.length * 60f; // 60 fps
    }

    private float GetAnimationEndFrame(Animator animator)
    {
        AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
        return animationState.length * 60f; 
    }

}
