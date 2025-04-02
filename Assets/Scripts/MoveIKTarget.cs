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
    private List<Vector3> points = new List<Vector3>();
    private List<Quaternion> rots = new List<Quaternion>();
    private GameObject pickPoint;
    private GameObject female;
    private Vector3 tempPos;
    private Quaternion tempRot;
    float time = 0f;
    int posIdx = 1;
    bool atPrevWaitPoint = false;
    bool switchDir = true;
    int tick = 0;
    int socketsCurrSize;
    int dropOption = 0;

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
    }

    void Update()
    {
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
        if (state == State.Drop && dropOption == 0)
        {
            dropOption = 1;
        }
    }

    public void DropBadButton()
    {
        if (state == State.Drop && dropOption == 0)
        {
            dropOption = 2;
        }
    }

    void DropState()
    {
        if (dropOption == 1)
        {
            DropGoodFunc();
        }
        else if (dropOption == 2)
        {
            DropBadFunc();
        }
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
        bool stepsDone;
        if (switchDir)
        {
            stepsDone = TinyStepLerp(points[posIdx - 1], rots[posIdx - 1], dropTarget.position, dropTarget.rotation);
            if (stepsDone)
            {
                tempPos = female.transform.position;
                tempRot = female.transform.rotation;

                Transform femaleParent = female.transform.parent;
                if (femaleParent != null)
                {
                    femaleParent.parent = null;

                    // Add rigidbody safely
                    Rigidbody rb = femaleParent.gameObject.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = femaleParent.gameObject.AddComponent<Rigidbody>();
                    }
                    rb.isKinematic = false;

                    Rigidbody[] childRigidbodies = femaleParent.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody childRb in childRigidbodies)
                    {
                        if (childRb != null)
                            childRb.isKinematic = false;
                    }
                }

                GameObject femaleToDestroy = female; // cache it
                sockets.RemoveAt(socketsCurrSize - 1);
                socketsCurrSize--;
                switchDir = false;

                female = socketsCurrSize > 0 ? sockets[socketsCurrSize - 1] : null;

                if (femaleToDestroy != null)
                    Destroy(femaleToDestroy);
            }
        }
        else
        {
            stepsDone = TinyStepLerp(tempPos, tempRot, points[posIdx - 1], rots[posIdx - 1]);
            if (stepsDone)
            {
                switchDir = true;
                move = true;
                dropOption = 0;
                state = State.Move;
            }
        }
    }

    IEnumerator RemoveRigidbodiesAfterDelay(GameObject parent, float delay)
    {
        yield return new WaitForSeconds(delay);

        Rigidbody parentRb = parent.GetComponent<Rigidbody>();
        if (parentRb != null)
        {
            Destroy(parentRb);
        }

        Rigidbody[] childRigidbodies = parent.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody childRb in childRigidbodies)
        {
            Destroy(childRb);
        }
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
