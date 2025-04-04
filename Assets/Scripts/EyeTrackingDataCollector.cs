using UnityEngine;
using System.Collections;
using System.IO;

public class EyeTrackingDataCollector : MonoBehaviour
{
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    private OVRCameraRig ovrCameraRig;
    private StreamWriter writer;
    private float maxRaycastDistance = 500f;
    private LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;

    // Controller button states
    private bool[] leftControllerButtonStates;
    private bool[] rightControllerButtonStates;
    
    void Start()
    {
        StartCoroutine(InitializeDataCollection());
    }

    IEnumerator InitializeDataCollection()
    {
        yield return new WaitUntil(() => OVRPlugin.eyeTrackingEnabled);

        ovrCameraRig = GetComponent<OVRCameraRig>();
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig component not found!");
            yield break;
        }

        leftEyeGaze = GetComponentInChildren<OVREyeGaze>(true);
        rightEyeGaze = GetComponentInChildren<OVREyeGaze>(true);
        if (leftEyeGaze == null || rightEyeGaze == null)
        {
            Debug.LogError("Eye gaze components not found!");
            yield break;
        }

        // Initialize button states (now 8 buttons per controller, including thumbstick press)
        leftControllerButtonStates = new bool[8];
        rightControllerButtonStates = new bool[8];

        // Set file path to Downloads folder
        string downloadsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
        string filePath = Path.Combine(downloadsPath, "comprehensive_vr_data_collection.csv");
        writer = new StreamWriter(filePath, true);

        // Write header
        writer.WriteLine("Timestamp," +
                         "LeftEyeX,LeftEyeY,LeftEyeZ,RightEyeX,RightEyeY,RightEyeZ,LeftEyeObject,RightEyeObject," +
                         "HeadPositionX,HeadPositionY,HeadPositionZ," +
                         "HeadRotationX,HeadRotationY,HeadRotationZ,HeadRotationW," +
                         "HeadEulerX,HeadEulerY,HeadEulerZ," +
                         "LeftHandPositionX,LeftHandPositionY,LeftHandPositionZ," +
                         "LeftHandRotationX,LeftHandRotationY,LeftHandRotationZ,LeftHandRotationW," +
                         "LeftHandEulerX,LeftHandEulerY,LeftHandEulerZ," +
                         "RightHandPositionX,RightHandPositionY,RightHandPositionZ," +
                         "RightHandRotationX,RightHandRotationY,RightHandRotationZ,RightHandRotationW," +
                         "RightHandEulerX,RightHandEulerY,RightHandEulerZ," +
                         "LeftJoystickX,LeftJoystickY,RightJoystickX,RightJoystickY," +
                         "LeftPrimaryButton,LeftSecondaryButton,LeftTriggerButton,LeftGripButton,LeftStartButton,LeftThumbstickButton,LeftTouchpadButton,LeftThumbstickPress," +
                         "RightPrimaryButton,RightSecondaryButton,RightTriggerButton,RightGripButton,RightStartButton,RightThumbstickButton,RightTouchpadButton,RightThumbstickPress");

        StartCoroutine(CollectComprehensiveVRData());
    }

    IEnumerator CollectComprehensiveVRData()
    {
        while (true)
        {
            if (leftEyeGaze.isActiveAndEnabled && rightEyeGaze.isActiveAndEnabled)
            {
                Vector3 leftEyePosition = leftEyeGaze.transform.position;
                Vector3 rightEyePosition = rightEyeGaze.transform.position;
                string leftEyeObject = GetLookedAtObject(leftEyeGaze);
                string rightEyeObject = GetLookedAtObject(rightEyeGaze);

                Transform centerEyeAnchor = ovrCameraRig.centerEyeAnchor;
                Transform leftHandAnchor = ovrCameraRig.leftHandAnchor;
                Transform rightHandAnchor = ovrCameraRig.rightHandAnchor;

                // Get joystick positions
                Vector2 leftJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
                Vector2 rightJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);

                // Check button states
                UpdateButtonStates(OVRInput.Controller.LTouch, ref leftControllerButtonStates);
                UpdateButtonStates(OVRInput.Controller.RTouch, ref rightControllerButtonStates);

                string dataLine = $"{Time.time}," +
                                  $"{leftEyePosition.x},{leftEyePosition.y},{leftEyePosition.z}," +
                                  $"{rightEyePosition.x},{rightEyePosition.y},{rightEyePosition.z}," +
                                  $"{leftEyeObject},{rightEyeObject}," +
                                  $"{centerEyeAnchor.position.x},{centerEyeAnchor.position.y},{centerEyeAnchor.position.z}," +
                                  $"{centerEyeAnchor.rotation.x},{centerEyeAnchor.rotation.y},{centerEyeAnchor.rotation.z},{centerEyeAnchor.rotation.w}," +
                                  $"{centerEyeAnchor.eulerAngles.x},{centerEyeAnchor.eulerAngles.y},{centerEyeAnchor.eulerAngles.z}," +
                                  $"{leftHandAnchor.position.x},{leftHandAnchor.position.y},{leftHandAnchor.position.z}," +
                                  $"{leftHandAnchor.rotation.x},{leftHandAnchor.rotation.y},{leftHandAnchor.rotation.z},{leftHandAnchor.rotation.w}," +
                                  $"{leftHandAnchor.eulerAngles.x},{leftHandAnchor.eulerAngles.y},{leftHandAnchor.eulerAngles.z}," +
                                  $"{rightHandAnchor.position.x},{rightHandAnchor.position.y},{rightHandAnchor.position.z}," +
                                  $"{rightHandAnchor.rotation.x},{rightHandAnchor.rotation.y},{rightHandAnchor.rotation.z},{rightHandAnchor.rotation.w}," +
                                  $"{rightHandAnchor.eulerAngles.x},{rightHandAnchor.eulerAngles.y},{rightHandAnchor.eulerAngles.z}," +
                                  $"{leftJoystick.x},{leftJoystick.y},{rightJoystick.x},{rightJoystick.y}," +
                                  $"{string.Join(",", leftControllerButtonStates)}," +
                                  $"{string.Join(",", rightControllerButtonStates)}";

                writer.WriteLine(dataLine);
            }
            yield return new WaitForSeconds(0.01f); // Collect data every 10ms
        }
    }

    void UpdateButtonStates(OVRInput.Controller controller, ref bool[] buttonStates)
    {
        buttonStates[0] = OVRInput.Get(OVRInput.Button.One, controller);
        buttonStates[1] = OVRInput.Get(OVRInput.Button.Two, controller);
        buttonStates[2] = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller);
        buttonStates[3] = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller);
        buttonStates[4] = OVRInput.Get(OVRInput.Button.Start, controller);
        buttonStates[5] = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller);
        buttonStates[6] = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp, controller); // Using Up as a proxy for Touchpad
        buttonStates[7] = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller); // Thumbstick press
    }

    string GetLookedAtObject(OVREyeGaze eyeGaze)
    {
        RaycastHit hit;
        if (Physics.Raycast(eyeGaze.transform.position, eyeGaze.transform.forward, out hit, maxRaycastDistance, raycastLayerMask))
        {
                        return hit.collider.gameObject.name;
        }
                
        return "None";
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}
