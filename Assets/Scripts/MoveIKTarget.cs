using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveIKTarget : MonoBehaviour
{
    public enum State
    {
        Move,
        Grab,
        Connect,
        Drop
    }

    public List<Transform> objList = new List<Transform>();
    public Transform DropGood;
    public Transform DropBad;
    public Transform start;
    public bool move;
    public int countTest = 0;
    public State state = State.Move;

    private List<GameObject> sockets = new List<GameObject>();
    private ParticipantManager participantManager;
    private List<Vector3> points = new List<Vector3>();
    private List<Quaternion> rots = new List<Quaternion>();
    private RuntimeMaterialChange materialChanger;
    private GameObject pickPoint;
    private GameObject female;
    private Vector3 tempPos;
    private Quaternion tempRot;
    private bool isCycleTiming = false;
    private float cycleTimer = 0f;
    float time = 0f;
    int posIdx = 1;
    bool atPrevWaitPoint = false;
    bool switchDir = true;
    int tick = 0;
    int socketsCurrSize;
    int dropOption = 0;
    private string lastButtonPressed = "None";
    private bool acceptingDropButtonPress = false;
    public bool tutorialMode = false;
    public bool tutorialCycleCompleted = false;


    readonly float durationMultiplier = 4f;
    readonly int tickDuration = 150;
    readonly float rangeDelta = 0.0001f;

    void Start()
    {
        foreach (Transform obj in objList)
        {
            points.Add(obj.position);
            rots.Add(obj.rotation);
        }

        sockets = GameObject.FindGameObjectsWithTag("female").ToList();
        socketsCurrSize = sockets.Count;

        pickPoint = GameObject.FindWithTag("pickPoint");

        transform.position = start.position;
        transform.rotation = start.rotation;

        female = sockets[socketsCurrSize - 1];
        participantManager = FindObjectOfType<ParticipantManager>();
        materialChanger = FindObjectOfType<RuntimeMaterialChange>();


    }

    void Update()
    {

        if (move && !isCycleTiming)
        {
            isCycleTiming = true;
            cycleTimer = 0f;
        }

        if (isCycleTiming)
        {
            cycleTimer += Time.deltaTime;
        }


        if (socketsCurrSize > 0)
        {
            switch (state)
            {
                case State.Move:
                    if (move == true)
                    {
                        if (MainStepsLerp() == false)
                            move = false;
                    }
                    break;
                case State.Grab:
                    GrabState();
                    break;
                case State.Connect:
                    ConnectState();
                    break;
                case State.Drop:
                    DropState();
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.Log("No more parts");
            this.enabled = false;
        }
    }

    void GrabState()
    {

        if (female == null || female.Equals(null))
            return;

        bool stepsDone;
        if (switchDir)
        {
            stepsDone = TinyStepLerp(points[posIdx - 1], rots[posIdx - 1], female.transform.position, female.transform.rotation);
            if (stepsDone)
            {
                switchDir = false;
                female.transform.parent.parent = pickPoint.transform;
                tempPos = female.transform.position;
                tempRot = female.transform.rotation;
                countTest++;
            }
        }
        else
        {
            stepsDone = TinyStepLerp(tempPos, tempRot, points[posIdx - 1], rots[posIdx - 1]);
            if (stepsDone)
            {
                switchDir = true;
                move = true;
                state = State.Move;
            }
        }
    }

    void ConnectState()
    {
        SnapParts snapParts = female.transform.parent.GetComponentInChildren<SnapParts>();

        if (snapParts.snapped == true)
        {
            if (tick < tickDuration)
            {
                tick += 1;
            }
            else
            {
                move = true;
                state = State.Move;
                tick = 0;
            }
        }
    }

    public void DropGoodButton()
    {
        if (state == State.Drop && dropOption == 0 && acceptingDropButtonPress)
        {
            dropOption = 1;
            lastButtonPressed = "Good";
        }
    }

    public void DropBadButton()
    {
        if (state == State.Drop && dropOption == 0 && acceptingDropButtonPress)
        {
            dropOption = 2;
            lastButtonPressed = "Bad";
        }
    }


    void DropState()
    {
        // Always log all relevant state info at the start of every frame in DropState
            //    Debug.Log($"    female: {(female != null ? female.name : "null")}");
     //   Debug.Log($"    female.parent: {(female?.transform.parent != null ? female.transform.parent.name : "null")}");
      //  Debug.Log($"    cycleTimer: {cycleTimer:F2}");
     //   Debug.Log($"    position: {transform.position}");
      //  Debug.Log($"    rotation: {transform.rotation.eulerAngles}");

        // Make sure we accept input
        acceptingDropButtonPress = true;

        if (dropOption == 1)
        {
           // Debug.Log("[DropState] ✅ dropOption == 1 → calling DropGoodFunc()");
           // Debug.Log($"    PressedButton: {lastButtonPressed}");
            acceptingDropButtonPress = false;
            DropGoodFunc();
        }
        else if (dropOption == 2)
        {
           // Debug.Log("[DropState] ✅ dropOption == 2 → calling DropBadFunc()");
           // Debug.Log($"    PressedButton: {lastButtonPressed}");
            acceptingDropButtonPress = false;
            DropBadFunc();
        }
        else
        {
          //  Debug.Log("[DropState] ⏳ No button press detected yet this frame.");
        }

      //  Debug.Log("[DropState] ---- End of DropState frame ----");
    }



    void DropGoodFunc()
    {
        HandleDrop(DropGood);
    }

    void DropBadFunc()
    {
        HandleDrop(DropBad);
    }

    void HandleDrop(Transform dropTarget)
    {
        bool stepsDone = false;

        // Turn off overlays only in AugOnPrompt condition
        if (participantManager != null &&
            GameObject.FindWithTag("AR") != null)
        {
            var matChanger = participantManager.materialChanger;
            if (matChanger != null)
            {
                matChanger.DeactivatePromptOverlay();
            }
        }

        if (switchDir)
        {
            stepsDone = TinyStepLerp(points[posIdx - 1], rots[posIdx - 1], dropTarget.position, dropTarget.rotation);

            if (stepsDone)
            {
                tempPos = female.transform.position;
                tempRot = female.transform.rotation;

                GameObject key = female;
                Transform femaleParent = key.transform.parent;

                if (femaleParent != null)
                {
                    key.transform.parent = femaleParent;

                    Rigidbody keyRB = key.GetComponent<Rigidbody>();
                    if (keyRB != null)
                    {
                        Destroy(keyRB);
                    }

                    Collider keyCol = key.GetComponent<Collider>();
                    if (keyCol != null)
                    {
                        keyCol.enabled = false;
                    }

                    femaleParent.parent = null;

                    Rigidbody rb = femaleParent.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = femaleParent.gameObject.AddComponent<Rigidbody>();
                    }

                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.drag = 1f;
                    rb.angularDrag = 1f;
                }

                // Update socket list
                GameObject femaleToDestroy = female;
                sockets.RemoveAt(socketsCurrSize - 1);
                socketsCurrSize--;
                switchDir = false;

                // Do NOT reassign `female` until after decision logic

                // Report data
                string button = ConsumeLastButtonPressed();

                GameObject socket = femaleParent?.gameObject;  // ✅ Use femaleParent
                bool isKeyChipped = key != null && key.GetComponent<chipped>() != null;
                bool isSocketChipped = socket != null && socket.GetComponent<chipped>() != null;
                bool hasChip = isKeyChipped || isSocketChipped;

                string chipStatusString = "None";
                if (isKeyChipped && isSocketChipped) chipStatusString = "Both";
                else if (isKeyChipped) chipStatusString = "Key";
                else if (isSocketChipped) chipStatusString = "Socket";

                SnapParts snapParts = femaleParent?.GetComponentInChildren<SnapParts>();
                bool isSnapCorrect = snapParts != null && !snapParts.isFlipped;
                bool participantSaidGood = button == "Good";
                bool participantSaidBad = button == "Bad";

                // ✅ Final decision logic
                bool decisionCorrect = false;
                if (isSnapCorrect && !hasChip && participantSaidGood)
                {
                    decisionCorrect = true;
                }
                else if ((!isSnapCorrect || hasChip) && participantSaidBad)
                {
                    decisionCorrect = true;
                }

                // ✅ Report result
                if (participantManager != null && (button == "Good" || button == "Bad"))
                {
                    participantManager.ReportSnap(!isSnapCorrect, button, chipStatusString, decisionCorrect);
                }

                if (femaleToDestroy != null)
                {
                    Destroy(femaleToDestroy);
                }

                // ✅ Now assign the next female
                female = socketsCurrSize > 0 ? sockets[socketsCurrSize - 1] : null;
            }
        }
        else
        {
            stepsDone = TinyStepLerp(tempPos, tempRot, points[posIdx - 1], rots[posIdx - 1]);
            if (stepsDone)
            {
                switchDir = true;

                if (tutorialMode)
                {
                    move = false;
                    tutorialMode = false;
                    tutorialCycleCompleted = true;
                    state = State.Move;
                    return;
                }

                move = true;
                state = State.Move;
                dropOption = 0;

                if (participantManager != null &&
                    participantManager.CurrentCondition == "AugOnPrompt" &&
                    materialChanger != null)
                {
                    materialChanger.ActivatePromptOverlay(participantManager.overlayObject);
                }
            }
        }
    }


    IEnumerator RemoveRigidbodiesAfterDelay(GameObject parent, float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log($"[Cleanup] Removing rigidbodies from: {parent.name}");

        Rigidbody parentRb = parent.GetComponent<Rigidbody>();
        if (parentRb != null)
        {
            //     Debug.Log($"[Cleanup] Destroying parent RB: isKinematic={parentRb.isKinematic}, useGravity={parentRb.useGravity}");
            Destroy(parentRb);
        }

        Rigidbody[] childRigidbodies = parent.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody childRb in childRigidbodies)
        {
            if (childRb != null)
            {
                //     Debug.Log($"[Cleanup] Destroying child RB on: {childRb.gameObject.name}, isKinematic={childRb.isKinematic}, useGravity={childRb.useGravity}");
                Destroy(childRb);
            }
        }

    }

    IEnumerator MakeRigidbodiesKinematicAfterDelay(GameObject parent, float delay)
    {
        yield return new WaitForSeconds(delay);

        Rigidbody parentRb = parent.GetComponent<Rigidbody>();
        if (parentRb != null)
        {
            parentRb.isKinematic = true;
            parentRb.velocity = Vector3.zero;
            parentRb.angularVelocity = Vector3.zero;
        }

        Rigidbody[] childRigidbodies = parent.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody childRb in childRigidbodies)
        {
            if (childRb != null)
            {
                childRb.isKinematic = true;
                childRb.velocity = Vector3.zero;
                childRb.angularVelocity = Vector3.zero;
            }
        }
    }
    public void ForceRestartCycle()
    {

        foreach (Transform child in pickPoint.transform)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
        // Stop movement and logic
        move = false;
        state = State.Move;
        isCycleTiming = false;
        cycleTimer = 0f;
        time = 0f;
        posIdx = 1;
        atPrevWaitPoint = false;
        switchDir = true;
        tick = 0;
        dropOption = 0;
        acceptingDropButtonPress = false;
        tutorialMode = false;
        tutorialCycleCompleted = false;

        // Immediately stop using the current female object
        GameObject oldFemale = female;
        female = null; // prevent any access in Update()

        if (oldFemale != null)
            Destroy(oldFemale); // destroy safely

        // Rebuild socket list
        sockets = GameObject.FindGameObjectsWithTag("female").ToList();
        socketsCurrSize = sockets.Count;

        if (socketsCurrSize > 0)
        {
            female = sockets[socketsCurrSize - 1];
        }

        if (participantManager != null &&
        participantManager.CurrentCondition == "AugOnPrompt" &&
        materialChanger != null)
        {
            materialChanger.DeactivatePromptOverlay();
        }


        // Move robot to start
        transform.position = start.position;
        transform.rotation = start.rotation;

        // Resume movement
        move = true;
        if (participantManager != null &&
        participantManager.CurrentCondition == "AugOnPrompt" &&
        materialChanger != null)
        {
            materialChanger.ActivatePromptOverlay(participantManager.overlayObject);
        }
    }




    public string ConsumeLastButtonPressed()
    {
        string result = lastButtonPressed;
        lastButtonPressed = "None";
        return result;
    }



    public void ResetCycleTimer()
    {
        isCycleTiming = false;
        cycleTimer = 0f;
    }

    bool MainStepsLerp()
    {
        string currPointTag = objList[posIdx - 1].tag;
        if (atPrevWaitPoint == false && currPointTag != "Untagged")
        {
            if (currPointTag == "Grab")
                state = State.Grab;
            else if (currPointTag == "Connect")
                state = State.Connect;
            else if (currPointTag == "Drop")
                state = State.Drop;

            atPrevWaitPoint = true;
            return false;
        }

        int pointsLength = points.Count;
        if (posIdx >= pointsLength)
        {
            float dist = Vector3.Distance(points[pointsLength - 1], points[0]);
            float t = time / (dist * durationMultiplier);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(points[pointsLength - 1], points[0], t);
            transform.rotation = Quaternion.Lerp(rots[pointsLength - 1], rots[0], t);

            if (InRange(transform.position, points[0], rangeDelta))
            {
                time = 0;
                posIdx = 1;
                atPrevWaitPoint = false;
            }
        }
        else
        {
            float dist = Vector3.Distance(points[posIdx - 1], points[posIdx]);
            float t = time / (dist * durationMultiplier);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(points[posIdx - 1], points[posIdx], t);
            transform.rotation = Quaternion.Lerp(rots[posIdx - 1], rots[posIdx], t);

            if (InRange(transform.position, points[posIdx], rangeDelta))
            {
                time = 0;
                posIdx += 1;
                atPrevWaitPoint = false;
            }
        }

        time += Time.deltaTime;
        return true;
    }

    bool TinyStepLerp(Vector3 initialPos, Quaternion initialRot, Vector3 targetPos, Quaternion targetRot)
    {
        float dist = Vector3.Distance(initialPos, targetPos);
        float t = time / (dist * durationMultiplier);
        t = t * t * (3f - 2f * t);

        transform.position = Vector3.Lerp(initialPos, targetPos, t);
        transform.rotation = Quaternion.Lerp(initialRot, targetRot, t);

        if (InRange(transform.position, targetPos, rangeDelta))
        {
            time = 0;
            return true;
        }

        time += Time.deltaTime;
        return false;
    }

    static bool InRange(Vector3 a, Vector3 b, float delta)
    {
        return Mathf.Abs(a.x - b.x) <= delta && Mathf.Abs(a.y - b.y) <= delta && Mathf.Abs(a.z - b.z) <= delta;
    }
}