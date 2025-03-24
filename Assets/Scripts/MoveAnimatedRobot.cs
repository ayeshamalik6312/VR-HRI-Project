using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAnimatedRobot : MonoBehaviour
{
    public enum State
    {
        Move,
        Grab,
        Connect
    }

    public List<Transform> objList = new List<Transform>(); // List of points to move between
    public Transform start;  // Starting position of the robot
    public bool move = true;  // Flag to start moving automatically
    public State state = State.Move;  // Initial state is Move

    private List<GameObject> animatedSockets = new List<GameObject>();  // List of animated socket objects (previously 'female')
    private List<Vector3> points = new List<Vector3>();  // Positions to move between
    private List<Quaternion> rots = new List<Quaternion>();  // Rotations to match at each point
    private GameObject pickPoint;  // The point from which the robot picks up objects
    private Vector3 tempPos;
    private Quaternion tempRot;
    float time = 0f;
    int posIdx = 1;
    bool switchDir = true;  // Used for switching direction in the grab state
    int animatedSocketsCurrSize;

    readonly float durationMultiplier = 4f;  // Speed multiplier for movement
    readonly float rangeDelta = 0.0001f;  // Range for determining when the robot has reached a target

    void Start()
    {
        // Populate positions and rotations from the object list
        foreach (Transform obj in objList)
        {
            points.Add(obj.position);
            rots.Add(obj.rotation);
        }

        // Find all animated sockets by tag
        animatedSockets = GameObject.FindGameObjectsWithTag("animatedSocket").ToList();
        animatedSocketsCurrSize = animatedSockets.Count;

        // Find the pick point in the scene
        pickPoint = GameObject.FindWithTag("pickPoint");

        // Set the robot's starting position
        transform.position = start.position;
        transform.rotation = start.rotation;

        // If there are animated sockets available, start the movement
        if (animatedSocketsCurrSize > 0)
        {
            move = true;
            state = State.Move;
        }
    }

    void Update()
    {
        // Check if there are more animated sockets to process
        if (animatedSocketsCurrSize > 0)
        {
            switch (state)
            {
                case State.Move:
                    if (move)
                    {
                        // Call MainStepsLerp to move the robot, and set move to false once done
                        if (MainStepsLerp() == false)
                        {
                            move = false;
                        }
                    }
                    break;

                case State.Grab:
                    // Handle the grabbing of the animated socket
                    GrabState();
                    break;

                case State.Connect:
                    // Handle the connection process (waiting for animation, etc.)
                    ConnectState();
                    break;

                default:
                    break;
            }
        }
        else
        {
            Debug.Log("No more parts");
            this.enabled = false;  // Disable script once no more parts are left
        }
    }

    // This method gets called by RobotIKControl when the animation is done, signaling to start moving again
    public void ReadyForNextTarget()
    {
        move = true;
        state = State.Move;  // Set the state back to Move so the robot can start over
    }

    // This method moves the robot between points with smooth interpolation
    bool MainStepsLerp()
    {
        string currPointTag = objList[posIdx - 1].tag;

        // Check if we're at a wait point (grab, connect, etc.)
        if (currPointTag != "Untagged")
        {
            // Switch to the appropriate state when we hit a point with a specific tag
            if (currPointTag == "Grab")
                state = State.Grab;
            else if (currPointTag == "Connect")
                state = State.Connect;

            return false;  // Return false to stop moving
        }

        // Calculate the distance between points and interpolate the position
        int pointsLength = points.Count;
        float dist = Vector3.Distance(points[posIdx - 1], points[posIdx]);
        float t = time / (dist * durationMultiplier);
        t = t * t * (3f - 2f * t);  // Smooth step interpolation

        transform.position = Vector3.Lerp(points[posIdx - 1], points[posIdx], t);
        transform.rotation = Quaternion.Lerp(rots[posIdx - 1], rots[posIdx], t);

        if (InRange(transform.position, points[posIdx], rangeDelta))
        {
            // Reset time and move to the next position
            time = 0;
            posIdx += 1;

            if (posIdx >= pointsLength)
            {
                posIdx = 1;  // Loop back to the start when finished
            }
        }

        time += Time.deltaTime;
        return true;  // Return true while still moving
    }

    // Function for grabbing the animated socket
    void GrabState()
    {
        // Implement logic for grabbing the animated socket
        bool stepsDone;
        if (switchDir)
        {
            stepsDone = TinyStepLerp(points[posIdx - 1], rots[posIdx - 1], animatedSockets[animatedSocketsCurrSize - 1].transform.position, animatedSockets[animatedSocketsCurrSize - 1].transform.rotation);
            if (stepsDone)
            {
                switchDir = false;
                animatedSockets[animatedSocketsCurrSize - 1].transform.parent.parent = pickPoint.transform;
                tempPos = animatedSockets[animatedSocketsCurrSize - 1].transform.position;
                tempRot = animatedSockets[animatedSocketsCurrSize - 1].transform.rotation;
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

    // Function for connecting the socket and waiting for animation to complete
    void ConnectState()
    {
        // You could use your animation control or waiting logic here
        if (true) // Replace with condition for when the connection animation is done
        {
            move = true;
            state = State.Move;
        }
    }

    // Lerp movement with smaller steps
    bool TinyStepLerp(Vector3 initialPos, Quaternion initialRot, Vector3 targetPos, Quaternion targetRot)
    {
        float dist = Vector3.Distance(initialPos, targetPos);
        float t = time / (dist * durationMultiplier);
        t = t * t * (3f - 2f * t);  // Smooth step interpolation

        transform.position = Vector3.Lerp(initialPos, targetPos, t);
        transform.rotation = Quaternion.Lerp(initialRot, targetRot, t);

        if (InRange(transform.position, targetPos, rangeDelta))
        {
            time = 0;
            return true;  // Movement finished
        }

        time += Time.deltaTime;
        return false;  // Still moving
    }

    // Utility function to check if two positions are within a range
    static bool InRange(Vector3 a, Vector3 b, float delta)
    {
        return (Mathf.Abs(a.x - b.x) <= delta && Mathf.Abs(a.y - b.y) <= delta && Mathf.Abs(a.z - b.z) <= delta);
    }
}
