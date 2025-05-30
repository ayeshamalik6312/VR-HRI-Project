using UnityEngine;
using System.Collections;
using System.IO;

public class TrackingDataCollector : MonoBehaviour
{
    private OVREyeGaze leftEyeGaze;
    private OVREyeGaze rightEyeGaze;
    private OVRCameraRig ovrCameraRig;
    private StreamWriter writer;
    private float maxRaycastDistance = 500f;
    private ParticipantManager participantManager;

    private LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;
    private bool[] leftControllerButtonStates = new bool[8];
    private bool[] rightControllerButtonStates = new bool[8];

    private void Awake()
    {
        participantManager = FindObjectOfType<ParticipantManager>();
    }

    public void StartLogging(string filePath)
    {
        StopAllCoroutines();
        StartCoroutine(StartWithPath(filePath));
    }

    private IEnumerator StartWithPath(string filePath)
    {
        yield return new WaitUntil(() => OVRPlugin.initialized);
        StartCoroutine(InitializeDataCollectionWithCustomPath(filePath));
    }

    private IEnumerator InitializeDataCollectionWithCustomPath(string filePath)
    {
        ovrCameraRig = GetComponent<OVRCameraRig>();
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig component not found!");
            yield break;
        }

        leftEyeGaze = GetComponentInChildren<OVREyeGaze>(true);
        rightEyeGaze = GetComponentInChildren<OVREyeGaze>(true);

        try
        {
            writer = new StreamWriter(filePath, true);
        }
        catch (IOException ex)
        {
            Debug.LogError("File open error: " + ex.Message);
            yield break;
        }

        writer.WriteLine(
            "CountdownTime,AbsoluteTime,RelativeTime,TimeStep," +
            "LeftEyePosX,LeftEyePosY,LeftEyePosZ," +
            "RightEyePosX,RightEyePosY,RightEyePosZ," +
            "LeftEyeHitObject,RightEyeHitObject," +
            "HeadPosX,HeadPosY,HeadPosZ," +
            "HeadRotX,HeadRotY,HeadRotZ,HeadRotW," +
            "HeadEulerX,HeadEulerY,HeadEulerZ," +
            "LHandPosX,LHandPosY,LHandPosZ," +
            "LHandRotX,LHandRotY,LHandRotZ,LHandRotW," +
            "LHandEulerX,LHandEulerY,LHandEulerZ," +
            "RHandPosX,RHandPosY,RHandPosZ," +
            "RHandRotX,RHandRotY,RHandRotZ,RHandRotW," +
            "RHandEulerX,RHandEulerY,RHandEulerZ," +
            "LeftJoyX,LeftJoyY,RightJoyX,RightJoyY," +
            "L_One,L_Two,L_IndexTrigger,L_HandTrigger,L_Start,L_Thumbstick,L_ThumbUp,L_ThumbClick," +
            "R_One,R_Two,R_IndexTrigger,R_HandTrigger,R_Start,R_Thumbstick,R_ThumbUp,R_ThumbClick"

        );

        StartCoroutine(CollectComprehensiveVRData());
    }

    private IEnumerator CollectComprehensiveVRData()
    {
        float previousTime = Time.time;

        while (true)
        {
            float currentTime = Time.time;
            float deltaTime = currentTime - previousTime;
            previousTime = currentTime;

            string absoluteTime = System.DateTime.Now.ToString("HH:mm:ss.fff");

            string countdownFormatted = "NA";
            if (participantManager != null)
            {
                float remaining = participantManager.GetTimeRemaining();
                int minutes = Mathf.FloorToInt(remaining / 60f);
                float seconds = remaining % 60f;
                countdownFormatted = $"{minutes:D2}:{seconds:00.0000}";
            }

            Transform head = ovrCameraRig.centerEyeAnchor;
            Transform leftHand = ovrCameraRig.leftHandAnchor;
            Transform rightHand = ovrCameraRig.rightHandAnchor;

            Vector2 leftJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
            Vector2 rightJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);

            UpdateButtonStates(OVRInput.Controller.LTouch, ref leftControllerButtonStates);
            UpdateButtonStates(OVRInput.Controller.RTouch, ref rightControllerButtonStates);

            string leftEyeObj = "None";
            string rightEyeObj = "None";
            Vector3 leftEyePos = Vector3.zero;
            Vector3 rightEyePos = Vector3.zero;

            bool gazeEnabled = OVRPlugin.eyeTrackingSupported && OVRPlugin.eyeTrackingEnabled &&
                               leftEyeGaze != null && rightEyeGaze != null &&
                               leftEyeGaze.isActiveAndEnabled && rightEyeGaze.isActiveAndEnabled;

            if (gazeEnabled)
            {
                leftEyePos = leftEyeGaze.transform.position;
                rightEyePos = rightEyeGaze.transform.position;
                leftEyeObj = GetLookedAtObject(leftEyeGaze);
                rightEyeObj = GetLookedAtObject(rightEyeGaze);
            }

            string line = $"{countdownFormatted}," +
                          $"{absoluteTime},{currentTime:0.0000},{deltaTime:0.000000}," +
                          $"{leftEyePos.x},{leftEyePos.y},{leftEyePos.z}," +
                          $"{rightEyePos.x},{rightEyePos.y},{rightEyePos.z}," +
                          $"{leftEyeObj},{rightEyeObj}," +
                          $"{head.position.x},{head.position.y},{head.position.z}," +
                          $"{head.rotation.x},{head.rotation.y},{head.rotation.z},{head.rotation.w}," +
                          $"{head.eulerAngles.x},{head.eulerAngles.y},{head.eulerAngles.z}," +
                          $"{leftHand.position.x},{leftHand.position.y},{leftHand.position.z}," +
                          $"{leftHand.rotation.x},{leftHand.rotation.y},{leftHand.rotation.z},{leftHand.rotation.w}," +
                          $"{leftHand.eulerAngles.x},{leftHand.eulerAngles.y},{leftHand.eulerAngles.z}," +
                          $"{rightHand.position.x},{rightHand.position.y},{rightHand.position.z}," +
                          $"{rightHand.rotation.x},{rightHand.rotation.y},{rightHand.rotation.z},{rightHand.rotation.w}," +
                          $"{rightHand.eulerAngles.x},{rightHand.eulerAngles.y},{rightHand.eulerAngles.z}," +
                          $"{leftJoystick.x},{leftJoystick.y},{rightJoystick.x},{rightJoystick.y}," +
                          $"{string.Join(",", leftControllerButtonStates)}," +
                          $"{string.Join(",", rightControllerButtonStates)}";

            writer.WriteLine(line);
            yield return new WaitForSeconds(0.01f); // ~100Hz
        }
    }

    private void UpdateButtonStates(OVRInput.Controller controller, ref bool[] states)
    {
        states[0] = OVRInput.Get(OVRInput.Button.One, controller);
        states[1] = OVRInput.Get(OVRInput.Button.Two, controller);
        states[2] = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller);
        states[3] = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller);
        states[4] = OVRInput.Get(OVRInput.Button.Start, controller);
        states[5] = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller);
        states[6] = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp, controller);
        states[7] = OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown, controller); // or choose a distinct button
    }

    private string GetLookedAtObject(OVREyeGaze eye)
    {
        Ray ray = new Ray(eye.transform.position, eye.transform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastLayerMask)
            ? hit.collider.gameObject.name
            : "None";
    }

    public void StopLogging()
    {
        StopAllCoroutines();
        if (writer != null)
        {
            writer.Close();
            writer = null;
        }
    }

    private void OnApplicationQuit()
    {
        StopLogging();
    }
}
